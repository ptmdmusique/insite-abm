from mesa import Agent


class CitAgent(Agent):
    """An agent with fixed initial wealth."""

    def __init__(self, model, attr_list):
        super().__init__(attr_list['id'], model)
        # Delete the id key:value since we already used it
        if attr_list.pop('id', None) is None:
            print("ERROR --- agent with unknown id found {}".format(attr_list))

        # Take in the attribute list and turn it
        #   into class attributes
        for k, v in attr_list.items():
            setattr(self, k, v)

    # def step(self):
