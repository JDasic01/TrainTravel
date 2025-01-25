import pandas as pd
import spacy

# Load spaCy's small English model
nlp = spacy.load("en_core_web_sm")

# Load the CSV file
file_path = "csv-files/lat_lon_croatia_dataset.csv"  # Replace with your input CSV file path
df = pd.read_csv(file_path)

# Function to extract key entities from the description
def extract_entities(description):
    doc = nlp(description)
    entities = []
    for ent in doc.ents:
        if ent.label_ in {"PERSON", "GPE", "ORG", "DATE", "TIME", "NORP", "EVENT", "LOC", "FAC"}:
            entities.append(ent.text)
    return ", ".join(entities)

# Apply the entity extraction to the description column
df['entities'] = df['description'].fillna('').apply(extract_entities)

# Combine `article`, `type`, `title`, and `entities` into a single `input` column
df['input'] = df[['article', 'type', 'title', 'entities']].apply(
    lambda x: ' | '.join(x.astype(str).replace('nan', '').tolist()), axis=1
)

# Keep the `description` column as the `output` column
df['output'] = df['description']

# Drop unnecessary columns
df = df[['input', 'output']]

# Save the new dataset to a CSV file
output_file_path = "csv-files/cro_output_with_ner.csv"  # Replace with your desired output file path
df.to_csv(output_file_path, index=False)

print(f"Transformed dataset with NER saved to {output_file_path}")
