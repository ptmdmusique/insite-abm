# Core libraries
from sh_agent import StakeholderAgent
from mesa import Model
from mesa.time import BaseScheduler
from mesa_geo import GeoSpace
from shapely.geometry import Polygon
# Statistics and loggings
from mesa.datacollection import DataCollector
from geopy import distance
import pprint
# Custom libraries
from cit_agent import CitAgent
from model_helper import CoalitionHelper, ModelCalculator
from agent_helper import AgentHelper
'''NOTES'''
# sh: stakeholder
# cit: citizen


class NetLogoModel(Model):
    """A model with some number of agents."""
    cit_list = []   # List of citizen
    cbo_list = []   # List of cits' CBOs
    sh_list = []    # List of stakeholder
    sh_cbo_list = []    # List of stakeholder's CBOs
    # For performance issue
    #   we use dict for fast lookup and modification
    agent_dict = {}

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

        self.talk_span = meta_data['talk_span']
        self.total_cit = meta_data['actual_num_cit']
        disruption = meta_data['disruption']
        NGO_message = meta_data['NGO_message']
        self.neighbor_type = neighbor_type
        self.efficiency_parameter = efficiency_parameter

        sum_power = 0
        # * Initialize agents
        if self.log_level >= 2:
            print("Initializing citizens")

        # --- Create citizens
        cit_list = cit_pd.to_dict(orient='records')
        for cit_attr in cit_list:
            # ? Should we leave this here
            if cit_attr['proximity'] > 1:
                continue

            # Extract the geojson of that agent
            shape = Polygon(
                geojson_list[str(cit_attr['id'])]['coordinates'][0])

            attr_list = cit_attr.copy()   # Make a copy to avoid side-effect
            attr_list['disruption'] = disruption
            attr_list['NGO_message'] = NGO_message
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
        sh_list = stakeholder_pd.to_dict(orient='records')
        for sh_attr in sh_list:
            attr_list = sh_attr.copy()   # Make a copy to avoid side-effect
            attr_list['disruption'] = disruption
            attr_list['NGO_message'] = NGO_message
            attr_list['efficiency_parameter'] = efficiency_parameter

            # Then create an agent out of those
            agent = StakeholderAgent(self, attr_list, cit_power=sum_power)
            self.schedule.add(agent)
            self.sh_list.append(agent)

            # Add the reference to that agent
            self.agent_dict[sh_attr['id']] = agent

        # * Initialize data collectors
        if self.log_level >= 2:
            print("Initializing data collectors")

        # Set up data collector
        self.datacollector = DataCollector(
            model_reporters={
                # "Total preference":
                # ModelCalculator.compute_total("pref"),
                # "Total utility":
                # ModelCalculator.compute_total("utility"),
                # "Total salient":
                # ModelCalculator.compute_total("salience"),
                # #  "Total salient preference":
                # #  ModelCalculator.compute_total("tpreference"),
                # "Idatt":
                # ModelCalculator.compute_total("idatt"),
                # "Total influence message":
                # ModelCalculator.compute_total("im"),
                # "Total own preference":
                # ModelCalculator.compute_total("own_pref"),
                # #  "Total power":
                # #  ModelCalculator.compute_total("power"),
                # "Total Message":
                # ModelCalculator.compute_total("message"),
            },
            agent_reporters={
                # "Preference": "pref",
                # "Utility": "utility",
                # "Salience": "salience",
                # #  "Salient preference": "tpreference",
                # "Idatt": "idatt",
                # "Influence message": "im",
                # #  "Own pref": "own_pref",
                # #  "Power": "power",
                # "Message": "message"
            }
        )

    def step(self):
        self.print_log(1, f"---STARTING tick {self.schedule.steps}")

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
            agent_1.update_cit_coalition_attrs(coalition)
            agent_2.update_cit_coalition_attrs(coalition)
            self.cbo_list.append(agent_1)
            self.cbo_list.append(agent_2)

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
            agent_1.update_sh_coalition_attrs(coalition)
            agent_2.update_sh_coalition_attrs(coalition)
            self.sh_cbo_list.append(agent_1)
            self.sh_cbo_list.append(agent_2)

        # ******** Advance the model by one step
        self.print_log(2, "Agents start stepping")
        self.schedule.step()

        # ******** Check for stakeholder
        for cit in self.schedule.agents:
            if cit.isSh is True and cit not in self.sh_list:
                self.sh_list.append(cit)

        # ******** Collect stats
        self.print_log(2, "Collecting data")
        self.datacollector.collect(self)

        self.print_log(1, f"---ENDING tick {self.schedule.steps}\n")

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
