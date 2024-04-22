using System;
using cAlgo.API;

namespace cAlgo.Robots;

[Robot(AccessRights = AccessRights.FullAccess)]
public class NNFXStrategy : Robot
{
    private Strategy _strategy;

    private const string GeneralGroup = "General Settings";

    [Parameter("Verbose", DefaultValue = Logger.Verbose.Debug, Group = GeneralGroup)]
    public Logger.Verbose Verbose { get; set; }


    protected override void OnStart() { _strategy = new NNFX(this, Verbose, false); }

    protected override void OnError(Error error) { _strategy.OnError(error); }

    protected override void OnException(Exception exception) { _strategy.OnException(exception); }

    protected override void OnStop() { _strategy.OnShutdown(); }
}

public class NNFX : Strategy
{
    public NNFX(Robot robot, Logger.Verbose verbose, bool global) : base(robot, verbose, global) { }

    protected override void OnBullishSignal(double riskPercentage, double slPips, double tpPips)
    {
        var position = Robot.Positions.Find(Robot.InstanceId);
        if (position != null)
            Robot.ClosePosition(position);
        var volume = Robot.Symbol.VolumeForProportionalRisk(ProportionalAmountType.Balance, riskPercentage, slPips, RoundingMode.Down);
        Robot.ExecuteMarketOrder(TradeType.Buy, Robot.SymbolName, volume, Robot.InstanceId, slPips, null);
    }

    protected override void OnSidewaysSignal()
    {
        var position = Robot.Positions.Find(Robot.InstanceId);
        if (position != null)
            Robot.ClosePosition(position);
    }

    protected override void OnBearishSignal(double riskPercentage, double slPips, double tpPips)
    {

        var position = Robot.Positions.Find(Robot.InstanceId);
        if (position != null)
            Robot.ClosePosition(position);
        var volume = Robot.Symbol.VolumeForProportionalRisk(ProportionalAmountType.Balance, riskPercentage, slPips, RoundingMode.Down);
        Robot.ExecuteMarketOrder(TradeType.Sell, Robot.SymbolName, volume, Robot.InstanceId, slPips, null);
    }

    protected override void OnModifyVolume(int pid, double volumePercentage)
    {
        var position = Robot.Positions.Find(Robot.InstanceId);
        if (position == null || position.Id != pid)
            return;
        var volume = Robot.Symbol.NormalizeVolumeInUnits(volumePercentage, RoundingMode.Down);
        Robot.ModifyPosition(position, volume);
    }

    protected override void OnModifyStopLoss(int pid, double slPrice)
    {
        var position = Robot.Positions.Find(Robot.InstanceId);
        if (position == null || position.Id != pid)
            return;
        Robot.ModifyPosition(position, slPrice, null);
    }
}