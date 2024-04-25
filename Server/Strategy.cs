using System;
using System.IO;
using System.Diagnostics;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public abstract class Strategy
{
    private readonly Api _api;
    private readonly Logger _logger;
    private readonly Robot _robot;

    private double? _lastPositionVolume;
    private double? _lastPositionStopLoss;
    private double? _lastPositionTakeProfit;
    private double? _lastPositionAskAboveTarget;
    private double? _lastPositionAskBelowTarget;
    private double? _lastPositionBidAboveTarget;
    private double? _lastPositionBidBelowTarget;

    private void CallShutdown() { _api.PackShutdown(); HandleCallback(); }
    private void CallComplete() { _api.PackComplete(); HandleCallback(); }
    private void CallAccount(IAccount account) { _api.PackAccount(account); HandleCallback(); }
    private void CallSymbol(Symbol symbol) { _api.PackSymbol(symbol); HandleCallback(); }
    private void CallOpenedBuy(Position position) { _api.PackOpenedBuy(position); HandleCallback(); }
    private void CallOpenedSell(Position position) { _api.PackOpenedSell(position); HandleCallback(); }
    private void CallModifiedBuyVolume(Position position) { _api.PackModifiedBuyVolume(position); HandleCallback(); }
    private void CallModifiedBuyStopLoss(Position position) { _api.PackModifiedBuyStopLoss(position); HandleCallback(); }
    private void CallModifiedBuyTakeProfit(Position position) { _api.PackModifiedBuyTakeProfit(position); HandleCallback(); }
    private void CallModifiedSellVolume(Position position) { _api.PackModifiedSellVolume(position); HandleCallback(); }
    private void CallModifiedSellStopLoss(Position position) { _api.PackModifiedSellStopLoss(position); HandleCallback(); }
    private void CallModifiedSellTakeProfit(Position position) { _api.PackModifiedSellTakeProfit(position); HandleCallback(); }
    private void CallClosedBuy(Position position) { _api.PackClosedBuy(position); HandleCallback(); }
    private void CallClosedSell(Position position) { _api.PackClosedSell(position); HandleCallback(); }
    private void CallBar(Bar bar) { _api.PackBar(bar); HandleCallback(); }
    private void CallAskAboveTarget(double ask) { _api.PackAskAboveTarget(ask); HandleCallback(); }
    private void CallAskBelowTarget(double ask) { _api.PackAskBelowTarget(ask); HandleCallback(); }
    private void CallBidAboveTarget(double bid) { _api.PackBidAboveTarget(bid); HandleCallback(); }
    private void CallBidBelowTarget(double bid) { _api.PackBidBelowTarget(bid); HandleCallback(); }

    private bool ClosePosition()
    {
        var position = _robot.Positions.Find(_robot.InstanceId);
        if (position != null && !_robot.ClosePosition(position).IsSuccessful) return false;
        _lastPositionVolume = null;
        _lastPositionStopLoss = null;
        _lastPositionTakeProfit = null;
        _lastPositionAskAboveTarget = null;
        _lastPositionAskBelowTarget = null;
        _lastPositionBidAboveTarget = null;
        _lastPositionBidBelowTarget = null;
        return true;
    }
    private void OpenPositionFixed(TradeType type, double volume, double? slPips, double? tpPips)
    {
        if (!ClosePosition()) return;
        var result = _robot.ExecuteMarketOrder(type, _robot.SymbolName, volume, _robot.InstanceId, slPips, tpPips);
        if (!result.IsSuccessful) return;
        _lastPositionVolume = result.Position.VolumeInUnits;
        _lastPositionStopLoss = result.Position.StopLoss;
        _lastPositionTakeProfit = result.Position.TakeProfit;
    }

    private void OpenPositionDynamic(TradeType type, double percentage, double slPips, double? tpPips)
    {
        var volume = _robot.Symbol.VolumeForProportionalRisk(ProportionalAmountType.Balance, percentage, slPips, RoundingMode.Down);
        OpenPositionFixed(type, volume, slPips, tpPips);
    }

    private void OnSignalBullishFixed(double volume, double? slPips, double? tpPips) { OpenPositionFixed(TradeType.Buy, volume, slPips, tpPips); }
    private void OnSignalBullishDynamic(double percentage, double slPips, double? tpPips) { OpenPositionDynamic(TradeType.Buy, percentage, slPips, tpPips); }
    private void OnSignalBearishFixed(double volume, double? slPips, double? tpPips) { OpenPositionFixed(TradeType.Sell, volume, slPips, tpPips); }
    private void OnSignalBearishDynamic(double percentage, double slPips, double? tpPips) { OpenPositionDynamic(TradeType.Sell, percentage, slPips, tpPips); }
    private void OnSignalSideways() { ClosePosition(); }

    private void OnModifyVolume(double percentage)
    {
        var position = _robot.Positions.Find(_robot.InstanceId);
        if (position == null) return;
        var volume = _robot.Symbol.NormalizeVolumeInUnits(position.VolumeInUnits * percentage / 100, RoundingMode.Down);
        _robot.ModifyPosition(position, volume);
    }

    private void OnModifyStopLoss(double? slPrice)
    {
        var position = _robot.Positions.Find(_robot.InstanceId);
        if (position == null) return;
        _robot.ModifyPosition(position, slPrice, position.TakeProfit);
    }

    private void OnModifyTakeProfit(double? tpPrice)
    {
        var position = _robot.Positions.Find(_robot.InstanceId);
        if (position == null) return;
        _robot.ModifyPosition(position, position.StopLoss, tpPrice);
    }

    private void HandleCallback()
    {
        var call = _api.UnpackHeader();
        double obligatoryVolume, obligatoryStopLoss;
        double? optionalStopLoss, optionalTakeProfit;
        switch (call)
        {
            case Api.IdReceive.Complete:
                break;
            case Api.IdReceive.SignalBullishFixed:
                (obligatoryVolume, optionalStopLoss, optionalTakeProfit) = _api.UnpackSignalFixed();
                OnSignalBullishFixed(obligatoryVolume, optionalStopLoss, optionalTakeProfit);
                break;
            case Api.IdReceive.SignalBullishDynamic:
                (obligatoryVolume, obligatoryStopLoss, optionalTakeProfit) = _api.UnpackSignalDynamic();
                OnSignalBullishDynamic(obligatoryVolume, obligatoryStopLoss, optionalTakeProfit);
                break;
            case Api.IdReceive.SignalSideways:
                OnSignalSideways();
                break;
            case Api.IdReceive.SignalBearishFixed:
                (obligatoryVolume, optionalStopLoss, optionalTakeProfit) = _api.UnpackSignalFixed();
                OnSignalBearishFixed(obligatoryVolume, optionalStopLoss, optionalTakeProfit);
                break;
            case Api.IdReceive.SignalBearishDynamic:
                (obligatoryVolume, obligatoryStopLoss, optionalTakeProfit) = _api.UnpackSignalDynamic();
                OnSignalBearishDynamic(obligatoryVolume, obligatoryStopLoss, optionalTakeProfit);
                break;
            case Api.IdReceive.ModifyVolume:
                OnModifyVolume(_api.UnpackObligatoryValue());
                break;
            case Api.IdReceive.ModifyStopLoss:
                OnModifyStopLoss(_api.UnpackOptionalValue());
                break;
            case Api.IdReceive.ModifyTakeProfit:
                OnModifyTakeProfit(_api.UnpackOptionalValue());
                break;
            case Api.IdReceive.AskAboveTarget:
                _lastPositionAskAboveTarget = _api.UnpackOptionalValue();
                break;
            case Api.IdReceive.AskBelowTarget:
                _lastPositionAskBelowTarget = _api.UnpackOptionalValue();
                break;
            case Api.IdReceive.BidAboveTarget:
                _lastPositionBidAboveTarget = _api.UnpackOptionalValue();
                break;
            case Api.IdReceive.BidBelowTarget:
                _lastPositionBidBelowTarget = _api.UnpackOptionalValue();
                break;
        }
    }

    protected Strategy(Robot robot, Logger.Verbose verbose)
    {
        _logger = new Logger(robot, verbose);
        _robot = robot;

        _robot.Positions.Opened += OnPositionOpened;
        _robot.Positions.Modified += OnPositionModified;
        _robot.Positions.Closed += OnPositionClosed;
        _robot.Bars.BarClosed += OnBar;
        _robot.Symbol.Tick += OnTick;

        _api = new Api(_robot.SymbolName, _robot.TimeFrame.Name, _logger);
        _api.Initialize();

        var baseDirectory = new DirectoryInfo(Environment.CurrentDirectory).Parent?.Parent?.Parent?.FullName;
        var scriptName = GetType().Name;
        var scriptPath = $@"{baseDirectory}\Sources\Client\{scriptName}.py";
        var scriptArgs = $"--verbose {verbose} --symbol {_robot.SymbolName} --timeframe {_robot.TimeFrame.Name}";
        var tabTitle = $"{scriptName} {_robot.SymbolName} {_robot.TimeFrame.Name}";
        var command = $"cmd.exe /k \"conda activate quant && python \"{scriptPath}\" {scriptArgs}\"";
        Process.Start("wt.exe", $"--window 0 new-tab --title \"{tabTitle}\" {command}");

        _api.Connect();

        CallAccount(_robot.Account);
        CallSymbol(_robot.Symbol);
        for (var i = 0; i < _robot.Bars.Count-1; i++) { CallBar(_robot.Bars[i]); }
        CallComplete();
    }

    private void OnPositionOpened(PositionOpenedEventArgs args)
    {
        if (!string.Equals(args.Position.Label, _robot.InstanceId)) return;
        if (args.Position.TradeType == TradeType.Buy) CallOpenedBuy(args.Position); else CallOpenedSell(args.Position);
    }

    private void OnPositionModified(PositionModifiedEventArgs args)
    {
        if (!string.Equals(args.Position.Label, _robot.InstanceId)) return;
        if (_lastPositionVolume != null && Math.Abs(args.Position.VolumeInUnits - (double)_lastPositionVolume) > double.Epsilon)
        {
            _lastPositionVolume = args.Position.VolumeInUnits;
            if (args.Position.TradeType == TradeType.Buy) CallModifiedBuyVolume(args.Position); else CallModifiedSellVolume(args.Position);
            return;
        }
        if ((_lastPositionStopLoss == null && args.Position.StopLoss != null) ||
            (_lastPositionStopLoss != null && args.Position.StopLoss == null) ||
            (_lastPositionStopLoss != null && args.Position.StopLoss != null && Math.Abs((double)args.Position.StopLoss - (double)_lastPositionStopLoss) > double.Epsilon))
        {
            _lastPositionStopLoss = args.Position.StopLoss;
            if (args.Position.TradeType == TradeType.Buy) CallModifiedBuyStopLoss(args.Position); else CallModifiedSellStopLoss(args.Position);
            return;
        }
        if ((_lastPositionTakeProfit == null && args.Position.TakeProfit != null) ||
            (_lastPositionTakeProfit != null && args.Position.TakeProfit == null) ||
            (_lastPositionTakeProfit != null && args.Position.TakeProfit != null && Math.Abs((double)args.Position.TakeProfit - (double)_lastPositionTakeProfit) > double.Epsilon))
        {
            _lastPositionTakeProfit = args.Position.TakeProfit;
            if (args.Position.TradeType == TradeType.Buy) CallModifiedBuyTakeProfit(args.Position); else CallModifiedSellTakeProfit(args.Position);
        }
    }

    private void OnPositionClosed(PositionClosedEventArgs args)
    {
        if (!string.Equals(args.Position.Label, _robot.InstanceId)) return;
        if (args.Position.TradeType == TradeType.Buy) CallClosedBuy(args.Position); else CallClosedSell(args.Position);
    }

    private void OnBar(BarClosedEventArgs args) { CallBar(args.Bars.LastBar); }

    private void OnTick(SymbolTickEventArgs args)
    {
        if (_lastPositionAskAboveTarget != null && args.Ask >= _lastPositionAskAboveTarget) { CallAskAboveTarget(args.Ask); _lastPositionAskAboveTarget = null; }
        if (_lastPositionAskBelowTarget != null && args.Ask <= _lastPositionAskBelowTarget) { CallAskBelowTarget(args.Ask); _lastPositionAskBelowTarget = null; }
        if (_lastPositionBidAboveTarget != null && args.Bid >= _lastPositionBidAboveTarget) { CallBidAboveTarget(args.Bid); _lastPositionBidAboveTarget = null; }
        if (_lastPositionBidBelowTarget != null && args.Bid <= _lastPositionBidBelowTarget) { CallBidBelowTarget(args.Bid); _lastPositionBidBelowTarget = null; }
    }

    public void OnError(Error error)
    {
        _logger.Error("An unexpected error occured in the server execution");
        _logger.Error(error.TradeResult.ToString());
        _robot.Stop();
    }

    public void OnException(Exception exception)
    {
        _logger.Error("An unexpected exception occured in the server execution");
        _logger.Error(exception.ToString());
        _robot.Stop();
    }

    public void OnShutdown()
    {
        _logger.Warning("Shutdown strategy and safely terminate operations");
        CallShutdown();
        _api.Disconnect();
    }
}