from netlogo_model import NetLogoModel
import pandas as pd

tick_path = "../tick0.csv"

# Load in the data file then pass it into the model
agent_list = pd.read_csv(tick_path,
                         usecols=["who", "xcor", "ycor", "own-pref", "own-power",
                                  "ideo", "idatt", "pref", "tpreference", "power",
                                  "prox", "proximity", "salience", "im", "ran", "own-eu"])
"""Note:
    im - influence message
    tpreference - salience preference
    pref - preference
    own-pref - ?
    own-power - ?
"""

# Migrate name
name_dict = {"who": "id"}
agent_list = agent_list.rename(columns=name_dict)

# Load in and run the model
empty_model = NetLogoModel(agent_list.to_dict(orient='records'))
empty_model.step()
