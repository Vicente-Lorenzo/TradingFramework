from .Transition import Transition


class State:

    def __init__(self, name, end):
        self.name = name
        self.end = end

        self.shutdown_transition = None
        self.complete_transition = None
        self.account_transition = None
        self.symbol_transition = None
        self.opened_buy_transition = None
        self.opened_sell_transition = None
        self.modified_volume_transition = None
        self.modified_stop_loss_transition = None
        self.modified_take_profit_transition = None
        self.closed_buy_transition = None
        self.closed_sell_transition = None
        self.bar_transition = None
        self.tick_transition = None
    
    def on_shutdown(self, trigger, action, to, reason):
        self.shutdown_transition = Transition(trigger, action, to, reason)

    def on_complete(self, trigger, action, to, reason):
        self.complete_transition = Transition(trigger, action, to, reason)

    def on_account(self, trigger, action, to, reason):
        self.account_transition = Transition(trigger, action, to, reason)

    def on_symbol(self, trigger, action, to, reason):
        self.symbol_transition = Transition(trigger, action, to, reason)

    def on_opened_buy(self, trigger, action, to, reason):
        self.opened_buy_transition = Transition(trigger, action, to, reason)

    def on_opened_sell(self, trigger, action, to, reason):
        self.opened_sell_transition = Transition(trigger, action, to, reason)

    def on_modified_volume(self, trigger, action, to, reason):
        self.modified_volume_transition = Transition(trigger, action, to, reason)

    def on_modified_stop_loss(self, trigger, action, to, reason):
        self.modified_stop_loss_transition = Transition(trigger, action, to, reason)

    def on_modified_take_profit(self, trigger, action, to, reason):
        self.modified_take_profit_transition = Transition(trigger, action, to, reason)

    def on_closed_buy(self, trigger, action, to, reason):
        self.closed_buy_transition = Transition(trigger, action, to, reason)

    def on_closed_sell(self, trigger, action, to, reason):
        self.closed_sell_transition = Transition(trigger, action, to, reason)

    def on_bar(self, trigger, action, to, reason):
        self.bar_transition = Transition(trigger, action, to, reason)

    def on_tick(self, trigger, action, to, reason):
        self.tick_transition = Transition(trigger, action, to, reason)
