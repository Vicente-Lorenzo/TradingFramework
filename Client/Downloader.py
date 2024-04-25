import argparse
import pandas as pd

from Utility.Logger import Logger
from Database.Database import Database
from Strategy.Machine import Machine
from Strategy.Strategy import Strategy


class Downloader(Strategy):

    def __init__(self, db, iid, symbol, timeframe, logger):
        super().__init__(iid, symbol, timeframe, logger)
        self.db = db
        self.raw_dates = []
        self.raw_open_prices = []
        self.raw_high_prices = []
        self.raw_low_prices = []
        self.raw_close_prices = []
        self.raw_volumes = []

    def create_signal_management(self):
        machine = Machine("Main", self.symbol, self.timeframe, self.logger)

        state0 = machine.create_state(name="Start", end=False)
        state1 = machine.create_state(name="Section", end=False)
        state2 = machine.create_state(name="End", end=True)

        state0.on_bar(action=self.append_data, to=state0, reason=None)
        state0.on_complete(action=None, to=state1, reason="Complete")
        state0.on_shutdown(action=self.save_data, to=state2, reason="Error")

        state1.on_bar(action=self.append_data, to=state1, reason=None)
        state1.on_shutdown(action=self.save_data, to=state2, reason="Terminated")

        return machine

    def append_data(self, date, open_price, high_price, low_price, close_price, volume):
        self.raw_dates.append(date)
        self.raw_open_prices.append(open_price)
        self.raw_high_prices.append(high_price)
        self.raw_low_prices.append(low_price)
        self.raw_close_prices.append(close_price)
        self.raw_volumes.append(volume)

    def save_data(self):
        raw_data = {"Date": self.raw_dates, "Open": self.raw_open_prices, "High": self.raw_high_prices, "Low": self.raw_low_prices, "Close": self.raw_close_prices, "Volume": self.raw_volumes}
        self.db.save_data(pd.DataFrame(raw_data).set_index("Date"))
        self.db.clean_data()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--iid", type=str, help="InstanceId attribute from the robot", required=True)
    parser.add_argument("--symbol", type=str, help="Symbol in which the robot will operate", required=True)
    parser.add_argument("--timeframe", type=str, help="Timeframe in which the robot will operate", required=True)
    parser.add_argument("--verbose", type=str, help="Logging verbose level", required=True)
    args = parser.parse_args()

    iid = args.iid
    symbol = args.symbol.upper()
    timeframe = args.timeframe.capitalize()
    verbose = args.verbose.upper()
    logger = Logger(verbose)

    db = Database("OHLCV", symbol, timeframe, logger)
    strategy = Downloader(db, iid, symbol, timeframe, logger)
    strategy.run()


if __name__ == "__main__":
    main()
