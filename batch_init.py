from initializer import analyze_input
from os import path
import numpy as np

''' Paramers and its corresponding value set/range used for batch running '''

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
    "disruptions": {
        "type": "set",
        # Set of values
        "values": [0.1, 0.2, 0.5, 0.75, 1],
        "default": 1,
    },
    "initial_numbers": {
        "type": "set",
        # Set of values
        "values": [100, 250, 500, 1000],
        "default": 100,
    },
    "acceptable_range": {  # In km
        "type": "range",
        # min, max, step
        "values": [0.05, 10, 0.2],
        "default": 0.5,
    },
}
# Used for running the model
model_batch_params = {
    "talk_spans": {
        "type": "set",
        "values": [1, 5, 10],
    },
    # "needs": {
    #     "type": "range",
    #     # min, max, step
    #     "values": [0, 10, 0.5],
    # },
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


def get_path(key, type="input", file_name_suffix=None):
    # Get the joined path based on key
    folder_path = PATHS['{}-folder'.format(type)]
    file_name = PATHS[key]
    if file_name_suffix is not None:
        file_name += file_name_suffix
    ext = PATHS['{}-ext'.format(key)]
    return path.join(folder_path, '{}.{}'.format(file_name, ext))


def get_default_value(key):
    try:
        return initializer_batch_params[key]['default']
    except KeyError:
        print('Encounting key error')


default_meta_data = {
    # Number of cits we want to extract
    'initial_number': get_default_value('initial_numbers'),
    # Disruption
    'disruption': get_default_value('disruptions'),
    # Acceptable distance for cits (in km)
    'acceptable_range': get_default_value('acceptable_range'),
    # Path to citizen shapefile
    'cit_path': get_path('cit-in'),
    # Path to route shapefile
    'route_path': get_path('route-in'),
    # Path to csv out file
    'out_CSV_path': get_path('tick0', 'output'),
    # Path to cit json out file
    'out_cit_path': get_path('cit-coord', 'output'),
    # Path to chosen cit geojson
    'out_shape_path': get_path('cit-shape', 'output'),
    # Path to chosen cit geojson with minimal info
    'out_compact_path': get_path('cit-compact', 'output'),
    # Path to other information
    'out_meta_path': get_path('meta', 'output'),
}


# Run intializer
counter = 0
for key, value in initializer_batch_params.items():
    # Use the values list to get what to run next
    print(key)
    if value['type'] == 'range':
        start = value['values'][0]
        step = value['values'][2]
        end = value['values'][1] + step
        for cur_batch_value in np.arange(start, end, step):
            # Compute new file name
            get_path('tick0', 'output', '_{}'.format(counter))
            counter += 1

            # Compute new meta data
            meta_data = default_meta_data.copy()
            meta_data[key] = cur_batch_value

            # Run intializer
            analyze_input(meta_data)
    elif value['type'] == 'set':
        for cur_batch_value in value['values']:
            get_path('tick0', 'output', '_{}'.format(counter))
            counter += 1

            # Compute new meta data
            meta_data = default_meta_data.copy()
            meta_data[key] = cur_batch_value

            # Run intializer
            analyze_input(meta_data)
