import requests

API_BTC_USDT= "https://data.gateapi.io/api2/1/ticker/btc_usdt"

def get_api(api_url):
    resp = requests.post(api_url) # , json={}
    if resp.status_code != 200:
        print("# [session] WARNING try send api request failed. status_code:", resp_data.status_code)
        raise Exception("Status_Code_Not_200")
    resp_data = resp.json()
    return resp_data