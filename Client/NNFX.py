import argparse
import pandas as pd

from Utility.Logger import Logger
from Database.Database import Database
from Strategy.Machine import Machine
from Strategy.Strategy import Strategy


class NNFX(Strategy):

    def __init__(self, db, symbol, timeframe, logger):
        super().__init__(symbol, timeframe, logger)
        self.db = db
        self.data = []

    def create_risk_management(self):
        pass

    def create_signal_management(self):
        pass


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
    strategy = NNFX(db, symbol, timeframe, logger)
    strategy.run()


if __name__ == "__main__":
    main()
