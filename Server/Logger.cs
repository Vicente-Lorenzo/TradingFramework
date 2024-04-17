using cAlgo.API;

namespace cAlgo.Robots;

public class Logger
{
    private readonly Robot _robot;
    public enum Verbose { Quiet, Error, Warning, Info, Debug }
    private readonly Verbose _verbose;

    private readonly string _defaultErrorLog;
    private readonly string _defaultWarningLog;
    private readonly string _defaultInfoLog;
    private readonly string _defaultDebugLog;

    public Logger(Robot robot, Verbose verbose)
    {
        _verbose = verbose;
        _robot = robot;

        _defaultErrorLog = BuildDefaultLog(Verbose.Error);
        _defaultWarningLog = BuildDefaultLog(Verbose.Warning);
        _defaultInfoLog = BuildDefaultLog(Verbose.Info);
        _defaultDebugLog = BuildDefaultLog(Verbose.Debug);
    }

    private string BuildDefaultLog(Verbose verbose)
    {
        return $"[ {verbose} ] ";
    }

    private void LogMessage(Verbose customVerbose, string defaultLog, string message)
    {
        if (customVerbose > _verbose)
            return;
        string logMessage = defaultLog + message;
        _robot.Print(logMessage);
    }

    public void Error(string message) { LogMessage(Verbose.Error, _defaultErrorLog, message); }
    public void Warning(string message) { LogMessage(Verbose.Warning, _defaultWarningLog, message); }
    public void Info(string message) { LogMessage(Verbose.Info, _defaultInfoLog, message); }
    public void Debug(string message) { LogMessage(Verbose.Debug, _defaultDebugLog, message); }
}
