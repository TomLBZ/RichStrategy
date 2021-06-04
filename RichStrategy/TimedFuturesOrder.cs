using System;
using System.Threading.Tasks;
using Io.Gate.GateApi.Model;

namespace RichStrategy
{
    public class TimedFuturesOrder
    {
        private enum OrderMode
        {
            M_INIT,
            M_TIMEDOWN,
            M_CANCEL,
            M_COMPARE,
            M_COLLECT,
            M_RESOLVED
        }
        public FuturesOrder InnerOrder { get; private set; }
        public Candle ReferenceCandle { get; private set; }
        public double ReferenceATR { get; private set; }
        public double StartPrice { get; private set; }
        public double TokenizedGain { get; private set; }
        public double TargetPrice { get; private set; }
        public double StopLossPrice { get; private set; }
        public long FullfilledAmount { get; private set; }
        public bool IsResolved { get; private set; }
        private int Timeout;
        private readonly string Settle;
        private readonly string Contract;
        private readonly double PriceOffset;
        private double RewardRiskRatio;
        private bool IsBuying = false;
        private bool IsSelling = false;
        private long OrderAmount;
        private long CollectedAmount;
        private FuturesOrder CollectOrder;
        private OrderMode Mode = OrderMode.M_INIT;
        public TimedFuturesOrder(long tradeAmount, double priceOffset, double startPrice, int timeout, string settle = "btc", string contract = "BTC_USD")
        {
            InnerOrder = new(contract, tradeAmount, 0, startPrice.ToString());
            Timeout = timeout;
            Settle = settle;
            Contract = contract;
            PriceOffset = priceOffset;
        }
        public async void PlaceOrder(double refATR, double rewardRiskRatio, Candle refCandle)
        {
            ReferenceCandle = refCandle;
            ReferenceATR = refATR;
            RewardRiskRatio = rewardRiskRatio;
            await Task.Factory.StartNew(() =>
            {
                InnerOrder = API.GateIO.PlaceFuturesOrder(InnerOrder, Settle);
                double fundingRate = API.GateIO.GetCurrentFundingRate();
                StartPrice = double.Parse(InnerOrder.Price) * (1 + fundingRate);
                UpdateOrderStatus();
            });
            UpdatePricePoints();
        }
        private void UpdateOrderStatus()
        {
            IsBuying = InnerOrder.Size > 0;
            IsSelling = InnerOrder.Size < 0;
            OrderAmount = Math.Abs(InnerOrder.Size);
            FullfilledAmount = OrderAmount - Math.Abs(InnerOrder.Left);
        }
        private void UpdatePricePoints()
        {
            if (IsBuying)
            {
                StopLossPrice = Math.Min(StartPrice - ReferenceATR * RewardRiskRatio, ReferenceCandle.Low - ReferenceATR);
                TargetPrice = StartPrice + (StartPrice - StopLossPrice) * RewardRiskRatio;
            }
            else if (IsSelling)
            {
                StopLossPrice = Math.Max(StartPrice + ReferenceATR * RewardRiskRatio, ReferenceCandle.High + ReferenceATR);
                TargetPrice = StartPrice - (StopLossPrice - StartPrice) * RewardRiskRatio;
            }
            else // close order
            {
                StopLossPrice = double.NegativeInfinity;
                TargetPrice = double.PositiveInfinity;
            }
        }
        public void Tick(double marketPrice, double newATR)
        {
            switch (Mode)
            {
                case OrderMode.M_INIT:
                    {

                    }
                    break;
                case OrderMode.M_TIMEDOWN:
                    {

                    }
                    break;
                case OrderMode.M_CANCEL:
                    {

                    }
                    break;
                case OrderMode.M_COMPARE:
                    {

                    }
                    break;
                case OrderMode.M_COLLECT:
                    {

                    }
                    break;
                case OrderMode.M_RESOLVED:
                    break;
                default:
                    break;
            }
            if (newATR != 0)
            {
                ReferenceATR = newATR;
                UpdatePricePoints();
            }
            InnerOrder = API.GateIO.GetFuturesOrder(InnerOrder, Settle);
            UpdateOrderStatus();
            if (InnerOrder.Status == FuturesOrder.StatusEnum.Finished)
            {
                Timeout = -1;
                CheckAgainstATR(marketPrice);
            }
            else
            {
                if (Timeout > 0) Timeout--;
                if (Timeout == 0)
                {
                    InnerOrder = API.GateIO.CancelFuturesOrder(InnerOrder, Settle);
                    UpdateOrderStatus();
                }
            }
            IsResolved = Timeout == -1 && InnerOrder.Status == FuturesOrder.StatusEnum.Finished
                && CollectedAmount == FullfilledAmount;
        }
        private async void CollectBack(long amount, double price, double marketPrice, bool isWinning)
        {
            if (amount == 0) return;
            await Task.Factory.StartNew(() =>
            {
                CollectOrder = new(Contract, amount, 0, price.ToString(), false, false, FuturesOrder.TifEnum.Ioc);
                CollectOrder = API.GateIO.PlaceFuturesOrder(CollectOrder, Settle);
            });
            CollectedAmount += Math.Abs(CollectOrder.Size) - Math.Abs(CollectOrder.Left);
            double pricediff = Math.Abs(StartPrice - marketPrice);
            TokenizedGain = pricediff * CollectedAmount * (isWinning ? 1 : -1);
        }
        private void CheckAgainstATR(double marketPrice)
        {
            if (null != CollectOrder)
            {
                CollectedAmount += Math.Abs(CollectOrder.Size) - Math.Abs(CollectOrder.Left);
            }
            if (IsSelling)
            {
                if (marketPrice < TargetPrice)
                {
                    // buy back the amount to earn the profit
                    CollectBack(FullfilledAmount - CollectedAmount, TargetPrice, marketPrice, true);
                }
                else if (marketPrice > StopLossPrice)
                {
                    // buy back the amount to stop the loss
                    CollectBack(FullfilledAmount - CollectedAmount, marketPrice + PriceOffset, marketPrice, false);
                }
            }
            else if (IsBuying)
            {
                if (marketPrice > TargetPrice)
                {
                    // sell back the amount to earn the profit
                    CollectBack(CollectedAmount - FullfilledAmount, TargetPrice, marketPrice, true);
                }
                else if (marketPrice < StopLossPrice)
                {
                    // sell back the amount to stop the loss
                    CollectBack(CollectedAmount - FullfilledAmount, marketPrice - PriceOffset, marketPrice, false);
                }
            }
        }
        public string GetCandleString()
        {
            return null == ReferenceCandle ? "null" : string.Format("[\r\n    Open:{0},\r\n    Close:{1},\r\n    High:{2},\r\n    Low{3}\r\n  ]",
                ReferenceCandle.Open.ToString("C"), ReferenceCandle.Close.ToString("C"), ReferenceCandle.High.ToString("C"), ReferenceCandle.Low.ToString("C"));
        }
        public string GetMode()
        {
            return Mode switch
            {
                OrderMode.M_INIT => "Initialized",
                OrderMode.M_TIMEDOWN => "Counting Down",
                OrderMode.M_CANCEL => "Cancelling Order",
                OrderMode.M_COMPARE => "Comparing Prices",
                OrderMode.M_COLLECT => "Collecting Results",
                OrderMode.M_RESOLVED => "Finished",
                _ => "Error",
            };
        }
    }
}
