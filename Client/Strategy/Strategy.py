from abc import ABC, abstractmethod

from .Api import API, IdReceive, IdSend
from .Machine import Machine


class Strategy(ABC):

    def __init__(self, symbol, timeframe, logger):
        self.symbol = symbol
        self.timeframe = timeframe
        self.logger = logger

        self.money_machine: Machine = self.create_money_management()
        self.signal_machine: Machine = self.create_signal_management()
        self.risk_machine: Machine = self.create_risk_management()

    def run(self):
        with API(self.symbol, self.timeframe, self.logger) as self.api:
            while not (self.risk_machine.at.end and self.signal_machine.at.end):
                call = self.api.unpack_header()
                match call:
                    case IdReceive.Shutdown.value:
                        self.logger.warning("Shutdown strategy and safely terminate operations")
                        callback = self.__call_shutdown()
                    case IdReceive.Complete.value:
                        callback = self.__call_complete()
                    case IdReceive.Account.value:
                        callback = self.__call_account(self.api.unpack_account())
                    case IdReceive.Symbol.value:
                        callback = self.__call_symbol(self.api.unpack_symbol())
                    case IdReceive.PositionOpened.value:
                        callback = self.__call_position_opened(self.api.unpack_position())
                    case IdReceive.PositionModified.value:
                        callback = self.__call_position_modified(self.api.unpack_position())
                    case IdReceive.PositionClosed.value:
                        callback = self.__call_position_closed(self.api.unpack_position())
                    case IdReceive.BarOpened.value:
                        callback = self.__call_bar_opened(self.api.unpack_bar())
                    case IdReceive.BarClosed.value:
                        callback = self.__call_bar_closed(self.api.unpack_bar())
                    case IdReceive.Tick.value:
                        callback = self.__call_tick(self.api.unpack_tick())
                match callback:
                    case IdSend.Complete:
                        self.api.pack_complete()
                    case IdSend.BullishSignal:
                        self.api.pack_bullish_market(self.volume, self.sl_price, self.tp_price)
                    case IdSend.SidewaysSignal:
                        self.api.pack_sideways_market()
                    case IdSend.BearishSignal:
                        self.api.pack_bearish_market(self.volume, self.sl_price, self.tp_price)
                    case IdSend.ModifyPosition:
                        self.api.pack_modify_position(self.volume, self.sl_price, self.tp_price)

    def __create_dummy_machine(self):
        machine = Machine(None, self.symbol, self.timeframe, self.logger)
        machine.create_state(None, True)
        return machine

    def create_money_management(self):
        return self.__create_dummy_machine()

    def create_risk_management(self):
        return self.__create_dummy_machine()

    def create_signal_management(self):
        return self.__create_dummy_machine()

    @staticmethod
    def __call(signal_call, risk_call, *args):
        signal_callback = signal_call(*args)
        risk_callback = risk_call(*args)
        if signal_callback is not None:
            return signal_callback
        if risk_callback is not None:
            return risk_callback
        return IdSend.Complete

    def __call_shutdown(self):
        return self.__call(self.signal_machine.call_shutdown, self.risk_machine.call_shutdown)

    def __call_complete(self):
        return self.__call(self.signal_machine.call_complete, self.risk_machine.call_complete)

    def __call_account(self, account):
        return self.__call(self.signal_machine.call_account, self.risk_machine.call_account, account)

    def __call_symbol(self, symbol):
        return self.__call(self.signal_machine.call_symbol, self.risk_machine.call_symbol, symbol)

    def __call_position_opened(self, position):
        return self.__call(self.signal_machine.call_position_opened, self.risk_machine.call_position_opened, position)

    def __call_position_modified(self, position):
        return self.__call(self.signal_machine.call_position_modified, self.risk_machine.call_position_modified, position)

    def __call_position_closed(self, position):
        return self.__call(self.signal_machine.call_position_closed, self.risk_machine.call_position_closed, position)

    def __call_bar_opened(self, bar):
        return self.__call(self.signal_machine.call_bar_opened, self.risk_machine.call_bar_opened, bar)

    def __call_bar_closed(self, bar):
        return self.__call(self.signal_machine.call_bar_closed, self.risk_machine.call_bar_closed, bar)

    def __call_tick(self, tick):
        return self.__call(self.signal_machine.call_tick, self.risk_machine.call_tick, tick)
