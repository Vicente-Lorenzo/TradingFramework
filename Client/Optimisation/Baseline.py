import talib

from backtesting import Strategy
from backtesting.lib import crossover

BaselineIndicators = [
    {
        "name": "SMA",
        "series": lambda self: self.data.Close,
        "ranges": {"timeperiod": range(5, 51, 1)},
        "buy_signal": lambda self, indicator: crossover(self.data.Close, indicator),
        "sell_signal": lambda self, indicator: crossover(indicator, self.data.Close)
    },
    {
        "name": "EMA",
        "series": lambda self: self.data.Close,
        "ranges": {"timeperiod": range(5, 51, 1)},
        "buy_signal": lambda self, indicator: crossover(self.data.Close, indicator),
        "sell_signal": lambda self, indicator: crossover(indicator, self.data.Close)
    },
    {
        "name": "DEMA",
        "series": lambda self: self.data.Close,
        "ranges": {"timeperiod": range(5, 51, 1)},
        "buy_signal": lambda self, indicator: crossover(self.data.Close, indicator),
        "sell_signal": lambda self, indicator: crossover(indicator, self.data.Close)
    },
    {
        "name": "TEMA",
        "series": lambda self: self.data.Close,
        "ranges": {"timeperiod": range(5, 51, 1)},
        "buy_signal": lambda self, indicator: crossover(self.data.Close, indicator),
        "sell_signal": lambda self, indicator: crossover(indicator, self.data.Close)
    },
    {
        "name": "TRIMA",
        "series": lambda self: self.data.Close,
        "ranges": {"timeperiod": range(5, 51, 1)},
        "buy_signal": lambda self, indicator: crossover(self.data.Close, indicator),
        "sell_signal": lambda self, indicator: crossover(indicator, self.data.Close)
    },
    {
        "name": "WMA",
        "series": lambda self: self.data.Close,
        "ranges": {"timeperiod": range(5, 51, 1)},
        "buy_signal": lambda self, indicator: crossover(self.data.Close, indicator),
        "sell_signal": lambda self, indicator: crossover(indicator, self.data.Close)
    },
    {
        "name": "KAMA",
        "series": lambda self: self.data.Close,
        "ranges": {"timeperiod": range(5, 51, 1)},
        "buy_signal": lambda self, indicator: crossover(self.data.Close, indicator),
        "sell_signal": lambda self, indicator: crossover(indicator, self.data.Close)
    },
]
"""
    {
        "name": "MAMA",
        "series": lambda self: self.data.Close,
        "ranges": {"fastlimit": 0.5, "slowlimit": 0.05},
        "buy_signal": lambda self, indicator: crossover(self.data.Close, indicator),
        "sell_signal": lambda self, indicator: crossover(indicator, self.data.Close)
    },
"""


class BaselineStrategy(Strategy):
    indicator = None
    atr = None

    name = None
    series = None
    buy_signal = None
    sell_signal = None

    timeperiod = None
    fastlimit = None
    slowlimit = None

    def init(self):
        self.indicator = self.I(getattr(talib, self.name), self.series())
        self.atr = self.I(talib.ATR, self.data.High, self.data.Low, self.data.Close, timeperiod=14)

    def next(self):

        if self.buy_signal(self.indicator):
            self.position.close()
            self.buy()
        elif self.sell_signal(self.indicator):
            self.position.close()
            self.sell()


def BaselineMetric(stats):
    return 1/stats["# Trades"]
