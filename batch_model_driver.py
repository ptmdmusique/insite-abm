from model_driver import run_model

import os
import os.path
import time
from pathlib import Path

TICK0_FILENAME = "tick0.csv"
CIT_GEOJSON_FILENAME = "compact-cit-shape.json"
META_DATA_FILENAME = "meta-data.json"

STAKEHOLDER_PATH = "data/input/stakeholders.csv"
REGULATOR_PATH = "data/input/regulators.csv"
CIT_OUTPUT_PATH = "data/output/cit_data.csv"
MODEL_OUTPUT_PATH = "data/output/model_data.csv"

STEP0_FOLDER_PATH = "data/step0"
NAME_PREFIX = "batch_"
num_batch = len(os.listdir(STEP0_FOLDER_PATH))


def run_with_timer(func, purpose, log_level=0):
    print(f'---{purpose}---', flush=True)

    start_time = time.time()
    if log_level >= 1:
        print(f"~~~Start time: {start_time}", flush=True)

    result = func()

    if log_level >= 1:
        print(f"~~~End time: {time.time()}", flush=True)
        print(f'~~~Time taken: {time.time() - start_time}', flush=True)
        print(f'==={purpose}===\n', flush=True)

    return result


# Clean up old file
run_with_timer(lambda:
               Path(CIT_OUTPUT_PATH).unlink(missing_ok=True),
               "Cleaning old agent file")
run_with_timer(lambda:
               Path(MODEL_OUTPUT_PATH).unlink(missing_ok=True),
               "Cleaning old model file")


# Then batch run
def run_batch():
    for cur_batch in range(num_batch):
        print(f"\tBatch {cur_batch}/{num_batch}=======", flush=True)

        cur_folder_path = os.path.join(
            STEP0_FOLDER_PATH, NAME_PREFIX + str(cur_batch))

        tick0_path = os.path.join(cur_folder_path, TICK0_FILENAME)
        cit_geojson_path = os.path.join(cur_folder_path, CIT_GEOJSON_FILENAME)
        meta_data_path = os.path.join(cur_folder_path, META_DATA_FILENAME)

        run_model(tick_path=tick0_path,
                  cit_geojson_path=cit_geojson_path,
                  meta_data_path=meta_data_path,
                  stakeholder_path=STAKEHOLDER_PATH,
                  regulator_path=REGULATOR_PATH,
                  cit_output_path=CIT_OUTPUT_PATH,
                  model_output_path=MODEL_OUTPUT_PATH)


run_with_timer(run_batch, "RUNNING ALL BATCH")
