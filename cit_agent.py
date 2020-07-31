from shapely.geometry.base import BaseGeometry
from mesa_geo import GeoAgent
import numpy as np


class CitAgent(GeoAgent):
    """A citizen in our model."""

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

        self.type = 1   # 1: regular citizen ; 2: CBO
        self.cbo_pref = 0
        self.base_power = self.power
        self.new_coalition = None

    def update_coalition_attributes(self):
        # Update attributes every step after coalitions are formed
        # If the agent is in a coalition
        # self.pref = self.coalition['coal_pref']
        setattr(self, "own-pref", self.new_coalition['coal_pref'])

        if self.unique_id == self.new_coalition['id_1']:
            self.utility = self.new_coalition['utility_1']
            self.cbo_utility = self.new_coalition['utility_1']
            self.cbo_power = self.new_coalition['coal_power']
        else:
            self.utility = self.new_coalition['utility_2']
            self.cbo_utility = self.new_coalition['utility_2']
            self.cbo_power = 0

        self.type = 2   # Change type to CBO
        self.cbo_pref = self.new_coalition['coal_pref']
        self.power *= self.efficiency_parameter
        self.new_coalition = None   # Clean up

    def update_post_tick_attribute(self):
        ''' utility-info '''
        # Generate a random number in [0, 0.05]
        random_float = np.random.random() * 0.05
        self.idatt = (1 + random_float) * \
            (self.idatt + self.NGO_message * 0.01)
        self.idatt = min(max(self.idatt, 0), 100)  # Cap between 100 and 0

        ''' Label up '''
        # Update the salience based on cits' type
        self.pref = ((self.proximity * 100) + self.idatt) / 2

        # (CBO or not CBO, that's the question)
        self.salience = self.disruption * self.proximity * self.type

        self.pref = min(max(self.pref, 0), 100)  # Cap between 100 and 0
        self.tpreference = self.pref * self.salience

        val_1 = self.pref * self.power * self.salience * 0.9
        val_2 = self.ran * 0.1
        self.im = (val_1 + val_2) * 1.2
        # / (200 * self.efficiency_parameter)

    def step(self):
        # Forming coalition will be handled by the model
        #   before this agent.step() is called

        if self.new_coalition is not None:
            self.update_coalition_attributes()
        self.update_post_tick_attribute()
