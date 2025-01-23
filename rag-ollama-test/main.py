from fastapi import FastAPI
from fastapi.responses import JSONResponse
from neo4j import GraphDatabase

app = FastAPI()

# Neo4j connection details
uri = "neo4j://<neo4j_host>:<neo4j_port>" 
user = "<neo4j_username>"
password = "<neo4j_password>"

# Create a Neo4j driver
driver = GraphDatabase.driver(uri, auth=(user, password))

@app.get("/")
async def root():
    """
    Endpoint to check FastAPI health.
    """
    return {"message": "FastAPI is running!"}

@app.get("/db_connection")
async def check_db_connection():
    """
    Endpoint to check Neo4j database connection.
    """
    try:
        with driver.session() as session:
            result = session.run("MATCH (n) RETURN n LIMIT 1")
            data = list(result)
            if data:
                return {"message": "Database connection successful!"}
            else:
                return {"message": "No data found in database."}, 500
    except Exception as e:
        return {"message": f"Error connecting to database: {str(e)}"}, 500