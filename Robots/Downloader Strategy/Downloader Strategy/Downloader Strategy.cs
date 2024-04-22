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


    protected override void OnStart() { _strategy = new Downloader(this, Verbose, false); }

    protected override void OnError(Error error) { _strategy.OnError(error); }

    protected override void OnException(Exception exception) { _strategy.OnException(exception); }

    protected override void OnStop() { _strategy.OnShutdown(); }
}

public class Downloader : Strategy
{
    public Downloader(Robot robot, Logger.Verbose verbose, bool global) : base(robot, verbose, global) { }
}
