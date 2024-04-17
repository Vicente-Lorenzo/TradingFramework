import argparse
import pandas as pd

from Utility.Logger import Logger
from Database.Database import Database
from Strategy.Machine import Machine
from Strategy.Strategy import Strategy


class Downloader(Strategy):

    def __init__(self, db, symbol, timeframe, logger):
        super().__init__(symbol, timeframe, logger)
        self.db = db
        self.data = []

    def create_risk_management(self):
        machine = Machine(self.symbol, self.timeframe, self.logger)
        machine.create_state()
        return machine

    def create_signal_management(self):
        machine = Machine(self.symbol, self.timeframe, self.logger)

        state0 = machine.create_state()
        state1 = machine.create_state()

        state0.on_bar_closed(None, self.append_data, state0)
        state0.on_complete(None, None, state1)

        state1.on_bar_closed(None, self.append_data, state1)
        state1.on_shutdown(None, self.save_data, state1)

        return machine

    def append_data(self, data):
        self.data.append(data)

    def save_data(self):
        self.db.save_data(pd.DataFrame(self.data).set_index("Date"))
        self.db.clean_data()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--verbose", type=str, help="Logging verbose level", required=True)
    parser.add_argument("--symbol", type=str, help="Symbol in which the robot will operate", required=True)
    parser.add_argument("--timeframe", type=str, help="Timeframe in which the robot will operate", required=True)
    args = parser.parse_args()

    verbose = args.verbose.upper()
    symbol = args.symbol.upper()
    timeframe = args.timeframe.capitalize()

    logger = Logger(verbose)

    db = Database("OHLCV", symbol, timeframe, logger)
    strategy = Downloader(db, symbol, timeframe, logger)
    strategy.run()


if __name__ == "__main__":
    main()
