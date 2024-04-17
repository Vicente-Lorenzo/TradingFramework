import time

from backtesting import Backtest

SIZE = 52


class Optimisation:

    capital = 10000

    def __init__(self, role, data, strategy, indicators, metric):
        self.role = role
        self.data = data
        self.strategy = strategy
        self.indicators = indicators
        self.metric = metric

        self.passes = 0
        self.optimal_score = None
        self.optimal_stats = None
        self.optimal_backtest = None
        self.optimal_indicator = None
        self.optimal_parameters = None

    @staticmethod
    def unpack_indicator(indicator):
        return indicator["name"], indicator["series"], indicator["ranges"], indicator["buy_signal"], indicator["sell_signal"]

    @staticmethod
    def extract_parameters(stats, ranges):
        return {parameter: getattr(stats["_strategy"], parameter) for parameter in ranges.keys()}

    def update_optimal(self, score, backtest, stats, name, parameters):
        if not self.optimal_score or self.optimal_score < score:
            self.optimal_score = score
            self.optimal_stats = stats
            self.optimal_backtest = backtest
            self.optimal_indicator = name
            self.optimal_parameters = parameters

    def run(self):
        outer_start_time = time.time()

        self.print_optimisation_start()

        for indicator in self.indicators:

            inner_start_time = time.time()

            name, series, ranges, buy_signal, sell_signal = self.unpack_indicator(indicator)

            self.print_indicator_start(name)

            self.strategy.name = name
            self.strategy.series = series
            self.strategy.buy_signal = buy_signal
            self.strategy.sell_signal = buy_signal

            backtest = Backtest(data=self.data, strategy=self.strategy, cash=self.capital)
            stats, heatmap = backtest.optimize(**ranges, return_heatmap=True, maximize="SQN")
            score = self.metric(stats)

            self.passes += len(heatmap)

            parameters = self.extract_parameters(stats, ranges)

            self.update_optimal(score, backtest, stats, name, parameters)

            inner_end_time = time.time()

            self.print_indicator_finish(parameters, score, inner_end_time - inner_start_time)

        outer_end_time = time.time()

        self.print_optimisation_finish(outer_end_time - outer_start_time)

        return self.optimal_backtest, self.optimal_indicator, self.optimal_parameters

    @staticmethod
    def format_print(label, value):
        padding = SIZE - len(label) - len(str(value))
        return f"{label}{' ' * padding}{value}"

    @staticmethod
    def print_indicator_start(name):
        print(f" Optimising {name} ".center(SIZE, "-"))

    @staticmethod
    def print_indicator_finish(parameters, score, elapsed):
        print(Optimisation.format_print("Time Elapsed", f"{elapsed} secs"))
        print(Optimisation.format_print("Score", score))
        print("Parameters:".ljust(SIZE))
        for param, value in parameters.items():
            print(Optimisation.format_print(param, value))

    def print_optimisation_start(self):
        print(f" {self.role} Optimisation ".center(SIZE, "-"))

    def print_optimisation_finish(self, elapsed):
        print(" Optimisation Summary ".center(SIZE, "-"))
        print(self.format_print("Time Elapsed", f"{elapsed} secs"))
        print(self.format_print("Total Passes", self.passes))
        print(self.format_print("Optimal Score", self.optimal_score))
        print(self.format_print("Optimal Indicator", self.optimal_indicator))
        print("Optimal Parameters:".ljust(SIZE))
        for param, value in self.optimal_parameters.items():
            print(self.format_print(param, value))
        print("Optimal Statistics:".ljust(SIZE))
        stats = self.optimal_stats[[not key.startswith('_') for key in self.optimal_stats.index]]
        for key, value in stats.items():
            print(self.format_print(key, value))
        print("-" * SIZE)
