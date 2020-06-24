from shapely.geometry.base import BaseGeometry
from mesa_geo import GeoAgent


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

    def step(self):
        # Forming coalition will be handled by the model
        #   before this agent.step() is called

        # Update attributes every step after coalitions are formed
        self.tpreference = self.pref * self.salience
