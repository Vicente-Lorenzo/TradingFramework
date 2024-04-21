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


    protected override void OnStart()
    {
        _strategy = new NNFX(this, Verbose);
        _strategy.OnStart();
    }

    protected override void OnError(Error error) { _strategy.OnError(error); }

    protected override void OnException(Exception exception) { _strategy.OnException(exception); }

    protected override void OnStop() { _strategy.OnShutdown(); }
}

public class NNFX : Strategy
{
    public NNFX(Robot robot, Logger.Verbose verbose) : base(robot, verbose) { }

    public override void OnStart()
    {
        CallSymbol(Robot.Symbol);
        for (var i = 0; i < Robot.Bars.Count-1; i++) { CallBarClosed(Robot.Bars[i]); }
        CallComplete();
    }

    public override void OnPositionOpened(PositionOpenedEventArgs args)
    {
        if (string.Equals(args.Position.Label, Robot.InstanceId))
            CallPositionOpened(args.Position);
    }

    public override void OnPositionModified(PositionModifiedEventArgs args)
    {
        if (string.Equals(args.Position.Label, Robot.InstanceId))
            CallPositionModified(args.Position);
    }

    public override void OnPositionClosed(PositionClosedEventArgs args)
    {
        if (string.Equals(args.Position.Label, Robot.InstanceId))
            CallPositionClosed(args.Position);
    }

    public override void OnBarClosed(BarClosedEventArgs args)
    {
        CallBarClosed(args.Bars.LastBar);
    }

    public override void OnTick(SymbolTickEventArgs args)
    {
        CallTick(args.Ask, args.Bid);
    }

    public override void OnBullishSignal(double volumeValue, double slValue, double tpValue)
    {
        var position = Robot.Positions.Find(Robot.InstanceId);
        if (position != null)
            Robot.ClosePosition(position);
        var volume = Robot.Symbol.VolumeForProportionalRisk(ProportionalAmountType.Balance, volumeValue, slValue, RoundingMode.Down);
        Robot.ExecuteMarketOrder(TradeType.Buy, Robot.SymbolName, volume, Robot.InstanceId, slValue, null);
    }

    public override void OnSidewaysSignal()
    {
        var position = Robot.Positions.Find(Robot.InstanceId);
        if (position != null)
            Robot.ClosePosition(position);
    }

    public override void OnBearishSignal(double volumeValue, double slValue, double tpValue)
    {

        var position = Robot.Positions.Find(Robot.InstanceId);
        if (position != null)
            Robot.ClosePosition(position);
        var volume = Robot.Symbol.VolumeForProportionalRisk(ProportionalAmountType.Balance, volumeValue, slValue, RoundingMode.Down);
        Robot.ExecuteMarketOrder(TradeType.Sell, Robot.SymbolName, volume, Robot.InstanceId, slValue, null);
    }

    public override void OnModifyPosition(double volumeValue, double slValue, double tpValue)
    {
        var position = Robot.Positions.Find(Robot.InstanceId);
        if (position == null)
            return;
        var volume = Robot.Symbol.NormalizeVolumeInUnits(volumeValue, RoundingMode.Down);
        Robot.ModifyPosition(position, volume);
        Robot.ModifyPosition(position, slValue, null);
    }
}