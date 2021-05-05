import threading
import time
import math
import json
import os

import requests 

from .common import *
from . import gateapi


class Scheduler():
    def __init__(self):
        print("# [scheduler] created.")


    def start(self):
        x = threading.Thread(target=self._run)
        print("# [scheduler] start scheduler thread ...")
        x.start()

    def _run(self):
        print("# [scheduler-thread] thread is running ...")
        DELTA_T = 3
        total_t = 0
        while True:
            time.sleep(DELTA_T)
            total_t += DELTA_T
            
            # query data
            data = gateapi.get_api(gateapi.API_BTC_USDT)
            print(data)