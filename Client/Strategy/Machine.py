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
            if transition.reason is not None:
                self.logger.info(f"Machine {self.name}: [{self.at.name}] > {transition.reason} > [{transition.to.name}]")
            self.at = transition.to
            return transition.perform_action(*args)

    def call_shutdown(self):
        return self.__call(self.at.shutdown_transition)

    def call_complete(self):
        return self.__call(self.at.complete_transition)

    def call_account(self, account):
        return self.__call(self.at.account_transition, account)

    def call_symbol(self, symbol):
        return self.__call(self.at.symbol_transition, symbol)

    def call_position_opened(self, position):
        return self.__call(self.at.position_opened_transition, position)

    def call_position_modified(self, position):
        return self.__call(self.at.position_modified_transition, position)

    def call_position_closed(self, position):
        return self.__call(self.at.position_closed_transition, position)

    def call_bar_opened(self, bar):
        return self.__call(self.at.bar_opened_transition, bar)

    def call_bar_closed(self, bar):
        return self.__call(self.at.bar_closed_transition, bar)

    def call_tick(self, tick):
        return self.__call(self.at.tick_transition, tick)
