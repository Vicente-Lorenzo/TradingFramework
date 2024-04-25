using System;
using System.Diagnostics;
using cAlgo.API;

namespace cAlgo.Robots;

[Robot(AccessRights = AccessRights.FullAccess)]
public class NNFXStrategy : Robot
{
    private Strategy _strategy;
    private readonly Stopwatch _timer = new();

    private const string GeneralGroup = "General Settings";

    [Parameter("Verbose", DefaultValue = Logger.Verbose.Debug, Group = GeneralGroup)]
    public Logger.Verbose Verbose { get; set; }


    protected override void OnStart()
    {
        _timer.Start();
        _strategy = new NNFX(this, Verbose);
    }

    protected override void OnError(Error error) { _strategy.OnError(error); }

    protected override void OnException(Exception exception) { _strategy.OnException(exception); }

    protected override void OnStop()
    {
        _strategy.OnShutdown();
        _timer.Stop();
        Print($"Execution elapsed {_timer.ElapsedMilliseconds} ms");
    }
}

public class NNFX : Strategy { public NNFX(Robot robot, Logger.Verbose verbose) : base(robot, verbose) { } }