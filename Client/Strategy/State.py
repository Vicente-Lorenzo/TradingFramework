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
        self.modified_buy_volume_transition = None
        self.modified_buy_stop_loss_transition = None
        self.modified_buy_take_profit_transition = None
        self.modified_sell_volume_transition = None
        self.modified_sell_stop_loss_transition = None
        self.modified_sell_take_profit_transition = None
        self.closed_buy_transition = None
        self.closed_sell_transition = None
        self.bar_transition = None
        self.ask_above_target_transition = None
        self.ask_below_target_transition = None
        self.bid_above_target_transition = None
        self.bid_below_target_transition = None

    def on_shutdown(self, action, to, reason):
        self.shutdown_transition = Transition(action, to, reason)

    def on_complete(self, action, to, reason):
        self.complete_transition = Transition(action, to, reason)

    def on_account(self, action, to, reason):
        self.account_transition = Transition(action, to, reason)

    def on_symbol(self, action, to, reason):
        self.symbol_transition = Transition(action, to, reason)

    def on_opened_buy(self, action, to, reason):
        self.opened_buy_transition = Transition(action, to, reason)

    def on_opened_sell(self, action, to, reason):
        self.opened_sell_transition = Transition(action, to, reason)

    def on_modified_buy_volume(self, action, to, reason):
        self.modified_buy_volume_transition = Transition(action, to, reason)

    def on_modified_buy_stop_loss(self, action, to, reason):
        self.modified_buy_stop_loss_transition = Transition(action, to, reason)

    def on_modified_buy_take_profit(self, action, to, reason):
        self.modified_buy_take_profit_transition = Transition(action, to, reason)

    def on_modified_sell_volume(self, action, to, reason):
        self.modified_sell_volume_transition = Transition(action, to, reason)

    def on_modified_sell_stop_loss(self, action, to, reason):
        self.modified_sell_stop_loss_transition = Transition(action, to, reason)

    def on_modified_sell_take_profit(self, action, to, reason):
        self.modified_sell_take_profit_transition = Transition(action, to, reason)

    def on_closed_buy(self, action, to, reason):
        self.closed_buy_transition = Transition(action, to, reason)

    def on_closed_sell(self, action, to, reason):
        self.closed_sell_transition = Transition(action, to, reason)

    def on_bar(self, action, to, reason):
        self.bar_transition = Transition(action, to, reason)

    def on_ask_above_target(self, action, to, reason):
        self.ask_above_target_transition = Transition(action, to, reason)

    def on_ask_below_target(self, action, to, reason):
        self.ask_below_target_transition = Transition(action, to, reason)

    def on_bid_above_target(self, action, to, reason):
        self.bid_above_target_transition = Transition(action, to, reason)

    def on_bid_below_target(self, action, to, reason):
        self.bid_below_target_transition = Transition(action, to, reason)
