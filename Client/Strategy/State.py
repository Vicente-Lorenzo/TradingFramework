from .Transition import Transition


class State:

    def __init__(self, name, end):
        self.name = name
        self.end = end

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
    
    def on_shutdown(self, trigger, action, to, reason):
        self.shutdown_transition = Transition(trigger, action, to, reason)

    def on_complete(self, trigger, action, to, reason):
        self.complete_transition = Transition(trigger, action, to, reason)

    def on_account(self, trigger, action, to, reason):
        self.account_transition = Transition(trigger, action, to, reason)

    def on_symbol(self, trigger, action, to, reason):
        self.symbol_transition = Transition(trigger, action, to, reason)

    def on_position_opened(self, trigger, action, to, reason):
        self.position_opened_transition = Transition(trigger, action, to, reason)

    def on_position_modified(self, trigger, action, to, reason):
        self.position_modified_transition = Transition(trigger, action, to, reason)

    def on_position_closed(self, trigger, action, to, reason):
        self.position_closed_transition = Transition(trigger, action, to, reason)

    def on_bar_opened(self, trigger, action, to, reason):
        self.bar_opened_transition = Transition(trigger, action, to, reason)

    def on_bar_closed(self, trigger, action, to, reason):
        self.bar_closed_transition = Transition(trigger, action, to, reason)

    def on_tick(self, trigger, action, to, reason):
        self.tick_transition = Transition(trigger, action, to, reason)
