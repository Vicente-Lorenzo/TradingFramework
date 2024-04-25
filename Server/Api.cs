using System;
using System.IO;
using System.IO.Pipes;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public class Api
{
    private readonly string _symbol;
    private readonly string _timeframe;
    private readonly Logger _logger;
    private NamedPipeServerStream _pipe;

    private enum IdSend
    {
        Shutdown = 0,
        Complete = 1,
        Account = 2,
        Symbol = 3,
        OpenedBuy = 4,
        OpenedSell = 5,
        ModifiedBuyVolume = 6,
        ModifiedBuyStopLoss = 7,
        ModifiedBuyTakeProfit = 8,
        ModifiedSellVolume = 9,
        ModifiedSellStopLoss = 10,
        ModifiedSellTakeProfit = 11,
        ClosedBuy = 12,
        ClosedSell = 13,
        Bar = 14,
        AskAboveTarget = 15,
        AskBelowTarget = 16,
        BidAboveTarget = 17,
        BidBelowTarget = 18
    }

    public enum IdReceive
    {
        Complete = 0,
        SignalBullishFixed = 1,
        SignalBullishDynamic = 2,
        SignalSideways = 3,
        SignalBearishFixed = 4,
        SignalBearishDynamic = 5,
        ModifyVolume = 6,
        ModifyStopLoss = 7,
        ModifyTakeProfit = 8,
        AskAboveTarget = 9,
        AskBelowTarget = 10,
        BidAboveTarget = 11,
        BidBelowTarget = 12
    }

    private const double Sentinel = -1.0;

    public Api(string symbol, string timeframe, Logger logger)
    {
        _symbol = symbol;
        _timeframe = timeframe;
        _logger = logger;
    }

    public void Initialize()
    {
        _pipe = new NamedPipeServerStream(
            $"{_symbol}/{_timeframe}",
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
        _logger.Info($"API {_symbol} {_timeframe}: Pipe initialized");
    }

    public void Connect()
    {
        _pipe.WaitForConnection();
        _logger.Info($"API {_symbol} {_timeframe}: Connected");
    }

    public void Disconnect()
    {
        if (_pipe is null) return;
        _pipe.Close();
        _pipe.Dispose();
        _logger.Info($"API {_symbol} {_timeframe}: Disconnected");
    }

    private void Pack(byte[] message)
    {
        _pipe.Write(message, 0, message.Length);
        _pipe.Flush();
    }

    public void PackShutdown()
    {
        Pack(new[] {(byte)IdSend.Shutdown});
    }

    public void PackComplete()
    {
        Pack(new[] {(byte)IdSend.Complete});
    }

    public void PackAccount(IAccount account)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);
        writer.Write((byte)IdSend.Account);
        writer.Write(account.Balance);
        writer.Write(account.Equity);
        Pack(memoryStream.ToArray());
    }

    public void PackSymbol(Symbol symbol)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);
        writer.Write((byte)IdSend.Symbol);
        writer.Write(symbol.Digits);
        writer.Write(symbol.PipSize);
        writer.Write(symbol.TickSize);
        Pack(memoryStream.ToArray());
    }

    private void PackPosition(IdSend positionType, Position position)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);
        writer.Write((byte)positionType);
        writer.Write(position.VolumeInUnits);
        writer.Write(position.EntryPrice);
        writer.Write(position.StopLoss ?? Sentinel);
        writer.Write(position.TakeProfit ?? Sentinel);
        Pack(memoryStream.ToArray());
    }

    public void PackOpenedBuy(Position position) { PackPosition(IdSend.OpenedBuy, position); }
    public void PackOpenedSell(Position position) { PackPosition(IdSend.OpenedSell, position); }
    public void PackModifiedBuyVolume(Position position) { PackPosition(IdSend.ModifiedBuyVolume, position); }
    public void PackModifiedBuyStopLoss(Position position) { PackPosition(IdSend.ModifiedBuyStopLoss, position); }
    public void PackModifiedBuyTakeProfit(Position position) { PackPosition(IdSend.ModifiedBuyTakeProfit, position); }
    public void PackModifiedSellVolume(Position position) { PackPosition(IdSend.ModifiedSellVolume, position); }
    public void PackModifiedSellStopLoss(Position position) { PackPosition(IdSend.ModifiedSellStopLoss, position); }
    public void PackModifiedSellTakeProfit(Position position) { PackPosition(IdSend.ModifiedSellTakeProfit, position); }
    public void PackClosedBuy(Position position) { PackPosition(IdSend.ClosedBuy, position); }
    public void PackClosedSell(Position position) { PackPosition(IdSend.ClosedSell, position); }

    public void PackBar(Bar bar)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);
        writer.Write((byte)IdSend.Bar);
        writer.Write(((DateTimeOffset)bar.OpenTime).ToUnixTimeMilliseconds());
        writer.Write(bar.Open);
        writer.Write(bar.High);
        writer.Write(bar.Low);
        writer.Write(bar.Close);
        writer.Write(bar.TickVolume);
        Pack(memoryStream.ToArray());
    }

    private void PackTarget(IdSend targetType, double target)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);
        writer.Write((byte)targetType);
        writer.Write(target);
        Pack(memoryStream.ToArray());
    }

    public void PackAskAboveTarget(double ask) { PackTarget(IdSend.AskAboveTarget, ask); }
    public void PackAskBelowTarget(double ask) { PackTarget(IdSend.AskBelowTarget, ask); }
    public void PackBidAboveTarget(double bid) { PackTarget(IdSend.BidAboveTarget, bid); }
    public void PackBidBelowTarget(double bid) { PackTarget(IdSend.BidBelowTarget, bid); }

    private byte[] Unpack(int size)
    {
        var buffer = new byte[size];
        _ = _pipe.Read(buffer, 0, buffer.Length);
        return buffer;
    }

    public IdReceive UnpackHeader() { return (IdReceive)Unpack(sizeof(byte))[0]; }

    public (double, double?, double?) UnpackSignalFixed()
    {
        var content = Unpack(3 * sizeof(double));
        var volume = BitConverter.ToDouble(content, 0 * sizeof(double));
        var slAux = BitConverter.ToDouble(content, 1 * sizeof(double));
        var tpAux = BitConverter.ToDouble(content, 2 * sizeof(double));
        double? slPips = slAux, tpPips = tpAux;
        if (Math.Abs(slAux - Sentinel) < double.Epsilon) slPips = null;
        if (Math.Abs(tpAux - Sentinel) < double.Epsilon) tpPips = null;
        return (volume, slPips, tpPips);
    }

    public (double, double, double?) UnpackSignalDynamic()
    {
        var content = Unpack(3 * sizeof(double));
        var volume = BitConverter.ToDouble(content, 0 * sizeof(double));
        var slPips = BitConverter.ToDouble(content, 1 * sizeof(double));
        var tpAux = BitConverter.ToDouble(content, 2 * sizeof(double));
        double? tpPips = tpAux;
        if (Math.Abs(tpAux - Sentinel) < double.Epsilon) tpPips = null;
        return (volume, slPips, tpPips);
    }

    public double UnpackObligatoryValue()
    {
        var content = Unpack(1 * sizeof(double));
        var volume = BitConverter.ToDouble(content, 0 * sizeof(double));
        return volume;
    }

    public double? UnpackOptionalValue()
    {
        var content = Unpack(1 * sizeof(double));
        var limitAux = BitConverter.ToDouble(content, 0 * sizeof(double));
        double? limit = limitAux;
        if (Math.Abs(limitAux - Sentinel) < double.Epsilon) limit = null;
        return limit;
    }
}