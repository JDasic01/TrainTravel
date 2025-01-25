import pandas as pd

# Load the processed CSV file
input_file_path = "output_with_ner.csv"  # Replace with your file path
df = pd.read_csv(input_file_path)

# Add a column for the length of the input
df['input_length'] = df['input'].apply(len)

# Sort the dataset by input length in descending order
df_sorted = df.sort_values(by='input_length', ascending=False)

# Select the top 3000 rows
df_top_3000 = df_sorted.head(3000)

# Drop the input_length column as it's no longer needed
df_top_3000 = df_top_3000.drop(columns=['input_length'])

# Save the top 3000 rows to a new CSV file
output_file_path = "top_3000_longest_inputs.csv"  # Replace with your desired output path
df_top_3000.to_csv(output_file_path, index=False)

print(f"Top 3000 inputs with the longest input length saved to {output_file_path}")
