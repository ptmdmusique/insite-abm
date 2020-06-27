import sys  # System args
import json
import numpy as np  # Number and Math handler
import math
from random import sample

from shapely.geometry import MultiLineString, LineString, Point  # Geometry
from shapely.ops import nearest_points  # Geometric Operations

import pandas as pd  # Dataframe

# Initial parameters
initialNumber = int(sys.argv[1])
disruption = float(sys.argv[2])  # Disruption
# Acceptable distance for cits (in km)
acceptableRange = float(sys.argv[3])
# Path
citPath = sys.argv[4]           # Path to citizen shapefile
routePath = sys.argv[5]         # Path to route shapefile
outCSVPath = sys.argv[6]        # Path to csv out file
outCitPath = sys.argv[7]        # Path to cit json out file
outShapePath = sys.argv[8]      # Path to chosen cit geojson
outCompactShapePath = sys.argv[9]      # Path to chosen cit geojson
outOtherPath = sys.argv[9]      # Path to other information
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


def getDistanceBetweenCoords(origin, destination):
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


# Column info
inputColumnHeaders = [
    'id', 'blarea', 'bname', 'lattitude', 'longitude', 'ppower',
    'density', 'census tract'
]
outputColumnHeaders = [
    'id', 'xcor', 'ycor',
    'own-pref', 'ideo', 'idatt', 'pref', 'tpreference',
    'power', 'closest_distance', 'proximity', 'salience', 'im', 'ran',
    'utility'
]

# Keys
keyList = [
    ["ALAND"],
    ["GEOID"],
    ["INT", "LAT"],
    # Since there is a mispell in 2010, this looks really weird...
    ["INT", "ON"],
    ["POWER"],
    ["DENSITY"],
    ["TRACTCE"]
]
# Read in cit geojson file
citPropertyList = []
citShapeList = []
citShapeJSON = ""
geoIDList = []

with open(citPath) as jsonFile:
    citShapeJSON = json.load(jsonFile)
    citFeatures = citShapeJSON['features']

    for feature in citFeatures:
        citID = feature['id']

        citShapeList.append({
            "type": "Feature",
            "id": int(citID),
            "geometry": feature['geometry']
        })

        citProperty = {'id': citID}
        # for key in ['ALAND10_NO',
        #             'GEOID10_NO',
        #             'INTPLAT10_',
        #             'INTPLTON10',
        #             'POWER_NO',
        #             'DENSITY_SQ',
        #             'TRACTCE10_']:
        #     citProperty[key] = feature['properties'][key]
        for key in keyList:
            for propKey in feature['properties'].keys():
                if propKey.startswith(key[0]):
                    if (len(key) > 1 and key[1] in propKey) \
                            or len(key) == 1:
                        if key[0] == "GEOID":
                            if "NO" not in propKey:
                                citProperty[propKey] = feature['properties'][propKey]
                                geoIDList.append(
                                    feature['properties'][propKey])
                        else:
                            citProperty[propKey] = feature['properties'][propKey]

        citPropertyList.append(citProperty)

# Initialize dataframe for input
dfr = pd.DataFrame(data=[row.values()
                         for row in citPropertyList],
                   columns=inputColumnHeaders)
dfr = dfr.apply(pd.to_numeric)  # Transform them into number
# Get the list of coordinates and transform into points
citPointList = [Point(i) for i in list(zip(dfr.longitude, dfr.lattitude))]
idList = [id for id in dfr.id]

# Also change the features list to just the chosen cits
citShapeJSON['features'] = citShapeList

# Read in route geojson file
routeList = []
with open(routePath) as jsonFile:
    jsonData = json.load(jsonFile)
    routeFeatures = jsonData['features']

    for feature in routeFeatures:
        routeGeometry = feature['geometry']

        # 2 cases: Linestring, MultiLineString
        geometryType = routeGeometry['type']
        if (geometryType == "LineString"):
            routeList.append(LineString(routeGeometry['coordinates']))
        elif (geometryType == "MultiLineString"):
            routeList.append(MultiLineString(routeGeometry['coordinates']))


# Find the shortest distance and nearest point for each cit to power line
closestDistanceList = []  # List of shortest distance
npList = []  # List of nearest points
for point in citPointList:
    curMinDist = float('Inf')
    nearestLine = None
    for line in routeList:
        curDist = point.distance(line)
        if curDist < curMinDist:
            curMinDist = curDist
            nearestLine = line
    nearestPoint = nearest_points(nearestLine, point)[
        0].coords[:][0]  # Find the nearest point
    # Calculate the distance in real world km instead of Cartesian distance
    closestDistanceList.append(getDistanceBetweenCoords(
        nearestPoint, (point.x, point.y)))  # Store the shortest distance
    # closestDistanceList.append(curMinDist)  # Store the shortest distance
    npList.append((nearestPoint[0], nearestPoint[1]))  # Long first then Lat


# Initialize dataframe for output (tick 0)
dfOut = pd.DataFrame(data=0,
                     #  index=np.arange(initialNumber),
                     index=np.arange(len(citPropertyList)),
                     columns=outputColumnHeaders)

# Formula can be found at : http://jasss.soc.surrey.ac.uk/16/3/6.html
# Append more columns into the dfOut dataframe
# NOTE: xcor is longitude, ycor is lattitude
dfOut[['id', 'xcor', 'ycor']] = dfr[['id', 'longitude', 'lattitude']].values
dfOut['id'] = dfOut['id'].astype(int)
# dfOut['ideo'] = np.random.normal(60, 10, initialNumber)
dfOut['ideo'] = np.random.normal(60, 10, len(citPropertyList))

# Generate random column
dfOut['ran'] = dfOut.apply(lambda row: np.random.randint(100), axis=1)

# Calculate idatt (3.5)
dfOut['idatt'] = dfOut.apply(
    lambda row: row['ideo'] * 0.9 + row['ran'] * 0.1, axis=1)

# Append the proximity column with the nearest point result array
dfOut['closest_distance'] = closestDistanceList
dfOut['proximity'] = dfOut.apply(
    lambda row: 1 / row['closest_distance'], axis=1)
dfOut['pref'] = dfOut.apply(lambda row: (
    (disruption * row['proximity'] * 100) + row['idatt']) / 2, axis=1)

# Power
dfr['power'] = dfr.apply(lambda row: 2 * row['ppower'], axis=1)
dfOut['own-power'] = dfOut['power'] = dfr['power'].values

dfOut['own-pref'] = dfOut.apply(lambda row: (
    (row['proximity'] * 100) + row['idatt']) / 2, axis=1)
dfOut['utility'] = dfOut.apply(lambda row: (
    100 - abs(row['own-pref'] - row['own-pref'])) * row['own-power'], axis=1)

dfOut['salience'] = dfOut.apply(
    lambda row: disruption * row['proximity'], axis=1)

dfOut['im'] = dfOut.apply(lambda row:
                          (row['pref'] * row['power'] *
                           row['salience'] * 0.9 + row['ran'] * 0.1)
                          * 1.2 / (200 * 1.5), axis=1)

dfOut['tpreference'] = dfOut.apply(
    lambda row: row['pref'] * row['salience'], axis=1)


# Then we filter out only the cit within acceptable range
dfOut = dfOut.sort_values(by=['closest_distance'])
dfOut = dfOut.reset_index(drop=True)  # Reset the index
# Convert acceptable range distance from km to degree: 111km ~ 1 degree
#  https://www.longitudestore.com/how-big-is-one-gps-degree.html
acceptableRange /= 111
# Get the index of the first cit that is out of range
# (we don't just filter because the <initialNumber> might be bigger
#   than the size of the list of filtered cits)
maxIndx = dfOut[['proximity']].gt(acceptableRange).idxmax().tolist()[0]

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
rIndexList = ()
if maxIndx == 0:
    rIndexList = np.array(sample(range(len(dfOut)), initialNumber))
else:
    if maxIndx <= initialNumber:
        rIndexList = list(range(maxIndx))
    else:
        rIndexList = np.array(sample(range(maxIndx), initialNumber))
# Get only the elements with chosen indices
dfOut = dfOut.iloc[rIndexList]

# Filter the shapeList and keep only those we have in the idList
compactCitShape = {shape['id']: shape['geometry']
                   for shape in citShapeList if shape['id'] in dfOut['id'].values}

# Output to file
# Tick for further processing
with open(outCSVPath, 'w+') as outFile:
    outFile.write("\n\n\n\n\n\n\n\n\n\n\n\n")
    dfOut.to_csv(outFile, index=False)
    outFile.close()


with open(outCitPath, 'w') as jsonFile:
    writeList = []
    for index in range(0, len(citPointList)):
        curPoint = citPointList[index].coords[:][0]
        writeList.append({
            # Lng first then lat
            "geoid": geoIDList[index],
            "id": idList[index],
            "myCoord": [curPoint[0], curPoint[1]],
            "npCoord": npList[index]
        })
    # Write to file
    json.dump(writeList, jsonFile)

# For heatmap
with open(outShapePath, 'w') as jsonFile:
    json.dump(citShapeJSON, jsonFile)

# For heatmap
with open(outCompactShapePath, 'w') as jsonFile:
    json.dump(compactCitShape, jsonFile)

# For other information
with open(outOtherPath, 'w') as jsonFile:
    json.dump({
        "actualNumCit": len(rIndexList),
        "disruption": disruption
    }, jsonFile)
