from .State import State
from .Transition import Transition


class Machine:

    def __init__(self, name, symbol, timeframe, logger):
        self.name = name
        self.symbol = symbol
        self.timeframe = timeframe
        self.logger = logger

        self.at = None
        self.states: list[State] = []

    def create_state(self, name, end):
        state = State(name, end)
        self.states.append(state)
        self.at = state if not self.at else self.at
        return state

    def __call(self, transition: Transition, *args):
        if transition is not None and transition.validate_trigger(*args):
            ret = transition.perform_action(*args)
            if transition.reason is not None:
                self.logger.info(f"Machine {self.name}: [{self.at.name}] > {transition.reason} > [{transition.to.name}]")
            self.at = transition.to
            return ret

    def call_shutdown(self):
        return self.__call(self.at.shutdown_transition)

    def call_complete(self):
        return self.__call(self.at.complete_transition)

    def call_account(self, account):
        return self.__call(self.at.account_transition, account)

    def call_symbol(self, symbol):
        return self.__call(self.at.symbol_transition, symbol)

    def call_opened_buy(self, position):
        return self.__call(self.at.opened_buy_transition, position)

    def call_opened_sell(self, position):
        return self.__call(self.at.opened_sell_transition, position)

    def call_modified_volume(self, position):
        return self.__call(self.at.modified_volume_transition, position)

    def call_modified_stop_loss(self, position):
        return self.__call(self.at.modified_stop_loss_transition, position)

    def call_modified_take_profit(self, position):
        return self.__call(self.at.modified_take_profit_transition, position)

    def call_closed_buy(self, position):
        return self.__call(self.at.closed_buy_transition, position)

    def call_closed_sell(self, position):
        return self.__call(self.at.closed_sell_transition, position)

    def call_bar(self, bar):
        return self.__call(self.at.bar_transition, bar)

    def call_tick(self, tick):
        return self.__call(self.at.tick_transition, tick)
