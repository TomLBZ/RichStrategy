import os
from . import common

class TradeLog:
    def __init__(self, log_file, order_extract_func):
        self.log_file = log_file
        self.order_extract_func = order_extract_func
        self.data = []
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


    def  _append_single(self, order_idx, entry):
        assert(order_idx > self.largest_order)
        self.data.append([order_idx, entry])
        self.ordered_data[order_idx] = entry
        self.largest_order = order_idx

    def _append_multiple(self, entries):
        for entry in entries:
            order_idx = self.order_extract_func(entry)
            if order_idx not in self.ordered_data:
                assert(order_idx > self.largest_order)
                self.data.append([order_idx, entry])
                self.ordered_data[order_idx] = entry
                self.larget_order = order_idx
        

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