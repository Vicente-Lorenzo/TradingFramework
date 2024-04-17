
class Machine:

    def __init__(self, states, symbol, timeframe, logger):
        self.symbol = symbol
        self.timeframe = timeframe
        self.logger = logger

        self.current_state = states[0]
        self.possible_states = states

    def __call(self, transitions, *args):
        for transition in transitions:
            if transition.validate_trigger(*args):
                transition.perform_action(*args)
                self.current_state = transition.state
                break

    def call_shutdown(self):
        self.__call(self.current_state.shutdown_transitions)

    def call_complete(self):
        self.__call(self.current_state.complete_transitions)

    def call_account(self, account):
        self.__call(self.current_state.account_transitions, account)

    def call_symbol(self, symbol):
        self.__call(self.current_state.symbol_transitions, symbol)

    def call_position_opened(self, position):
        self.__call(self.current_state.position_opened_transitions, position)

    def call_position_modified(self, position):
        self.__call(self.current_state.position_modified_transitions, position)

    def call_position_closed(self, position):
        self.__call(self.current_state.position_closed_transitions, position)

    def call_bar_opened(self, bar):
        self.__call(self.current_state.bar_opened_transitions, bar)

    def call_bar_closed(self, bar):
        self.__call(self.current_state.bar_closed_transitions, bar)

    def call_tick(self, tick):
        self.__call(self.current_state.tick_transitions, tick)
