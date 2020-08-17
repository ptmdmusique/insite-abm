from typing import Dict
from mesa import Agent
from mesa.model import Model

from agent_helper import AgentHelper


class RegulatorAgent(Agent):
    """A pure stakeholder in our model."""
    is_sh = True  # Always a stakeholder
    is_regulator = True

    def __init__(self, model: Model, attr_list: Dict, sh_power: float):
        super().__init__(attr_list['id'], model)

        # Delete the id key:value since we already used it
        if attr_list.pop('id', None) is None:
            print("ERROR --- agent with unknown id found {}".format(attr_list))

        # Take in the attribute list and turn it
        #   into class attributes
        for k, v in attr_list.items():
            setattr(self, k, v)

        # ! Refactor this later
        self.regulator_pref = attr_list['rown-pref']
        self.sh_pref = self.regulator_pref
        # ? What is this constant?
        self.regulator_power = sh_power * attr_list['scale'] * 2
        self.sh_power = self.regulator_power
        self.regulator_utility = 100 * self.regulator_power

    def update_regulator_coalition_attrs(self, pending_regulator_coalition):
        AgentHelper.update_regulator_coalition_attrs(
            self, pending_regulator_coalition)
