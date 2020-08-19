"""
Use mesa and mesa-geo to build Insite Software agent based modelling
    based on Techno Social Energy Infrastructure

* mesa: https://mesa.readthedocs.io/en/master/tutorials/adv_tutorial.html
* mesa-geo: https://github.com/Corvince/mesa-geo

'''NOTES'''
# sh: stakeholder
# cit: citizen
"""

# File system
import json
import os
# Stats
import time
# Helper
import pandas as pd
import matplotlib.pyplot as plt
import uuid

from netlogo_model import InsiteModel


def read_JSON(path):
    with open(path) as jsonFile:
        return json.load(jsonFile)


def run_with_timer(func, purpose, log_level=0):
    print(f'---{purpose}---', flush=True)

    start_time = time.time()
    if log_level >= 1:
        print(f"~~~Start time: {start_time}", flush=True)

    result = func(log_level=log_level)

    if log_level >= 1:
        print(f"~~~End time: {time.time()}", flush=True)
        print(f'~~~Time taken: {time.time() - start_time}', flush=True)
        print(f'==={purpose}===\n', flush=True)

    return result


def run_model(tick_path, cit_geojson_path, meta_data_path,
              stakeholder_path, regulator_path,
              total_ticks=26,
              agent_output_path=None, model_output_path=None):
    def drive_model(log_level=0):
        '''RUNNING MODEL'''
        # Load in and run the model
        netlogo_model = InsiteModel(
            cit_pd=agent_list,
            geojson_list=cit_geojson,

            stakeholder_pd=stakeholder_list,
            regulator_pd=regulator_list,

            meta_data=meta_data,

            neighbor_type=0,
            efficiency_parameter=1.25,
            log_level=log_level)
        for _ in range(total_ticks):
            netlogo_model.step()

        return netlogo_model

    # * LOADING DATA
    # Load in the data file then pass it into the model
    """Note:
        im - influence message
        tpreference - salient preference
        pref - preference
        own_pref - ?
    """
    agent_list = pd.read_csv(tick_path)

    # Load geojson files
    cit_geojson = read_JSON(cit_geojson_path)

    # Load other data
    meta_data = read_JSON(meta_data_path)

    # Load stakeholder and regulator
    stakeholder_list = pd.read_csv(stakeholder_path)
    regulator_list = pd.read_csv(regulator_path)
    # Add unique id to each agent
    stakeholder_list['id'] = [uuid.uuid4()
                              for _ in range(len(stakeholder_list))]
    regulator_list['id'] = [uuid.uuid4()
                            for _ in range(len(regulator_list))]

    # * RUN MODEL
    # Drive the model!
    netlogo_model = run_with_timer(
        drive_model, "Running Netlogo model",
        log_level=0)

    # * PLOTTING RESULT
    model_data = netlogo_model.datacollector.get_model_vars_dataframe()
    for axis_key in model_data.columns.values:
        plot = model_data.reset_index().plot.line(x="index", y=axis_key)
        plot.set_xlabel("Tick")

    agent_data = netlogo_model.datacollector.get_agent_vars_dataframe()
    for axis_key in agent_data.columns.values:
        plot = agent_data.reset_index().plot.scatter(x="Step", y=axis_key)
        plot.set_xlabel("Tick")

    print(agent_data.corr())
    agent_data.reset_index().corr().to_csv("./correlation.csv")

    # plt.show()  # comment or uncomment for batch run

    # * OUTPUTING
    if model_output_path is not None:
        append_write = 'w'  # make a new file if not
        if os.path.exists(model_output_path):
            append_write = 'a'  # append if already exists

        # Append new meta column into the file
        model_data.insert(1, "Num Cit", meta_data['actual_num_cit'])
        model_data.insert(1, "Disruption", meta_data['disruption'])
        model_data.insert(1, "Talk span", meta_data['talk_span'])
        model_data.insert(1, "NGO Message", meta_data['NGO_message'])
        # And write it out to file
        with open(model_output_path, append_write) as out_file:
            model_data.to_csv(out_file,
                              header=out_file.tell() == 0,
                              line_terminator='\n')

    if agent_output_path is not None:
        append_write = 'w'  # make a new file if not
        if os.path.exists(agent_output_path):
            append_write = 'a'  # append if already exists

        # Append new meta column into the file
        agent_data.insert(1, "Num Cit", meta_data['actual_num_cit'])
        agent_data.insert(1, "Disruption", meta_data['disruption'])
        agent_data.insert(1, "Talk span", meta_data['talk_span'])
        agent_data.insert(1, "NGO Message", meta_data['NGO_message'])
        # And write it out to file
        with open(agent_output_path, append_write) as out_file:
            agent_data.to_csv(out_file,
                              header=out_file.tell() == 0,
                              line_terminator='\n')


if __name__ == '__main__':
    TICK_PATH = "./data/tick0.csv"
    CIT_GEOJSON_PATH = "./data/compact-cit-shape.json"
    STAKEHOLDER_PATH = "./data/input/stakeholders.csv"
    REGULATOR_PATH = "./data/input/regulators.csv"
    META_DATA_PATH = "data/other-data.json"

    TOTAL_TICKS = 26

    run_model(tick_path=TICK_PATH,
              cit_geojson_path=CIT_GEOJSON_PATH,
              meta_data_path=META_DATA_PATH,
              stakeholder_path=STAKEHOLDER_PATH,
              regulator_path=REGULATOR_PATH
              #   agent_output_path='data/output/agent_data.csv'
              )
