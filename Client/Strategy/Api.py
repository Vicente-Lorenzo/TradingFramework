import struct
import win32file
import pywintypes

from enum import Enum
from datetime import datetime, timezone


class IdSend(Enum):
    Complete = 0
    SignalBullishFixed = 1
    SignalBullishDynamic = 2
    SignalSideways = 3
    SignalBearishFixed = 4
    SignalBearishDynamic = 5
    ModifyVolume = 6
    ModifyStopLoss = 7
    ModifyTakeProfit = 8
    AskAboveTarget = 9
    AskBelowTarget = 10
    BidAboveTarget = 11
    BidBelowTarget = 12


class IdReceive(Enum):
    Shutdown = 0
    Complete = 1
    Account = 2
    Symbol = 3
    OpenedBuy = 4
    OpenedSell = 5
    ModifiedBuyVolume = 6
    ModifiedBuyStopLoss = 7
    ModifiedBuyTakeProfit = 8
    ModifiedSellVolume = 9
    ModifiedSellStopLoss = 10
    ModifiedSellTakeProfit = 11
    ClosedBuy = 12
    ClosedSell = 13
    Bar = 14
    AskAboveTarget = 15
    AskBelowTarget = 16
    BidAboveTarget = 17
    BidBelowTarget = 18


class MarketDirection(Enum):
    Bullish = 1
    Sideways = 0
    Bearish = -1


Sentinel = -1.0


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

    def pack_signal_bullish_fixed(self, volume, sl_pips, tp_pips):
        sl_pips = sl_pips if sl_pips is not None else Sentinel
        tp_pips = tp_pips if tp_pips is not None else Sentinel
        self.__pack(struct.pack("<1b3d", IdSend.SignalBullishFixed.value, volume, sl_pips, tp_pips))

    def pack_signal_bullish_dynamic(self, percentage, sl_pips, tp_pips):
        tp_pips = tp_pips if tp_pips is not None else Sentinel
        self.__pack(struct.pack("<1b3d", IdSend.SignalBullishDynamic.value, percentage, sl_pips, tp_pips))

    def pack_signal_sideways(self):
        self.__pack(struct.pack("<1b", IdSend.SignalSideways.value))

    def pack_signal_bearish_fixed(self, volume, sl_pips, tp_pips):
        sl_pips = sl_pips if sl_pips is not None else Sentinel
        tp_pips = tp_pips if tp_pips is not None else Sentinel
        self.__pack(struct.pack("<1b3d", IdSend.SignalBearishFixed.value, volume, sl_pips, tp_pips))

    def pack_signal_bearish_dynamic(self, volume, sl_pips, tp_pips):
        tp_pips = tp_pips if tp_pips is not None else Sentinel
        self.__pack(struct.pack("<1b3d", IdSend.SignalBearishDynamic.value, volume, sl_pips, tp_pips))

    def pack_modify_volume(self, volume):
        self.__pack(struct.pack("<1b1d", IdSend.ModifyVolume.value, volume))

    def pack_modify_stop_loss(self, sl_price):
        sl_price = sl_price if sl_price is not None else Sentinel
        self.__pack(struct.pack("<1b1d", IdSend.ModifyStopLoss.value, sl_price))

    def pack_modify_take_profit(self, tp_price):
        tp_price = tp_price if tp_price is not None else Sentinel
        self.__pack(struct.pack("<1b1d", IdSend.ModifyTakeProfit.value, tp_price))

    def pack_ask_above_target(self, target):
        target = target if target is not None else Sentinel
        self.__pack(struct.pack("<1b1d", IdSend.AskAboveTarget.value, target))

    def pack_ask_below_target(self, target):
        target = target if target is not None else Sentinel
        self.__pack(struct.pack("<1b1d", IdSend.AskBelowTarget.value, target))

    def pack_bid_above_target(self, target):
        target = target if target is not None else Sentinel
        self.__pack(struct.pack("<1b1d", IdSend.BidAboveTarget.value, target))

    def pack_bid_below_target(self, target):
        target = target if target is not None else Sentinel
        self.__pack(struct.pack("<1b1d", IdSend.BidBelowTarget.value, target))

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
        content = self.__unpack(size="<4d")
        sl = content[2]
        tp = content[3]
        sl = sl if sl is not Sentinel else None
        tp = tp if tp is not Sentinel else None
        return content[0], content[1], sl, tp,

    def unpack_bar(self):
        content = self.__unpack(size="<1q4d1q")
        date = datetime.fromtimestamp(content[0] / 1000.0, tz=timezone.utc)
        return date, content[1], content[2], content[3], content[4], content[5]

    def unpack_target(self):
        return self.__unpack(size="<1d")[0]
