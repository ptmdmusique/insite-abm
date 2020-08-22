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
from os.path import join
# Stats
import time
# Helper
import pandas as pd
import matplotlib.pyplot as plt
import uuid

from insite_model import InsiteModel


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
              cit_output_path=None,
              sh_output_path=None,
              regulator_output_path=None,
              model_output_path=None):
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

    def draw_scatter_plot(data_set, x_label):
        for axis_key in data_set.columns.values:
            plot = data_set.reset_index().plot.scatter(x=x_label, y=axis_key)
            plot.set_xlabel("Tick")

    def draw_line_plot(data_set, x_label):
        for axis_key in data_set.columns.values:
            plot = data_set.reset_index().plot.line(x=x_label, y=axis_key)
            plot.set_xlabel("Tick")

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
    insite_model = run_with_timer(
        drive_model, "Running Netlogo model",
        log_level=0)

    # * COLLECTING DATA
    cit_model_data = insite_model.datacollector.get_model_vars_dataframe()
    cit_data = insite_model.datacollector.get_agent_vars_dataframe()
    sh_model_data = insite_model.sh_collector.get_model_vars_dataframe()
    sh_data = insite_model.sh_collector.get_agent_vars_dataframe()
    regulator_model_data = insite_model.regulator_collector.get_model_vars_dataframe()
    regulator_data = insite_model.regulator_collector.get_agent_vars_dataframe()

    cit_data.reset_index().corr().to_csv("./correlation.csv")

    # * PLOTTING RESULT
    # comment or uncomment for batch run
    # draw_line_plot(cit_model_data, "index")
    # draw_scatter_plot(cit_data, "Step")
    # draw_line_plot(sh_model_data, "index")
    # draw_scatter_plot(sh_data, "Step")
    # draw_line_plot(regulator_model_data, "index")
    # draw_scatter_plot(regulator_data, "Step")
    # print(cit_data.corr())
    # plt.show()

    # * OUTPUTING
    if model_output_path is not None:
        append_write = 'w'  # make a new file if not
        if os.path.exists(model_output_path):
            append_write = 'a'  # append if already exists

        # Append new meta column into the file
        cit_model_data.insert(1, "Num Cit", meta_data['actual_num_cit'])
        cit_model_data.insert(1, "Disruption", meta_data['disruption'])
        cit_model_data.insert(1, "Talk span", meta_data['talk_span'])
        cit_model_data.insert(1, "NGO Message", meta_data['NGO_message'])
        # And write it out to file
        with open(model_output_path, append_write) as out_file:
            dfs = [cit_model_data, sh_model_data, regulator_model_data]
            final_df = pd.concat([df.stack() for df in dfs], axis=0).unstack()
            final_df.to_csv(out_file,
                            header=out_file.tell() == 0,
                            line_terminator='\n')

    if cit_output_path is not None:
        append_write = 'w'  # make a new file if not
        if os.path.exists(cit_output_path):
            append_write = 'a'  # append if already exists

        # Append new meta column into the file
        cit_data.insert(1, "Num Cit", meta_data['actual_num_cit'])
        cit_data.insert(1, "Disruption", meta_data['disruption'])
        cit_data.insert(1, "Talk span", meta_data['talk_span'])
        cit_data.insert(1, "NGO Message", meta_data['NGO_message'])
        # And write it out to file
        with open(cit_output_path, append_write) as out_file:
            cit_data.to_csv(out_file,
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
