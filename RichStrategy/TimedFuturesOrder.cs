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
        public bool IsProfitting { get; private set; }
        private int Timeout;
        private readonly string Settle;
        private readonly string Contract;
        private readonly double PriceOffset;
        private readonly double RewardRiskRatio;
        private bool IsBuying = false;
        private bool IsSelling = false;
        private long OrderAmount;
        private long CollectedAmount;
        private FuturesOrder CollectOrder = null;
        private OrderMode Mode = OrderMode.M_INIT;
        public TimedFuturesOrder(long tradeAmount, double priceOffset, double startPrice, int timeout,
            double rewardToRiskRatio, Candle refCandle, string settle = "btc", string contract = "BTC_USD")
        {
            InnerOrder = new(contract, tradeAmount, 0, startPrice.ToString());
            Timeout = timeout;
            Settle = settle;
            Contract = contract;
            PriceOffset = priceOffset;
            RewardRiskRatio = rewardToRiskRatio;
            ReferenceCandle = refCandle;
        }
        private async void PlaceOrder(double refATR)
        {
            ReferenceATR = refATR;
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
        private void UpdateInnerOrderRoutine(double newATR)
        {
            InnerOrder = API.GateIO.GetFuturesOrder(InnerOrder, Settle);
            UpdateOrderStatus();
            if (newATR != 0)
            {
                ReferenceATR = newATR;
                UpdatePricePoints();
            }
        }
        private async void CollectBack(long amount, double price)
        {
            if (amount == 0) return;
            await Task.Factory.StartNew(() =>
            {
                CollectOrder = new(Contract, amount, 0, price.ToString(), false, false, FuturesOrder.TifEnum.Ioc);
                CollectOrder = API.GateIO.PlaceFuturesOrder(CollectOrder, Settle);
            });
        }
        private void CheckAgainstATR(double marketPrice)
        {
            if (IsSelling)
            {
                if (marketPrice < TargetPrice)
                {
                    // buy back the amount to earn the profit
                    CollectBack(FullfilledAmount - CollectedAmount, TargetPrice);
                    IsProfitting = true;
                }
                else if (marketPrice > StopLossPrice)
                {
                    // buy back the amount to stop the loss
                    CollectBack(FullfilledAmount - CollectedAmount, marketPrice + PriceOffset);
                    IsProfitting = false;
                }
            }
            else if (IsBuying)
            {
                if (marketPrice > TargetPrice)
                {
                    // sell back the amount to earn the profit
                    CollectBack(CollectedAmount - FullfilledAmount, TargetPrice);
                    IsProfitting = true;
                }
                else if (marketPrice < StopLossPrice)
                {
                    // sell back the amount to stop the loss
                    CollectBack(CollectedAmount - FullfilledAmount, marketPrice - PriceOffset);
                    IsProfitting = false;
                }
            }
        }
        public void Tick(double marketPrice, double newATR)
        {
            switch (Mode)
            {
                case OrderMode.M_INIT:
                    {
                        PlaceOrder(newATR);
                        Mode = OrderMode.M_TIMEDOWN;
                    }
                    break;
                case OrderMode.M_TIMEDOWN:
                    {
                        UpdateInnerOrderRoutine(newATR);
                        if (InnerOrder.Status == FuturesOrder.StatusEnum.Finished) Mode = OrderMode.M_COMPARE;
                        else
                        {
                            if (Timeout > 1) Timeout--;
                            else Mode = OrderMode.M_CANCEL;
                        }
                    }
                    break;
                case OrderMode.M_CANCEL:
                    {
                        UpdateInnerOrderRoutine(newATR);
                        if (InnerOrder.Status == FuturesOrder.StatusEnum.Open)
                        {
                            InnerOrder = API.GateIO.CancelFuturesOrder(InnerOrder, Settle);
                            UpdateOrderStatus();
                        }
                        else
                        {
                            if (InnerOrder.FinishAs == FuturesOrder.FinishAsEnum.Cancelled)
                            {
                                if (FullfilledAmount != CollectedAmount) Mode = OrderMode.M_COMPARE;
                                else Mode = OrderMode.M_RESOLVED;
                            }
                            else Mode = OrderMode.M_COMPARE;
                        }
                    }
                    break;
                case OrderMode.M_COMPARE:
                    {
                        UpdateInnerOrderRoutine(newATR);
                        if (null != CollectOrder)
                        {
                            CollectOrder = API.GateIO.GetFuturesOrder(CollectOrder, Settle);
                            CollectedAmount += Math.Abs(CollectOrder.Size) - Math.Abs(CollectOrder.Left);
                            TokenizedGain = (marketPrice - StartPrice) * CollectedAmount * Math.Sign(InnerOrder.Size);
                        }
                        if (CollectedAmount < FullfilledAmount) CheckAgainstATR(marketPrice);
                        else Mode = OrderMode.M_RESOLVED;
                    }
                    break;
                case OrderMode.M_RESOLVED:
                    break;
                default:
                    break;
            }
        }
        private async void DebugPlaceOrder(double refATR)
        {
            ReferenceATR = refATR;
            await Task.Factory.StartNew(() =>
            {
                double fundingRate = API.GateIO.GetCurrentFundingRate();
                StartPrice = double.Parse(InnerOrder.Price) * (1 + fundingRate);
            });
            DebugUpdateOrderStatus();
            DebugUpdatePricePoints();
        }
        private void DebugUpdateOrderStatus()
        {
            IsBuying = InnerOrder.Size > 0;
            IsSelling = InnerOrder.Size < 0;
            OrderAmount = Math.Abs(InnerOrder.Size);
            FullfilledAmount = OrderAmount;
        }
        private void DebugUpdatePricePoints()
        {
            UpdatePricePoints();
        }
        private void DebugUpdateInnerOrderRoutine(double newATR)
        {
            DebugUpdateOrderStatus();
            if (newATR != 0)
            {
                ReferenceATR = newATR;
                DebugUpdatePricePoints();
            }
        }
        private async void DebugCollectBack(long amount, double price)
        {
            if (amount == 0) return;
            await Task.Factory.StartNew(() =>
            {
                CollectOrder = new(Contract, amount, 0, price.ToString(), false, false, FuturesOrder.TifEnum.Ioc);
            });
        }
        private void DebugCheckAgainstATR(double marketPrice)
        {
            if (IsSelling)
            {
                if (marketPrice < TargetPrice)
                {
                    // buy back the amount to earn the profit
                    DebugCollectBack(FullfilledAmount - CollectedAmount, TargetPrice);
                    IsProfitting = true;
                }
                else if (marketPrice > StopLossPrice)
                {
                    // buy back the amount to stop the loss
                    DebugCollectBack(FullfilledAmount - CollectedAmount, marketPrice + PriceOffset);
                    IsProfitting = false;
                }
            }
            else if (IsBuying)
            {
                if (marketPrice > TargetPrice)
                {
                    // sell back the amount to earn the profit
                    DebugCollectBack(CollectedAmount - FullfilledAmount, TargetPrice);
                    IsProfitting = true;
                }
                else if (marketPrice < StopLossPrice)
                {
                    // sell back the amount to stop the loss
                    DebugCollectBack(CollectedAmount - FullfilledAmount, marketPrice - PriceOffset);
                    IsProfitting = false;
                }
            }
        }
        public void DebugTick(double marketPrice, double newATR)
        {
            // identicle to Tick but not actually placing orders
            // get the target-hit : stop-loss ratio and display it
            // if strategy proves to work, change the calculation of fees
            // if strategy does not work, change to a simpler strategy
            switch (Mode)
            {
                case OrderMode.M_INIT:
                    {
                        DebugPlaceOrder(newATR);
                        Mode = OrderMode.M_TIMEDOWN;
                    }
                    break;
                case OrderMode.M_TIMEDOWN:
                    {
                        DebugUpdateInnerOrderRoutine(newATR);
                        double rnd = new Random().NextDouble();
                        if (rnd > 0.5) Mode = OrderMode.M_COMPARE;
                        else
                        {
                            if (Timeout > 1) Timeout--;
                            else Mode = OrderMode.M_CANCEL;
                        }
                    }
                    break;
                case OrderMode.M_CANCEL:
                    {
                        DebugUpdateInnerOrderRoutine(newATR);
                        Random rnd = new();
                        if (rnd.NextDouble() > 0.5)
                        {
                            DebugUpdateOrderStatus();
                        }
                        else
                        {
                            if (rnd.NextDouble() > 0.5)
                            {
                                if (FullfilledAmount != CollectedAmount) Mode = OrderMode.M_COMPARE;
                                else Mode = OrderMode.M_RESOLVED;
                            }
                            else Mode = OrderMode.M_COMPARE;
                        }
                    }
                    break;
                case OrderMode.M_COMPARE:
                    {
                        DebugUpdateInnerOrderRoutine(newATR);
                        if (null != CollectOrder)
                        {
                            CollectedAmount += Math.Abs(CollectOrder.Size);
                            TokenizedGain = (marketPrice - StartPrice) * CollectedAmount * Math.Sign(InnerOrder.Size);
                        }
                        if (CollectedAmount < FullfilledAmount) DebugCheckAgainstATR(marketPrice);
                        else Mode = OrderMode.M_RESOLVED;
                    }
                    break;
                case OrderMode.M_RESOLVED:
                    break;
                default:
                    break;
            }
        }
        public string GetDirectionString()
        {
            if (IsBuying) return "Buy";
            if (IsSelling) return "Sell";
            return "Error";
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
                OrderMode.M_COMPARE => "Comparing: " + (IsProfitting ? "Profitting" : "Losing"),
                OrderMode.M_RESOLVED => "Finished",
                _ => "Error",
            };
        }
        public bool IsResolved()
        {
            return Mode == OrderMode.M_RESOLVED;
        }
    }
}
