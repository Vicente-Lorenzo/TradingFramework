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

        self.position = None

    def create_risk_management(self):
        machine = Machine("Risk Management", self.symbol, self.timeframe, self.logger)

        state0 = machine.create_state(name="No Position", end=False)
        state1 = machine.create_state(name="Position Waiting TP", end=False)
        state2 = machine.create_state(name="Position Waiting TSL", end=False)
        state3 = machine.create_state(name="Position Waiting Close", end=False)
        state4 = machine.create_state(name="End", end=True)

        state0.on_position_opened(trigger=None, action=self.position_opened_action, to=state1, reason="Position Opened")
        state0.on_shutdown(trigger=None, action=None, to=state4, reason="Terminated")

        state1.on_position_closed(trigger=None, action=self.position_closed_action, to=state0, reason="Position Closed")
        state1.on_tick(trigger=self.scaling_out_trigger, action=self.scaling_out_action, to=state2, reason="Detected Scaling Out")
        state1.on_position_modified(trigger=None, action=self.position_modified_action, to=state2, reason="Performed Scaling Out")
        state1.on_shutdown(trigger=None, action=None, to=state4, reason="Terminated")

        state2.on_position_closed(trigger=None, action=self.position_closed_action, to=state0, reason="Position Closed")
        state2.on_tick(trigger=self.tsl_activation_trigger, action=self.tsl_activation_action, to=state3, reason="Detected TSL")
        state2.on_position_modified(trigger=None, action=self.position_modified_action, to=state3, reason="Activated TSL")
        state2.on_shutdown(trigger=None, action=None, to=state4, reason="Terminated")

        state3.on_position_closed(trigger=None, action=self.position_closed_action, to=state0, reason="Position Closed")
        state3.on_tick(trigger=self.tsl_activation_trigger, action=self.tsl_activation_action, to=state3, reason="Detected TSL")
        state3.on_position_modified(trigger=None, action=self.position_modified_action, to=state3, reason="Updated TSL")
        state3.on_shutdown(trigger=None, action=None, to=state4, reason="Terminated")

        return machine

    def position_opened_action(self, position):
        self.position = position

    def position_modified_action(self, position):
        self.position = position

    def position_closed_action(self, position):
        self.position = None

    def scaling_out_trigger(self, tick):
        return False

    def scaling_out_action(self, tick):
        pass

    def tsl_activation_trigger(self, tick):
        return False

    def tsl_activation_action(self, tick):
        pass

    def create_signal_management(self):
        machine = Machine("Signal Management", self.symbol, self.timeframe, self.logger)

        state0 = machine.create_state(name="Start", end=False)
        state1 = machine.create_state(name=None, end=True)

        state0.on_bar_closed(trigger=None, action=self.process_bar_action, to=state0, reason=None)
        state0.on_complete(trigger=None, action=None, to=state1, reason="Complete")
        state0.on_shutdown(trigger=None, action=self.save_data_action, to=state1, reason="Error")

        state1.on_bar_closed(trigger=None, action=self.process_bar_action, to=state1, reason=None)
        state1.on_shutdown(trigger=None, action=self.save_data_action, to=state1, reason="Complete")

        return machine

    def process_bar_action(self, bar):
        self.data.append(bar)

    def save_data_action(self):
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
    strategy = NNFX(db, symbol, timeframe, logger)
    strategy.run()


if __name__ == "__main__":
    main()
