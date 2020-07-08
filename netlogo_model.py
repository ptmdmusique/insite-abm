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
                 efficiency_parameter=1.5, verbose=False):
        self.verbose = verbose

        # Initialize the schedule
        self.grid = GeoSpace(crs={"init": "epsg:4326"})
        self.schedule = BaseScheduler(self)
        self.agents = agent_list.to_dict(orient='records')
        self.efficiency_parameter = efficiency_parameter

        # Other info
        self.talk_span = other_data['talk_span']
        self.total_cit = other_data['actual_num_cit']
        disruption = other_data['disruption']
        NGO_message = other_data['NGO_message']

        # For performance issue
        #   we use dict for fast lookup and modification
        self.agent_dict = {}

        # Get the max preference to normalize it
        max_pref = agent_list['pref'].max()

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

        if self.verbose:
            print("STARTING tick {}".format(self.schedule.steps))

        # Reset citizen's type
        for agent in self.schedule.agents:
            # Set the coalition that this citizen is in in this tick
            setattr(agent, "coalition", None)

        # Forming coalition
        coalition_helper = CoalitionHelper(
            self.schedule.agents,
            "unique_id", "power", "own-pref", "utility",
            self.efficiency_parameter)
        coalition_list = coalition_helper.form_coalition(self, neighbor_type=0)

        if self.verbose:
            print("List of all coalition: ")
            pp.pprint(coalition_list)
            print("")

        # Update the coalition of all eligible agent
        for coalition in coalition_list:
            agent_1 = self.agent_dict[coalition['id_1']]
            agent_2 = self.agent_dict[coalition['id_2']]
            setattr(agent_1, "coalition", coalition)
            setattr(agent_2, "coalition", coalition)

        # Advance the model by one step
        self.schedule.step()

        # Collect stats
        self.datacollector.collect(self)

        if self.verbose:
            print("ENDING tick {}\n".format(self.schedule.steps))
