import pandas as pd

from .Agent import Agent


class TrendStrategy(Agent):

    def __init__(self, api, db, logger):
        super().__init__(api, db, logger)
        self.data = []
        self.atr = None

    def start(self):
        while bar := self.api.receive_message():
            self.data.append(bar)
        self.data = pd.DataFrame(self.data).set_index("Date")
        self.api.send_complete_message()

    def tick(self, tick):
        position = self.api.receive_message()
        if position:
            pass

    def bar(self, bar):
        pass

    def stop(self):
        pass
