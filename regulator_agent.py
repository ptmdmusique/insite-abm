from typing import Dict
from mesa import Agent
from mesa.model import Model

from agent_helper import AgentHelper


class RegulatorAgent(Agent):
    """A pure stakeholder in our model."""

    def __init__(self, model: Model, attr_list: Dict, sh_power: float):
        super().__init__(attr_list['id'], model)
        self.can_negotiate_sh = True  # Always a stakeholder
        self.can_negotiate_regulator = True

        # Delete the id key:value since we already used it
        if attr_list.pop('id', None) is None:
            print("ERROR --- agent with unknown id found {}".format(attr_list))

        # Take in the attribute list and turn it
        #   into class attributes
        for k, v in attr_list.items():
            setattr(self, k, v)

        # ! Refactor this later
        self.regulator_pref = attr_list['rown-pref']
        self.own_pref = self.regulator_pref
        self.sh_pref = self.regulator_pref
        # ? What is this constant?
        self.regulator_power = sh_power * attr_list['scale'] * 2
        self.sh_power = self.regulator_power
        self.regulator_utility = 100 * self.regulator_power
        self.sh_utility = 100 * self.regulator_utility

    def update_sh_coalition_attrs(self, pending_coalition):
        AgentHelper.update_sh_coalition_attrs(self, pending_coalition)

    def update_regulator_coalition_attrs(self, pending_coalition):
        AgentHelper.update_regulator_coalition_attrs(self, pending_coalition)
