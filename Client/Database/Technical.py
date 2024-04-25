import pandas as pd
import talib
import numpy as np

Indicators = {
    # ------ General Indicators ------
    "AVGPRICE": lambda lib, data: lib.AVGPRICE(data.Open, data.High, data.Low, data.Close),
    "MEDPRICE": lambda lib, data: lib.MEDPRICE(data.High, data.Low),
    "TYPPRICE": lambda lib, data: lib.TYPPRICE(data.High, data.Low, data.Close),
    "WCLPRICE": lambda lib, data: lib.WCLPRICE(data.High, data.Low, data.Close),
    "DIFF": lambda lib, data: lib.MOM(data.Close, timeperiod=1),
    "RET": lambda lib, data: lib.ROCR(data.Close, timeperiod=1),
    "LOGRET": lambda lib, data: np.log(lib.ROCR(data.Close, timeperiod=1)),

    # ------ Trend Indicators ------
    "SMA10": lambda lib, data: lib.SMA(data.Close, timeperiod=10),
    "SMA20": lambda lib, data: lib.SMA(data.Close, timeperiod=20),
    "SMA50": lambda lib, data: lib.SMA(data.Close, timeperiod=50),
    "SMA100": lambda lib, data: lib.SMA(data.Close, timeperiod=100),

    "EMA10": lambda lib, data: lib.EMA(data.Close, timeperiod=10),
    "EMA20": lambda lib, data: lib.EMA(data.Close, timeperiod=20),
    "EMA50": lambda lib, data: lib.EMA(data.Close, timeperiod=50),
    "EMA100": lambda lib, data: lib.EMA(data.Close, timeperiod=100),

    "DEMA10": lambda lib, data: lib.DEMA(data.Close, timeperiod=10),
    "DEMA20": lambda lib, data: lib.DEMA(data.Close, timeperiod=20),
    "DEMA50": lambda lib, data: lib.DEMA(data.Close, timeperiod=50),
    "DEMA100": lambda lib, data: lib.DEMA(data.Close, timeperiod=100),

    "TEMA10": lambda lib, data: lib.TEMA(data.Close, timeperiod=10),
    "TEMA20": lambda lib, data: lib.TEMA(data.Close, timeperiod=20),
    "TEMA50": lambda lib, data: lib.TEMA(data.Close, timeperiod=50),
    "TEMA100": lambda lib, data: lib.TEMA(data.Close, timeperiod=100),

    "TRIMA10": lambda lib, data: lib.TRIMA(data.Close, timeperiod=10),
    "TRIMA20": lambda lib, data: lib.TRIMA(data.Close, timeperiod=20),
    "TRIMA50": lambda lib, data: lib.TRIMA(data.Close, timeperiod=50),
    "TRIMA100": lambda lib, data: lib.TRIMA(data.Close, timeperiod=100),

    "WMA10": lambda lib, data: lib.WMA(data.Close, timeperiod=10),
    "WMA20": lambda lib, data: lib.WMA(data.Close, timeperiod=20),
    "WMA50": lambda lib, data: lib.WMA(data.Close, timeperiod=50),
    "WMA100": lambda lib, data: lib.WMA(data.Close, timeperiod=100),

    "KAMA10": lambda lib, data: lib.KAMA(data.Close, timeperiod=10),
    "KAMA20": lambda lib, data: lib.KAMA(data.Close, timeperiod=20),
    "KAMA50": lambda lib, data: lib.KAMA(data.Close, timeperiod=50),
    "KAMA100": lambda lib, data: lib.KAMA(data.Close, timeperiod=100),

    "MAMA": lambda lib, data: lib.MAMA(data.Close, fastlimit=0.5, slowlimit=0.05)[0],
    "FAMA": lambda lib, data: lib.MAMA(data.Close, fastlimit=0.5, slowlimit=0.05)[1],

    # ------ Momentum Indicators ------
    "DX": lambda lib, data: lib.DX(data.High, data.Low, data.Close, timeperiod=14),
    "ADX": lambda lib, data: lib.ADX(data.High, data.Low, data.Close, timeperiod=14),
    "ADXR": lambda lib, data: lib.ADXR(data.High, data.Low, data.Close, timeperiod=14),
    "APO": lambda lib, data: lib.APO(data.Close, fastperiod=12, slowperiod=26),
    "AROONOSC": lambda lib, data: lib.AROONOSC(data.High, data.Low, timeperiod=25),
    "BBOSC": lambda lib, data: BBOSC(lib, data, timeperiod=20, nbdevup=2, nbdevdn=2),
    "BOP": lambda lib, data: lib.BOP(data.Open, data.High, data.Low, data.Close),
    "CCI": lambda lib, data: lib.CCI(data.High, data.Low, data.Close, timeperiod=20),
    "CMO": lambda lib, data: lib.CMO(data.Close, timeperiod=14),
    "MACDOSC": lambda lib, data: lib.MACD(data.Close, fastperiod=12, slowperiod=26, signalperiod=9)[2],
    "MFI": lambda lib, data: lib.MFI(data.High, data.Low, data.Close, data.Volume, timeperiod=14),
    "MOM": lambda lib, data: lib.MOM(data.Close, timeperiod=10),
    "SAR": lambda lib, data: lib.SAR(data.High, data.Low, acceleration=0.02, maximum=0.2),
    "RSI": lambda lib, data: lib.RSI(data.Close, timeperiod=14),
    "STOCHRSIOSC": lambda lib, data: STOCHRSIOSC(lib, data, timeperiod=14, fastk_period=5, fastd_period=3),
    "STOCHOSC": lambda lib, data: STOCHOSC(lib, data, fastk_period=5, slowk_period=3, slowd_period=3),
    "STOCHFOSC": lambda lib, data: STOCHFOSC(lib, data, fastk_period=5, fastd_period=3),
    "TRIX": lambda lib, data: lib.TRIX(data.Close, timeperiod=30),
    "TSF": lambda lib, data: lib.TSF(data.Close, timeperiod=14),
    "ULTOSC": lambda lib, data: lib.ULTOSC(data.High, data.Low, data.Close, timeperiod1=7, timeperiod2=14, timeperiod3=28),
    "WILLR": lambda lib, data: lib.WILLR(data.High, data.Low, data.Close, timeperiod=14),

    # ------ Volume Indicators ------
    "AD": lambda lib, data: lib.AD(data.High, data.Low, data.Close, data.Volume),
    "ADOSC": lambda lib, data: lib.ADOSC(data.High, data.Low, data.Close, data.Volume, fastperiod=3, slowperiod=10),
    "OBV": lambda lib, data: lib.OBV(data.Close, data.Volume),

    # ------ Volatility Indicators ------
    "ATR": lambda lib, data: lib.ATR(data.High, data.Low, data.Close, timeperiod=14),
    "STDDEV": lambda lib, data: lib.STDDEV(data.Close, timeperiod=5, nbdev=1),
    "VAR": lambda lib, data: lib.VAR(data.Close, timeperiod=5, nbdev=1),
}


def BBOSC(lib, data, timeperiod, nbdevup, nbdevdn):
    upper_band, middle_band, lower_band = lib.BBANDS(data.Close, timeperiod=timeperiod, nbdevup=nbdevup, nbdevdn=nbdevdn)
    return (upper_band - lower_band) / middle_band


def STOCHRSIOSC(lib, data, timeperiod, fastk_period, fastd_period):
    fastk, fastd = lib.STOCHRSI(data.Close, timeperiod=timeperiod, fastk_period=fastk_period, fastd_period=fastd_period)
    return fastk - fastd


def STOCHOSC(lib, data, fastk_period, slowk_period, slowd_period):
    slowk, slowd = lib.STOCH(data.High, data.Low, data.Close, fastk_period=fastk_period, slowk_period=slowk_period, slowd_period=slowd_period)
    return slowk - slowd


def STOCHFOSC(lib, data, fastk_period, fastd_period):
    fastk, fastd = lib.STOCHF(data.High, data.Low, data.Close, fastk_period=fastk_period, fastd_period=fastd_period)
    return fastk - fastd


def offline_technical(market_data, indicators):
    indicator_data = pd.DataFrame(index=market_data.index)
    for name, indicator in indicators.items():
        indicator_data[name] = indicator(talib, market_data)
    return indicator_data


def online_technical(market_data, indicators):
    indicator_data = []
    for name, indicator in indicators.items():
        indicator_data.append(indicator(talib.stream, market_data))
    return indicator_data
