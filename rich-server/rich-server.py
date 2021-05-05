from flask import Flask, request
from flask_restful import Api
from flask_cors import CORS, cross_origin
import sys
import os
import json

abs_join = lambda p1, p2 : os.path.abspath(os.path.join(p1, p2))
SCRIPT_DIR = os.path.abspath(os.path.dirname(__file__))

app = Flask(__name__)
CORS(app)
api = Api(app)

config = {
    "port": 8012
}

def main():
    # update config
    print("\n\n      Rich Server\n\n")
    print("# Configuration:")
    print(json.dumps(config, indent=2))

    # running the server
    print("Api running on port : {} ".format(config["port"]))
    app.run(host="0.0.0.0", port=config["port"], debug=True)


@app.route('/')
@cross_origin()
def welcome():
    return """
<h2>This is Rich server</h2>
"""

@app.route('/posttest', methods=["POST"])
@cross_origin()
def api_posttest():
    post_data = request.json
    return {"data": post_data}

@app.route('/status')
@cross_origin()
def api_status():
    return {"status": "hahaha"}


if __name__ == "__main__":
    main()