import requests


TRADE_SETTINGS = {
    "btc_usdt": {},
    "doge_usdt": {},
    "etc_usdt": {},
    "eth_usdt": {},
    "xmr_usdt": {},
    "ada_usdt": {}
}

# https://www.gate.io/api2

API2_MARKET_LIST = "https://data.gateapi.io/api2/1/marketlist"

# real-time detail
API2_TICKER = lambda trade_type : f"https://data.gateapi.io/api2/1/ticker/{trade_type}"
# trade log
API2_TRADEHISTORY = lambda trade_type : f"https://data.gateapi.io/api2/1/tradeHistory/{trade_type}"
# candle stick
API2_CANDLE = lambda trade_type, group_sec, range_hour : f"https://data.gateapi.io/api2/1/candlestick2/{trade_type}?group_sec={group_sec}&range_hour={range_hour}"
# depth
API2_ORDERBOOK = lambda trade_type : f"https://data.gateapi.io/api2/1/orderBook/{trade_type}"

def get_api(api_url):
    resp = requests.get(api_url) # , json={}
    if resp.status_code != 200:
        print("# [session] WARNING try send api request failed. status_code:", resp.status_code)
        raise Exception("Status_Code_Not_200")
    resp_data = resp.json()
    return resp_data