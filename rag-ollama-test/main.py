import os
import pandas as pd
from datetime import datetime
from fastapi import FastAPI, File, UploadFile
from fastapi.responses import JSONResponse
from neo4j import GraphDatabase
from langchain_experimental.graph_transformers import LLMGraphTransformer
from langchain_neo4j import Neo4jGraph, Neo4jVector
from langchain_core.documents import Document
from langchain_ollama import ChatOllama, OllamaEmbeddings

app = FastAPI()

# Neo4j connection details
uri = "bolt://database:7687"
user = "neo4j"
password = "root"

# Create Neo4j driver
driver = GraphDatabase.driver(uri, auth=(user, password))

# Create Neo4j graph object
graph = Neo4jGraph(url=uri, username=user, password=password, refresh_schema=False)


@app.get("/")
async def root():
    """Endpoint to check if FastAPI is running."""
    return {"message": "FastAPI is running!"}


@app.get("/db_connection")
async def check_db_connection():
    """Check Neo4j database connection."""
    try:
        with driver.session() as session:
            result = session.run("MATCH (n) RETURN n LIMIT 1")
            data = list(result)
            if data:
                return {"message": "Database connection successful!"}
            else:
                return JSONResponse(status_code=500, content={"message": "No data found in database."})
    except Exception as e:
        return JSONResponse(status_code=500, content={"message": f"Error connecting to database: {str(e)}"})


def ingest_guide(city_id, city_name, section_title, section_text):
    """Ingests section text into the Neo4j knowledge base."""
    
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

    # Link sections to the city
    graph.query("""
        MATCH (c:City {id: $city_id}), (d:Document {text: $section_text})
        MERGE (c)-[:HAS_SECTION]->(d)
    """, {"city_id": city_id, "section_text": section_text})

    # Create vector embeddings for advanced search
    embed = OllamaEmbeddings(model="mxbai-embed-large")
    vector_index = Neo4jVector.from_existing_graph(
        embedding=embed,
        url=uri,
        username=user,
        password=password,
        search_type="hybrid",
        node_label="Document",
        text_node_properties=["text"],
        embedding_node_property="embedding"
    )

    global vector_retriever
    vector_retriever = vector_index.as_retriever()


@app.post("/upload_csv/")
async def upload_csv(file: UploadFile = File(...)):
    """
    API endpoint to upload and process a CSV file.
    - Saves file to 'Data/' directory with a unique timestamp-based filename.
    - Processes and ingests CSV content into Neo4j.
    """

    try:
        # Ensure the Data directory exists
        data_dir = "csv-files"
        os.makedirs(data_dir, exist_ok=True)

        # Generate a unique filename with timestamp (YYYYMMDD_HHMMSS)
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        file_path = os.path.join(data_dir, f"tourist_guides_{timestamp}.csv")

        # Save the uploaded file
        with open(file_path, "wb") as buffer:
            buffer.write(file.file.read())

        # Load CSV into DataFrame
        df = pd.read_csv(file_path)

        # Process each row
        for _, row in df.iterrows():
            city_id = row["city_id"]
            city_name = row["city_name"]
            section_title = row["section_title"]
            section_text = row["section_text"]

            ingest_guide(city_id, city_name, section_title, section_text)

        return JSONResponse(status_code=200, content={"message": f"CSV file processed successfully!", "filename": file_path})

    except Exception as e:
        return JSONResponse(status_code=500, content={"message": f"Error processing CSV file: {str(e)}"})