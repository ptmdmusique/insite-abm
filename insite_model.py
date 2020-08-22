# Core libraries
from libpysal.weights.util import order
from shapely.geometry.polygon import orient
from regulator_agent import RegulatorAgent
from mesa import Model
from mesa.time import BaseScheduler
from mesa_geo import GeoSpace
from shapely.geometry import Polygon
from geopy import distance
# Statistics and loggings
from mesa.datacollection import DataCollector
import pprint
# Custom libraries
from stat_helper import CustomAgentDataCollector
from model_helper import CoalitionHelper, ModelCalculator
# Types
from typing import List, Dict
from cit_agent import CitAgent
from sh_agent import StakeholderAgent
from model_types import All_Agent_Type
'''NOTES'''
# sh: stakeholder
# cit: citizen


class InsiteModel(Model):
    """A model with some number of agents."""

    def __init__(self,
                 cit_pd, stakeholder_pd, regulator_pd,
                 geojson_list, meta_data, neighbor_type=0,
                 efficiency_parameter=1.5, log_level=0):
        self.log_level = log_level

        # * Default values
        # List of citizen
        self.cit_list: List[CitAgent] = []
        self.cbo_list = []   # List of cits' CBOs
        # List of those who can negotiate with stakeholder
        self.sh_negotiator_list = []
        # List of CBOs created after negotiating with stakeholder
        self.sh_in_coalition_list = []    # Can includes cits or stakeholder
        # Strictly regulator
        self.regulator_negotiator_list = []
        self.regulator_in_coalition_list: List[RegulatorAgent] = []
        # For performance issue
        #   we use dict for fast lookup and modification
        self.agent_dict: Dict[str, All_Agent_Type] = {}
        # List of stakeholder and regulator from csv file
        self.sh_list:  List[StakeholderAgent] = []
        self.regulator_list: List[RegulatorAgent] = []
        # Big-NGO and Utility-info
        self.need: float = 0
        self.procedure: float = 0
        # Otherb
        self.is_regulator_anti: bool = False

        # * Store all the dataframe for later use
        self.cit_pd = cit_pd
        self.stakeholder_pd = stakeholder_pd
        self.regulator_pd = regulator_pd

        self.print_log(2, "Initializing model")

        # * Initialize mesa
        self.grid = GeoSpace(crs={"init": "epsg:4326"})
        self.schedule = BaseScheduler(self)

        self.talk_span: float = meta_data['talk_span']
        self.total_cit: int = meta_data['actual_num_cit']

        self.disruption: float = meta_data['disruption']
        self.need: float = meta_data['need']
        self.NGO_message: float = meta_data['NGO_message']
        self.procedure: float = meta_data['procedure']
        self.sponsor_message: float = meta_data['sponsor_message']

        self.neighbor_type: int = neighbor_type
        self.efficiency_parameter: float = efficiency_parameter

        # * Initialize citizens
        self.print_log(2, "Initializing citizens")
        self.setup_cit(geojson_list)

        # * Initialize data collectors
        self.print_log(2, "Initializing data collectors")

        # Set up data collector
        self.datacollector = DataCollector(
            model_reporters={
                "Total preference": ModelCalculator.compute_total("pref"),
                "Total power": ModelCalculator.compute_total("power"),
            },
            agent_reporters={
                "Preference": "pref",
                "Power": "power",
            }
        )

        self.sh_collector: CustomAgentDataCollector = CustomAgentDataCollector(
            model_reporters={
                "Total sh preference": ModelCalculator.compute_total("sh_pref"),
            },
            agent_reporters={
                "Stakeholder preference": "sh_pref",
            },
            agent_list=self.sh_list
        )
        self.regulator_collector: CustomAgentDataCollector = CustomAgentDataCollector(
            model_reporters={
                "Total regulator preference": ModelCalculator.compute_total("regulator_pref"),
            },
            agent_reporters={
                "Regulator preference": "regulator_pref",
            },
            agent_list=self.regulator_list
        )

    def step(self):
        cur_tick = self.schedule.steps
        self.print_log(1, f"---STARTING tick {cur_tick}")
        '''Order'''
        # Cit will send messages starting from tick 0
        # Stakeholder will send messages starting from tick 1
        #   with whoever in stakeholder negotiator list
        # Regulator will send messages starting from tick 15
        #   with whoever in stakeholder negotiator list
        # Regulator will talk among themself starting from tick 21

        # * 1. Cits talking
        if cur_tick <= 20:
            # ******** Forming citizen coalition
            self.send_cit_messages()    # Turtle-talk

        # * 2. Regulator and stakeholder setup
        if cur_tick == 1:
            self.setup_stakeholder()    # Stakeholder-setup
        if cur_tick > 1:
            self.update_stakeholder_pre_tick()  # Stakeholder-setup 2

        if cur_tick == 15:
            self.setup_regulator()  # Regulator-setup
        if cur_tick > 15:
            self.update_regulator_pre_tick()  # Regulator-setup 2

        # * 3. Regulator and stakeholder talking
        if cur_tick <= 20 and cur_tick >= 1:
            # ******** Forming sh coalition
            self.send_sh_messages()     # Stakeholder-talk

        if cur_tick >= 21:
            self.send_regulator_messages()     # Regulator-talk

        # * Post tick update
        if cur_tick >= 1:
            # ******** Utility and Big-NGO
            self.communicate_sponsor_risk()  # Utility-info
            self.communicate_big_ngo_risk()  # Big-NGO

        self.merge_cbo()

        if cur_tick == 25:
            self.start_regulator_vote()

        self.execute_influence_model()
        self.execute_label_up()

        # ******** Advance the model by one step
        self.print_log(2, "Agents start stepping")
        self.schedule.step()

        # ******** Collect stats
        self.print_log(2, "Collecting data")
        self.datacollector.collect(self)
        self.sh_collector.collect(cur_tick)
        self.regulator_collector.collect(cur_tick)

        self.print_log(1, f"---ENDING tick {cur_tick}\n")

    #
    #
    '''------Main Methods------'''

    # * Cit stuff
    def setup_cit(self, geojson_list):
        # --- Create citizens
        for cit_attr in self.cit_pd.to_dict(orient='records'):
            # * Citizen
            # ? Should we leave this here
            if cit_attr['proximity'] > 1:
                continue

            # Extract the geojson of that agent
            shape = Polygon(
                geojson_list[str(cit_attr['id'])]['coordinates'][0])

            attr_list = cit_attr.copy()   # Make a copy to avoid side-effect
            attr_list['disruption'] = self.disruption
            attr_list['NGO_message'] = self.NGO_message
            attr_list['sponsor_message'] = self.sponsor_message
            attr_list['efficiency_parameter'] = self.efficiency_parameter

            # Then create an agent out of those
            agent = CitAgent(self, attr_list, shape)
            self.grid.add_agents(agent)
            self.schedule.add(agent)
            self.cit_list.append(agent)

            # Add the reference to that agent
            self.agent_dict[cit_attr['id']] = agent

    def send_cit_messages(self):
        # ******** Forming citizen coalition
        self.print_log(2, "Sending cit messages STARTS")

        coalition_helper = CoalitionHelper(
            "unique_id", "power",
            "own_pref", "utility",
            self.efficiency_parameter, log_level=self.log_level)
        # * Trying to form all coalitions
        #   and ignore the one who is already in CBO
        potential_coalition_list = coalition_helper.form_coalition(
            self.get_neighbor_dispatcher(self.neighbor_type),
            agent_list=self.cit_list,
            ignored_list=self.cbo_list)

        self.print_log(3, "List of all cits coalition: ",
                       potential_coalition_list)

        # * Update the cit coalition of all eligible agent
        for coalition in potential_coalition_list:
            agent_1 = self.agent_dict[coalition['id_1']]
            agent_2 = self.agent_dict[coalition['id_2']]
            # Make sure this is a citizen
            assert isinstance(agent_1, CitAgent)
            assert isinstance(agent_2, CitAgent)

            agent_1.update_cit_coalition_attrs(coalition)
            agent_2.update_cit_coalition_attrs(coalition)
            self.cbo_list.append(agent_1)
            self.cbo_list.append(agent_2)

            # Always add only agent 1 as sh
            self.sh_negotiator_list.append(agent_1)
        self.print_log(2, "Sending cit messages ENDS")

    def communicate_sponsor_risk(self):
        # Formerly utility-info
        counter = 0
        sum_pref = 0
        for stakeholder in self.sh_list:
            if stakeholder.is_sponsor:
                counter += 1
                sum_pref += stakeholder.sh_pref

        for cit in self.cit_list:
            cit.communicate_sponsor_risk(self.need, sum_pref / counter)

    def communicate_big_ngo_risk(self):
        # Formerly big-NGO
        counter = 0
        sum_pref = 0
        for stakeholder in self.sh_list:
            if stakeholder.is_big_ngo:
                counter += 1
                sum_pref += stakeholder.sh_pref

        for cit in self.cit_list:
            cit.communicate_big_ngo_risk(self.procedure, sum_pref / counter)

    def execute_influence_model(self):
        for cits in self.cit_list:
            cits.execute_influence_model()

    def execute_label_up(self):
        for cits in self.cit_list:
            cits.execute_label_up()

    # * Stakeholder stuff
    def setup_stakeholder(self):
        # --- Create stakeholders
        # First find the sum power of all cit
        sum_power = 0
        for cit in self.cit_list:
            sum_power += cit.power

        # Then create stakeholder based on that
        for sh_attr in self.stakeholder_pd.to_dict(orient='records'):
            attr_list = sh_attr.copy()   # Make a copy to avoid side-effect

            # Then create an agent out of those
            agent = StakeholderAgent(self, attr_list, cit_power=sum_power)
            # self.schedule.add(agent)  # ! Double check this
            self.sh_negotiator_list.append(agent)
            self.sh_list.append(agent)

            # Add the reference to that agent
            self.agent_dict[sh_attr['id']] = agent

    def update_stakeholder_pre_tick(self):
        pass

    def send_sh_messages(self):
        # ******** Forming sh coalition
        self.print_log(2, "Sending stakeholder messages STARTS")

        coalition_helper = CoalitionHelper(
            "unique_id", "sh_power",
            "sh_pref", "sh_utility",
            self.efficiency_parameter, log_level=self.log_level)
        # * Trying to form all coalitions
        #   and ignore the one who is already in CBO
        potential_coalition_list = coalition_helper.form_coalition(
            self.get_neighbor_dispatcher(0),    # Always get all neighbors
            agent_list=self.sh_negotiator_list,
            ignored_list=self.sh_in_coalition_list)

        self.print_log(3, "List of all sh coalition: ",
                       potential_coalition_list)

        # * Update the stakeholder coalition of all eligible agent
        for coalition in potential_coalition_list:
            agent_1 = self.agent_dict[coalition['id_1']]
            agent_2 = self.agent_dict[coalition['id_2']]
            # Make sure this is a citizen or stakeholder
            assert isinstance(
                agent_1, (StakeholderAgent, CitAgent, RegulatorAgent))
            assert isinstance(
                agent_2, (StakeholderAgent, CitAgent, RegulatorAgent))

            agent_1.update_sh_coalition_attrs(coalition)
            agent_2.update_sh_coalition_attrs(coalition)
            self.sh_in_coalition_list.append(agent_1)
            self.sh_in_coalition_list.append(agent_2)
        self.print_log(2, "Sending stakeholder messages ENDS")

    # * Regulator stuff
    def setup_regulator(self):
        # --- Create stakeholders
        # First find the sum power of all cit
        sum_power = 0
        for stakeholder in self.sh_negotiator_list:
            sum_power += stakeholder.sh_power

        # Then create stakeholder based on that
        for regulator_attr in self.regulator_pd.to_dict(orient='records'):
            # Make a copy to avoid side-effect
            attr_list = regulator_attr.copy()

            # Then create an agent out of those
            agent = RegulatorAgent(self, attr_list, sh_power=sum_power)
            # self.schedule.add(agent)  # ! Double check this
            self.regulator_list.append(agent)
            self.regulator_negotiator_list.append(agent)
            # Also negotiate with other stakeholders
            self.sh_negotiator_list.append(agent)

            # Add the reference to that agent
            self.agent_dict[regulator_attr['id']] = agent

    def update_regulator_pre_tick(self):
        pass

    def send_regulator_messages(self):
        # ******** Forming regulator coalition
        self.print_log(2, "Sending regluator messages STARTS")

        coalition_helper = CoalitionHelper(
            "unique_id", "regulator_power",
            "regulator_pref", "regulator_utility",
            self.efficiency_parameter, log_level=self.log_level)
        # * Trying to form all coalitions
        #   and ignore the one who is already in CBO
        potential_regulator_coalition_list = coalition_helper.form_coalition(
            self.get_neighbor_dispatcher(0),    # Always get all neighbors
            agent_list=self.regulator_negotiator_list,
            ignored_list=self.regulator_in_coalition_list)

        self.print_log(3, "List of all regulator coalition: ",
                       potential_regulator_coalition_list)

        # * Update the regulator coalition of all eligible agent
        for coalition in potential_regulator_coalition_list:
            agent_1 = self.agent_dict[coalition['id_1']]
            agent_2 = self.agent_dict[coalition['id_2']]
            # Make sure this is a regulator
            assert isinstance(agent_1, RegulatorAgent)
            assert isinstance(agent_2, RegulatorAgent)

            agent_1.update_regulator_coalition_attrs(coalition)
            agent_2.update_regulator_coalition_attrs(coalition)
            self.regulator_in_coalition_list.append(agent_1)
            self.regulator_in_coalition_list.append(agent_2)
        self.print_log(2, "Sending regulator messages ENDS")

    # * Others
    def merge_cbo(self):
        # ? Used for later displaying
        if self.schedule.steps == 1:
            pass
        elif self.schedule.steps <= 20:
            if self.schedule.steps == 20:
                # Halt cit in cbo
                pass
        else:
            pass

    def start_regulator_vote(self):
        num_regulator_anti = 0
        num_regulator_pro = 0
        for regulator in self.regulator_list:
            if regulator.regulator_pref <= 50:
                num_regulator_pro += 1
            else:
                num_regulator_anti
        if num_regulator_pro <= num_regulator_anti:
            self.is_regulator_anti = True

    #
    #
    '''------Helpers------'''

    # Loggings
    def print_log(self, log_level, log_string, json_data=None):
        if self.log_level >= log_level:
            print(log_string, flush=True)
            if json_data is not None:
                pp = pprint.PrettyPrinter(indent=2)
                pp.pprint(json_data)

    # Neighbor issues...
    # Helper to trieve neighbor list of an agent
    def get_neighbor_dispatcher(self, neighbor_type):
        """Generate list of neighbor of the "agent"

        Args:
            agent_list ([list]): list of agent to check
            agent ([object]): current agent
            neighbor_type ([int]): type of neightbor to get for each cit
                0: treats all cits as neighbor
                1: direct neighbor
                2: small world network # TODO
                3: talk span in km
                (default: {0})
        """

        if neighbor_type == 0:
            return self.get_all_as_neighbors
        if neighbor_type == 1:
            return self.get_direct_neighbors
        if neighbor_type == 3:
            return self.get_neighbors_within_talk_span

    def get_all_as_neighbors(self, _, agent_list):
        # Make a copy of the agent list
        return agent_list[:]

    def get_direct_neighbors(self, agent, _):
        # Get the direct neighbor of the specified agent
        potential_neighbors = self.grid.get_neighbors(agent)
        return potential_neighbors

    def get_neighbors_within_talk_span(self, agent, agent_list):
        result = []
        agent_center = agent.shape.centroid
        # Reverse tuple for lat long
        agent_coords = list(agent_center.coords)[0][::-1]

        for other in agent_list:
            if agent is other or other:
                continue

            other_center = other.shape.centroid
            other_coords = list(other_center.coords)[0][::-1]

            coord_distance = distance.distance(agent_coords, other_coords).km
            if coord_distance < self.talk_span:
                result.append(other)

        return result
