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
        PositionOpened = 4,
        PositionModified = 5,
        PositionClosed = 6,
        BarOpened = 7,
        BarClosed = 8,
        Tick = 9,
    }

    public enum IdReceive
    {
        Complete = 0,
        BullishSignal = 1,
        SidewaysSignal = 2,
        BearishSignal = 3,
        ModifyPosition = 4,
    }

    public enum MarketDirection
    {
        Bullish = 1,
        Bearish = -1,
        Sideways = 0,
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
        _logger.Info("Connection pipe initialized successfully");
    }

    public void Connect()
    {
        _pipe.WaitForConnection();
        _logger.Info($"Connected to the client at {_symbol} ({_timeframe})");
    }

    public void Disconnect()
    {
        if (_pipe is null) return;
        _pipe.Close();
        _pipe.Dispose();
        _logger.Info($"Disconnected from the server at {_symbol} ({_timeframe})");
    }

    private void Pack(byte[] message)
    {
        if (_pipe is null) return;
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
        writer.Write(account.Credit);
        writer.Write(account.Equity);
        writer.Write(account.Margin);
        writer.Write(account.FreeMargin);
        writer.Write(account.PreciseLeverage);
        Pack(memoryStream.ToArray());
    }

    public void PackSymbol(Symbol symbol)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);
        writer.Write((byte)IdSend.Symbol);
        writer.Write(symbol.Commission);
        writer.Write(symbol.Digits);
        writer.Write(symbol.LotSize);
        writer.Write(symbol.PipSize);
        writer.Write(symbol.TickSize);
        writer.Write(symbol.VolumeInUnitsMin);
        writer.Write(symbol.VolumeInUnitsMax);
        writer.Write(symbol.VolumeInUnitsStep);
        Pack(memoryStream.ToArray());
    }

    private void PackPosition(IdSend positionType, Position position)
    {
        if (position is null) { PackComplete(); }
        else
        {
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);
            writer.Write((byte)positionType);
            writer.Write(((DateTimeOffset)position.EntryTime).ToUnixTimeMilliseconds());
            writer.Write((byte)position.TradeType);
            writer.Write(position.VolumeInUnits);
            writer.Write(position.Quantity);
            writer.Write(position.EntryPrice);
            writer.Write(position.CurrentPrice);
            writer.Write(position.StopLoss ?? -1);
            writer.Write(position.TakeProfit ?? -1);
            writer.Write(position.Pips);
            writer.Write(position.GrossProfit);
            writer.Write(position.Commissions);
            writer.Write(position.Swap);
            writer.Write(position.NetProfit);
            writer.Write(position.Margin);
            Pack(memoryStream.ToArray());
        }
    }

    public void PackPositionOpened(Position position) { PackPosition(IdSend.PositionOpened, position); }

    public void PackPositionModified(Position position) { PackPosition(IdSend.PositionModified, position); }

    public void PackPositionClosed(Position position) { PackPosition(IdSend.PositionClosed, position); }

    private void PackBar(IdSend barType, Bar bar)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);
        writer.Write((byte)barType);
        writer.Write(((DateTimeOffset)bar.OpenTime).ToUnixTimeMilliseconds());
        writer.Write(bar.Open);
        writer.Write(bar.High);
        writer.Write(bar.Low);
        writer.Write(bar.Close);
        writer.Write(bar.TickVolume);
        Pack(memoryStream.ToArray());
    }

    public void PackBarOpened(Bar bar) { PackBar(IdSend.BarOpened, bar); }

    public void PackBarClosed(Bar bar) { PackBar(IdSend.BarClosed, bar); }

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

    public (double, double, double) UnpackPosition()
    {
        var content = Unpack(3 * sizeof(double));
        var volume = BitConverter.ToDouble(content, 0);
        var slPrice = BitConverter.ToDouble(content, 1);
        var tpPrice = BitConverter.ToDouble(content, 2);
        return (volume, slPrice, tpPrice);
    }
}