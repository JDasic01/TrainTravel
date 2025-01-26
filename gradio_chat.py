import gradio as gr
from neo4j import GraphDatabase
import random
from transformers import pipeline

# Connect to Neo4j database
uri = "bolt://ip:7687"  # Replace with your database URI
username = "user"       # Replace with your Neo4j username
password = "pass"       # Replace with your Neo4j password

driver = GraphDatabase.driver(uri, auth=(username, password))


llm_pipeline = pipeline("text-generation", model="gpt2")
def get_random_answer_and_generate_text():
    cypher_query = "MATCH (n) RETURN n.answer AS answer"
    
    with driver.session() as session:
        result = session.run(cypher_query)
        answers = [record['answer'] for record in result]
        
        if answers:
            random_answer = random.choice(answers)
        else:
            random_answer = "No answers found in the database."

    llm_response = llm_pipeline(f"Here is the random answer: {random_answer}. What do you think?", max_length=100)[0]["generated_text"]
    
    return random_answer, llm_response

iface = gr.Interface(
    fn=get_random_answer_and_generate_text, 
    inputs=[], 
    outputs=["text", "text"], 
    live=True, 
    title="RAG - Neo4j & LLM Integration", 
    description="Fetches a random answer from a Neo4j database and generates a response using a Hugging Face LLM model."
)

iface.launch()
