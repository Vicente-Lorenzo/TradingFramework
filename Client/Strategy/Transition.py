class Transition:

    def __init__(self, trigger, action, to, reason):
        self.trigger = trigger
        self.action = action
        self.to = to
        self.reason = reason

    def validate_trigger(self, *args):
        return self.trigger is None or self.trigger(*args)

    def perform_action(self, *args):
        return self.action(*args) if self.action is not None else (None, None)
