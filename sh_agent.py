from typing import Dict
from mesa import Agent
from mesa.model import Model

from agent_helper import AgentHelper


class StakeholderAgent(Agent):
    """A pure stakeholder in our model."""
    can_negotiate_sh = True  # Always a stakeholder
    is_sponsor = False
    is_big_ngo = False

    def __init__(self, model: Model, attr_list: Dict, cit_power: float):
        super().__init__(attr_list['id'], model)

        # Delete the id key:value since we already used it
        if attr_list.pop('id', None) is None:
            print("ERROR --- agent with unknown id found {}".format(attr_list))

        # Take in the attribute list and turn it
        #   into class attributes
        for k, v in attr_list.items():
            setattr(self, k, v)

        # ! Refactor this later
        self.sh_pref = attr_list['sown-pref']
        # ? What is this constant?
        self.sh_power = cit_power * attr_list['scale'] / 18.5265
        self.sh_utility = 100 * self.sh_power

        if attr_list['sponsor'] == 1:
            self.is_sponsor = True

        if attr_list['big_ngo'] == 1:
            self.is_big_ngo = True

    def update_sh_coalition_attrs(self, pending_sh_coalition):
        AgentHelper.update_sh_coalition_attrs(self, pending_sh_coalition)
