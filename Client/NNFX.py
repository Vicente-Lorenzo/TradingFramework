import argparse
import pandas as pd

from Utility.Logger import Logger
from Database.Database import Database
from Database.Technical import offline_technical, online_technical
from Strategy.Api import IdSend, MarketDirection
from Strategy.Machine import Machine
from Strategy.Strategy import Strategy

WINDOW = 100


class NNFX(Strategy):

    def __init__(self, db, symbol, timeframe, logger):
        super().__init__(symbol, timeframe, logger)

        self.db = db
        self.raw_data = []
        self.window_data = None

        self.indicators = {"ATR": lambda lib, data: lib.ATR(data.High, data.Low, data.Close, timeperiod=14),
                           "SMA10": lambda lib, data: lib.SMA(data.Close, timeperiod=10),
                           "SMA20": lambda lib, data: lib.SMA(data.Close, timeperiod=20)}

        self.position_direction = MarketDirection.Sideways.value
        self.position_entry_price = None
        self.symbol_pip_size = None
        self.last_atr_value = None
        self.generated_signal = None

        self.position_tp_value = -1.0

    def create_risk_management(self):
        machine = Machine("Risk Management", self.symbol, self.timeframe, self.logger)

        state0 = machine.create_state(name="No Position", end=False)
        state1 = machine.create_state(name="Waiting TP", end=False)
        state2 = machine.create_state(name="Waiting BE", end=False)
        state3 = machine.create_state(name="Waiting TSL", end=False)
        state4 = machine.create_state(name="Waiting Close", end=False)
        state5 = machine.create_state(name="End", end=True)

        state0.on_position_opened(trigger=None, action=self.position_opened_action, to=state1, reason="Position Opened")
        state0.on_shutdown(trigger=None, action=None, to=state5, reason="Terminated")

        state1.on_position_closed(trigger=None, action=self.position_closed_action, to=state0, reason="Position Closed")
        state1.on_tick(trigger=self.scaling_out_trigger, action=self.scaling_out_action, to=state1, reason="Detected Scaling Out")
        state1.on_position_modified(trigger=None, action=None, to=state2, reason="Performed Partial Close")
        state1.on_shutdown(trigger=None, action=None, to=state5, reason="Terminated")

        state2.on_position_closed(trigger=None, action=self.position_closed_action, to=state0, reason="Position Closed")
        state2.on_position_modified(trigger=None, action=self.update_position_action, to=state3, reason="Performed Break-Even")
        state2.on_shutdown(trigger=None, action=None, to=state5, reason="Terminated")

        state3.on_position_closed(trigger=None, action=self.position_closed_action, to=state0, reason="Position Closed")
        state3.on_tick(trigger=self.tsl_update_trigger, action=self.tsl_update_action, to=state3, reason=None)
        state3.on_position_modified(trigger=None, action=self.update_position_action, to=state4, reason="Activated TSL")
        state3.on_shutdown(trigger=None, action=None, to=state5, reason="Terminated")

        state4.on_position_closed(trigger=None, action=self.position_closed_action, to=state0, reason="Position Closed")
        state4.on_tick(trigger=self.tsl_update_trigger, action=self.tsl_update_action, to=state4, reason=None)
        state4.on_position_modified(trigger=None, action=self.update_position_action, to=state4, reason=None)
        state4.on_shutdown(trigger=None, action=None, to=state5, reason="Terminated")

        return machine

    def position_opened_action(self, position):
        self.position_direction = position["Type"]
        self.position_entry_price = position["Entry"]
        self.position_volume_value = position["Volume"]
        self.position_sl_value = position["StopLoss"]

    def position_closed_action(self, position):
        self.position_direction = MarketDirection.Sideways.value
        self.position_entry_price = None
        self.position_volume_value = None
        self.position_sl_value = None

    def scaling_out_trigger(self, tick):
        if self.position_direction == MarketDirection.Bullish.value:
            return tick["Ask"] >= self.position_entry_price + 1.0 * self.last_atr_value
        if self.position_direction == MarketDirection.Bearish.value:
            return tick["Bid"] <= self.position_entry_price - 1.0 * self.last_atr_value
        return False

    def scaling_out_action(self, _):
        self.position_volume_value = 0.5 * self.position_volume_value
        self.position_sl_value = self.position_entry_price
        return IdSend.ModifyPosition.value

    def tsl_update_trigger(self, tick):
        if self.position_direction == MarketDirection.Bullish.value:
            return tick["Ask"] - self.position_sl_value >= 1.5 * self.last_atr_value
        if self.position_direction == MarketDirection.Bearish.value:
            return self.position_sl_value - tick["Bid"] >= 1.5 * self.last_atr_value
        return False

    def tsl_update_action(self, tick):
        if self.position_direction == MarketDirection.Bullish.value:
            self.position_sl_value = tick["Ask"] - 1.5 * self.last_atr_value
        if self.position_direction == MarketDirection.Bearish.value:
            self.position_sl_value = tick["Bid"] + 1.5 * self.last_atr_value
        return IdSend.ModifyPosition.value

    def update_position_action(self, position):
        self.position_volume_value = position["Volume"]
        self.position_sl_value = position["StopLoss"]

    def create_signal_management(self):
        machine = Machine("Signal Management", self.symbol, self.timeframe, self.logger)

        state0 = machine.create_state(name="Start", end=False)
        state1 = machine.create_state(name="Trading", end=False)
        state2 = machine.create_state(name="End", end=True)

        state0.on_symbol(trigger=None, action=self.symbol_action, to=state0, reason="Symbol Received")
        state0.on_bar_closed(trigger=None, action=self.append_data_action, to=state0, reason=None)
        state0.on_complete(trigger=None, action=self.prepare_data_action, to=state1, reason="Prepared")
        state0.on_shutdown(trigger=None, action=self.save_data_action, to=state2, reason="Error")

        state1.on_bar_closed(trigger=None, action=self.process_signal_action, to=state1, reason=None)
        state1.on_tick(trigger=self.generate_signal_trigger, action=self.generate_signal_action, to=state1, reason=None)
        state1.on_shutdown(trigger=None, action=self.save_data_action, to=state2, reason="Terminated")

        return machine

    def symbol_action(self, symbol):
        self.symbol_pip_size = symbol["PipSize"]

    def append_data_action(self, bar):
        self.raw_data.append(bar)

    def prepare_data_action(self):
        self.db.save_data(pd.DataFrame(self.raw_data).set_index("Date"))
        self.window_data = self.db.load_data(start=self.raw_data[0]["Date"], end=self.raw_data[-1]["Date"], tail=WINDOW)
        self.window_data = offline_technical(self.window_data, self.indicators)

    def process_signal_action(self, bar):
        self.window_data = pd.concat([self.window_data, pd.DataFrame([bar]).set_index("Date")])
        self.window_data = online_technical(self.window_data, self.indicators)
        previous = self.window_data.iloc[-2]
        current = self.window_data.iloc[-1]
        self.last_atr_value = current.ATR
        if current.SMA10 > current.SMA20 and previous.SMA10 < previous.SMA20:
            self.generated_signal = IdSend.BullishSignal.value
        elif current.SMA10 < current.SMA20 and previous.SMA10 > previous.SMA20:
            self.generated_signal = IdSend.BearishSignal.value

    def generate_signal_trigger(self, _):
        return self.generated_signal is not None

    def generate_signal_action(self, _):
        signal = self.generated_signal
        self.generated_signal = None
        self.position_volume_value = 2
        self.position_sl_value = 1.5 * self.last_atr_value / self.symbol_pip_size
        return signal

    def save_data_action(self):
        self.db.save_data(self.window_data)
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
