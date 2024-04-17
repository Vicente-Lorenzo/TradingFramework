from .Transition import Transition


class State:

    def __init__(self):
        self.shutdown_transition = None
        self.complete_transition = None
        self.account_transition = None
        self.symbol_transition = None
        self.position_opened_transition = None
        self.position_modified_transition = None
        self.position_closed_transition = None
        self.bar_opened_transition = None
        self.bar_closed_transition = None
        self.tick_transition = None
    
    def on_shutdown(self, trigger, action, state):
        self.shutdown_transition = Transition(trigger, action, state)

    def on_complete(self, trigger, action, state):
        self.complete_transition = Transition(trigger, action, state)

    def on_account(self, trigger, action, state):
        self.account_transition = Transition(trigger, action, state)

    def on_symbol(self, trigger, action, state):
        self.symbol_transition = Transition(trigger, action, state)

    def on_position_opened(self, trigger, action, state):
        self.position_opened_transition = Transition(trigger, action, state)

    def on_position_modified(self, trigger, action, state):
        self.position_modified_transition = Transition(trigger, action, state)

    def on_position_closed(self, trigger, action, state):
        self.position_closed_transition = Transition(trigger, action, state)

    def on_bar_opened(self, trigger, action, state):
        self.bar_opened_transition = Transition(trigger, action, state)

    def on_bar_closed(self, trigger, action, state):
        self.bar_closed_transition = Transition(trigger, action, state)

    def on_tick(self, trigger, action, state):
        self.tick_transition = Transition(trigger, action, state)
