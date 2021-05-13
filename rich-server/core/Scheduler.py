import threading
import time
import math
import json
import os

import requests

from core.TradeLog import TradeLogCandle, TradeLogOrderbook, TradeLogTradeHistory 

from .common import *
from . import gateapi


coin_name_to_log_file = lambda coin_name, log_type: abs_join(SCRIPT_DIR, f"../../outputs/{coin_name}/{log_type}.log")

class Scheduler():
    def __init__(self):
        print("# [scheduler] created.")
        self.sched_thread = None
        self.is_stopping = False
        self.data_loggers = {
            coin_name:{
                "orderbook": TradeLogOrderbook(coin_name_to_log_file(coin_name, "orderbook")),
                "tradehistory": TradeLogTradeHistory(coin_name_to_log_file(coin_name, "tradehistory")),
                "candle": TradeLogCandle(coin_name_to_log_file(coin_name, "candle"))
            } 
            for coin_name in gateapi.TRADE_SETTINGS
        }

    def start(self):
        self.sched_thread = threading.Thread(target=self._run)
        print("# [scheduler] start scheduler thread ...")
        self.sched_thread.start()

    def stop(self):
        self.is_stopping = True
        self.sched_thread.join()

    def _run(self):
        print("# [scheduler-thread] thread is running ...")
        DELTA_T = 5
        total_t = 0
        while not self.is_stopping:            
            # query data
            print("\n# [scheduler-thread] Querying data ... ", total_t)
            for key in gateapi.TRADE_SETTINGS:
                if self.is_stopping: break
                data = gateapi.get_api(gateapi.API2_TICKER(key))
                print("# status(ticker):", data)
                data = gateapi.get_api(gateapi.API2_ORDERBOOK(key))
                # self.data_loggers[key]["orderbook"]._update_multiple()
                print("# depth(orderbook):", len(data))
                data = gateapi.get_api(gateapi.API2_TRADEHISTORY(key))
                print("# tradehistory:", len(data))
                data = gateapi.get_api(gateapi.API2_CANDLE(key, 60, 1)) # 1 minute candle for 1 hour
                
                # TODO: process data, drop last
                self.data_loggers[key]["candle"]._append_multiple(data["data"])
                print("# candle log entries count:", len(self.data_loggers[key]["candle"].ordered_data))

            time.sleep(DELTA_T)
            total_t += DELTA_T
    
        print("# [scheduler-thread] done.")