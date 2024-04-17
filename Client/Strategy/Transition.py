class Transition:

    def __init__(self, trigger, action, state):
        self.trigger = trigger
        self.action = action
        self.state = state

    def validate_trigger(self, *args):
        return self.trigger is None or self.trigger(*args)

    def perform_action(self, *args):
        return self.action is None or self.action(*args)
