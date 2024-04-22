using System;
using System.IO;
using System.Diagnostics;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public abstract class Strategy
{
    private readonly Api _api;
    private readonly Logger _logger;
    private readonly bool _global;

    protected readonly Robot Robot;

    private double _lastPositionVolume;
    private double _lastPositionStopLoss;
    private double _lastPositionTakeProfit;

    private void CallShutdown() { _api.PackShutdown(); HandleCallback(); }

    private void CallComplete() { _api.PackComplete(); HandleCallback(); }

    private void CallAccount(IAccount account) { _api.PackAccount(account); HandleCallback(); }

    private void CallSymbol(Symbol symbol) { _api.PackSymbol(symbol); HandleCallback(); }

    private void CallOpenedBuy(Position position) { _api.PackOpenedBuy(position); HandleCallback(); }

    private void CallOpenedSell(Position position) { _api.PackOpenedSell(position); HandleCallback(); }

    private void CallModifiedVolume(Position position) { _api.PackModifiedVolume(position); HandleCallback(); }

    private void CallModifiedStopLoss(Position position) { _api.PackModifiedStopLoss(position); HandleCallback(); }

    private void CallModifiedTakeProfit(Position position) { _api.PackModifiedTakeProfit(position); HandleCallback(); }

    private void CallClosedBuy(Position position) { _api.PackClosedBuy(position); HandleCallback(); }

    private void CallClosedSell(Position position) { _api.PackClosedSell(position); HandleCallback(); }

    private void CallBar(Bar bar) { _api.PackBar(bar); HandleCallback(); }

    private void CallTick(double ask, double bid) { _api.PackTick(ask, bid); HandleCallback(); }

    private void HandleCallback()
    {
        var call = _api.UnpackHeader();
        int pid;
        double volume, sl, tp;
        switch (call)
        {
            case Api.IdReceive.Complete:
                break;
            case Api.IdReceive.BullishSignal:
                (volume, sl, tp) = _api.UnpackSignal();
                OnBullishSignal(volume, sl, tp);
                break;
            case Api.IdReceive.SidewaysSignal:
                OnSidewaysSignal();
                break;
            case Api.IdReceive.BearishSignal:
                (volume, sl, tp) = _api.UnpackSignal();
                OnBearishSignal(volume, sl, tp);
                break;
            case Api.IdReceive.ModifyVolume:
                (pid, volume) = _api.UnpackModify();
                OnModifyVolume(pid, volume);
                break;
            case Api.IdReceive.ModifyStopLoss:
                (pid, sl) = _api.UnpackModify();
                OnModifyStopLoss(pid, sl);
                break;
            case Api.IdReceive.ModifyTakeProfit:
                (pid, tp) = _api.UnpackModify();
                OnModifyTakeProfit(pid, tp);
                break;
        }
    }

    protected Strategy(Robot robot, Logger.Verbose verbose, bool global)
    {
        _logger = new Logger(robot, verbose);
        _global = global;
        Robot = robot;

        Robot.Positions.Opened += OnPositionOpened;
        Robot.Positions.Modified += OnPositionModified;
        Robot.Positions.Closed += OnPositionClosed;
        Robot.Bars.BarClosed += OnBar;
        Robot.Symbol.Tick += OnTick;

        _api = new Api(Robot.SymbolName, Robot.TimeFrame.Name, _logger);
        _api.Initialize();

        var baseDirectory = new DirectoryInfo(Environment.CurrentDirectory).Parent?.Parent?.Parent?.FullName;
        var scriptName = GetType().Name;
        var scriptPath = $@"{baseDirectory}\Sources\Client\{scriptName}.py";
        var scriptArgs = $"--verbose {verbose} --symbol {Robot.SymbolName} --timeframe {Robot.TimeFrame.Name}";
        var tabTitle = $"{scriptName} {Robot.SymbolName} {Robot.TimeFrame.Name}";
        var command = $"cmd.exe /k \"conda activate quant && python \"{scriptPath}\" {scriptArgs}\"";
        Process.Start("wt.exe", $"--window 0 new-tab --title \"{tabTitle}\" {command}");

        _api.Connect();

        CallAccount(Robot.Account);
        CallSymbol(Robot.Symbol);
        for (var i = 0; i < Robot.Bars.Count-1; i++) { CallBar(Robot.Bars[i]); }
        CallComplete();
    }

    private void OnPositionOpened(PositionOpenedEventArgs args)
    {
        if (!_global && !string.Equals(args.Position.Label, Robot.InstanceId)) return;
        if (args.Position.TradeType == TradeType.Buy)
            CallOpenedBuy(args.Position);
        else
            CallOpenedSell(args.Position);
    }

    private void OnPositionModified(PositionModifiedEventArgs args)
    {
        if (!_global && !string.Equals(args.Position.Label, Robot.InstanceId)) return;
        if (Math.Abs(args.Position.VolumeInUnits - _lastPositionVolume) > double.Epsilon)
        {
            _lastPositionVolume = args.Position.VolumeInUnits;
            CallModifiedVolume(args.Position);
        }
        if (args.Position.StopLoss != null && Math.Abs((double)args.Position.StopLoss - _lastPositionStopLoss) > double.Epsilon)
        {
            _lastPositionStopLoss = (double)args.Position.StopLoss;
            CallModifiedStopLoss(args.Position);
        }
        if (args.Position.TakeProfit != null && Math.Abs((double)args.Position.TakeProfit - _lastPositionTakeProfit) > double.Epsilon)
        {
            _lastPositionTakeProfit = (double)args.Position.TakeProfit;
            CallModifiedTakeProfit(args.Position);
        }
    }

    private void OnPositionClosed(PositionClosedEventArgs args)
    {
        if (!_global && !string.Equals(args.Position.Label, Robot.InstanceId)) return;
        if (args.Position.TradeType == TradeType.Buy)
            CallClosedBuy(args.Position);
        else
            CallClosedSell(args.Position);
    }

    private void OnBar(BarClosedEventArgs args)
    {
        CallBar(args.Bars.LastBar);
    }

    private void OnTick(SymbolTickEventArgs args)
    {
        CallTick(args.Ask, args.Bid);
    }

    public void OnError(Error error)
    {
        _logger.Error("An unexpected error occured in the server execution");
        _logger.Error(error.TradeResult.ToString());
        Robot.Stop();
    }

    public void OnException(Exception exception)
    {
        _logger.Error("An unexpected exception occured in the server execution");
        _logger.Error(exception.ToString());
        Robot.Stop();
    }

    public void OnShutdown()
    {
        _logger.Warning("Shutdown strategy and safely terminate operations");
        CallShutdown();
        _api.Disconnect();
    }

    protected virtual void OnBullishSignal(double volume, double slPips, double tpPips) { }

    protected virtual void OnSidewaysSignal() { }

    protected virtual void OnBearishSignal(double volume, double slPips, double tpPips) { }

    protected virtual void OnModifyVolume(int pid, double volume) { }

    protected virtual void OnModifyStopLoss(int pid, double slPrice) { }

    protected virtual void OnModifyTakeProfit(int pid, double tpPrice) { }
}