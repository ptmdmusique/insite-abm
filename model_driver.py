"""
Use mesa and mesa-geo to build Insite Software agent based modelling
    based on Techno Social Energy Infrastructure

* mesa: https://mesa.readthedocs.io/en/master/tutorials/adv_tutorial.html
* mesa-geo: https://github.com/Corvince/mesa-geo
"""

import matplotlib.pyplot as plt
import pandas as pd
import json

from netlogo_model import NetLogoModel


def read_JSON(path):
    with open(path) as jsonFile:
        return json.load(jsonFile)


TICK_PATH = "data/tick0.csv"
CIT_GEOJSON_PATH = "data/compact-cit-shape.json"
OTHER_DATA_PATH = "data/other-data.json"
TOTAL_TICKS = 26

'''LOADING DATA'''
# Load in the data file then pass it into the model
"""Note:
    im - influence message
    tpreference - salient preference
    pref - preference
    own-pref - ?
"""
agent_list = pd.read_csv(TICK_PATH)

# Load geojson files
cit_geojson = read_JSON(CIT_GEOJSON_PATH)

# Load other data
other_data = read_JSON(OTHER_DATA_PATH)


'''RUNNING MODEL'''
# Load in and run the model
netlogo_model = NetLogoModel(
    agent_list, cit_geojson, other_data, verbose=False)
for tick in range(TOTAL_TICKS):
    netlogo_model.step()

'''PLOTTING RESULT'''
model_data = netlogo_model.datacollector.get_model_vars_dataframe()
for axis_key in model_data.columns.values:
    plot = model_data.reset_index().plot.line(x="index", y=axis_key)
    plot.set_xlabel("Tick")

agent_data = netlogo_model.datacollector.get_agent_vars_dataframe()
for axis_key in agent_data.columns.values:
    plot = agent_data.reset_index().plot.scatter(x="Step", y=axis_key)
    plot.set_xlabel("Tick")

print(agent_data.corr())

plt.show()
