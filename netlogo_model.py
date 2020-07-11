# Core libraries
from mesa import Model
from mesa.time import BaseScheduler
from mesa_geo import GeoSpace
from shapely.geometry import Polygon
# Statistics and loggings
from mesa.datacollection import DataCollector
import pprint
# Custom libraries
from cit_agent import CitAgent
from model_helper import CoalitionHelper, ModelCalculator


class NetLogoModel(Model):
    """A model with some number of agents."""

    def __init__(self, agent_list, geojson_list, other_data,
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
        self.talk_span = other_data['talk_span'] * 1000  # * 1000 for km to m
        self.total_cit = other_data['actual_num_cit']
        disruption = other_data['disruption']
        NGO_message = other_data['NGO_message']

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
                             },
            agent_reporters={"Preference": "pref",
                             "Utility": "utility",
                             "Salient preference": "tpreference",
                             "Idatt": "idatt",
                             "Influence message": "im",
                             "Own pref": "own-pref"
                             }
        )

    def get_neighbors(self, agent):
        # Get the direct neighbor of the specified agent
        return self.grid.get_neighbors(agent)

    def step(self):
        pp = pprint.PrettyPrinter(indent=4)

        if self.log_level >= 1:
            print("STARTING tick {}".format(self.schedule.steps), flush=True)

        if self.log_level >= 2:
            print("Restting agents", flush=True)

        # Reset citizen's type
        for agent in self.schedule.agents:
            # Set the coalition that this citizen is in in this tick
            setattr(agent, "coalition", None)

        if self.log_level >= 2:
            print("Sending messages for potential coalitions", flush=True)

        # Forming coalition
        coalition_helper = CoalitionHelper(
            self.schedule.agents,
            "unique_id", "power", "own-pref", "utility",
            self.efficiency_parameter)
        coalition_list = coalition_helper.form_coalition(
            self, neighbor_type=3, talk_span=self.talk_span)

        if self.log_level >= 3:
            print("List of all coalition: ", flush=True)
            pp.pprint(coalition_list)

        if self.log_level >= 2:
            print("Setting agents' coalition list", flush=True)

        # Update the coalition of all eligible agent
        for coalition in coalition_list:
            agent_1 = self.agent_dict[coalition['id_1']]
            agent_2 = self.agent_dict[coalition['id_2']]
            setattr(agent_1, "coalition", coalition)
            setattr(agent_2, "coalition", coalition)

        if self.log_level >= 2:
            print("Agents start stepping", flush=True)

        # Advance the model by one step
        self.schedule.step()

        if self.log_level >= 2:
            print("Collecting data", flush=True)

        # Collect stats
        self.datacollector.collect(self)

        if self.log_level >= 1:
            print("ENDING tick {}\n".format(self.schedule.steps), flush=True)
