from netlogo_model import NetLogoModel
import pandas as pd
import json

tick_path = "data/tick0.csv"
cit_geojson_path = "data/compact-cit-shape.json"

# Load in the data file then pass it into the model
agent_list = pd.read_csv(tick_path,
                         usecols=["id", "xcor", "ycor", "own-pref",
                                  "ideo", "idatt", "pref", "tpreference", "power",
                                  "prox", "proximity", "salience", "im", "own-eu"])
"""Note:
    im - influence message
    tpreference - salience preference
    pref - preference
    own-pref - ?
"""

# Load geojson files
with open(cit_geojson_path) as jsonFile:
    cit_geojson = json.load(jsonFile)
# Load in and run the model
empty_model = NetLogoModel(agent_list.to_dict(orient='records'), cit_geojson)
empty_model.step()
