from abc import ABC

from .Api import API, IdReceive, IdSend
from .Machine import Machine


class Strategy(ABC):

    def __init__(self, symbol, timeframe, logger):
        self.symbol = symbol
        self.timeframe = timeframe
        self.logger = logger

        self.signal_machine: Machine = self.create_signal_management()
        self.risk_machine: Machine = self.create_risk_management()

    def run(self):
        with API(self.symbol, self.timeframe, self.logger) as self.api:
            while not (self.risk_machine.at.end and self.signal_machine.at.end):
                call = self.api.unpack_header()
                match call:
                    case IdReceive.Shutdown.value:
                        self.logger.warning("Shutdown strategy and safely terminate operations")
                        callback, *callback_args = self.__call_shutdown()
                    case IdReceive.Complete.value:
                        callback, *callback_args = self.__call_complete()
                    case IdReceive.Account.value:
                        callback, *callback_args = self.__call_account(*self.api.unpack_account())
                    case IdReceive.Symbol.value:
                        callback, *callback_args = self.__call_symbol(*self.api.unpack_symbol())
                    case IdReceive.OpenedBuy.value:
                        callback, *callback_args = self.__call_opened_buy(*self.api.unpack_position())
                    case IdReceive.OpenedSell.value:
                        callback, *callback_args = self.__call_opened_sell(*self.api.unpack_position())
                    case IdReceive.ModifiedBuyVolume.value:
                        callback, *callback_args = self.__call_modified_buy_volume(*self.api.unpack_position())
                    case IdReceive.ModifiedBuyStopLoss.value:
                        callback, *callback_args = self.__call_modified_buy_stop_loss(*self.api.unpack_position())
                    case IdReceive.ModifiedBuyTakeProfit.value:
                        callback, *callback_args = self.__call_modified_buy_take_profit(*self.api.unpack_position())
                    case IdReceive.ModifiedSellVolume.value:
                        callback, *callback_args = self.__call_modified_sell_volume(*self.api.unpack_position())
                    case IdReceive.ModifiedSellStopLoss.value:
                        callback, *callback_args = self.__call_modified_sell_stop_loss(*self.api.unpack_position())
                    case IdReceive.ModifiedSellTakeProfit.value:
                        callback, *callback_args = self.__call_modified_sell_take_profit(*self.api.unpack_position())
                    case IdReceive.ClosedBuy.value:
                        callback, *callback_args = self.__call_closed_buy(*self.api.unpack_position())
                    case IdReceive.ClosedSell.value:
                        callback, *callback_args = self.__call_closed_sell(*self.api.unpack_position())
                    case IdReceive.Bar.value:
                        callback, *callback_args = self.__call_bar(*self.api.unpack_bar())
                    case IdReceive.AskAboveTarget.value:
                        callback, *callback_args = self.__call_ask_above_target(self.api.unpack_target())
                    case IdReceive.AskBelowTarget.value:
                        callback, *callback_args = self.__call_ask_below_target(self.api.unpack_target())
                    case IdReceive.BidAboveTarget.value:
                        callback, *callback_args = self.__call_bid_above_target(self.api.unpack_target())
                    case IdReceive.BidBelowTarget.value:
                        callback, *callback_args = self.__call_bid_below_target(self.api.unpack_target())
                match callback:
                    case IdSend.Complete.value:
                        self.api.pack_complete()
                    case IdSend.SignalBullishFixed.value:
                        self.api.pack_signal_bullish_fixed(*callback_args)
                    case IdSend.SignalBullishDynamic.value:
                        self.api.pack_signal_bullish_dynamic(*callback_args)
                    case IdSend.SignalSideways.value:
                        self.api.pack_signal_sideways()
                    case IdSend.SignalBearishFixed.value:
                        self.api.pack_signal_bearish_fixed(*callback_args)
                    case IdSend.SignalBearishDynamic.value:
                        self.api.pack_signal_bearish_dynamic(*callback_args)
                    case IdSend.ModifyVolume.value:
                        self.api.pack_modify_volume(*callback_args)
                    case IdSend.ModifyStopLoss.value:
                        self.api.pack_modify_stop_loss(*callback_args)
                    case IdSend.ModifyTakeProfit.value:
                        self.api.pack_modify_take_profit(*callback_args)
                    case IdSend.AskAboveTarget.value:
                        self.api.pack_ask_above_target(*callback_args)
                    case IdSend.AskBelowTarget.value:
                        self.api.pack_ask_below_target(*callback_args)
                    case IdSend.BidAboveTarget.value:
                        self.api.pack_bid_above_target(*callback_args)
                    case IdSend.BidBelowTarget.value:
                        self.api.pack_bid_below_target(*callback_args)

    def __create_dummy_machine(self):
        machine = Machine(None, self.symbol, self.timeframe, self.logger)
        machine.create_state(None, True)
        return machine

    def create_risk_management(self):
        return self.__create_dummy_machine()

    def create_signal_management(self):
        return self.__create_dummy_machine()

    @staticmethod
    def __call(signal_call, risk_call, *call_args):
        signal_return = signal_call(*call_args)
        risk_return = risk_call(*call_args)
        if signal_return is not None:
            callback, *callback_args = signal_return
            return callback, *callback_args
        if risk_return is not None:
            callback, *callback_args = risk_return
            return callback, *callback_args
        return IdSend.Complete.value, None

    def __call_shutdown(self):
        return self.__call(self.signal_machine.call_shutdown, self.risk_machine.call_shutdown)

    def __call_complete(self):
        return self.__call(self.signal_machine.call_complete, self.risk_machine.call_complete)

    def __call_account(self, *account):
        return self.__call(self.signal_machine.call_account, self.risk_machine.call_account, *account)

    def __call_symbol(self, *symbol):
        return self.__call(self.signal_machine.call_symbol, self.risk_machine.call_symbol, *symbol)

    def __call_opened_buy(self, *position):
        return self.__call(self.signal_machine.call_opened_buy, self.risk_machine.call_opened_buy, *position)

    def __call_opened_sell(self, *position):
        return self.__call(self.signal_machine.call_opened_sell, self.risk_machine.call_opened_sell, *position)

    def __call_modified_buy_volume(self, *position):
        return self.__call(self.signal_machine.call_modified_buy_volume, self.risk_machine.call_modified_buy_volume, *position)

    def __call_modified_buy_stop_loss(self, *position):
        return self.__call(self.signal_machine.call_modified_buy_stop_loss, self.risk_machine.call_modified_buy_stop_loss, *position)

    def __call_modified_buy_take_profit(self, *position):
        return self.__call(self.signal_machine.call_modified_buy_take_profit, self.risk_machine.call_modified_buy_take_profit, *position)

    def __call_modified_sell_volume(self, *position):
        return self.__call(self.signal_machine.call_modified_sell_volume, self.risk_machine.call_modified_sell_volume, *position)

    def __call_modified_sell_stop_loss(self, *position):
        return self.__call(self.signal_machine.call_modified_sell_stop_loss, self.risk_machine.call_modified_sell_stop_loss, *position)

    def __call_modified_sell_take_profit(self, *position):
        return self.__call(self.signal_machine.call_modified_sell_take_profit, self.risk_machine.call_modified_sell_take_profit, *position)

    def __call_closed_buy(self, *position):
        return self.__call(self.signal_machine.call_closed_buy, self.risk_machine.call_closed_buy, *position)

    def __call_closed_sell(self, *position):
        return self.__call(self.signal_machine.call_closed_sell, self.risk_machine.call_closed_sell, *position)

    def __call_bar(self, *bar):
        return self.__call(self.signal_machine.call_bar, self.risk_machine.call_bar, *bar)

    def __call_ask_above_target(self, *target):
        return self.__call(self.signal_machine.call_ask_above_target, self.risk_machine.call_ask_above_target, *target)

    def __call_ask_below_target(self, *target):
        return self.__call(self.signal_machine.call_ask_below_target, self.risk_machine.call_ask_below_target, *target)

    def __call_bid_above_target(self, *target):
        return self.__call(self.signal_machine.call_bid_above_target, self.risk_machine.call_bid_above_target, *target)

    def __call_bid_below_target(self, *target):
        return self.__call(self.signal_machine.call_bid_below_target, self.risk_machine.call_bid_below_target, *target)
