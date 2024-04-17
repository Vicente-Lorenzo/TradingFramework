from abc import ABC, abstractmethod

from .Api import API, IdReceive
from .Machine import Machine


class Strategy(ABC):

    def __init__(self, symbol, timeframe, logger):
        self.symbol = symbol
        self.timeframe = timeframe
        self.logger = logger

        self.risk_machine: Machine = self.create_risk_strategy()
        self.signal_machine: Machine = self.create_signal_strategy()

    def run(self):
        with API(self.symbol, self.timeframe, self.logger) as self.api:
            while True:
                call = self.api.unpack_header()
                match call:
                    case IdReceive.Shutdown.value:
                        self.logger.warning("Shutdown strategy and safely terminate operations")
                        self.__call_shutdown()
                        break
                    case IdReceive.Complete.value:
                        self.__call_complete()
                    case IdReceive.Account.value:
                        self.__call_account(self.api.unpack_account())
                    case IdReceive.Symbol.value:
                        self.__call_symbol(self.api.unpack_symbol())
                    case IdReceive.PositionOpened.value:
                        self.__call_position_opened(self.api.unpack_position())
                    case IdReceive.PositionModified.value:
                        self.__call_position_modified(self.api.unpack_position())
                    case IdReceive.PositionClosed.value:
                        self.__call_position_closed(self.api.unpack_position())
                    case IdReceive.BarOpened.value:
                        self.__call_bar_opened(self.api.unpack_bar())
                    case IdReceive.BarClosed.value:
                        self.__call_bar_closed(self.api.unpack_bar())
                    case IdReceive.Tick.value:
                        self.__call_tick(self.api.unpack_tick())
                    case _:
                        self.logger.error(f"Received an unexpected call from the server: {call}")
                        break

    @abstractmethod
    def create_risk_strategy(self):
        pass

    @abstractmethod
    def create_signal_strategy(self):
        pass

    def __call_shutdown(self):
        if self.risk_machine is not None:
            self.risk_machine.call_shutdown()
        if self.signal_machine is not None:
            self.signal_machine.call_shutdown()

    def __call_complete(self):
        if self.risk_machine is not None:
            self.risk_machine.call_complete()
        if self.signal_machine is not None:
            self.signal_machine.call_complete()

    def __call_account(self, account):
        if self.risk_machine is not None:
            self.risk_machine.call_account(account)
        if self.signal_machine is not None:
            self.signal_machine.call_account(account)

    def __call_symbol(self, symbol):
        if self.risk_machine is not None:
            self.risk_machine.call_symbol(symbol)
        if self.signal_machine is not None:
            self.signal_machine.call_symbol(symbol)

    def __call_position_opened(self, position):
        if self.risk_machine is not None:
            self.risk_machine.call_position_opened(position)
        if self.signal_machine is not None:
            self.signal_machine.call_position_opened(position)

    def __call_position_modified(self, position):
        if self.risk_machine is not None:
            self.risk_machine.call_position_modified(position)
        if self.signal_machine is not None:
            self.signal_machine.call_position_modified(position)

    def __call_position_closed(self, position):
        if self.risk_machine is not None:
            self.risk_machine.call_position_closed(position)
        if self.signal_machine is not None:
            self.signal_machine.call_position_closed(position)

    def __call_bar_opened(self, bar):
        if self.risk_machine is not None:
            self.risk_machine.call_bar_opened(bar)
        if self.signal_machine is not None:
            self.signal_machine.call_bar_opened(bar)

    def __call_bar_closed(self, bar):
        if self.risk_machine is not None:
            self.risk_machine.call_bar_closed(bar)
        if self.signal_machine is not None:
            self.signal_machine.call_bar_closed(bar)

    def __call_tick(self, tick):
        if self.risk_machine is not None:
            self.risk_machine.call_tick(tick)
        if self.signal_machine is not None:
            self.signal_machine.call_tick(tick)
