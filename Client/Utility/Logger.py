import logging


class Logger:
    def __init__(self, verbose):
        self.verbose = verbose
        self.logger = self.create_logger()

    def create_logger(self):
        logger = logging.getLogger(__name__)
        logger.setLevel(self.verbose)
        console_handler = logging.StreamHandler()
        formatter = logging.Formatter('%(asctime)s - %(levelname)s - %(message)s')
        console_handler.setFormatter(formatter)
        logger.addHandler(console_handler)
        return logger

    def debug(self, message):
        self.logger.debug(message)

    def info(self, message):
        self.logger.info(message)

    def warning(self, message):
        self.logger.warning(message)

    def error(self, message):
        self.logger.error(message)
