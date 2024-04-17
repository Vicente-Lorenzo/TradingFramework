using cAlgo.API;

namespace Server;

public class NNFX : Strategy
{
    public NNFX(Robot robot, Logger logger) : base(robot, logger) { }

    private bool UpdateMarketPosition(Position position, TradeType? direction, double volume)
    {
        if (position is null)
        {
            if (direction is TradeType.Buy)
                return Robot.ExecuteMarketOrder(TradeType.Buy, Robot.SymbolName, volume, Robot.InstanceId).IsSuccessful;
            if (direction is TradeType.Sell)
                return Robot.ExecuteMarketOrder(TradeType.Sell, Robot.SymbolName, volume, Robot.InstanceId).IsSuccessful;
            return true;
        }
        if (position.TradeType is TradeType.Buy)
        {
            if (direction is null)
                return position.Close().IsSuccessful;
            if (direction is TradeType.Sell)
                return position.Close().IsSuccessful && Robot.ExecuteMarketOrder(TradeType.Sell, Robot.SymbolName, volume, Robot.InstanceId).IsSuccessful;
            return true;
        }
        if (position.TradeType is TradeType.Sell)
        {
            if (direction is null)
                return position.Close().IsSuccessful;
            if (direction is TradeType.Buy)
                return position.Close().IsSuccessful && Robot.ExecuteMarketOrder(TradeType.Buy, Robot.SymbolName, volume, Robot.InstanceId).IsSuccessful;
            return true;
        }
        return false;
    }


}