using cAlgo.API;
using cAlgo.API.Indicators;
using AlgorithmicTrading.Log;
using AlgorithmicTrading.Position;
using AlgorithmicTrading.Strategy;
using AlgorithmicTrading.Strategy.PositionStrategy;
using AlgorithmicTrading.Strategy.SignalStrategy;
using AlgorithmicTrading.Utility;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.FullAccess)]
    public class UltimateStrategy : Robot
    {
        private const string LOGGING_GROUP = "(General) Logging Settings";
        private const string RISK_MANAGMENT_GROUP = "(General) Risk Managment Settings";
        private const string POSITION_MANAGMENT_GROUP = "(General) Position Managment Settings";
        private const string VOLATILITY_GROUP = "(Volatility) Average True Range Settings";
        private const string MOVING_AVERAGE_GROUP = "(Baseline) Moving Average Settings";
        private const string KIJUN_SEN_GROUP = "(Baseline) Kijun Sen Settings";
        private const string MOVING_AVERAGE_CROSSOVER_GROUP = "(Trend) Moving Average Crossover Settings";
        private const string AROON_GROUP = "(Trend) Aroon Settings";
        private const string DIRECTIONAL_MOVEMENT_SYSTEM_GROUP = "(Trend) Directional Movement System Settings";
        private const string PARABOLIC_SAR_GROUP = "(Trend) Parabolic Sar Settings";
        private const string SUPERTREND_GROUP = "(Trend) SuperTrend Settings";
        private const string MACD_CROSSOVER_GROUP = "(Oscillator) MACD Crossover Settings";
        private const string AWESOME_OSCILLATOR_GROUP = "(Oscillator) Awesome Oscillator Settings";
        private const string ACCELERATOR_OSCILLATOR_GROUP = "(Oscillator) Accelerator Oscillator Settings";
        private const string CYBER_CYCLE_GROUP = "(Oscillator) Cyber Cycle Settings";
        private const string LINEAR_REGRESSION_SLOPE_GROUP = "(Oscillator) Linear Regression Slope Settings";
        private const string STOCHASTIC_OSCILLATOR_GROUP = "(Oscillator) Stochastic Oscillator Settings";
        private const string ICHIMOKU_KINKO_HYO_GROUP = "(Other) Ichimoku Kinko Hyo Settings";
        private const string ALLIGATOR_GROUP = "(Other) Alligator Settings";
        private const string CENTER_OF_GRAVITY_GROUP = "(Other) Center of Gravity Settings";
        private const string POSITION_MANAGER_ID = "M";
        private const string STATISTICS_MANAGER_ID = "S";

        // ======== (GENERAL) LOGGING PARAMETERS ========
        [Parameter("Verbose Level", DefaultValue = Logger.VerboseLevel.Debug, Group = LOGGING_GROUP)]
        public Logger.VerboseLevel VerboseLevel { get; set; }
        [Parameter("Telegram Alerts?", DefaultValue = false, Group = LOGGING_GROUP)]
        public bool UseTelegramAlerts { get; set; }
        [Parameter("Telegram Token", DefaultValue = "5777371710:AAEle3_cDlc2zCRURRHyLETzcf_H7Ed_VeY", Group = LOGGING_GROUP)]
        public string TelegramToken { get; set; }
        [Parameter("Telegram Chat Id", DefaultValue = "681929783", Group = LOGGING_GROUP)]
        public string TelegramChatId { get; set; }

        // ======== (GENERAL) RISK MANAGMENT PARAMETERS ========
        [Parameter("TSL Scale", DefaultValue = 1.5, Group = RISK_MANAGMENT_GROUP, MinValue = 0.1, Step = 0.1)]
        public double TrailingStopScale { get; set; }
        [Parameter("TSL Activation Scale", DefaultValue = 1.5, Group = RISK_MANAGMENT_GROUP, MinValue = 0.1, Step = 0.1)]
        public double TrailingStopActivationScale { get; set; }
        [Parameter("Update TSL on Bar?", DefaultValue = false, Group = RISK_MANAGMENT_GROUP)]
        public bool UpdateTrailingStopOnBar { get; set; }
        [Parameter("TP Scale", DefaultValue = 1.0, Group = RISK_MANAGMENT_GROUP, MinValue = 0.1, Step = 0.1)]
        public double TakeProfitScale { get; set; }
        [Parameter("TP Volume %", DefaultValue = 50, Group = RISK_MANAGMENT_GROUP, MinValue = 0.1, MaxValue = 100, Step = 0.1)]
        public double TakeProfitVolumePercentage { get; set; }

        // ======== (GENERAL) POSITION MANAGMENT PARAMETERS ========
        [Parameter("Risk Per Trade %", DefaultValue = 2.0, Group = POSITION_MANAGMENT_GROUP, MinValue = 0.1, MaxValue = 10, Step = 0.1)]
        public double RiskPerTrade { get; set; }
        [Parameter("SL Scale", DefaultValue = 1.5, Group = POSITION_MANAGMENT_GROUP, MinValue = 0.1, Step = 0.1)]
        public double StopLossScale { get; set; }

        // ======== (VOLATILITY) AVERAGE TRUE RANGE PARAMETERS ========
        [Parameter("Period", DefaultValue = 14, Group = VOLATILITY_GROUP, MinValue = 2, MaxValue = 50, Step = 1)]
        public int AtrPeriod { get; set; }
        [Parameter("MA Type", DefaultValue = MovingAverageType.Simple, Group = VOLATILITY_GROUP)]
        public MovingAverageType AtrMaType { get; set; }

        // ======== (BASELINE) MOVING AVERAGE PARAMETERS ========
        [Parameter("Trigger Mode", Group = MOVING_AVERAGE_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode MovingAverageTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = MOVING_AVERAGE_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode MovingAverageTradeMode { get; set; }
        [Parameter("Source", Group = MOVING_AVERAGE_GROUP)]
        public DataSeries MovingAverageSource { get; set; }
        [Parameter("Period", Group = MOVING_AVERAGE_GROUP, DefaultValue = 200, MinValue = 5, MaxValue = 250, Step = 5)]
        public int MovingAveragePeriod { get; set; }
        [Parameter("MA Type", Group = MOVING_AVERAGE_GROUP, DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType MovingAverageMaType { get; set; }

        // ======== (BASELINE) KIJUN SEN PARAMETERS ========
        [Parameter("Trigger Mode", Group = KIJUN_SEN_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode KijunSenTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = KIJUN_SEN_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode KijunSenTradeMode { get; set; }
        [Parameter("Period", Group = KIJUN_SEN_GROUP, DefaultValue = 26, MinValue = 2, MaxValue = 200, Step = 1)]
        public int KijunSenPeriod { get; set; }

        // ======== (TREND) MOVING AVERAGE CROSSOVER PARAMETERS ========
        [Parameter("Trigger Mode", Group = MOVING_AVERAGE_CROSSOVER_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode MovingAverageCrossoverTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = MOVING_AVERAGE_CROSSOVER_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode MovingAverageCrossoverTradeMode { get; set; }
        [Parameter("Source", Group = MOVING_AVERAGE_CROSSOVER_GROUP)]
        public DataSeries MovingAverageCrossoverSource { get; set; }
        [Parameter("Long Period", Group = MOVING_AVERAGE_CROSSOVER_GROUP, DefaultValue = 50, MinValue = 5, MaxValue = 100, Step = 5)]
        public int MovingAverageCrossoverLongPeriod { get; set; }
        [Parameter("Short Period", Group = MOVING_AVERAGE_CROSSOVER_GROUP, DefaultValue = 20, MinValue = 5, MaxValue = 100, Step = 5)]
        public int MovingAverageCrossoverShortPeriod { get; set; }
        [Parameter("MA Type", Group = MOVING_AVERAGE_CROSSOVER_GROUP, DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType MovingAverageCrossoverMaType { get; set; }

        // ======== (TREND) AROON INDICATOR PARAMETERS ========
        [Parameter("Trigger Mode", Group = AROON_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode AroonTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = AROON_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode AroonTradeMode { get; set; }
        [Parameter("Period", Group = AROON_GROUP, DefaultValue = 25, MinValue = 2, MaxValue = 100, Step = 1)]
        public int AroonPeriod { get; set; }

        // ======== (TREND) DIRECTIONAL MOVEMENT SYSTEM PARAMETERS ========
        [Parameter("Trigger Mode", Group = DIRECTIONAL_MOVEMENT_SYSTEM_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode DmsTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = DIRECTIONAL_MOVEMENT_SYSTEM_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode DmsTradeMode { get; set; }
        [Parameter("Period", Group = DIRECTIONAL_MOVEMENT_SYSTEM_GROUP, DefaultValue = 14, MinValue = 2, MaxValue = 100, Step = 1)]
        public int DmsPeriod { get; set; }

        // ======== (TREND) PARABOLIC SAR PARAMETERS ========
        [Parameter("Trigger Mode", Group = PARABOLIC_SAR_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode PsarTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = PARABOLIC_SAR_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode PsarTradeMode { get; set; }
        [Parameter("Min AF", Group = PARABOLIC_SAR_GROUP, DefaultValue = 0.02, MinValue = 0.01, MaxValue = 1, Step = 0.01)]
        public double PsarMinAf { get; set; }
        [Parameter("Max AF", Group = PARABOLIC_SAR_GROUP, DefaultValue = 0.2, MinValue = 0.01, MaxValue = 1, Step = 0.01)]
        public double PsarMaxAf { get; set; }

        // ======== (TREND) SUPERTREND PARAMETERS ========
        [Parameter("Trigger Mode", Group = SUPERTREND_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode SuperTrendTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = SUPERTREND_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode SuperTrendTradeMode { get; set; }
        [Parameter("Period", Group = SUPERTREND_GROUP, DefaultValue = 10, MinValue = 2, MaxValue = 50, Step = 1)]
        public int SuperTrendPeriod { get; set; }
        [Parameter("Multiplier", Group = SUPERTREND_GROUP, DefaultValue = 3, MinValue = 0.5, MaxValue = 10, Step = 0.5)]
        public double SuperTrendMultiplier { get; set; }

        // ======== (OSCILLATOR) MACD CROSSOVER PARAMETERS ========
        [Parameter("Trigger Mode", Group = MACD_CROSSOVER_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode MacdCrossoverTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = MACD_CROSSOVER_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode MacdCrossoverTradeMode { get; set; }
        [Parameter("Source", Group = MACD_CROSSOVER_GROUP)]
        public DataSeries MacdCrossoverSource { get; set; }
        [Parameter("Long Period", Group = MACD_CROSSOVER_GROUP, DefaultValue = 26, MinValue = 2, MaxValue = 100, Step = 1)]
        public int MacdCrossoverLongPeriod { get; set; }
        [Parameter("Short Period", Group = MACD_CROSSOVER_GROUP, DefaultValue = 12, MinValue = 2, MaxValue = 100, Step = 1)]
        public int MacdCrossoverShortPeriod { get; set; }
        [Parameter("Signal Period", Group = MACD_CROSSOVER_GROUP, DefaultValue = 9, MinValue = 2, MaxValue = 100, Step = 1)]
        public int MacdCrossoverPeriodSignal { get; set; }

        // ======== (OSCILLATOR) AWESOME OSCILLATOR PARAMETERS ========
        [Parameter("Trigger Mode", Group = AWESOME_OSCILLATOR_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode AwesomeOscillatorTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = AWESOME_OSCILLATOR_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode AwesomeOscillatorTradeMode { get; set; }

        // ======== (OSCILLATOR) ACCELERATOR OSCILLATOR PARAMETERS ========
        [Parameter("Trigger Mode", Group = ACCELERATOR_OSCILLATOR_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode AcceleratorOscillatorTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = ACCELERATOR_OSCILLATOR_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode AcceleratorOscillatorTradeMode { get; set; }

        // ======== (OSCILLATOR) CYBER CYCLE PARAMETERS ========
        [Parameter("Trigger Mode", Group = CYBER_CYCLE_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode CyberCycleTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = CYBER_CYCLE_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode CyberCycleTradeMode { get; set; }
        [Parameter("Alpha", Group = CYBER_CYCLE_GROUP, DefaultValue = 0.07, MinValue = 0.01, MaxValue = 1, Step = 0.01)]
        public double CyberCycleAlpha { get; set; }

        // ======== (OSCILLATOR) LINEAR REGRESSION SLOPE PARAMETERS ========
        [Parameter("Trigger Mode", Group = LINEAR_REGRESSION_SLOPE_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode LinearRegressionTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = LINEAR_REGRESSION_SLOPE_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode LinearRegressionTradeMode { get; set; }
        [Parameter("Source", Group = LINEAR_REGRESSION_SLOPE_GROUP)]
        public DataSeries LinearRegressionSource { get; set; }
        [Parameter("Period", Group = LINEAR_REGRESSION_SLOPE_GROUP, DefaultValue = 9, MinValue = 2, MaxValue = 100, Step = 1)]
        public int LinearRegressionPeriod { get; set; }

        // ======== (OSCILLATOR) STOCHASTIC OSCILLATOR PARAMETERS ========
        [Parameter("Trigger Mode", Group = STOCHASTIC_OSCILLATOR_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode StochasticOscillatorTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = STOCHASTIC_OSCILLATOR_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode StochasticOscillatorTradeMode { get; set; }
        [Parameter("%K Period", Group = STOCHASTIC_OSCILLATOR_GROUP, DefaultValue = 9, MinValue = 2, MaxValue = 100, Step = 1)]
        public int StochasticOscillatorKPeriod { get; set; }
        [Parameter("%K Slowing", Group = STOCHASTIC_OSCILLATOR_GROUP, DefaultValue = 3, MinValue = 2, MaxValue = 100, Step = 1)]
        public int StochasticOscillatorKSlowing { get; set; }
        [Parameter("%D Period", Group = STOCHASTIC_OSCILLATOR_GROUP, DefaultValue = 9, MinValue = 2, MaxValue = 100, Step = 1)]
        public int StochasticOscillatorDPeriod { get; set; }
        [Parameter("Ma Type", Group = STOCHASTIC_OSCILLATOR_GROUP, DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType StochasticOscillatorMaType { get; set; }

        // ======== (OTHER) ICHIMOKU KINKO HYO PARAMETERS ========
        [Parameter("Trigger Mode", Group = ICHIMOKU_KINKO_HYO_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode IchimokuTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = ICHIMOKU_KINKO_HYO_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode IchimokuTradeMode { get; set; }
        [Parameter("Long Period", Group = ICHIMOKU_KINKO_HYO_GROUP, DefaultValue = 26, MinValue = 2, MaxValue = 40, Step = 1)]
        public int IchimokuLongPeriod { get; set; }
        [Parameter("Short Period", Group = ICHIMOKU_KINKO_HYO_GROUP, DefaultValue = 9, MinValue = 2, MaxValue = 40, Step = 1)]
        public int IchimokuShortPeriod { get; set; }

        // ======== (OTHER) ALLIGATOR PARAMETERS ========
        [Parameter("Trigger Mode", Group = ALLIGATOR_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode AlligatorTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = ALLIGATOR_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode AlligatorTradeMode { get; set; }
        [Parameter("Long Period", Group = ALLIGATOR_GROUP, DefaultValue = 8, MinValue = 2, MaxValue = 40, Step = 1)]
        public int AlligatorLongPeriod { get; set; }
        [Parameter("Short Period", Group = ALLIGATOR_GROUP, DefaultValue = 5, MinValue = 2, MaxValue = 40, Step = 1)]
        public int AlligatorShortPeriod { get; set; }

        // ======== (OTHER) CENTER OF GRAVITY PARAMETERS ========
        [Parameter("Trigger Mode", Group = CENTER_OF_GRAVITY_GROUP, DefaultValue = IndicatorManager.TriggerMode.None)]
        public IndicatorManager.TriggerMode CenterOfGravityTriggerMode { get; set; }
        [Parameter("Trade Mode", Group = CENTER_OF_GRAVITY_GROUP, DefaultValue = IndicatorManager.TradeMode.Both)]
        public IndicatorManager.TradeMode CenterOfGravityTradeMode { get; set; }
        [Parameter("Period", Group = CENTER_OF_GRAVITY_GROUP, DefaultValue = 10, MinValue = 2, MaxValue = 100, Step = 1)]
        public int CenterOfGravityPeriod { get; set; }

        private AverageTrueRange _iAtr;
        private IndicatorManager _analyser;
        private MovingAverage _iMovingAverage, _iMovingAverageCrossoverLong, _iMovingAverageCrossoverShort;
        private IchimokuKinkoHyo _iKijunSen, _iIchimokuKinkoHyo;
        private Aroon _iAroon;
        private DirectionalMovementSystem _iDms;
        private ParabolicSAR _iParabolicSar;
        private Supertrend _iSuperTrend;
        private MacdCrossOver _iMacdCrossover;
        private AwesomeOscillator _iAwesomeOscillator;
        private AcceleratorOscillator _iAcceleratorOscillator;
        private CyberCycle _iCyberCycle;
        private LinearRegressionSlope _iLinearRegression;
        private StochasticOscillator _iStochasticOscillator;
        private Alligator _iAlligator;
        private CenterOfGravity _iCenterOfGravity;
        
        protected override void OnStart()
        {       
            _iAtr = Indicators.AverageTrueRange(AtrPeriod, AtrMaType);
            
            var logger = new Logger(VerboseLevel, this, UseTelegramAlerts ? new Telegram(TelegramToken, TelegramChatId) : null);
            var position = new PositionManager(POSITION_MANAGER_ID, STATISTICS_MANAGER_ID, this, logger);
            
            var strategy = new StrategyManager(this, logger, true, true, false);
            
            var riskManagment = strategy.CreatePositionStrategyInterface(position);
            var riskManagmentSetup = new NnfxPositionStrategySetup(TrailingStopScale, TrailingStopActivationScale, UpdateTrailingStopOnBar, TakeProfitScale, TakeProfitVolumePercentage, _iAtr, new TickManager(this), this);
            riskManagmentSetup.SetupStrategy(riskManagment);
            
            var signalManagment = strategy.CreateSignalStrategyInterface(position);
            var signalManagmentSetup = new NnfxSignalStrategySetup(EntryBuyTrigger, EntrySellTrigger, ExitBuyTrigger, ExitSellTrigger, RiskPerTrade, StopLossScale, _iAtr, this);
            signalManagmentSetup.SetupStrategy(signalManagment);

            if (MovingAverageCrossoverShortPeriod >= MovingAverageCrossoverLongPeriod)
            {
                logger.Error($"Invalid Moving Average Crossover parameters: Long Period = {MovingAverageCrossoverLongPeriod} | Short Period = {MovingAverageCrossoverShortPeriod}");
                Stop();
            }
            if (MacdCrossoverShortPeriod >= MacdCrossoverLongPeriod)
            {
                logger.Error($"Invalid MACD Crossover parameters: Long Period = {MacdCrossoverLongPeriod} | Short Period = {MacdCrossoverShortPeriod}");
                Stop();
            }
            if (IchimokuShortPeriod >= IchimokuLongPeriod)
            {
                logger.Error($"Invalid Ichimoku Kinko Hyo parameters: Long Period = {IchimokuLongPeriod} | Short Period = {IchimokuShortPeriod}");
                Stop();
            }
            if (AlligatorShortPeriod >= AlligatorLongPeriod)
            {
                logger.Error($"Invalid Alligator parameters: Long Period = {AlligatorLongPeriod} | Short Period = {AlligatorShortPeriod}");
                Stop();
            }

            _analyser = new IndicatorManager();
            if (_analyser.AddIndicator(MovingAverageTriggerMode, MovingAverageTradeMode, MovingAverageBuyConfirmation, MovingAverageSellConfirmation, MovingAverageBuySignal, MovingAverageSellSignal))
                _iMovingAverage = Indicators.MovingAverage(MovingAverageSource, MovingAveragePeriod, MovingAverageMaType);
            if (_analyser.AddIndicator(KijunSenTriggerMode, KijunSenTradeMode, KijunSenBuyConfirmation, KijunSenSellConfirmation, KijunSenBuySignal, KijunSenSellSignal))
                _iKijunSen = Indicators.IchimokuKinkoHyo(9, KijunSenPeriod, 52);
            if (_analyser.AddIndicator(MovingAverageCrossoverTriggerMode, MovingAverageCrossoverTradeMode, MovingAverageCrossoverBuyConfirmation, MovingAverageCrossoverSellConfirmation, MovingAverageCrossoverBuySignal, MovingAverageCrossoverSellSignal))
            {
                _iMovingAverageCrossoverLong = Indicators.MovingAverage(MovingAverageCrossoverSource, MovingAverageCrossoverLongPeriod, MovingAverageCrossoverMaType);
                _iMovingAverageCrossoverShort = Indicators.MovingAverage(MovingAverageCrossoverSource, MovingAverageCrossoverShortPeriod, MovingAverageCrossoverMaType);
            }
            if (_analyser.AddIndicator(AroonTriggerMode, AroonTradeMode, AroonBuyConfirmation, AroonSellConfirmation, AroonBuySignal, AroonSellSignal))
                _iAroon = Indicators.Aroon(AroonPeriod);
            if (_analyser.AddIndicator(DmsTriggerMode, DmsTradeMode, DmsBuyConfirmation, DmsSellConfirmation, DmsBuySignal, DmsSellSignal))
                _iDms = Indicators.DirectionalMovementSystem(DmsPeriod);
            if (_analyser.AddIndicator(PsarTriggerMode, PsarTradeMode, ParabolicSarBuyConfirmation, ParabolicSarSellConfirmation, ParabolicSarBuySignal, ParabolicSarSellSignal))
                _iParabolicSar = Indicators.ParabolicSAR(PsarMinAf, PsarMaxAf);
            if (_analyser.AddIndicator(SuperTrendTriggerMode, SuperTrendTradeMode, SuperTrendBuyConfirmation, SuperTrendSellConfirmation, SuperTrendBuySignal, SuperTrendSellSignal))
                _iSuperTrend = Indicators.Supertrend(SuperTrendPeriod, SuperTrendMultiplier);
            if (_analyser.AddIndicator(MacdCrossoverTriggerMode, MacdCrossoverTradeMode, MacdCrossoverBuyConfirmation, MacdCrossoverSellConfirmation, MacdCrossoverBuySignal, MacdCrossoverSellSignal))
                _iMacdCrossover = Indicators.MacdCrossOver(MacdCrossoverSource, MacdCrossoverLongPeriod, MacdCrossoverShortPeriod, MacdCrossoverPeriodSignal);
            if (_analyser.AddIndicator(AwesomeOscillatorTriggerMode, AwesomeOscillatorTradeMode, AwesomeOscillatorBuyConfirmation, AwesomeOscillatorSellConfirmation, AwesomeOscillatorBuySignal, AwesomeOscillatorSellSignal))
                _iAwesomeOscillator = Indicators.AwesomeOscillator();
            if (_analyser.AddIndicator(AcceleratorOscillatorTriggerMode, AcceleratorOscillatorTradeMode, AcceleratorOscillatorBuyConfirmation, AcceleratorOscillatorSellConfirmation, AcceleratorOscillatorBuySignal, AcceleratorOscillatorSellSignal))
                _iAcceleratorOscillator = Indicators.AcceleratorOscillator();
            if (_analyser.AddIndicator(CyberCycleTriggerMode, CyberCycleTradeMode, CycleCyberBuyConfirmation, CycleCyberSellConfirmation, CycleCyberBuySignal, CycleCyberSellSignal))
                _iCyberCycle = Indicators.CyberCycle(CyberCycleAlpha);
            if (_analyser.AddIndicator(LinearRegressionTriggerMode, LinearRegressionTradeMode, LinearRegressionBuyConfirmation, LinearRegressionSellConfirmation, LinearRegressionBuySignal, LinearRegressionSellSignal))
                _iLinearRegression = Indicators.LinearRegressionSlope(LinearRegressionSource, LinearRegressionPeriod);
            if (_analyser.AddIndicator(StochasticOscillatorTriggerMode, StochasticOscillatorTradeMode, StochasticOscillatorBuyConfirmation, StochasticOscillatorSellConfirmation, StochasticOscillatorBuySignal, StochasticOscillatorSellSignal))
                _iStochasticOscillator = Indicators.StochasticOscillator(StochasticOscillatorKPeriod, StochasticOscillatorKSlowing, StochasticOscillatorDPeriod, StochasticOscillatorMaType);
            if (_analyser.AddIndicator(IchimokuTriggerMode, IchimokuTradeMode, IchimokuKinkoHyoBuyConfirmation, IchimokuKinkoHyoSellConfirmation, IchimokuKinkoHyoBuySignal, IchimokuKinkoHyoSellSignal))
                _iIchimokuKinkoHyo = Indicators.IchimokuKinkoHyo(IchimokuShortPeriod, IchimokuLongPeriod, 52);
            if (_analyser.AddIndicator(AlligatorTriggerMode, AlligatorTradeMode, AlligatorBuyConfirmation, AlligatorSellConfirmation, AlligatorBuySignal, AlligatorSellSignal))
                _iAlligator = Indicators.Alligator(0, 0, AlligatorLongPeriod, 0, AlligatorShortPeriod, 0);
            if (_analyser.AddIndicator(CenterOfGravityTriggerMode, CenterOfGravityTradeMode, CenterOfGravityBuyConfirmation, CenterOfGravitySellConfirmation, CenterOfGravityBuySignal, CenterOfGravitySellSignal))
                _iCenterOfGravity = Indicators.CenterOfGravity(CenterOfGravityPeriod);
        }

        // ======== MOVING AVERAGE ========
        private bool MovingAverageBuyConfirmation()
        {
            return Bars.ClosePrices.Last(1) > _iMovingAverage.Result.Last(1);
        }
        private bool MovingAverageSellConfirmation()
        {
            return Bars.ClosePrices.Last(1) < _iMovingAverage.Result.Last(1);
        }
        private bool MovingAverageBuySignal()
        {
            return Bars.ClosePrices.Last(1) > _iMovingAverage.Result.Last(1) && Bars.ClosePrices.Last(2) < _iMovingAverage.Result.Last(2);
        }
        private bool MovingAverageSellSignal()
        {
            return Bars.ClosePrices.Last(1) < _iMovingAverage.Result.Last(1) && Bars.ClosePrices.Last(2) > _iMovingAverage.Result.Last(2);
        }
        // ======== KIJUN SEN ========
        private bool KijunSenBuyConfirmation()
        {
            return Bars.ClosePrices.Last(1) > _iKijunSen.KijunSen.Last(1);
        }
        private bool KijunSenSellConfirmation()
        {
            return Bars.ClosePrices.Last(1) < _iKijunSen.KijunSen.Last(1);
        }
        private bool KijunSenBuySignal()
        {
            return Bars.ClosePrices.Last(1) > _iKijunSen.KijunSen.Last(1) && Bars.ClosePrices.Last(2) < _iKijunSen.KijunSen.Last(2);
        }
        private bool KijunSenSellSignal()
        {
            return Bars.ClosePrices.Last(1) < _iKijunSen.KijunSen.Last(1) && Bars.ClosePrices.Last(2) > _iKijunSen.KijunSen.Last(2);
        }
        // ======== MOVING AVERAGE CROSSOVER ========
        private bool MovingAverageCrossoverBuyConfirmation()
        {
            return _iMovingAverageCrossoverShort.Result.Last(1) > _iMovingAverageCrossoverLong.Result.Last(1);
        }
        private bool MovingAverageCrossoverSellConfirmation()
        {
            return _iMovingAverageCrossoverShort.Result.Last(1) < _iMovingAverageCrossoverLong.Result.Last(1);
        }
        private bool MovingAverageCrossoverBuySignal()
        {
            return _iMovingAverageCrossoverShort.Result.Last(1) > _iMovingAverageCrossoverLong.Result.Last(1) && _iMovingAverageCrossoverShort.Result.Last(2) < _iMovingAverageCrossoverLong.Result.Last(2);
        }
        private bool MovingAverageCrossoverSellSignal()
        {
            return _iMovingAverageCrossoverShort.Result.Last(1) < _iMovingAverageCrossoverLong.Result.Last(1) && _iMovingAverageCrossoverShort.Result.Last(2) > _iMovingAverageCrossoverLong.Result.Last(2);
        }
        // ======== AROON ========
        private bool AroonBuyConfirmation()
        {
            return _iAroon.Up.Last(1) > _iAroon.Down.Last(1);
        }
        private bool AroonSellConfirmation()
        {
            return _iAroon.Up.Last(1) < _iAroon.Down.Last(1);
        }
        private bool AroonBuySignal()
        {
            return _iAroon.Up.Last(1) > _iAroon.Down.Last(1) && _iAroon.Up.Last(2) < _iAroon.Down.Last(2);
        }
        private bool AroonSellSignal()
        {
            return _iAroon.Up.Last(1) < _iAroon.Down.Last(1) && _iAroon.Up.Last(2) > _iAroon.Down.Last(2);
        }
        // ======== DIRECTIONAL MOVEMENT SYSTEM ========
        private bool DmsBuyConfirmation()
        {
            return _iDms.DIPlus.Last(1) > _iDms.DIMinus.Last(1);
        }
        private bool DmsSellConfirmation()
        {
            return _iDms.DIPlus.Last(1) < _iDms.DIMinus.Last(1);
        }
        private bool DmsBuySignal()
        {
            return _iDms.DIPlus.Last(1) > _iDms.DIMinus.Last(1) && _iDms.DIPlus.Last(2) < _iDms.DIMinus.Last(2);
        }
        private bool DmsSellSignal()
        {
            return _iDms.DIPlus.Last(1) < _iDms.DIMinus.Last(1) && _iDms.DIPlus.Last(2) > _iDms.DIMinus.Last(2);
        }
        // ======== PARABOLIC SAR ========
        private bool ParabolicSarBuyConfirmation()
        {
            return Bars.ClosePrices.Last(1) > _iParabolicSar.Result.Last(1);
        }
        private bool ParabolicSarSellConfirmation()
        {
            return Bars.ClosePrices.Last(1) < _iParabolicSar.Result.Last(1);
        }
        private bool ParabolicSarBuySignal()
        {
            return Bars.ClosePrices.Last(1) > _iParabolicSar.Result.Last(1) && Bars.ClosePrices.Last(2) < _iParabolicSar.Result.Last(2);
        }
        private bool ParabolicSarSellSignal()
        {
            return Bars.ClosePrices.Last(1) < _iParabolicSar.Result.Last(1) && Bars.ClosePrices.Last(2) > _iParabolicSar.Result.Last(2);
        }
        // ======== SUPERTREND ========
        private bool SuperTrendBuyConfirmation()
        {
            return Bars.ClosePrices.Last(1) > _iSuperTrend.UpTrend.Last(1);
        }
        private bool SuperTrendSellConfirmation()
        {
            return Bars.ClosePrices.Last(1) < _iSuperTrend.DownTrend.Last(1);
        }
        private bool SuperTrendBuySignal()
        {
            return Bars.ClosePrices.Last(1) > _iSuperTrend.UpTrend.Last(1) && Bars.ClosePrices.Last(2) < _iSuperTrend.DownTrend.Last(2);
        }
        private bool SuperTrendSellSignal()
        {
            return Bars.ClosePrices.Last(1) < _iSuperTrend.DownTrend.Last(1) && Bars.ClosePrices.Last(2) > _iSuperTrend.UpTrend.Last(2);
        }
        // ======== MACD CROSSOVER ========
        private bool MacdCrossoverBuyConfirmation()
        {
            return _iMacdCrossover.MACD.Last(1) > _iMacdCrossover.Signal.Last(1);
        }
        private bool MacdCrossoverSellConfirmation()
        {
            return _iMacdCrossover.MACD.Last(1) < _iMacdCrossover.Signal.Last(1);
        }
        private bool MacdCrossoverBuySignal()
        {
            return _iMacdCrossover.MACD.Last(1) > _iMacdCrossover.Signal.Last(1) && _iMacdCrossover.MACD.Last(2) < _iMacdCrossover.Signal.Last(2);
        }
        private bool MacdCrossoverSellSignal()
        {
            return _iMacdCrossover.MACD.Last(1) < _iMacdCrossover.Signal.Last(1) && _iMacdCrossover.MACD.Last(2) > _iMacdCrossover.Signal.Last(2);
        }
        // ======== AWESOME OSCILLATOR ========
        private bool AwesomeOscillatorBuyConfirmation()
        {
            return _iAwesomeOscillator.Result.Last(1) > 0;
        }
        private bool AwesomeOscillatorSellConfirmation()
        {
            return _iAwesomeOscillator.Result.Last(1) < 0;
        }
        private bool AwesomeOscillatorBuySignal()
        {
            return _iAwesomeOscillator.Result.Last(1) > 0 && _iAwesomeOscillator.Result.Last(2) < 0;
        }
        private bool AwesomeOscillatorSellSignal()
        {
            return _iAwesomeOscillator.Result.Last(1) < 0 && _iAwesomeOscillator.Result.Last(2) > 0;
        }
        // ======== ACCELERATOR OSCILLATOR ========
        private bool AcceleratorOscillatorBuyConfirmation()
        {
            return _iAcceleratorOscillator.Result.Last(1) > 0;
        }
        private bool AcceleratorOscillatorSellConfirmation()
        {
            return _iAcceleratorOscillator.Result.Last(1) < 0;
        }
        private bool AcceleratorOscillatorBuySignal()
        {
            return _iAcceleratorOscillator.Result.Last(1) > 0 && _iAcceleratorOscillator.Result.Last(2) < 0;
        }
        private bool AcceleratorOscillatorSellSignal()
        {
            return _iAcceleratorOscillator.Result.Last(1) < 0 && _iAcceleratorOscillator.Result.Last(2) > 0;
        }
        // ======== CYBER CYCLE ========
        private bool CycleCyberBuyConfirmation()
        {
            return _iCyberCycle.Cycle.Last(1) > _iCyberCycle.Trigger.Last(1);
        }
        private bool CycleCyberSellConfirmation()
        {
            return _iCyberCycle.Cycle.Last(1) < _iCyberCycle.Trigger.Last(1);
        }
        private bool CycleCyberBuySignal()
        {
            return _iCyberCycle.Cycle.Last(1) > _iCyberCycle.Trigger.Last(1) && _iCyberCycle.Cycle.Last(2) < _iCyberCycle.Trigger.Last(2);
        }
        private bool CycleCyberSellSignal()
        {
            return _iCyberCycle.Cycle.Last(1) < _iCyberCycle.Trigger.Last(1) && _iCyberCycle.Cycle.Last(2) > _iCyberCycle.Trigger.Last(2);
        }
        // ======== LINEAR REGRESSION SLOPE ========
        private bool LinearRegressionBuyConfirmation()
        {
            return _iLinearRegression.Result.Last(1) > 0;
        }
        private bool LinearRegressionSellConfirmation()
        {
            return _iLinearRegression.Result.Last(1) < 0;
        }
        private bool LinearRegressionBuySignal()
        {
            return _iLinearRegression.Result.Last(1) > 0 && _iLinearRegression.Result.Last(2) < 0;
        }
        private bool LinearRegressionSellSignal()
        {
            return _iLinearRegression.Result.Last(1) < 0 && _iLinearRegression.Result.Last(2) > 0;
        }
        // ======== STOCHASTIC OSCILLATOR ========
        private bool StochasticOscillatorBuyConfirmation()
        {
            return _iStochasticOscillator.PercentK.Last(1) > _iStochasticOscillator.PercentD.Last(1);
        }
        private bool StochasticOscillatorSellConfirmation()
        {
            return _iStochasticOscillator.PercentK.Last(1) < _iStochasticOscillator.PercentD.Last(1);
        }
        private bool StochasticOscillatorBuySignal()
        {
            return _iStochasticOscillator.PercentK.Last(1) > _iStochasticOscillator.PercentD.Last(1) && _iStochasticOscillator.PercentK.Last(2) < _iStochasticOscillator.PercentD.Last(2);
        }
        private bool StochasticOscillatorSellSignal()
        {
            return _iStochasticOscillator.PercentK.Last(1) < _iStochasticOscillator.PercentD.Last(1) && _iStochasticOscillator.PercentK.Last(2) > _iStochasticOscillator.PercentD.Last(2);
        }
        // ======== ICHIMOKU KINKO HYO ========
        private bool IchimokuKinkoHyoBuyConfirmation()
        {
            return _iIchimokuKinkoHyo.TenkanSen.Last(1) > _iIchimokuKinkoHyo.KijunSen.Last(1);
        }
        private bool IchimokuKinkoHyoSellConfirmation()
        {
            return _iIchimokuKinkoHyo.TenkanSen.Last(1) < _iIchimokuKinkoHyo.KijunSen.Last(1);
        }
        private bool IchimokuKinkoHyoBuySignal()
        {
            return _iIchimokuKinkoHyo.TenkanSen.Last(1) > _iIchimokuKinkoHyo.KijunSen.Last(1) && _iIchimokuKinkoHyo.TenkanSen.Last(2) < _iIchimokuKinkoHyo.KijunSen.Last(2);
        }
        private bool IchimokuKinkoHyoSellSignal()
        {
            return _iIchimokuKinkoHyo.TenkanSen.Last(1) < _iIchimokuKinkoHyo.KijunSen.Last(1) && _iIchimokuKinkoHyo.TenkanSen.Last(2) > _iIchimokuKinkoHyo.KijunSen.Last(2);
        }
        // ======== ALLIGATOR ========
        private bool AlligatorBuyConfirmation()
        {
            return _iAlligator.Lips.Last(1) > _iAlligator.Teeth.Last(1);
        }
        private bool AlligatorSellConfirmation()
        {
            return _iAlligator.Lips.Last(1) < _iAlligator.Teeth.Last(1);
        }
        private bool AlligatorBuySignal()
        {
            return _iAlligator.Lips.Last(1) > _iAlligator.Teeth.Last(1) && _iAlligator.Lips.Last(2) < _iAlligator.Teeth.Last(2);
        }
        private bool AlligatorSellSignal()
        {
            return _iAlligator.Lips.Last(1) < _iAlligator.Teeth.Last(1) && _iAlligator.Lips.Last(2) > _iAlligator.Teeth.Last(2);
        }
        // ======== CENTER OF GRAVITY ========
        private bool CenterOfGravityBuyConfirmation()
        {
            return _iCenterOfGravity.Result.Last(1) > _iCenterOfGravity.Lag.Last(1);
        }
        private bool CenterOfGravitySellConfirmation()
        {
            return _iCenterOfGravity.Result.Last(1) < _iCenterOfGravity.Lag.Last(1);
        }
        private bool CenterOfGravityBuySignal()
        {
            return _iCenterOfGravity.Result.Last(1) > _iCenterOfGravity.Lag.Last(1) && _iCenterOfGravity.Result.Last(2) < _iCenterOfGravity.Lag.Last(2);
        }
        private bool CenterOfGravitySellSignal()
        {
            return _iCenterOfGravity.Result.Last(1) < _iCenterOfGravity.Lag.Last(1) && _iCenterOfGravity.Result.Last(2) > _iCenterOfGravity.Lag.Last(2);
        }
        // ======== ENTRY TRIGGERS ========
        private bool EntryBuyTrigger(PositionManager position)
        {
            return _analyser.CheckEntryBuyIndicators();
        }
        private bool EntrySellTrigger(PositionManager position)
        {
            return _analyser.CheckEntrySellIndicators();
        }
        // ======== EXIT TRIGGERS ========
        private bool ExitBuyTrigger(PositionManager position)
        {
            return position.Position.TradeType == TradeType.Buy && _analyser.CheckExitBuyIndicators();
        }
        private bool ExitSellTrigger(PositionManager position)
        {
            return position.Position.TradeType == TradeType.Sell && _analyser.CheckExitSellIndicators();
        }
    }
}