using System;
using System.IO;
using System.Diagnostics;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public abstract class Strategy
{
    public readonly Robot Robot;
    public readonly Logger Logger;
    public readonly Api Api;

    public Strategy(Robot robot, Logger.Verbose verbose)
    {
        Robot = robot;
        Logger = new Logger(robot, verbose);

        Robot.Positions.Opened += OnPositionOpened;
        Robot.Positions.Modified += OnPositionModified;
        Robot.Positions.Closed += OnPositionClosed;
        Robot.Bars.BarOpened += OnBarOpened;
        Robot.Bars.BarClosed += OnBarClosed;
        Robot.Symbol.Tick += OnTick;

        Api = new Api(Robot.SymbolName, Robot.TimeFrame.Name, Logger);
        Api.Initialize();

        var baseDirectory = new DirectoryInfo(Environment.CurrentDirectory).Parent?.Parent?.Parent?.FullName;
        var scriptName = GetType().Name;
        var scriptPath = $@"{baseDirectory}\Sources\Client\{scriptName}.py";
        var scriptArgs = $"--verbose {verbose} --symbol {Robot.SymbolName} --timeframe {Robot.TimeFrame.Name}";
        var tabTitle = $"{scriptName} {Robot.SymbolName} {Robot.TimeFrame.Name}";
        var command = $"cmd.exe /k \"conda activate quant && python \"{scriptPath}\" {scriptArgs}\"";
        Process.Start("wt.exe", $"--window 0 new-tab --title \"{tabTitle}\" {command}");

        Api.Connect();
    }

    public virtual void OnStart() { }

    public virtual void OnPositionOpened(PositionOpenedEventArgs position) { }

    public virtual void OnPositionModified(PositionModifiedEventArgs position) { }

    public virtual void OnPositionClosed(PositionClosedEventArgs position) { }

    public virtual void OnBarOpened(BarOpenedEventArgs args) { }

    public virtual void OnBarClosed(BarClosedEventArgs args) { }

    public virtual void OnTick(SymbolTickEventArgs args) { }

    public virtual void OnError(Error error)
    {
        Logger.Error("An unexpected error occured in the server execution");
        Logger.Error(error.TradeResult.ToString());
        Robot.Stop();
    }

    public virtual void OnException(Exception exception)
    {
        Logger.Error("An unexpected exception occured in the server execution");
        Logger.Error(exception.ToString());
        Robot.Stop();
    }

    public virtual void OnShutdown()
    {
        Logger.Warning("Shutdown strategy and safely terminate operations");
        CallShutdown();
        Api.Disconnect();
    }

    public virtual void OnBullishSignal(double volumeValue, double slValue, double tpValue) { }

    public virtual void OnSidewaysSignal() { }

    public virtual void OnBearishSignal(double volumeValue, double slValue, double tpValue) { }

    public virtual void OnModifyPosition(double volumeValue, double slValue, double tpValue) { }

    public void CallShutdown() { Api.PackShutdown(); HandleCallback(); }

    public void CallComplete() { Api.PackComplete(); HandleCallback(); }

    public void CallAccount(IAccount account) { Api.PackAccount(account); HandleCallback(); }

    public void CallSymbol(Symbol symbol) { Api.PackSymbol(symbol); HandleCallback(); }

    public void CallPositionOpened(Position position) { Api.PackPositionOpened(position); HandleCallback(); }

    public void CallPositionModified(Position position) { Api.PackPositionModified(position); HandleCallback(); }

    public void CallPositionClosed(Position position) { Api.PackPositionClosed(position); HandleCallback(); }

    public void CallBarOpened(Bar bar) { Api.PackBarOpened(bar); HandleCallback(); }

    public void CallBarClosed(Bar bar) { Api.PackBarClosed(bar); HandleCallback(); }

    public void CallTick(double ask, double bid) { Api.PackTick(ask, bid); HandleCallback(); }


    private void HandleCallback()
    {
        var call = Api.UnpackHeader();
        double volumeValue, slValue, tpValue;
        switch (call)
        {
            case Api.IdReceive.Complete:
                break;
            case Api.IdReceive.BullishSignal:
                (volumeValue, slValue, tpValue) = Api.UnpackPosition();
                OnBullishSignal(volumeValue, slValue, tpValue);
                break;
            case Api.IdReceive.SidewaysSignal:
                OnSidewaysSignal();
                break;
            case Api.IdReceive.BearishSignal:
                (volumeValue, slValue, tpValue) = Api.UnpackPosition();
                OnBearishSignal(volumeValue, slValue, tpValue);
                break;
            case Api.IdReceive.ModifyPosition:
                (volumeValue, slValue, tpValue) = Api.UnpackPosition();
                OnModifyPosition(volumeValue, slValue, tpValue);
                break;
        }
    }
}