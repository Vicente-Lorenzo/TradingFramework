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
    ModifyVolume = 4
    ModifyStopLoss = 5
    ModifyTakeProfit = 6


class IdReceive(Enum):
    Shutdown = 0
    Complete = 1
    Account = 2
    Symbol = 3
    OpenedBuy = 4
    OpenedSell = 5
    ModifiedVolume = 6
    ModifiedStopLoss = 7
    ModifiedTakeProfit = 8
    ClosedBuy = 9
    ClosedSell = 10
    Bar = 12
    Tick = 13


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
        self.__pack(struct.pack("<1b", IdSend.Complete.value))

    def pack_bullish_signal(self, volume, sl_pips, tp_pips):
        self.__pack(struct.pack("<1b3d", IdSend.BullishSignal.value, volume, sl_pips, tp_pips))

    def pack_sideways_signal(self):
        self.__pack(struct.pack("<1b", IdSend.SidewaysSignal.value))

    def pack_bearish_signal(self, volume, sl_pips, tp_pips):
        self.__pack(struct.pack("<1b3d", IdSend.BearishSignal.value, volume, sl_pips, tp_pips))

    def pack_modify_volume(self, pid, volume):
        self.__pack(struct.pack("<1b1i1d", IdSend.ModifyVolume.value, pid, volume))

    def pack_modify_stop_loss(self, pid, sl_price):
        self.__pack(struct.pack("<1b1i1d", IdSend.ModifyStopLoss.value, pid, sl_price))

    def pack_modify_take_profit(self, pid, tp_price):
        self.__pack(struct.pack("<1b1i1d", IdSend.ModifyTakeProfit.value, pid, tp_price))

    def __unpack(self, size):
        buffer = win32file.AllocateReadBuffer(struct.calcsize(size))
        _, content = win32file.ReadFile(self.pipe, buffer)
        return struct.unpack(size, content)

    def unpack_header(self):
        return self.__unpack("<1b")[0]

    def unpack_account(self):
        content = self.__unpack(size="<2d")
        return content[0], content[1]

    def unpack_symbol(self):
        content = self.__unpack(size="<1i2d")
        return content[0], content[1], content[2]

    def unpack_position(self):
        content = self.__unpack(size="<1i1b4d")
        sl = content[3] if not -1 else None
        tp = content[4] if not -1 else None
        return content[0], content[1], content[2], sl, tp, content[5]

    def unpack_bar(self):
        content = self.__unpack(size="<1q4d1q")
        date = datetime.fromtimestamp(content[0] / 1000.0, tz=timezone.utc)
        return date, content[1], content[2], content[3], content[4], content[5]

    def unpack_tick(self):
        content = self.__unpack(size="<2d")
        return content[0], content[1]
