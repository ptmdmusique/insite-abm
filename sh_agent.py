import numpy as np
from mesa import Agent

from agent_helper import AgentHelper


class StakeholderAgent(Agent):
    """A pure stakeholder in our model."""
    isSh = True  # Always a stakeholder

    def __init__(self, model, attr_list, cit_power):
        super().__init__(attr_list['id'], model)

        # Delete the id key:value since we already used it
        if attr_list.pop('id', None) is None:
            print("ERROR --- agent with unknown id found {}".format(attr_list))

        # ! Refactor this later
        self.sh_pref = attr_list['sown-pref']
        # ? What is this constant?
        self.sh_power = cit_power * attr_list['scale'] / 18.5265
        self.sh_utility = 0  # ? What should this be?
        # Take in the attribute list and turn it
        #   into class attributes
        for k, v in attr_list.items():
            setattr(self, k, v)

    def update_sh_coalition_attrs(self, pending_sh_coalition):
        AgentHelper.update_sh_coalition_attrs(self, pending_sh_coalition)
