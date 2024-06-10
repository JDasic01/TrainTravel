import json
import requests as r

ENDPOINT_URL = "https://api-inference.huggingface.co/models/philschmid/bart-large-cnn-samsum"
HF_TOKEN = "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"

parameter_payload = {
    "inputs": """Create a tourist guide for the city of Rijeka. Describe what tourists can see and do in Rijeka. Make it informative and engaging. Use the following text for information
he best way to see Rijeka’s Cultural and historical monuments is to follow the tourist path that gathers all of the most important sights for this town and its history. Most of them are accessible by foot, as they are mostly located in or near the city centre, but to see Trsat Castle you will need to take a short car/bus ride. Other option, the more adventurous one, is to climb 561 Trsat stairs that lead from city centre to Trsat. The Trsat Castle is worth the effort.
Also, a helpful travel companion is free AdriaGuide Rijeka mobile application [13], for smart phones and GPS navigation.Trsat Castle represents a strategically embossed gazebo on a hill 138 meters above sea level that dominates Rijeka. As a parochial centre it was mentioned for the first time in 1288. Trsat Castle is one of the oldest fortifications on the Croatian Coast, where the characteristics of the early medieval town construction have been preserved. Today Trsat Castle, beside the souvenir shop and the coffee shop, is enriched with new facilities – gallery space where art exhibitions are held as well as open-air summer concerts and theatre performances, fashion shows and literary evenings.
City Tower, a symbol of Rijeka and a good example of a typical round tower access-point, which leads into the fortified town. Today it dominates the central part of Korzo and is often used as a meeting place for local people.
The Our Lady of Trsat Sanctuary is the largest centre of pilgrimage in western Croatia.  It is famous for its numerous concessions and for the pilgrimages by numerous believers throughout the year, and especially on the Assumption of Mary holiday.
No supermarket can replace the charm of the personal contact with the vendor or the excitement of the unpredictable purchase at the main City market – Placa. The harmonious compound of two pavilions and a Fish market building where, in the morning hours, the real Rijeka can be experienced.
Torpedo – launching ramp The launching ramp from 1930s is an item belonging to the closed torpedo production factory. It is proof of the technical inventive of Rijeka during this period and at the same time is an important world landmark of industrial heritage.
For other cultural and historical monuments of Rijeka such as The Governor's Palace, St. Vitus Cathedral, Molo Longo, The Old Gateway or Roman Arch, Capuchin church of Our Lady of Lourdes and many other interesting places visit the pages of Rijeka Tourist Board. [14]Museums, collections and exhibitions – Rijeka is a city with an unusual, turbulent past. The best places to discover the whole story on Rijeka are its museums, amongst its rich collections and exhibitions.Maritime and Historical Museum of the Croatian Littoral Located in the beautiful Governor’s Palace building, it preserves a large part of Rijeka's history and maritime tradition. Besides its continuous ethnographic exhibition, visit our collection of furniture and portraits of people from Rijeka’s public life.
Natural History Museum [15] Besides the botanical garden, the museum is a multimedia centre with an aquarium containing species from the Adriatic Sea. Besides fish, sharks and sea rays, the museum also conserves species of insects, reptiles, birds and amphibians. Ideal entertainment for both children and adults.
Rijeka City Museum The museum includes eleven collections: fine arts, arts &amp; crafts, numismatics, valuable objects, medals, arms from the Second World War and from the Croatian War of Independence, a collection of theatre and film material, philately, photography, press and technical collections.
Modern and Contemporary Art Museum [16] The museum collects works of art by Rijeka artists from 19th century and """,
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

