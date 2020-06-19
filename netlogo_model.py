from mesa import Model
from mesa.time import BaseScheduler
from cit_agent import CitAgent
from bsv import BSV
import pandas as pd


class NetLogoModel(Model):
    """A model with some number of agents."""

    def __init__(self, agent_list):
        # Initialize the schedule
        self.num_agents = len(agent_list)
        self.schedule = BaseScheduler(self)

        # Create agents
        for agent_attr in agent_list:
            agent = CitAgent(self, agent_attr)
            self.schedule.add(agent)

    def step(self):
        '''Advance the model by one step.'''
        self.schedule.step()

        # Forming coalition
        coalition = BSV(self.schedule.agents, "power", "pref", verbose=False)
        # Show a list of how agents group together
        print(coalition.result)
        # Show a list of how agents grouped together and each groups power and preference attribute
        print(coalition.result_verbose)
        # Show a dictionary of each group, the agents within that group
        # and each agents updated power and preference value based on their assimilation into the group
        print(coalition.subresults)
