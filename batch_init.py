# File system
from os import path
from pathlib import Path
import shutil
# Stats
import time
# Helper
import numpy as np

from initializer import analyze_input


def run_with_timer(func, purpose, verbose=True):
    print(f'---{purpose}---', flush=True)

    start_time = time.time()
    if verbose:
        print(f"~~~Start time: {start_time}", flush=True)

    func()

    if verbose:
        print(f"~~~End time: {time.time()}", flush=True)
        print(f'~~~Time taken: {time.time() - start_time}', flush=True)
        print(f'==={purpose}===\n', flush=True)


''' Paramers and its corresponding value set/range used for batch running '''
"""
    ${output_path}/batch_0 will be the folder to store the default run
    ${output_path}/batch_${counter} will store the other batches in order
"""

# Information about file system paths
PATHS = {
    "input-folder": "data/input",
    "output-folder": "data/step0",

    # Ticks
    "tick0": "tick0",
    "tick0-ext": "csv",

    # Shapefiles
    "cit-in": "cits",
    "cit-in-ext": "geojson",
    "route-in": "TRTP_Route_Project",
    "route-in-ext": "geojson",

    "cit-coord": "cit-coord",
    "cit-coord-ext": "json",

    "cit-coord": "cit-coord",
    "cit-ext": "json",

    "cit-shape": "cit-shape",
    "cit-shape-ext": "json",

    "cit-compact": "compact-cit-shape",
    "cit-compact-ext": "json",

    "meta": "meta-data",
    "meta-ext": "json",
}

# Used for initialization process
initializer_batch_params = {
    "disruption": {
        "type": "set",
        # Set of values
        "values": [0.1, 0.2, 0.5, 0.75, 1],
        "default": 1,
    },
    # Number of cits we want to extract
    "initial_number": {
        "type": "set",
        # Set of values
        "values": [100, 250, 500, 1000],
        "default": 100,
    },
    # Acceptable distance for cits (in km)
    "acceptable_range": {  # In km
        "type": "range",
        # min, max, step
        "values": [0.05, 10, 0.2],
        "default": 0.5,
    },
    "talk_span": {
        "type": "set",
        "values": [1, 5, 10],
        "default": 5,
    },
    "NGO_message": {
        "type": "set",
        "values": [1, 5, 10],
        "default": 4.5,
    },
}
# Used for running the model    # TODO: Remove this
model_batch_params = {
    "talk_spans": {
        "type": "set",
        "values": [1, 5, 10],
    },
    "needs": {
        "type": "range",
        # min, max, step
        "values": [0, 10, 0.5],
    },
    "utility_message": {
        "type": "set",
        "values": [1, 5, 10]
    },
    "NGO_message": {
        "type": "set",
        "values": [1, 5, 10]
    },
    "influence_threshold": {
        "type": "set",
        "values": [0, 1.0, 0.05]
    },
}


def get_path(key, file_name_suffix=None, folder_path=None, type="ouptut"):
    # Get the joined path based on key
    file_name = PATHS[key]
    if folder_path is None:
        folder_path = PATHS[f'{type}-folder']
    if file_name_suffix is not None:
        file_name += f'_{file_name_suffix}'
    ext = PATHS[f'{key}-ext']
    return path.join(folder_path, f'{file_name}.{ext}')


def get_default_value(key):
    try:
        return initializer_batch_params[key]['default']
    except KeyError:
        print('Encounting key error', flush=True)


# Construct the default values
default_meta_values = {}
for key, value in initializer_batch_params.items():
    default_meta_values[key] = get_default_value(key)


def run_initializer(key=None, counter=0, cur_batch_value=None, using_default=False):
    # Run initializer with the current meta data
    print(f'\twith value={cur_batch_value} counter={counter}', flush=True)
    # Create new folder
    out_folder = path.join(
        PATHS['output-folder'], f"batch_{counter}")
    Path(out_folder).mkdir(parents=True, exist_ok=True)

    # Compute new file name and location
    meta_paths = {
        # Path to citizen shapefile
        'cit_path': get_path('cit-in', type='input'),
        # Path to route shapefile
        'route_path': get_path('route-in', type='input'),
        # Path to csv out file
        'out_CSV_path': get_path('tick0', folder_path=out_folder),
        # Path to cit json out file
        'out_cit_path': get_path('cit-coord', folder_path=out_folder),
        # Path to chosen cit geojson
        'out_shape_path': get_path('cit-shape', folder_path=out_folder),
        # Path to chosen cit geojson with minimal info
        'out_compact_path': get_path('cit-compact', folder_path=out_folder),
        # Path to other information
        'out_meta_path': get_path('meta', folder_path=out_folder),
    }

    # Compute new meta data
    meta_data = default_meta_values.copy()
    if using_default is False:
        meta_data[key] = cur_batch_value
    meta_data.update(meta_paths)

    # Run intializer
    analyze_input(meta_data)


# Clean up old folder
def clean_old_folder():
    output_folder = PATHS['output-folder']
    if path.isdir(output_folder):
        shutil.rmtree(output_folder)


run_with_timer(clean_old_folder, 'Cleaning old folder')


# Run intializer
def run_batch():
    # Creating value batch with default values first
    print(f"Running default batch", flush=True)
    run_initializer()

    counter = 1
    for key, value in initializer_batch_params.items():
        print(f"Running batch for {key}", flush=True)
        # Use the values list to get what to run next
        if value['type'] == 'range':
            start = value['values'][0]
            step = value['values'][2]
            end = value['values'][1] + step
            for cur_batch_value in np.arange(start, end, step):
                run_initializer(key, counter, cur_batch_value)
                counter += 1

        elif value['type'] == 'set':
            for cur_batch_value in value['values']:
                run_initializer(key, counter, cur_batch_value)
                counter += 1


run_with_timer(run_batch, 'Initializing batches')
