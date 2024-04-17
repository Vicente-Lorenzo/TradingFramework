import os
import pandas as pd


class Database:

    def __init__(self, name, symbol, timeframe, logger):
        self.name = name
        self.symbol = symbol
        self.timeframe = timeframe
        self.logger = logger

        self.folder_path = f"{os.path.dirname(os.path.abspath(__file__))}\\Data\\{self.symbol}\\{self.timeframe}"
        self.file_path = f"{self.folder_path}\\{self.name}.h5"
        os.makedirs(self.folder_path, exist_ok=True)

    def save_data(self, data: pd.DataFrame):
        with pd.HDFStore(self.file_path, complevel=9, mode="a") as store:
            existing_count = 0
            saving_count = len(data)
            if self.name in store:
                existing_data = store[self.name]
                existing_count = len(existing_data)
                data = pd.concat([existing_data, data])
            data = data.reset_index().drop_duplicates(subset="Date", keep="last").set_index("Date").sort_index()
            store.put(self.name, data, format="table", data_columns=True)
            total_count = len(data)
            saved_count = total_count - existing_count
            updated_count = saving_count - saved_count
            self.logger.info(f"Saved data to {self.symbol} ({self.timeframe}) database [Saved: {saved_count} | Updated: {updated_count} | Total: {total_count}]")

    def load_data(self, start=None, end=None, head=None, tail=None):
        with pd.HDFStore(self.file_path, complevel=9, mode="r") as store:
            data = store[self.name].loc[start:end].iloc[-tail if tail else None:head]
            self.logger.info(f"Loaded data from {self.symbol} ({self.timeframe}) database [Loaded rows: {len(data)}]")
            return data

    def clean_data(self):
        temp_path = f"{self.file_path}_temp"
        with pd.HDFStore(self.file_path, complevel=9, mode="r") as store, pd.HDFStore(temp_path, complevel=9, mode="w") as temp_store:
            for key in store.keys():
                temp_store[key] = store[key]
        os.remove(self.file_path)
        os.rename(temp_path, self.file_path)
        self.logger.info(f"Cleaned and flushed data in {self.symbol} ({self.timeframe}) database")
