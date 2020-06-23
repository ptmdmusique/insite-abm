from netlogo_model import NetLogoModel
import pandas as pd
import json

tick_path = "data/tick0.csv"
cit_geojson_path = "data/compact-cit-shape.json"

# Load in the data file then pass it into the model
"""Note:
    im - influence message
    tpreference - salience preference
    pref - preference
    own-pref - ?
"""
agent_list = pd.read_csv(tick_path)

# Load geojson files
with open(cit_geojson_path) as jsonFile:
    cit_geojson = json.load(jsonFile)

# Load in and run the model
netlogo_model = NetLogoModel(agent_list, cit_geojson, verbose=True)
for tick in range(26):
    netlogo_model.step(tick)
