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
        self.sched_thread = None


    def start(self):
        self.sched_thread = threading.Thread(target=self._run)
        print("# [scheduler] start scheduler thread ...")
        self.sched_thread.start()

    def wait(self):
        self.sched_thread.join()

    def _run(self):
        print("# [scheduler-thread] thread is running ...")
        DELTA_T = 5
        total_t = 0
        while True:
            time.sleep(DELTA_T)
            total_t += DELTA_T
            
            # query data
            print("\n# [scheduler-thread] Querying data ... ", total_t)
            for key in gateapi.TRADE_SETTINGS:
                data = gateapi.get_api(gateapi.API2_TICKER(key))
                print(data)