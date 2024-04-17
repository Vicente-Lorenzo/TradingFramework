using System;
using cAlgo.API;

namespace cAlgo.Robots;

[Robot(AccessRights = AccessRights.FullAccess)]
public class NNFX : Robot
{
    private Strategy _strategy;

    private const string GeneralGroup = "General Settings";

    [Parameter("Verbose Level", DefaultValue = Logger.Verbose.Debug, Group = GeneralGroup)]
    public Logger.Verbose Verbose { get; set; }

    protected override void OnStart()
    {
        _strategy = new NNFXStrategy(this, Verbose);
        _strategy.OnStart();
    }

    protected override void OnError(Error error) { _strategy.OnError(error); }

    protected override void OnException(Exception exception) { _strategy.OnException(exception); }

    protected override void OnStop() { _strategy.OnShutdown(); }
}

public class NNFXStrategy : Strategy
{
    public NNFXStrategy(Robot robot, Logger.Verbose verbose) : base(robot, verbose) { }

    private bool UpdateMarketPosition(Position position, TradeType? direction, double volume)
    {
        if (position is null)
        {
            if (direction is TradeType.Buy)
                return Robot.ExecuteMarketOrder(TradeType.Buy, Robot.SymbolName, volume, Robot.InstanceId).IsSuccessful;
            if (direction is TradeType.Sell)
                return Robot.ExecuteMarketOrder(TradeType.Sell, Robot.SymbolName, volume, Robot.InstanceId).IsSuccessful;
            return true;
        }
        if (position.TradeType is TradeType.Buy)
        {
            if (direction is null)
                return position.Close().IsSuccessful;
            if (direction is TradeType.Sell)
                return position.Close().IsSuccessful && Robot.ExecuteMarketOrder(TradeType.Sell, Robot.SymbolName, volume, Robot.InstanceId).IsSuccessful;
            return true;
        }
        if (position.TradeType is TradeType.Sell)
        {
            if (direction is null)
                return position.Close().IsSuccessful;
            if (direction is TradeType.Buy)
                return position.Close().IsSuccessful && Robot.ExecuteMarketOrder(TradeType.Buy, Robot.SymbolName, volume, Robot.InstanceId).IsSuccessful;
            return true;
        }
        return false;
    }
}