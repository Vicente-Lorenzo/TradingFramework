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
        ModifiedVolume = 6,
        ModifiedStopLoss = 7,
        ModifiedTakeProfit = 8,
        ClosedBuy = 9,
        ClosedSell = 10,
        Bar = 12,
        Tick = 13
    }

    public enum IdReceive
    {
        Complete = 0,
        BullishSignal = 1,
        SidewaysSignal = 2,
        BearishSignal = 3,
        ModifyVolume = 4,
        ModifyStopLoss = 5,
        ModifyTakeProfit = 6
    }

    public enum MarketDirection
    {
        Bullish = 1,
        Bearish = -1,
        Sideways = 0
    }

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
        writer.Write(position.Id);
        writer.Write((sbyte)(position.TradeType == TradeType.Buy ? MarketDirection.Bullish : MarketDirection.Bearish));
        writer.Write(position.EntryPrice);
        writer.Write(position.StopLoss ?? -1);
        writer.Write(position.TakeProfit ?? -1);
        writer.Write(position.VolumeInUnits);
        Pack(memoryStream.ToArray());
    }

    public void PackOpenedBuy(Position position) { PackPosition(IdSend.OpenedBuy, position); }

    public void PackOpenedSell(Position position) { PackPosition(IdSend.OpenedSell, position); }

    public void PackModifiedVolume(Position position) { PackPosition(IdSend.ModifiedVolume, position); }

    public void PackModifiedStopLoss(Position position) { PackPosition(IdSend.ModifiedStopLoss, position); }

    public void PackModifiedTakeProfit(Position position) { PackPosition(IdSend.ModifiedTakeProfit, position); }

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

    public void PackTick(double ask, double bid)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);
        writer.Write((byte)IdSend.Tick);
        writer.Write(ask);
        writer.Write(bid);
        Pack(memoryStream.ToArray());
    }

    private byte[] Unpack(int size)
    {
        var buffer = new byte[size];
        _ = _pipe.Read(buffer, 0, buffer.Length);
        return buffer;
    }

    public IdReceive UnpackHeader()
    {
        return (IdReceive)Unpack(sizeof(byte))[0];
    }

    public (double, double, double) UnpackSignal()
    {
        var content = Unpack(3 * sizeof(double));
        var volume = BitConverter.ToDouble(content, 0 * sizeof(double));
        var slPips = BitConverter.ToDouble(content, 1 * sizeof(double));
        var tpPips = BitConverter.ToDouble(content, 2 * sizeof(double));
        return (volume, slPips, tpPips);
    }

    public (int, double) UnpackModify()
    {
        var content = Unpack(sizeof(int) + sizeof(double));
        var pid = BitConverter.ToInt32(content, 0 * sizeof(int));
        var value = BitConverter.ToDouble(content, 1 * sizeof(int));
        return (pid, value);
    }
}