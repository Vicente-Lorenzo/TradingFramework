import struct
import win32file
import pywintypes

from enum import Enum
from datetime import datetime, timezone


class IdSend(Enum):
    Complete = 0
    BullishSignal = 1
    SidewaysSignal = 2
    BearishSignal = 3
    ModifyPosition = 4


class IdReceive(Enum):
    Shutdown = 0
    Complete = 1
    Account = 2
    Symbol = 3
    PositionOpened = 4
    PositionModified = 5
    PositionClosed = 6
    BarOpened = 7
    BarClosed = 8
    Tick = 9


class MarketDirection(Enum):
    Bullish = 1
    Bearish = -1
    Sideways = 0


class API:
    def __init__(self, symbol, timeframe, logger):
        self.pipe = None
        self.symbol = symbol
        self.timeframe = timeframe
        self.logger = logger

    def __enter__(self):
        try:
            self.pipe = win32file.CreateFile(f"\\\\.\\pipe\\{self.symbol}\\{self.timeframe}",
                                             win32file.GENERIC_READ | win32file.GENERIC_WRITE, 0, None,
                                             win32file.OPEN_EXISTING, 0, None)
            self.logger.info(f"API {self.symbol} {self.timeframe}: Connected")
        except pywintypes.error as e:
            if e.winerror == 2:
                self.logger.error(f"API {self.symbol} {self.timeframe}: Unable to connect")
            elif e.winerror == 231:
                self.logger.error(f"API {self.symbol} {self.timeframe}: Another client is connected")
            raise
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        if self.pipe:
            win32file.CloseHandle(self.pipe)
            self.logger.info(f"API {self.symbol} {self.timeframe}: Disconnected")

    def __pack(self, message):
        win32file.WriteFile(self.pipe, message)

    def pack_complete(self):
        self.__pack(struct.pack("<b", IdSend.Complete.value))

    def pack_bullish_market(self, volume, sl_price, tp_price):
        self.__pack(struct.pack("<b3d", IdSend.BullishSignal.value, volume, sl_price, tp_price))

    def pack_sideways_market(self):
        self.__pack(struct.pack("<b", IdSend.SidewaysSignal.value))

    def pack_bearish_market(self, volume, sl_price, tp_price):
        self.__pack(struct.pack("<b3d", IdSend.BearishSignal.value, volume, sl_price, tp_price))

    def pack_modify_position(self, volume, sl_price, tp_price):
        self.__pack(struct.pack("<b3d", IdSend.ModifyPosition.value, volume, sl_price, tp_price))

    def __unpack(self, size):
        buffer = win32file.AllocateReadBuffer(struct.calcsize(size))
        _, content = win32file.ReadFile(self.pipe, buffer)
        return struct.unpack(size, content)

    def unpack_header(self):
        return self.__unpack("<b")[0]

    def unpack_account(self):
        content = self.__unpack(size="<6d")
        return {"Balance": content[0],
                "Credit": content[1],
                "Equity": content[2],
                "Margin": content[3],
                "FreeMargin": content[4],
                "PreciseLeverage": content[5]}

    def unpack_symbol(self):
        content = self.__unpack(size="<1d1i1q5d")
        return {"Commission": content[0],
                "Digits": content[1],
                "LotSize": content[2],
                "PipSize": content[3],
                "TickSize": content[4],
                "VolumeMin": content[5],
                "VolumeMax": content[6],
                "VolumeStep": content[7]}

    def unpack_position(self):
        content = self.__unpack(size="<1q1b12d")
        return {"Date": datetime.fromtimestamp(content[0] / 1000.0, tz=timezone.utc),
                "Type": content[1],
                "Volume": content[2],
                "Lots": content[3],
                "Entry": content[4],
                "Price": content[5],
                "StopLoss": content[6],
                "TakeProfit": content[7],
                "Pips": content[8],
                "GrossNPL": content[9],
                "Commission": content[10],
                "Swap": content[11],
                "NetNPL": content[12],
                "Margin": content[13]}

    def unpack_bar(self):
        content = self.__unpack(size="<1q4d1q")
        return {"Date": datetime.fromtimestamp(content[0] / 1000.0, tz=timezone.utc),
                "Open": content[1],
                "High": content[2],
                "Low": content[3],
                "Close": content[4],
                "Volume": content[5]}

    def unpack_tick(self):
        content = self.__unpack(size="<2d")
        return {"Ask": content[0],
                "Bid": content[1]}
