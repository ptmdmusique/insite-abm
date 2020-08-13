from shapely.geometry.base import BaseGeometry
from mesa_geo import GeoAgent
import numpy as np


class CitAgent(GeoAgent):
    """A citizen in our model."""
    # * Initial State
    type = 1   # 1: regular citizen ; 2: in CBO
    cbo_pref = 0
    message = 0  # Message cit sends out
    pent = 0   # For influence model 1
    pending_cit_coalition = None   # New coalition pending for this tick
    pending_sh_coalition = None   # New coalition pending for this tick

    # * Stakeholder parameter setup
    isSh = False  # Always start as a non stakeholder
    sh_power = 0
    sh_pref = 0
    sh_utility = 0
    isSh = 0

    # * Methods
    def __init__(self, model, attr_list, shape):
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

    def update_cit_coalition_attrs(self):
        # Update attributes every step after cits coalitions are formed
        self.own_pref = self.pending_cit_coalition['coal_pref']
        self.type = 2   # Change type to CBO
        self.cbo_pref = self.pending_cit_coalition['coal_pref']
        self.power *= self.efficiency_parameter

        # First agent on the coalition (initiative citizen)
        if self.unique_id == self.pending_cit_coalition['id_1']:
            self.utility = self.pending_cit_coalition['utility_1']
            self.cbo_utility = self.pending_cit_coalition['utility_1']
            self.cbo_power = self.pending_cit_coalition['coal_power']

            # They also become a sh
            # * Only the initiative citizen becomes the stake holder
            # *   since they are the one who sent out messages in the first
            self.isSh = True

            # Update sh stuff
            self.sh_pref = self.pending_cit_coalition['coal_pref']
            self.sh_power = self.pending_cit_coalition['coal_power']
            self.sh_utility = self.pending_cit_coalition['utility_1']
        else:
            self.utility = self.pending_cit_coalition['utility_2']
            self.cbo_utility = self.pending_cit_coalition['utility_2']
            self.cbo_power = 0  # TODO: Check this
            # self.cbo_power = self.pending_cit_coalition['coal_power']

        self.pending_cit_coalition = None   # Clean up

    def update_sh_coalition_attrs(self):
        # Update attributes every step after stakeholder coalitions are formed
        self.own_pref = self.pending_sh_coalition['coal_pref']
        self.sh_pref = self.pending_sh_coalition['coal_pref']

        self.sh_cbo_pref = self.pending_sh_coalition['coal_pref']
        self.sh_cbo_power = self.pending_sh_coalition['coal_power']

        # First agent on the coalition (initiative citizen)
        if self.unique_id == self.pending_sh_coalition['id_1']:
            self.sh_utility = self.pending_sh_coalition['utility_1']
            self.sh_cbo_utility = self.pending_sh_coalition['utility_1']
        else:
            self.sh_utility = self.pending_sh_coalition['utility_2']
            self.sh_cbo_utility = self.pending_sh_coalition['utility_2']
            self.sh_cbo_power = 0  # TODO: Check this

        self.pending_sh_coalition = None   # Clean up

    def update_post_tick_attribute(self):
        ''' utility-info '''
        # Generate a random number in [0, 0.05]
        random_float = np.random.random() * 0.05
        self.idatt = (1 + random_float) * \
            (self.idatt + self.NGO_message * 0.01)
        # self.idatt = min(max(self.idatt, 0), 100)  # Cap between 100 and 0

        ''' Label up '''
        # Update the salience based on cits' type
        self.pref = ((self.proximity * 100) + self.idatt) / 2

        # (CBO or not CBO, that's the question)
        self.salience = self.disruption * self.proximity * self.type

        # self.pref = min(max(self.pref, 0), 100)  # Cap between 100 and 0
        self.tpreference = self.pref * self.salience

        val_1 = self.pref * self.power * self.salience * 0.9
        if hasattr(self, "cbo_power"):
            val_1 = self.pref * self.cbo_power * self.salience * 0.9

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

    def step(self):
        # Forming coalition will be handled by the model
        #   before this agent.step() is called
        if self.pending_cit_coalition is not None:
            self.update_cit_coalition_attrs()

        if self.pending_sh_coalition is not None:
            self.update_sh_coalition_attrs()

        self.update_post_tick_attribute()
        self.execute_influence_model()
