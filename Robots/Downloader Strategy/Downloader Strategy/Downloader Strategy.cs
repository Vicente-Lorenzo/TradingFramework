using System;
using cAlgo.API;

namespace cAlgo.Robots;

[Robot(AccessRights = AccessRights.FullAccess)]
public class DownloaderStrategy : Robot
{
    private Strategy _strategy;

    private const string GeneralGroup = "General Settings";

    [Parameter("Verbose", DefaultValue = Logger.Verbose.Debug, Group = GeneralGroup)]
    public Logger.Verbose Verbose { get; set; }


    protected override void OnStart()
    {
        _strategy = new Downloader(this, Verbose);
        _strategy.OnStart();
    }

    protected override void OnError(Error error) { _strategy.OnError(error); }

    protected override void OnException(Exception exception) { _strategy.OnException(exception); }

    protected override void OnStop() { _strategy.OnShutdown(); }
}

public class Downloader : Strategy
{
    public Downloader(Robot robot, Logger.Verbose verbose) : base(robot, verbose) { }

    public override void OnStart()
    {
        for (var i = 0; i < Robot.Bars.Count-1; i++) { CallBarClosed(Robot.Bars[i]); }
        CallComplete();
    }

    public override void OnBarClosed(BarClosedEventArgs args)
    {
        CallBarClosed(args.Bars.LastBar);
    }
}
