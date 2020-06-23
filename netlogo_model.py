from mesa import Model
from mesa.time import BaseScheduler
from mesa_geo import GeoSpace
from shapely.geometry import Polygon
import pandas as pd

import pprint

from cit_agent import CitAgent
from model_helper import CoalitionHelper


class NetLogoModel(Model):
    """A model with some number of agents."""

    def __init__(self, agent_list, geojson_list, verbose=False):
        self.verbose = verbose

        # Initialize the schedule
        self.grid = GeoSpace(crs={"init": "epsg:4326"})
        self.schedule = BaseScheduler(self)
        self.agents = agent_list.to_dict(orient='records')
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
            attr_list['pref'] = attr_list['pref'] / max_pref * 100
            # print(attr_list['pref'])

            # Then create an agent out of those
            agent = CitAgent(self, attr_list, shape)
            self.grid.add_agents(agent)
            self.schedule.add(agent)

            # Add the reference to that agent
            self.agent_dict[agent_attr['id']] = agent

    def get_neighbors(self, agent):
        # Get the direct neighbor of the specified agent
        return self.grid.get_neighbors(agent)

    def step(self, cur_tick):
        pp = pprint.PrettyPrinter(indent=4)

        if self.verbose:
            print("STARTING tick {}".format(cur_tick))

        '''Advance the model by one step.'''
        self.schedule.step()

        # Forming coalition
        coalition_helper = CoalitionHelper(
            self.schedule.agents, "unique_id", "power", "pref")
        coalition_list = coalition_helper.form_coalition(self, neighbor_type=1)

        if self.verbose:
            print("List of all coalition: ")
            pp.pprint(coalition_list)
            print("")

        # Update the preference of all agent
        for coalition in coalition_list:
            agent_1 = self.agent_dict[coalition['id_1']]
            agent_2 = self.agent_dict[coalition['id_2']]
            setattr(agent_1, "pref", coalition['coal_pref'])
            setattr(agent_2, "pref", coalition['coal_pref'])

        if self.verbose:
            print("ENDING tick {}\n".format(cur_tick))