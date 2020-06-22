from mesa import Model
from mesa.time import BaseScheduler
from mesa_geo import GeoSpace, AgentCreator

from cit_agent import CitAgent
from model_helper import CoalitionHelper


class NetLogoModel(Model):
    """A model with some number of agents."""

    def __init__(self, agent_list):
        # Initialize the schedule
        self.schedule = BaseScheduler(self)

        self.grid = GeoSpace()
        state_agent_kwargs = dict(model=self)
        AC = AgentCreator(agent_class=CitAgent,
                          agent_kwargs=state_agent_kwargs)

        # Create agents
        for agent_attr in agent_list:
            agent = CitAgent(self, agent_attr)
            self.schedule.add(agent)

    def step(self):
        '''Advance the model by one step.'''
        self.schedule.step()

        coalition_helper = CoalitionHelper(
            self.schedule.agents, "unique_id", "power", "pref")
        coalition_helper.form_coalition()
        # # Forming coalition
        # coalition = BSV(self.schedule.agents, "power", "pref", verbose=False)
        # # Show a list of how agents group together
        # print(len(coalition.result))
        # print(len(coalition.result[0].split('.')))
        # print(len(coalition.result[1].split('.')))
        # print(coalition.result)
        # Show a list of how agents grouped together and each groups power and preference attribute
        # print(coalition.result_verbose)
        # Show a dictionary of each group, the agents within that group
        # and each agents updated power and preference value based on their assimilation into the group
        # print(coalition.subresults)
