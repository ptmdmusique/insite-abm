import matplotlib.pyplot as plt
import pandas as pd
import json

from netlogo_model import NetLogoModel

TICK_PATH = "data/tick0.csv"
CIT_GEOJSON_PATH = "data/compact-cit-shape.json"
TOTAL_TICKS = 26

# Load in the data file then pass it into the model
"""Note:
    im - influence message
    tpreference - salient preference
    pref - preference
    own-pref - ?
"""
agent_list = pd.read_csv(TICK_PATH)

# Load geojson files
with open(CIT_GEOJSON_PATH) as jsonFile:
    cit_geojson = json.load(jsonFile)

# Load in and run the model
netlogo_model = NetLogoModel(agent_list, cit_geojson, verbose=False)
for tick in range(TOTAL_TICKS):
    netlogo_model.step()

model_data = netlogo_model.datacollector.get_model_vars_dataframe()
# model_data.plot(ylim=(1000, 3000))
model_data.plot()
print(model_data)

agent_data = netlogo_model.datacollector.get_agent_vars_dataframe()

plt.show()
