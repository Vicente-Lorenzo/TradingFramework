class Transition:

    def __init__(self, action, to, reason):
        self.action = action
        self.to = to
        self.reason = reason

    def perform_action(self, *args):
        return self.action(*args) if self.action is not None else None
