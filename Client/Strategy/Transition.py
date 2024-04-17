class Transition:

    def __init__(self, trigger, action, to_state):
        self.trigger = trigger
        self.action = action
        self.to_state = to_state

    def validate_trigger(self, *args):
        return self.trigger is None or self.trigger(*args)

    def perform_action(self, *args):
        return self.action(*args) if self.action is not None else None
