# Core libraries
from mesa import Model
from mesa.time import BaseScheduler
from mesa_geo import GeoSpace
from shapely.geometry import Polygon
# Statistics and loggings
from mesa.datacollection import DataCollector
from geopy import distance
import pprint
# Custom libraries
from model_helper import CoalitionHelper, ModelCalculator
# Types
from typing import List, Dict
from cit_agent import CitAgent
from sh_agent import StakeholderAgent
from model_types import All_Agent_Type
'''NOTES'''
# sh: stakeholder
# cit: citizen


class NetLogoModel(Model):
    """A model with some number of agents."""
    # List of citizen
    cit_list: List[CitAgent] = []
    cbo_list = []   # List of cits' CBOs
    sh_list = []
    sh_cbo_list = []    # List of stakeholder's CBOs
    # For performance issue
    #   we use dict for fast lookup and modification
    agent_dict: Dict[str, All_Agent_Type] = {}

    # Other
    # List of stakeholder from csv file
    special_sh_list:  List[StakeholderAgent] = []

    # Big-NGO and Utility-info
    need: float = 0
    procedure: float = 0

    # * Default methods
    def __init__(self,
                 cit_pd,
                 stakeholder_pd,
                 regulator_pd,
                 geojson_list,

                 meta_data,

                 neighbor_type=0,
                 efficiency_parameter=1.5, log_level=0):
        self.log_level = log_level

        if self.log_level >= 2:
            print("Initializing model")

        # * Initialize mesa
        self.grid = GeoSpace(crs={"init": "epsg:4326"})
        self.schedule = BaseScheduler(self)

        self.talk_span: float = meta_data['talk_span']
        self.total_cit: int = meta_data['actual_num_cit']

        disruption: float = meta_data['disruption']
        self.need: float = meta_data['need']
        NGO_message: float = meta_data['NGO_message']
        self.procedure: float = meta_data['procedure']
        sponsor_message: float = meta_data['sponsor_message']

        self.neighbor_type: int = neighbor_type
        self.efficiency_parameter: float = efficiency_parameter

        # * Initialize agents
        sum_power = 0
        if self.log_level >= 2:
            print("Initializing citizens")

        # --- Create citizens
        for cit_attr in cit_pd.to_dict(orient='records'):
            # * Citizen
            # ? Should we leave this here
            if cit_attr['proximity'] > 1:
                continue

            # Extract the geojson of that agent
            shape = Polygon(
                geojson_list[str(cit_attr['id'])]['coordinates'][0])

            attr_list = cit_attr.copy()   # Make a copy to avoid side-effect
            attr_list['disruption'] = disruption
            attr_list['NGO_message'] = NGO_message
            attr_list['sponsor_message'] = sponsor_message
            attr_list['efficiency_parameter'] = efficiency_parameter

            # Then create an agent out of those
            agent = CitAgent(self, attr_list, shape)
            self.grid.add_agents(agent)
            self.schedule.add(agent)
            self.cit_list.append(agent)

            # Add the reference to that agent
            self.agent_dict[cit_attr['id']] = agent

            sum_power += attr_list['power']

        # --- Create stakeholders
        for sh_attr in stakeholder_pd.to_dict(orient='records'):
            # * Stakeholder
            attr_list = sh_attr.copy()   # Make a copy to avoid side-effect

            # Then create an agent out of those
            agent = StakeholderAgent(self, attr_list, cit_power=sum_power)
            # self.schedule.add(agent)  # ! Double check this
            self.sh_list.append(agent)
            self.special_sh_list.append(agent)

            # Add the reference to that agent
            self.agent_dict[sh_attr['id']] = agent

        # * Initialize data collectors
        if self.log_level >= 2:
            print("Initializing data collectors")

        # Set up data collector
        self.datacollector = DataCollector(
            model_reporters={
                "Total preference":
                ModelCalculator.compute_total("pref"),
                "Total utility":
                ModelCalculator.compute_total("utility"),
                "Total salient":
                ModelCalculator.compute_total("salience"),
                #  "Total salient preference":
                #  ModelCalculator.compute_total("tpreference"),
                "Idatt":
                ModelCalculator.compute_total("idatt"),
                "Total influence message":
                ModelCalculator.compute_total("im"),
                "Total own preference":
                ModelCalculator.compute_total("own_pref"),
                #  "Total power":
                #  ModelCalculator.compute_total("power"),
                "Total Message":
                ModelCalculator.compute_total("message"),
            },
            agent_reporters={
                "Preference": "pref",
                "Utility": "utility",
                "Salience": "salience",
                #  "Salient preference": "tpreference",
                "Idatt": "idatt",
                "Influence message": "im",
                #  "Own pref": "own_pref",
                #  "Power": "power",
                "Message": "message"
            }
        )

    def step(self):
        self.print_log(1, f"---STARTING tick {self.schedule.steps}")

        # ******** Forming citizen coalition
        self.send_cit_messages()

        # ******** Forming sh coalition
        self.send_sh_messages()

        # ******** Utility and Big-NGO
        self.communicate_big_ngo_risk()
        self.communicate_sponsor_risk()

        # ******** Advance the model by one step
        self.print_log(2, "Agents start stepping")
        self.schedule.step()

        # ******** Collect stats
        self.print_log(2, "Collecting data")
        self.datacollector.collect(self)

        self.print_log(1, f"---ENDING tick {self.schedule.steps}\n")

    #
    #
    '''------Main Methods------'''

    def send_cit_messages(self):
        # ******** Forming citizen coalition
        self.print_log(2, "Sending messages for potential coalitions")

        cit_coalition_helper = CoalitionHelper(
            "unique_id", "power",
            "own_pref", "utility",
            self.efficiency_parameter, log_level=self.log_level)
        # * Trying to form all coalitions
        #   and ignore the one who is already in CBO
        cit_coalition_list = cit_coalition_helper.form_coalition(
            self.get_neighbor_dispatcher(self.neighbor_type),
            agent_list=self.cit_list,
            ignored_list=self.cbo_list)

        self.print_log(3, "List of all coalition: ", cit_coalition_list)

        # * Update the cit coalition of all eligible agent
        self.print_log(2, "Sending messages to agent for new coalition list")
        for coalition in cit_coalition_list:
            agent_1 = self.agent_dict[coalition['id_1']]
            agent_2 = self.agent_dict[coalition['id_2']]
            # Make sure this is a citizen
            assert isinstance(agent_1, CitAgent)
            assert isinstance(agent_2, CitAgent)

            agent_1.update_cit_coalition_attrs(coalition)
            agent_2.update_cit_coalition_attrs(coalition)
            self.cbo_list.append(agent_1)
            self.cbo_list.append(agent_2)

            self.sh_list.append(agent_1)    # Always add only agent 1 as sh

    def send_sh_messages(self):
        # ******** Forming sh coalition
        self.print_log(2, "Sending messages for potential coalitions")

        sh_coalition_helper = CoalitionHelper(
            "unique_id", "sh_power",
            "sh_pref", "sh_utility",
            self.efficiency_parameter, log_level=self.log_level)
        # * Trying to form all coalitions
        #   and ignore the one who is already in CBO
        sh_coalition_list = sh_coalition_helper.form_coalition(
            self.get_neighbor_dispatcher(0),    # Always get all neighbors
            agent_list=self.sh_list,
            ignored_list=self.sh_cbo_list)

        self.print_log(3, "List of all coalition: ", sh_coalition_list)

        # * Update the stakeholder coalition of all eligible agent
        self.print_log(2, "Sending messages to agent for new coalition list")
        for coalition in sh_coalition_list:
            agent_1 = self.agent_dict[coalition['id_1']]
            agent_2 = self.agent_dict[coalition['id_2']]
            # Make sure this is a citizen or stakeholder
            assert isinstance(agent_1, (StakeholderAgent, CitAgent))
            assert isinstance(agent_2, (StakeholderAgent, CitAgent))

            agent_1.update_sh_coalition_attrs(coalition)
            agent_2.update_sh_coalition_attrs(coalition)
            self.sh_cbo_list.append(agent_1)
            self.sh_cbo_list.append(agent_2)

    def communicate_sponsor_risk(self):
        # Formerly utility-info
        counter = 0
        sum_pref = 0
        for stakeholder in self.special_sh_list:
            if stakeholder.is_sponsor:
                counter += 1
                sum_pref += stakeholder.sh_pref

        for cit in self.cit_list:
            cit.communicate_sponsor_risk(self.need, sum_pref / counter)

    def communicate_big_ngo_risk(self):
        # Formerly big-NGO
        counter = 0
        sum_pref = 0
        for stakeholder in self.special_sh_list:
            if stakeholder.is_big_ngo:
                counter += 1
                sum_pref += stakeholder.sh_pref

        for cit in self.cit_list:
            cit.communicate_big_ngo_risk(self.procedure, sum_pref / counter)

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
        neighbors = list(
            filter(lambda n: n not in self.cbo_list, potential_neighbors))
        return neighbors

    def get_neighbors_within_talk_span(self, agent, agent_list):
        result = []
        agent_center = agent.shape.centroid
        # Reverse tuple for lat long
        agent_coords = list(agent_center.coords)[0][::-1]

        for other in agent_list:
            if agent is other:
                continue

            other_center = other.shape.centroid
            other_coords = list(other_center.coords)[0][::-1]

            coord_distance = distance.distance(agent_coords, other_coords).km
            if coord_distance < self.talk_span:
                result.append(other)

        return result
