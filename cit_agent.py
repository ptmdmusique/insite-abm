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

    def step(self):
        # Forming coalition will be handled by the model
        #   before this agent.step() is called

        # Update attributes every step after coalitions are formed
        if self.coalition is not None:
            # If the agent is in a coalition
            self.pref = self.coalition['coal_pref']

            if self.unique_id == self.coalition['id_1']:
                self.utility = self.coalition['utility_1']
            else:
                self.utility = self.coalition['utility_2']

            self.type = 2   # Change type to CBO
            # ? TODO: Check whether we should set cbo_power for agent 2 to 0
            #   like in Netlogo
            self.cbo_power = self.coalition['coal_power']
            self.cbo_pref = self.coalition['coal_pref']

            self.salience = self.disruption * self.proximity * self.type
            self.power *= 1.5
        else:
            # Else just reset the type
            self.type = 1

        ''' utility-info '''
        # Generate a random number in [0, 0.05]
        NGO_message = 1  # TODO: Should be taken from user input
        random_float = np.random.random() * 0.05
        self.idatt = (1 + random_float) * (self.idatt + NGO_message * 0.01)

        ''' Other '''
        self.pref = ((self.proximity * 100) + self.idatt) / 2
        self.tpreference = self.pref * self.salience

        val_1 = self.pref * self.power * self.salience * 0.9
        val_2 = self.ran * 0.1
        self.im = (val_1 + val_2) * 1.2 / (200 * self.efficiency_parameter)
