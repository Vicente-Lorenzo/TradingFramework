using System;
using cAlgo.API;

namespace cAlgo.Robots;

[Robot(AccessRights = AccessRights.FullAccess)]
public class NNFXStrategy : Robot
{
    private Strategy _strategy;

    private const string GeneralGroup = "General Settings";

    [Parameter("Path", DefaultValue = "C:\\Users\\vicen\\OneDrive\\Documents\\cAlgo\\Sources\\Client", Group = GeneralGroup)]
    public string Path { get; set; }
    [Parameter("Verbose", DefaultValue = Logger.Verbose.Debug, Group = GeneralGroup)]
    public Logger.Verbose Verbose { get; set; }


    protected override void OnStart()
    {
        _strategy = new NNFX(this, Path, Verbose);
        _strategy.OnStart();
    }

    protected override void OnError(Error error) { _strategy.OnError(error); }

    protected override void OnException(Exception exception) { _strategy.OnException(exception); }

    protected override void OnStop() { _strategy.OnShutdown(); }
}

public class NNFX : Strategy
{
    public NNFX(Robot robot, string path, Logger.Verbose verbose) : base(robot, path, verbose) { }

    public override void OnStart()
    {
        for (var i = 0; i < Robot.Bars.Count-1; i++) { CallBarClosed(Robot.Bars[i]); }
        CallComplete();
    }

    public override void OnPositionOpened(PositionOpenedEventArgs args)
    {
        CallPositionOpened(args.Position);
    }

    public override void OnPositionModified(PositionModifiedEventArgs args)
    {
        CallPositionModified(args.Position);
    }

    public override void OnPositionClosed(PositionClosedEventArgs args)
    {
        CallPositionClosed(args.Position);
    }

    public override void OnBarClosed(BarClosedEventArgs args)
    {
        CallBarClosed(args.Bars.LastBar);
    }

    public override void OnTick(SymbolTickEventArgs args)
    {
        //CallTick(args.Ask, args.Bid);
    }
}