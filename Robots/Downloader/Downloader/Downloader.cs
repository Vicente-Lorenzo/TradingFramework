using System;
using cAlgo.API;

namespace cAlgo.Robots;

[Robot(AccessRights = AccessRights.FullAccess)]
public class Downloader : Robot
{
    private Strategy _strategy;

    private const string GeneralGroup = "General Settings";

    [Parameter("Verbose Level", DefaultValue = Logger.Verbose.Debug, Group = GeneralGroup)]
    public Logger.Verbose Verbose { get; set; }

    protected override void OnStart()
    {
        _strategy = new DownloaderStrategy(this, Verbose);
        _strategy.OnStart();
    }

    protected override void OnError(Error error) { _strategy.OnError(error); }

    protected override void OnException(Exception exception) { _strategy.OnException(exception); }

    protected override void OnStop() { _strategy.OnShutdown(); }
}

public class DownloaderStrategy : Strategy
{
    public DownloaderStrategy(Robot robot, Logger.Verbose verbose) : base(robot, verbose) { }

    public override void OnStart()
    {
        for (var i = 0; i < Robot.Bars.Count-1; i++) { Api.PackBarClosed(Robot.Bars[i]); }
        Api.PackComplete();
        //Api.ReceiveMessage();
    }

    protected override void OnBarClosed(BarClosedEventArgs args)
    {
        Api.PackBarClosed(args.Bars.LastBar);
        //Api.ReceiveMessage();
    }
}
