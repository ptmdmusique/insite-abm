from mesa import Model
from mesa.time import BaseScheduler
from mesa_geo import GeoSpace
from shapely.geometry import Polygon

from cit_agent import CitAgent
from model_helper import CoalitionHelper


class NetLogoModel(Model):
    """A model with some number of agents."""

    def __init__(self, agent_list, geojson_list):
        # Initialize the schedule
        self.grid = GeoSpace(crs={"init": "epsg:4326"})
        self.schedule = BaseScheduler(self)

        # Create agents
        for agent_attr in agent_list:
            # Get the id, extract the geojson of that agent and
            #   create an agent out of that
            shape = Polygon(
                geojson_list[str(agent_attr['id'])]['coordinates'][0])
            agent = CitAgent(self, agent_attr, shape)
            self.grid.add_agents(agent)
            self.schedule.add(agent)

    def get_neighbors(self, agent):
        # Get the direct neighbor of the specified agent
        return self.grid.get_neighbors(agent)

    def step(self):
        '''Advance the model by one step.'''
        self.schedule.step()

        # Forming coalition
        coalition_helper = CoalitionHelper(
            self.schedule.agents, "unique_id", "power", "pref")
        coalition_helper.form_coalition(self, neighbor_type=1)
