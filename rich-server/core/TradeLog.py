import os
from . import common

class TradeLog:
    def __init__(self, log_file, order_extract_func):
        self.log_file = log_file
        self.order_extract_func = order_extract_func
        self.ordered_data = {}
        self.largest_order = None
        self._init_from_file()

    def _init_from_file(self):
        if os.path.exists(self.log_file):
            json_data = common.load_json(self.log_file)
            self.data = json_data
            self.ordered_data = {self.order_extract_func(x) for x in self.data}
            self.largest_order = self.order_extract_func(self.data[-1])
            print("# [TraceLog] init from:", self.log_file)
        else:
            logdir = os.path.dirname(self.log_file)
            if not os.path.exists(logdir):
                print("# [TraceLog] create directory", logdir)
                os.makedirs(logdir)


    def  _append_single(self, order_idx, entry):
        order_idx = int(order_idx)
        assert(self.largest_order is None or order_idx > self.largest_order)
        
        self.ordered_data[order_idx] = entry
        self.largest_order = order_idx
    

    def _append_multiple(self, entries):
        for entry in entries:
            order_idx = self.order_extract_func(entry)
            order_idx = int(order_idx)
            if order_idx not in self.ordered_data:
                assert(self.largest_order is None or order_idx > self.largest_order)
                self.ordered_data[order_idx] = entry
                self.larget_order = order_idx
            # if history data, TODO: check data equal

        

class TradeLogCandle(TradeLog):
    def __init__(self, log_file):
        order_extract_func = lambda x : x[0]
        TradeLog.__init__(self, log_file, order_extract_func)

class TradeLogOrderbook(TradeLog):
    def __init__(self, log_file):
        TradeLog.__init__(self, log_file, None)

class TradeLogTradeHistory(TradeLog):
    def __init__(self, log_file):
        order_extract_func = lambda x : x["tradeID"]
        TradeLog.__init__(self, log_file, order_extract_func)