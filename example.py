import json
import requests as r

ENDPOINT_URL = "https://api-inference.huggingface.co/models/gpt2"
HF_TOKEN = "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"

parameter_payload = {
    "inputs": "Create a tourist guide for the city of Rijeka. Describe what tourists can see and do in Rijeka. Make it informative and engaging.",
    "parameters": {
        "max_length": 1000  
    }
}

headers = {
    "Authorization": f"Bearer {HF_TOKEN}",
    "Content-Type": "application/json"
}

response = r.post(ENDPOINT_URL, headers=headers, json=parameter_payload)
generated_text = response.json()

print(json.dumps(generated_text, indent=2))
