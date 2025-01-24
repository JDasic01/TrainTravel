**Starting the project**
```
docker compose up --build
```
**Endpoints**
Blazor - localhost:8081
API - localhost:8082
- for development UI -> localhost:8082/swagger

**Dataset for finetuning**:
WikiVoyage.xml used for webscraping Wikivoyage

**Finetuning**
- HuggingFace llama2, validate with mlflow with llama3?
- RAG with llama2
- MlFlow with RAG

**Gradio ChatBot**
- finetuned model deployed to hugging face and used in blazor app


**Dataset for app**:
Webscraping VisitCroatia.