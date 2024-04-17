using System;
using cAlgo.API;

namespace cAlgo.Robots;

public abstract class Strategy
{
    protected readonly Robot Robot;
    protected readonly Logger Logger;
    protected readonly Api Api;

    protected Strategy(Robot robot, Logger.Verbose verbose)
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
        Api.Connect();
    }

    public virtual void OnStart() { }

    protected virtual void OnPositionOpened(PositionOpenedEventArgs position) { }

    protected virtual void OnPositionModified(PositionModifiedEventArgs position) { }

    protected virtual void OnPositionClosed(PositionClosedEventArgs position) { }

    protected virtual void OnBarOpened(BarOpenedEventArgs args) { }

    protected virtual void OnBarClosed(BarClosedEventArgs args) { }

    protected virtual void OnTick(SymbolTickEventArgs args) { }

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
        Api.PackShutdown();
        Api.Disconnect();
    }

    public object ReceiveMessage()
    {
        var call = Api.UnpackHeader();
        switch (call)
        {
            case Api.IdReceive.Shutdown:
                throw new Exception();
            case Api.IdReceive.Complete:
                return null;
            case Api.IdReceive.BullishSignal:
                return (Api.MarketDirection.Bullish, Api.UnpackMarket());
            case Api.IdReceive.SidewaysSignal:
                return Api.MarketDirection.Sideways;
            case Api.IdReceive.BearishSignal:
                return (Api.MarketDirection.Bearish, Api.UnpackMarket());
            default:
                Logger.Error($"Received an unexpected message from the client: {call}");
                throw new Exception();
        }
    }
}