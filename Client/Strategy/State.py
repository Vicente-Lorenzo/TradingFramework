from .Transition import Transition


class State:

    def __init__(self):
        self.shutdown_transitions = []
        self.complete_transitions = []
        self.account_transitions = []
        self.symbol_transitions = []
        self.position_opened_transitions = []
        self.position_modified_transitions = []
        self.position_closed_transitions = []
        self.bar_opened_transitions = []
        self.bar_closed_transitions = []
        self.tick_transitions = []
    
    def on_shutdown(self, trigger, action, state):
        self.shutdown_transitions.append(Transition(trigger, action, state))

    def on_complete(self, trigger, action, state):
        self.complete_transitions.append(Transition(trigger, action, state))

    def on_account(self, trigger, action, state):
        self.account_transitions.append(Transition(trigger, action, state))

    def on_symbol(self, trigger, action, state):
        self.symbol_transitions.append(Transition(trigger, action, state))

    def on_position_opened(self, trigger, action, state):
        self.position_opened_transitions.append(Transition(trigger, action, state))

    def on_position_modified(self, trigger, action, state):
        self.position_modified_transitions.append(Transition(trigger, action, state))

    def on_position_closed(self, trigger, action, state):
        self.position_closed_transitions.append(Transition(trigger, action, state))

    def on_bar_opened(self, trigger, action, state):
        self.bar_opened_transitions.append(Transition(trigger, action, state))

    def on_bar_closed(self, trigger, action, state):
        self.bar_closed_transitions.append(Transition(trigger, action, state))

    def on_tick(self, trigger, action, state):
        self.tick_transitions.append(Transition(trigger, action, state))
