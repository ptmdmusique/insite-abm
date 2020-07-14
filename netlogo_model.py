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
from cit_agent import CitAgent
from model_helper import CoalitionHelper, ModelCalculator


class NetLogoModel(Model):
    """A model with some number of agents."""

    def __init__(self, agent_list, geojson_list, other_data,
                 neighbor_type=0,
                 efficiency_parameter=1.5, log_level=0):
        self.log_level = log_level

        if self.log_level >= 2:
            print("Initializing model")

        # Initialize the schedule
        self.grid = GeoSpace(crs={"init": "epsg:4326"})
        self.schedule = BaseScheduler(self)
        self.agents = agent_list.to_dict(orient='records')
        self.efficiency_parameter = efficiency_parameter

        # Other info
        self.cbo_list = []
        self.talk_span = other_data['talk_span']
        self.total_cit = other_data['actual_num_cit']
        disruption = other_data['disruption']
        NGO_message = other_data['NGO_message']
        self.neighbor_type = neighbor_type

        # For performance issue
        #   we use dict for fast lookup and modification
        self.agent_dict = {}

        # Get the max preference to normalize it
        # max_pref = agent_list['pref'].max()

        if self.log_level >= 2:
            print("Initializing agents")

        # Create agents
        for agent_attr in self.agents:
            # Extract the geojson of that agent
            shape = Polygon(
                geojson_list[str(agent_attr['id'])]['coordinates'][0])

            # Normalize the preference to the range [0, 100]
            #   to make sure no agent has pref > 100
            #   (clamping from [0, maxPref] to [0, 100])
            # ? Is this a good solution?
            attr_list = agent_attr.copy()   # Make a copy to avoid side-effect
            # TODO: Check whether we should normalize stuff here
            # attr_list['pref'] = attr_list['pref'] / max_pref * 100

            attr_list['disruption'] = disruption
            attr_list['NGO_message'] = NGO_message
            attr_list['efficiency_parameter'] = efficiency_parameter

            # Then create an agent out of those
            agent = CitAgent(self, attr_list, shape)
            self.grid.add_agents(agent)
            self.schedule.add(agent)

            # Add the reference to that agent
            self.agent_dict[agent_attr['id']] = agent

        if self.log_level >= 2:
            print("Initializing data collectors")

        # Set up data collector
        self.datacollector = DataCollector(
            model_reporters={"Total preference":
                             ModelCalculator.compute_total("pref"),
                             "Total own preference":
                             ModelCalculator.compute_total("own-pref"),
                             "Total utility":
                             ModelCalculator.compute_total("utility"),
                             "Total salient preference":
                             ModelCalculator.compute_total("tpreference"),
                             "Idatt":
                             ModelCalculator.compute_total("idatt"),
                             "Total influence message":
                             ModelCalculator.compute_total("im"),
                             "Total power":
                             ModelCalculator.compute_total("power"),
                             },
            agent_reporters={"Preference": "pref",
                             "Utility": "utility",
                             "Salient preference": "tpreference",
                             "Idatt": "idatt",
                             "Influence message": "im",
                             "Own pref": "own-pref",
                             "Power": "power"
                             }
        )

    def step(self):
        self.print_log(1, f"---STARTING tick {self.schedule.steps}")

        # Forming coalition
        self.print_log(2, "Sending messages for potential coalitions")
        coalition_helper = CoalitionHelper(
            self.schedule.agents,
            "unique_id", "power", "own-pref", "utility",
            self.efficiency_parameter, log_level=self.log_level)
        coalition_list = coalition_helper.form_coalition(
            self.get_neighbor_dispatcher(), self.cbo_list)
        self.print_log(3, "List of all coalition: ", coalition_list)

        # Update the coalition of all eligible agent
        self.print_log(2, "Sending messages to agent for new coalition list")
        for coalition in coalition_list:
            agent_1 = self.agent_dict[coalition['id_1']]
            agent_2 = self.agent_dict[coalition['id_2']]
            setattr(agent_1, "new_coalition", coalition)
            setattr(agent_2, "new_coalition", coalition)
            self.cbo_list.append(agent_1)
            self.cbo_list.append(agent_2)

        # TODO: Clean this up
        # self.print_log(2, "Sending messages to agent for EXPIRE coalition")

        # Advance the model by one step
        self.print_log(2, "Agents start stepping")
        self.schedule.step()

        # Collect stats
        self.print_log(2, "Collecting data")
        self.datacollector.collect(self)

        self.print_log(1, f"---ENDING tick {self.schedule.steps}\n")

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
    def get_neighbor_dispatcher(self):
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

        if self.neighbor_type == 0:
            return self.get_all_as_neighbors
        if self.neighbor_type == 1:
            return self.get_direct_neighbors
        if self.neighbor_type == 3:
            return self.get_neighbors_within_talk_span

    def get_all_as_neighbors(self, _):
        # Make a copy of the agent list
        return self.schedule.agents[:]

    def get_direct_neighbors(self, agent):
        # Get the direct neighbor of the specified agent
        potential_neighbors = self.grid.get_neighbors(agent)
        neighbors = list(
            filter(lambda n: n not in self.cbo_list, potential_neighbors))
        return neighbors

    def get_neighbors_within_talk_span(self, agent):
        result = []
        agent_center = agent.shape.centroid
        # Reverse tuple for lat long
        agent_coords = list(agent_center.coords)[0][::-1]

        for other in self.schedule.agents:
            if agent is other:
                continue

            other_center = other.shape.centroid
            other_coords = list(other_center.coords)[0][::-1]

            coord_distance = distance.distance(agent_coords, other_coords).km
            if coord_distance < self.talk_span:
                result.append(other)

        return result
