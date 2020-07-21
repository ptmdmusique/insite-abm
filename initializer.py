import sys  # System args
import json
import numpy as np  # Number and Math handler
import math
from random import sample

from shapely.geometry import MultiLineString, LineString, Point  # Geometry
from shapely.ops import nearest_points  # Geometric Operations

import pandas as pd  # Dataframe

# Process:
#   Read in cit geojson file
#     Extract cit data and corresponding area shape
#   Read in route geojson file
#     Extract corresponding coordinates
#   Find shortest distance from each qualified citizen to route
#   Create proper dataframe for tick 0
#   After calculating the proximity for each cits,
#     we filter out only the cits within the acceptable range
#     then randomly choose <initialNumber> amount of cits among those
#   If there is not enough, we expand the range and choose more
#   Dump data to output

# NOTE: As of 1/21/2020, the CRS we are using is WGS84 - crs 4326
# NOTE: For ESRI its almost always going to be:
#       Lat = Y Long = X
# https://gis.stackexchange.com/questions/11626/does-y-mean-latitude-and-x-mean-longitude-in-every-gis-software/11628


def get_coord_distance(origin, destination):
    # Source: https://gist.github.com/rochacbruno/2883505
    lon1, lat1 = origin
    lon2, lat2 = destination
    radius = 6371  # Earth radius in km

    dlat = math.radians(lat2-lat1)
    dlon = math.radians(lon2-lon1)
    a = math.sin(dlat/2) * math.sin(dlat/2) + math.cos(math.radians(lat1)) \
        * math.cos(math.radians(lat2)) * math.sin(dlon/2) * math.sin(dlon/2)
    c = 2 * math.atan2(math.sqrt(a), math.sqrt(1-a))
    d = radius * c

    return d


def analyze_input(meta_data):
    # Column info
    input_columns = [
        'id', 'blarea', 'bname', 'lattitude', 'longitude', 'ppower',
        'density', 'census tract'
    ]
    output_columns = [
        'id', 'xcor', 'ycor',
        'own-pref', 'ideo', 'idatt', 'pref', 'tpreference',
        'power', 'closest_distance', 'proximity', 'salience', 'im', 'ran',
        'utility'
    ]

    # Keys
    key_list = [
        ["ALAND"],
        ["GEOID"],
        ["INT", "LAT"],
        # Since there is a mispell in 2010, this looks really weird...
        ["INT", "ON"],
        ["POWER"],
        ["DENSITY"],
        ["TRACTCE"]
    ]

    # Initial parameters
    # Number of cits we want to extract
    initial_number = meta_data['initial_number']
    # Disruption
    disruption = meta_data['disruption']
    # Acceptable distance for cits (in km)
    acceptable_range = meta_data['acceptable_range']
    # Misc data
    talk_span = meta_data['talk_span']
    NGO_message = meta_data['NGO_message']

    # Paths
    # Path to citizen shapefile
    cit_path = meta_data['cit_path']
    # Path to route shapefile
    route_path = meta_data['route_path']
    # Path to csv out file
    out_CSV_path = meta_data['out_CSV_path']
    # Path to cit json out file
    out_cit_path = meta_data['out_cit_path']
    # Path to chosen cit geojson
    out_shape_path = meta_data['out_shape_path']
    # Path to chosen cit geojson with minimal info
    out_compact_path = meta_data['out_compact_path']
    # Path to other information
    out_meta_path = meta_data['out_meta_path']

    # Read in cit geojson file
    cit_properties = []      # Storing all the properties
    cit_shape_JSON = None       # Storing shape in JSON format
    cit_shapes = []         # Storing shape information
    geo_IDs = []            # List of all geo IDs

    with open(cit_path) as json_file:
        cit_shape_JSON = json.load(json_file)
        cit_features = cit_shape_JSON['features']

        # Extract crucial information from the shape file
        for feature in cit_features:
            cit_ID = feature['id']

            cit_shapes.append({
                "type": "Feature",
                "id": int(cit_ID),
                "geometry": feature['geometry']
            })

            cit_property = {'id': cit_ID}
            # for key in ['ALAND10_NO',
            #             'GEOID10_NO',
            #             'INTPLAT10_',
            #             'INTPLTON10',
            #             'POWER_NO',
            #             'DENSITY_SQ',
            #             'TRACTCE10_']:
            #     citProperty[key] = feature['properties'][key]
            for key in key_list:
                for propKey in feature['properties'].keys():
                    if propKey.startswith(key[0]):
                        if (len(key) > 1 and key[1] in propKey) \
                                or len(key) == 1:
                            if key[0] == "GEOID":
                                if "NO" not in propKey:
                                    cit_property[propKey] = feature['properties'][propKey]
                                    geo_IDs.append(
                                        feature['properties'][propKey])
                            else:
                                cit_property[propKey] = feature['properties'][propKey]

            cit_properties.append(cit_property)

    # Initialize input dataframe
    dfr = pd.DataFrame(data=[row.values()
                             for row in cit_properties],
                       columns=input_columns)
    dfr = dfr.apply(pd.to_numeric)  # Transform them into number
    # Get the list of coordinates and transform into points
    cit_point_list = [Point(i)
                      for i in list(zip(dfr.longitude, dfr.lattitude))]
    ids = [id for id in dfr.id]

    # Also change the features list to just the chosen cits
    cit_shape_JSON['features'] = cit_shapes

    # Read in route geojson file
    routes = []
    with open(route_path) as json_file:
        json_data = json.load(json_file)
        route_features = json_data['features']

        for feature in route_features:
            # Extract geometry of the route and create Shapely object out of it
            route_geometry = feature['geometry']

            # 2 cases: Linestring, MultiLineString
            geometry_type = route_geometry['type']
            if (geometry_type == "LineString"):
                routes.append(LineString(route_geometry['coordinates']))
            elif (geometry_type == "MultiLineString"):
                routes.append(MultiLineString(
                    route_geometry['coordinates']))

    # Find the shortest distance and nearest point for each cit to power line
    closest_distances = []  # List of shortest distance
    np_list = []            # List of nearest points
    for point in cit_point_list:
        cur_min_dist = float('Inf')
        nearest_line = None

        for line in routes:
            curDist = point.distance(line)
            if curDist < cur_min_dist:
                cur_min_dist = curDist
                nearest_line = line
        nearest_point = nearest_points(nearest_line, point)[
            0].coords[:][0]  # Find the nearest point

        # Calculate the distance in real world km instead of Cartesian distance
        closest_distances.append(get_coord_distance(
            nearest_point, (point.x, point.y)))  # Store the shortest distance
        # closestDistanceList.append(curMinDist)  # Store the shortest distance
        # Long first then Lat
        np_list.append((nearest_point[0], nearest_point[1]))

    # Initialize dataframe for output (tick 0)
    df_out = pd.DataFrame(data=0,
                          #  index=np.arange(initialNumber),
                          index=np.arange(len(cit_properties)),
                          columns=output_columns)

    # Formula can be found at : http://jasss.soc.surrey.ac.uk/16/3/6.html
    # Append more columns into the dfOut dataframe
    # NOTE: xcor is longitude, ycor is lattitude
    df_out[['id', 'xcor', 'ycor']] = dfr[[
        'id', 'longitude', 'lattitude']].values
    df_out['id'] = df_out['id'].astype(int)
    # dfOut['ideo'] = np.random.normal(60, 10, initialNumber)
    df_out['ideo'] = np.random.normal(60, 10, len(cit_properties))

    # Generate random column
    df_out['ran'] = df_out.apply(lambda row: np.random.randint(100), axis=1)

    # Calculate idatt (3.5)
    df_out['idatt'] = df_out.apply(
        lambda row: row['ideo'] * 0.9 + row['ran'] * 0.1, axis=1)

    # Append the proximity column with the nearest point result array
    df_out['closest_distance'] = closest_distances
    df_out['proximity'] = df_out.apply(
        lambda row: 1 / row['closest_distance'], axis=1)
    # TODO: DOUBLE CHECK THIS
    df_out['pref'] = df_out.apply(lambda row: (
        (disruption * row['proximity'] * 100) + row['idatt']) / 2, axis=1)
    # df_out['pref'] = df_out.apply(lambda row: (
    # (row['proximity'] * 100) + row['idatt']) / 2, axis=1)

    # Power
    dfr['power'] = dfr.apply(lambda row: 2 * row['ppower'], axis=1)
    df_out['own-power'] = df_out['power'] = dfr['power'].values

    df_out['own-pref'] = df_out.apply(lambda row: (
        (row['proximity'] * 100) + row['idatt']) / 2, axis=1)
    df_out['utility'] = df_out.apply(
        lambda row: 100 * row['own-power'], axis=1)

    df_out['salience'] = df_out.apply(
        lambda row: disruption * row['proximity'], axis=1)

    df_out['im'] = df_out.apply(lambda row:
                                (row['pref'] * row['power'] *
                                 row['salience'] * 0.9 + row['ran'] * 0.1)
                                * 1.2 / (200 * 1.5), axis=1)

    df_out['tpreference'] = df_out.apply(
        lambda row: row['pref'] * row['salience'], axis=1)

    # Then we filter out only the cit within acceptable range
    df_out = df_out.sort_values(by=['closest_distance'])
    df_out = df_out.reset_index(drop=True)  # Reset the index
    # Convert acceptable range distance from km to degree: 111km ~ 1 degree
    #  https://www.longitudestore.com/how-big-is-one-gps-degree.html
    acceptable_range /= 111
    # Get the index of the first cit that is out of range
    # (we don't just filter because the <initialNumber> might be bigger
    #   than the size of the list of filtered cits)
    max_indx = df_out[['proximity']].gt(acceptable_range).idxmax().tolist()[0]

    # 3 cases:
    '''
        1. not found (maxIndx = 0) or maxIndex is the first row (= 0, same meaning as not found)
        => choose randomly between 0 and len(dfOut) <initialNumber> number of citizen
        2. found and:
            a. maxIndx <= initialNumber:
            => choose first maxIndx rows, ignore the rest
            b. maxIndx > initialNumber:
            => choose randomly among the first maxIndx rows
    '''

    # Randomly generate a list of number that will
    #   act as the index for the data to take from input
    random_indices = ()
    if max_indx == 0:
        random_indices = np.array(sample(range(len(df_out)), initial_number))
    else:
        if max_indx <= initial_number:
            random_indices = list(range(max_indx))
        else:
            random_indices = np.array(sample(range(max_indx), initial_number))
    # Get only the elements with chosen indices
    df_out = df_out.iloc[random_indices]

    # Filter the shapeList and keep only those we have in the idList
    compact_cit_shape = {shape['id']: shape['geometry']
                         for shape in cit_shapes if shape['id'] in df_out['id'].values}

    # Output to file
    # Tick for further processing
    with open(out_CSV_path, 'w+') as json_file:
        json_file.write("\n\n\n\n\n\n\n\n\n\n\n\n")
        df_out.to_csv(json_file, index=False, line_terminator='\n')
        json_file.close()

    with open(out_cit_path, 'w') as json_file:
        writeList = []
        for index in range(0, len(cit_point_list)):
            curPoint = cit_point_list[index].coords[:][0]
            writeList.append({
                # Lng first then lat
                "geoid": geo_IDs[index],
                "id": ids[index],
                "myCoord": [curPoint[0], curPoint[1]],
                "npCoord": np_list[index]
            })
        # Write to file
        json.dump(writeList, json_file)

    # For heatmap
    with open(out_shape_path, 'w') as json_file:
        json.dump(cit_shape_JSON, json_file)

    # For heatmap
    with open(out_compact_path, 'w') as json_file:
        json.dump(compact_cit_shape, json_file)

    # For other information
    with open(out_meta_path, 'w') as json_file:
        json.dump({
            "actual_num_cit": len(random_indices),
            "disruption": disruption,
            "talk_span": talk_span,
            "NGO_message": NGO_message,
        }, json_file)


if __name__ == '__main__':
    # Initial parameters
    meta_data = {
        # Number of cits we want to extract
        'initial_number': int(sys.argv[1]),
        # Disruption
        'disruption': float(sys.argv[2]),
        # Acceptable distance for cits (in km)
        'acceptable_range': float(sys.argv[3]),
        # Other parameters
        'talk_span': float(sys.argv[4]),
        'NGO_message': float(sys.argv[5]),

        # Paths
        # Path to citizen shapefile
        'cit_path': sys.argv[6],
        # Path to route shapefile
        'route_path': sys.argv[7],
        # Path to csv out file
        'out_CSV_path': sys.argv[8],
        # Path to cit json out file
        'out_cit_path': sys.argv[9],
        # Path to chosen cit geojson
        'out_shape_path': sys.argv[10],
        # Path to chosen cit geojson with minimal info
        'out_compact_path': sys.argv[11],
        # Path to other information
        'out_meta_path': sys.argv[12],
    }

    analyze_input(meta_data)
