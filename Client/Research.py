import os
import argparse

from Utility.Logger import Logger
from Database.Database import Database
from Optimisation.Optimisation import Optimisation
from Optimisation.Baseline import BaselineIndicators, BaselineStrategy, BaselineMetric


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--verbose", type=str, help="Logging verbose level", default="Debug", choices=["Error", "Warning", "Info", "Debug"])
    parser.add_argument("--symbol", type=str, help="Symbol in which the robot will operate", required=True)
    parser.add_argument("--timeframe", type=str, help="Timeframe in which the robot will operate", required=True)
    args = parser.parse_args()

    verbose = args.verbose.upper()
    symbol = args.symbol.upper()
    timeframe = args.timeframe.capitalize()
    logger = Logger(verbose)

    db = Database(symbol, timeframe, logger)
    data = db.load_data(start="01-01-2015")

    os.makedirs(f"{os.path.dirname(os.path.abspath(__file__))}\\{symbol}\\{timeframe}", exist_ok=True)

    bl_backtest, bl_indicator, bl_parameters = Optimisation("Baseline", data, BaselineStrategy, BaselineIndicators, BaselineMetric).run()

    # bl_backtest.plot(filename=f"{folder}/Baseline_{bl_indicator}.html")
    # sns.heatmap(best_heatmap.groupby(list(best_params.keys())).mean().unstack(), cmap="plasma")


if __name__ == "__main__":
    main()
