import argparse
import pandas as pd

from Utility.Logger import Logger
from Database.Database import Database
from Database.Technical import offline_technical, online_technical
from Strategy.Api import IdSend
from Strategy.Machine import Machine
from Strategy.Strategy import Strategy

class NNFX(Strategy):

    def __init__(self, db, iid, symbol, timeframe, logger):
        super().__init__(iid, symbol, timeframe, logger)

        self.db = db
        self.raw_dates = []
        self.raw_open_prices = []
        self.raw_high_prices = []
        self.raw_low_prices = []
        self.raw_close_prices = []
        self.raw_volumes = []
        self.market_data = None
        self.indicator_data = None

        self.indicators = {"ATR": lambda lib, data: lib.ATR(data.High, data.Low, data.Close, timeperiod=14),
                           "SMA10": lambda lib, data: lib.SMA(data.Close, timeperiod=10),
                           "SMA20": lambda lib, data: lib.SMA(data.Close, timeperiod=20)}

        self.risk_percentage = 2.0
        self.stop_loss_scale = 1.5
        self.scaling_out_scale = 1.0
        self.scaling_out_percentage = 50.0
        self.scaling_out_protection = 1.0
        self.trailing_stop_loss_scale = 1.5
        self.window_size = 100

        self.symbol_pip_size = None
        self.current_atr_value = None

    def create_risk_management(self):
        machine = Machine("Risk Management", self.symbol, self.timeframe, self.logger)

        start = machine.create_state(name="No Position", end=False)
        waiting_so = machine.create_state(name="Waiting SO", end=False)
        waiting_tsl = machine.create_state(name="Waiting TSL", end=False)
        waiting_close = machine.create_state(name="Waiting Close", end=False)
        end = machine.create_state(name="End", end=True)

        start.on_opened_buy(action=self.define_so_buy_action, to=waiting_so, reason="Opened Buy Position")
        start.on_opened_sell(action=self.define_so_sell_action, to=waiting_so, reason="Opened Sell Position")
        start.on_shutdown(action=None, to=end, reason="Terminated")

        waiting_so.on_closed_buy(action=None, to=start, reason="Closed Buy Position")
        waiting_so.on_closed_sell(action=None, to=start, reason="Closed Sell Position")
        waiting_so.on_bid_above_target(action=self.close_partially_action, to=waiting_so, reason=None)
        waiting_so.on_ask_below_target(action=self.close_partially_action, to=waiting_so, reason=None)
        waiting_so.on_modified_buy_volume(action=self.breakeven_buy_action, to=waiting_so, reason="Closed Partially Position")
        waiting_so.on_modified_sell_volume(action=self.breakeven_sell_action, to=waiting_so, reason="Closed Partially Position")
        waiting_so.on_modified_buy_stop_loss(action=self.define_tsl_buy_action, to=waiting_tsl, reason="Moved Position to Break-Even")
        waiting_so.on_modified_sell_stop_loss(action=self.define_tsl_sell_action, to=waiting_tsl, reason="Moved Position to Break-Even")
        waiting_so.on_shutdown(action=None, to=end, reason="Terminated")

        waiting_tsl.on_closed_buy(action=None, to=start, reason="Closed Buy Position")
        waiting_tsl.on_closed_sell(action=None, to=start, reason="Closed Sell Position")
        waiting_tsl.on_bid_above_target(action=self.detected_tsl_buy_action, to=waiting_tsl, reason=None)
        waiting_tsl.on_ask_below_target(action=self.detected_tsl_sell_action, to=waiting_tsl, reason=None)
        waiting_tsl.on_modified_buy_stop_loss(action=self.define_tsl_buy_action, to=waiting_close, reason="Activated TSL")
        waiting_tsl.on_modified_sell_stop_loss(action=self.define_tsl_sell_action, to=waiting_close, reason="Activated TSL")
        waiting_tsl.on_shutdown(action=None, to=end, reason="Terminated")

        waiting_close.on_closed_buy(action=None, to=start, reason="Closed Buy Position")
        waiting_close.on_closed_sell(action=None, to=start, reason="Closed Sell Position")
        waiting_close.on_bid_above_target(action=self.detected_tsl_buy_action, to=waiting_close, reason=None)
        waiting_close.on_ask_below_target(action=self.detected_tsl_sell_action, to=waiting_close, reason=None)
        waiting_close.on_modified_buy_stop_loss(action=self.define_tsl_buy_action, to=waiting_close, reason=None)
        waiting_close.on_modified_sell_stop_loss(action=self.define_tsl_sell_action, to=waiting_close, reason=None)
        waiting_close.on_shutdown(action=None, to=end, reason="Terminated")

        return machine

    def define_so_buy_action(self, volume, entry, stop_loss, take_profit):
        return IdSend.BidAboveTarget.value, entry + self.scaling_out_scale * self.current_atr_value

    def define_so_sell_action(self, volume, entry, stop_loss, take_profit):
        return IdSend.AskBelowTarget.value, entry - self.scaling_out_scale * self.current_atr_value

    def close_partially_action(self, price):
        return IdSend.ModifyVolume.value, self.scaling_out_percentage

    def breakeven_buy_action(self, volume, entry, stop_loss, take_profit):
        return IdSend.ModifyStopLoss.value, entry + self.scaling_out_protection * self.symbol_pip_size

    def breakeven_sell_action(self, volume, entry, stop_loss, take_profit):
        return IdSend.ModifyStopLoss.value, entry - self.scaling_out_protection * self.symbol_pip_size

    def define_tsl_buy_action(self, volume, entry, stop_loss, take_profit):
        return IdSend.BidAboveTarget.value, stop_loss + self.trailing_stop_loss_scale * self.current_atr_value + self.symbol_pip_size

    def define_tsl_sell_action(self, volume, entry, stop_loss, take_profit):
        return IdSend.AskBelowTarget.value, stop_loss - self.trailing_stop_loss_scale * self.current_atr_value

    def detected_tsl_buy_action(self, bid):
        return IdSend.ModifyStopLoss.value, bid - self.trailing_stop_loss_scale * self.current_atr_value

    def detected_tsl_sell_action(self, ask):
        return IdSend.ModifyStopLoss.value, ask + self.trailing_stop_loss_scale * self.current_atr_value

    def create_signal_management(self):
        machine = Machine("Signal Management", self.symbol, self.timeframe, self.logger)

        start = machine.create_state(name="Start", end=False)
        trading = machine.create_state(name="Trading", end=False)
        end = machine.create_state(name="End", end=True)

        start.on_symbol(action=self.symbol_action, to=start, reason="Symbol Received")
        start.on_bar(action=self.append_data_action, to=start, reason=None)
        start.on_complete(action=self.prepare_data_action, to=trading, reason="Prepared")
        start.on_shutdown(action=self.save_data_action, to=end, reason="Error")

        trading.on_bar(action=self.process_signal_action, to=trading, reason=None)
        trading.on_shutdown(action=self.save_data_action, to=end, reason="Terminated")

        return machine

    def symbol_action(self, digits, pip_size, tick_size):
        self.symbol_pip_size = pip_size

    def append_data_action(self, date, open_price, high_price, low_price, close_price, volume):
        self.raw_dates.append(date)
        self.raw_open_prices.append(open_price)
        self.raw_high_prices.append(high_price)
        self.raw_low_prices.append(low_price)
        self.raw_close_prices.append(close_price)
        self.raw_volumes.append(volume)

    def prepare_data_action(self):
        raw_data = {"Date": self.raw_dates, "Open": self.raw_open_prices, "High": self.raw_high_prices, "Low": self.raw_low_prices, "Close": self.raw_close_prices, "Volume": self.raw_volumes}
        self.market_data =pd.DataFrame(raw_data).set_index("Date")
        self.db.save_data(self.market_data)
        self.market_data = self.db.load_data(start=self.raw_dates[0], end=self.raw_dates[-1], tail=self.window_size)
        self.indicator_data = offline_technical(self.market_data, self.indicators)

    def process_signal_action(self, date, open_price, high_price, low_price, close_price, volume):
        self.market_data.loc[date] = [open_price, high_price, low_price, close_price, volume]
        self.indicator_data.loc[date] = online_technical(self.market_data, self.indicators)
        previous = self.indicator_data.iloc[-2]
        current = self.indicator_data.iloc[-1]
        if current.SMA10 > current.SMA20 and previous.SMA10 < previous.SMA20:
            self.current_atr_value = current.ATR
            return IdSend.SignalBullishDynamic.value, self.risk_percentage, self.stop_loss_scale * self.current_atr_value / self.symbol_pip_size, None
        if current.SMA10 < current.SMA20 and previous.SMA10 > previous.SMA20:
            self.current_atr_value = current.ATR
            return IdSend.SignalBearishDynamic.value, self.risk_percentage, self.stop_loss_scale * self.current_atr_value / self.symbol_pip_size, None

    def save_data_action(self):
        self.db.save_data(self.market_data)
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
    strategy = NNFX(db, iid, symbol, timeframe, logger)
    strategy.run()


if __name__ == "__main__":
    main()
