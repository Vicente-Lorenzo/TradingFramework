from .State import State
from .Transition import Transition


class Machine:

    def __init__(self, symbol, timeframe, logger):
        self.symbol = symbol
        self.timeframe = timeframe
        self.logger = logger

        self.at_state = None
        self.states: list[State] = []

    def create_state(self):
        state = State()
        self.states.append(state)
        self.at_state = state if not self.at_state else self.at_state
        return state

    def __call(self, transition: Transition, *args):
        if transition is not None and transition.validate_trigger(*args):
            self.at_state = transition.to_state
            return transition.perform_action(*args)

    def call_shutdown(self):
        return self.__call(self.at_state.shutdown_transition)

    def call_complete(self):
        return self.__call(self.at_state.complete_transition)

    def call_account(self, account):
        return self.__call(self.at_state.account_transition, account)

    def call_symbol(self, symbol):
        return self.__call(self.at_state.symbol_transition, symbol)

    def call_position_opened(self, position):
        return self.__call(self.at_state.position_opened_transition, position)

    def call_position_modified(self, position):
        return self.__call(self.at_state.position_modified_transition, position)

    def call_position_closed(self, position):
        return self.__call(self.at_state.position_closed_transition, position)

    def call_bar_opened(self, bar):
        return self.__call(self.at_state.bar_opened_transition, bar)

    def call_bar_closed(self, bar):
        return self.__call(self.at_state.bar_closed_transition, bar)

    def call_tick(self, tick):
        return self.__call(self.at_state.tick_transition, tick)
