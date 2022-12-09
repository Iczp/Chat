using IczpNet.AbpCommons;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.MessageSections.RedEnvelopes
{
    public class RedEnvelopeGenerator : DomainService, IRedEnvelopeGenerator
    {
        public virtual async Task<IList<RedEnvelopeUnit>> MakeAsync(GrantModes grantMode, decimal amount, int count, decimal totalAmount, string text)
        {
            var result = new List<RedEnvelopeUnit>();

            var numList = grantMode == GrantModes.RandomAmount
                ? AllocateRandomResult(totalAmount, count)
                : AllocateFixedResult(amount, count);

            numList.ForEach(amount =>
            {
                result.Add(new RedEnvelopeUnit
                {
                    Amount = Convert.ToDecimal(amount),
                });
            });

            if (grantMode == GrantModes.RandomAmount)
            {
                var maxAmount = result.Max(x => x.Amount);

                result.FindAll(x => x.Amount == maxAmount).ForEach(x => x.IsTop = true);
            }
            return await Task.FromResult(result);
        }

        /// <summary>
        /// 固定红包
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected virtual List<decimal> AllocateFixedResult(decimal amount, int count)
        {
            var result = new List<decimal>();

            for (int i = 0; i < count; i++)
            {
                result.Add(amount);
            }
            return result;
        }

        /// <summary>
        /// 随机红包
        /// </summary>
        /// <param name="totalAmount"></param>
        /// <param name="count"></param>
        /// <param name="minAmount"></param>
        /// <returns></returns>
        protected virtual List<decimal> AllocateRandomResult(decimal totalAmount, int count, decimal minAmount = 0.01m)
        {
            //double minAmount = 0.01;
            var moneyList = new List<decimal>();

            decimal _totalAmount = totalAmount - minAmount * count;

            var r = new Random();

            for (int i = 1; i < count; i++)
            {
                decimal money = 0;

                if (_totalAmount > 0)
                {
                    decimal safeAmount = (_totalAmount - (count - i) * minAmount) / (count - i);

                    money = Convert.ToDecimal(NextDouble(r, Convert.ToDouble(minAmount * 100), Convert.ToDouble(safeAmount * 100)) / 100);

                    money = Math.Round(money, 2, MidpointRounding.AwayFromZero);

                    _totalAmount -= money;

                    _totalAmount = Math.Round(_totalAmount, 2, MidpointRounding.AwayFromZero);
                }
                moneyList.Add(money + minAmount);
                //Console.WriteLine("第" + i + "个红包：" + money + " 元，余额：" + _totalAmount + " 元");
            }
            moneyList.Add(_totalAmount + minAmount);

            var _sumAmount = moneyList.Sum();

            _sumAmount = Math.Round(_sumAmount, 2, MidpointRounding.AwayFromZero);

            Assert.If(!_sumAmount.Equals(totalAmount), $"Verification of red packet amount failed.");

            Assert.If(moneyList.Any(x => x < minAmount), $"Error in red packet algorithm.");
            
            //Console.WriteLine("第" + count + "个红包：" + _totalAmount + " 元，余额：0 元");
            return moneyList;
        }

        /// <summary>
        /// 生成设置范围内的Double的随机数
        /// eg:_random.NextDouble(1.5, 2.5)
        /// </summary>
        /// <param name="random">Random</param>
        /// <param name="minDouble">生成随机数的最大值</param>
        /// <param name="maxDouble">生成随机数的最小值</param>
        /// <returns>当Random等于NULL的时候返回0;</returns>
        protected virtual double NextDouble(Random random, double minDouble, double maxDouble)
        {
            if (random != null)
            {
                return random.NextDouble() * (maxDouble - minDouble) + minDouble;
            }
            return 0.0d;
        }
    }
}
