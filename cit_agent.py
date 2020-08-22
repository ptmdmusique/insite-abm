from typing import Dict
from mesa.model import Model
from shapely.geometry.base import BaseGeometry
from mesa_geo import GeoAgent
import numpy as np

from agent_helper import AgentHelper


class CitAgent(GeoAgent):
    """A citizen in our model."""

    # * Default methods
    def __init__(self, model: Model, attr_list: Dict, shape):
        # * Initial State
        self.type: int = 1   # 1: regular citizen ; 2: in CBO
        self.message: int = 0  # Message cit sends out
        self.pent: float = 0   # For influence model 1
        self.own_pref: float = 0
        self.power: float = 0

        self.proximity: float = 0
        self.disruption: float = 1
        self.efficiency_parameter: float = 1.5
        self.ran: float = 0

        # * CBO Stuff
        self.cbo_pref: float = 0
        self.cbo_power: float = 0

        # * Stakeholder parameter setup
        # Always start as a non stakeholder negotiator
        self.can_negotiate_sh: bool = False
        self.sh_power: float = 0
        self.sh_pref: float = 0
        self.sh_utility: float = 0

        # * Other
        self.NGO_message: float = 0
        self.sponsor_message: float = 0

        # Make sure the shape is a shapely object
        if not isinstance(shape, BaseGeometry):
            raise TypeError("Shape must be a Shapely Geometry")
        super().__init__(attr_list['id'], model, shape)

        # Delete the id key:value since we already used it
        if attr_list.pop('id', None) is None:
            print("ERROR --- agent with unknown id found {}".format(attr_list))

        # Take in the attribute list and turn it
        #   into class attributes
        for k, v in attr_list.items():
            setattr(self, k, v)

    # * Coalition stuff
    def update_cit_coalition_attrs(self, pending_cit_coalition):
        AgentHelper.update_cit_coalition_attrs(self, pending_cit_coalition)

    def update_sh_coalition_attrs(self, pending_sh_coalition):
        AgentHelper.update_sh_coalition_attrs(self, pending_sh_coalition)

    # * Risk communication
    def communicate_sponsor_risk(self, need: float, sponsor_pref: float):
        """
        The utility message is received according to random chance and the
            ideology of the agent.
        The more positive the agent's attitude, the more likely it is to
            accept the utility's message.
        If the random number is higher than the agent's attitude, the agent
            becomes more disposed to the utility position by a random amount.
        If the random number is lower, there is a chance the agent will be
            "turned off" by the utility and become more opposed.
        """
        self.communicate_risk(need, self.sponsor_message, sponsor_pref)

    def communicate_big_ngo_risk(self, procedure: float, ngo_pref: float):
        """
        The mechanics for the NGO's message mimic those of the utility.
        If the random number is less than the attitude, augmented by
            the madcount, it accepts the message.
        If it is more, the madcount is increased.
        """
        self.communicate_risk(procedure, self.NGO_message, ngo_pref)

    def communicate_risk(self, main_attr: float, message, preference: float):
        if main_attr >= 5:
            # Generate a random number in [0, 0.05]
            random_float = np.random.random() * 0.15
            self.idatt += message * 10 / \
                abs(self.own_pref - preference)

            factor = 1
            cond_1 = message > main_attr and self.own_pref < preference
            cond_2 = message <= main_attr and self.own_pref > preference
            if cond_1 or cond_2:
                factor = -1

            self.idatt *= (1 + factor * random_float)
        else:
            # Citizen will revert back to NGO_message if main_attr is too low
            random_float = np.random.random() * 0.05
            self.idatt = (1 + random_float) * \
                (self.idatt + self.NGO_message * 0.01)
            # self.idatt = min(max(self.idatt, 0), 100)# Cap between 100 and 0

    # * Other
    def execute_label_up(self):
        ''' Label up '''
        self.pref = ((self.proximity * 100) + self.idatt) / 2
        # self.pref = min(max(self.pref, 0), 100)  # Cap between 100 and 0

        # Update the salience based on cits' type
        # (CBO or not CBO, that's the question)
        self.salience = self.disruption * self.proximity * self.type

        # Salient preference
        self.tpreference = self.pref * self.salience

        val_1 = self.pref * self.power * self.salience * 0.9
        # if hasattr(self, "cbo_power"):
        #     val_1 = self.pref * self.cbo_power * self.salience * 0.9

        val_2 = self.ran * 0.1
        self.im = (val_1 + val_2) * 1.2 / (200 * self.efficiency_parameter)

    def execute_influence_model(self):
        # * Influence model 1
        # self.pent += self.im
        # if self.pent >= 1:
        #     self.message += 1
        #     self.pent = 0
        # * Influence model 2
        self.message += self.im

    # * Main
    def step(self):
        pass
