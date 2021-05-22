import math
import json
import os

abs_join = lambda p1, p2 : os.path.abspath(os.path.join(p1, p2))
SCRIPT_DIR = os.path.abspath(os.path.dirname(__file__))

# read value to a dict by keys path
def dict_keys_write(mydict, keys, value):
    assert(len(keys) > 0)
    cur_key = keys[0]
    rest_keys = keys[1:]
    if len(rest_keys) == 0:
        mydict[cur_key] = value
    else:
        if cur_key not in mydict:  
            mydict[cur_key] = {}
        dict_keys_write(mydict[cur_key], keys[1:], value)
    

# read from a dict by keys path
def dict_keys_read(mydict, keys):
    assert(len(keys) > 0)
    cur_key = keys[0]
    if cur_key in mydict:
        rest_keys = keys[1:]
        if len(rest_keys) == 0:
            return mydict[cur_key]
        return dict_keys_read(mydict[cur_key], keys[1:])
    else:
        return None

def load_json(filename):
    print("# [scheduler] load_json:", filename)
    data = None
    if not os.path.exists(filename):
        return None
    with open(filename, 'r') as f:
        data = json.load(f)
    return data

def save_json(filename, data):
    print("# [scheduler] save_json:", filename)
    with open(filename, 'w') as f:
        json.dump(data, f, indent=2)