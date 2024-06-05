from flask import Flask, request, jsonify
from gpt4all import GPT4All

app = Flask(__name__)
model = GPT4All(model_name='orca-mini-3b-gguf2-q4_0.gguf')

@app.route('/generate', methods=['POST'])
def generate():
    data = request.get_json()
    prompt = data.get('prompt', '')
    temp = data.get('temp', 0)
    with model.chat_session():
        response = model.generate(prompt=prompt, temp=temp)
        session = model.current_chat_session
    return jsonify({'response': response, 'session': session})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
