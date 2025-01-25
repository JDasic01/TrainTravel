import os
import pandas as pd
from langchain_experimental.graph_transformers import LLMGraphTransformer
from langchain_neo4j import Neo4jGraph, Neo4jVector
from langchain_core.documents import Document
from langchain_ollama import ChatOllama
from langchain_ollama import OllamaEmbeddings

# 1️⃣ CONNECT TO NEO4J DATABASE
graph = Neo4jGraph(
    url="bolt://localhost:7689",
    username="neo4j",
    password="password",
    refresh_schema=False
)

# 2️⃣ FUNCTION TO INGEST TEXT INTO THE GRAPH
def ingest_guide(city_id, city_name, section_title, section_text):
    """Converts section text into a graph and stores it in Neo4j under the city entity."""

    # Create or retrieve the city node
    graph.query("""
        MERGE (c:City {id: $city_id, name: $city_name})
        RETURN c
    """, {"city_id": city_id, "city_name": city_name})

    # Convert section into a document
    documents = [Document(page_content=section_text, metadata={"city": city_name, "section": section_title})]

    # Use LLM to extract relationships
    llm = ChatOllama(model="llama3", temperature=0)
    graph_transformer = LLMGraphTransformer(llm=llm, ignore_tool_usage=True)

    # Convert text to graph format
    graph_documents = graph_transformer.convert_to_graph_documents(documents)

    # Store the extracted graph data in Neo4j and link it to the city
    graph.add_graph_documents(graph_documents, baseEntityLabel=True, include_source=True)

    # Link sections to the city in the database
    graph.query("""
        MATCH (c:City {id: $city_id}), (d:Document {text: $section_text})
        MERGE (c)-[:HAS_SECTION]->(d)
    """, {"city_id": city_id, "section_text": section_text})

    # Create vector embeddings for advanced search
    embed = OllamaEmbeddings(model="mxbai-embed-large")
    vector_index = Neo4jVector.from_existing_graph(
        embedding=embed,
        url="bolt://localhost:7689",
        username="neo4j",
        password="password",
        search_type="hybrid",
        node_label="Document",
        text_node_properties=["text"],
        embedding_node_property="embedding"
    )

    global vector_retriever
    vector_retriever = vector_index.as_retriever()


if __name__ == "__main__":
    # Define the CSV file path dynamically with today's date
    from datetime import datetime
    today_date = datetime.today().strftime("%Y-%m-%d")
    CSV_FILE = fr"VisitCroatiaScraper\Data\tourist_guides_{today_date}.csv"

    # Check if the CSV file exists
    if not os.path.exists(CSV_FILE):
        print(f"❌ CSV file '{CSV_FILE}' not found!")
    else:
        print(f"📂 Loading data from '{CSV_FILE}'...")

        # Load the CSV file
        df = pd.read_csv(CSV_FILE)

        # Process each row in the CSV
        for _, row in df.iterrows():
            city_id = row["city_id"]
            city_name = row["city_name"]
            section_title = row["section_title"]
            section_text = row["section_text"]

            print(f"📄 Ingesting section '{section_title}' for city '{city_name}'...")

            ingest_guide(city_id, city_name, section_title, section_text)

            print(f"✅ Section '{section_title}' ingested successfully!")

        print("🚀 All sections processed!")