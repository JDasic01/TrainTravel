FROM neo4j:latest

# Add a non-root user (optional)
RUN adduser --disabled-password --gecos '' myuser
USER myuser

# Set the working directory
WORKDIR /app

# Copy files to the working directory (if needed)
COPY . /app

# Expose the Neo4j ports (adjust if needed)
EXPOSE 7474 7473 7687

# Example: Execute Cypher query using neo4j-admin
CMD ["neo4j-admin", "cypher-shell", "-u", "neo4j", "-p", "your_password", "--format", "verbose", "MATCH (n) RETURN n;"]
