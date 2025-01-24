import pandas as pd
from shapely.geometry import Point, shape
import json


df = pd.read_csv('csv-files/wikivoyage-listings-en.csv', sep=",", low_memory=False)
column_names = df.columns
final_table_columns = ['article', 'type', 'title', 'description', 'latitude', 'longitude']
df = df.drop(columns=[col for col in df if col not in final_table_columns])
df.groupby(['article', 'type']).size().sort_values(ascending=False).reset_index(name='count')
df['latitude'] = df.groupby('article')['latitude'].transform(lambda x: x.fillna(method='ffill').fillna(method='bfill'))
df['longitude'] = df.groupby('article')['longitude'].transform(lambda x: x.fillna(method='ffill').fillna(method='bfill'))

def clean_coordinates(value):
    try:
        return float(value)  # Attempt to convert to float
    except ValueError:
        return None  # If conversion fails, return None
df['latitude'] = df['latitude'].apply(clean_coordinates)
df['longitude'] = df['longitude'].apply(clean_coordinates)

df = df.dropna(subset=['latitude', 'longitude'])
filter_df = df[df['in_croatia'] == False]
filter_df.to_csv('csv-files/lat_lon_world_no_CRO_dataset.csv', index=False)
#check if in croatia
with open('csv-files/hr.json', 'r') as f:  
    croatia_data = json.load(f)
croatia_border = shape(croatia_data['features'][0]['geometry'])

def is_in_croatia(lat, lon):
    point = Point(lon, lat)
    return croatia_border.contains(point)


df['in_croatia'] = df.apply(lambda row: is_in_croatia(row['latitude'], row['longitude']), axis=1)
filtered_df = df[df['in_croatia'] == True]
filtered_df.to_csv('csv-files/lat_lon_croatia_dataset.csv', index=False)




