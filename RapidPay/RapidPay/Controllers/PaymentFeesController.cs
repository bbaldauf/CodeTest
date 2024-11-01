using RapidPay.Services;

using Microsoft.AspNetCore.Mvc;
using RapidPay.Models;
using Microsoft.EntityFrameworkCore;


namespace RapidPay.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentFeesController : ControllerBase
    {
        private readonly CreditCardContext _context;
        public PaymentFeesController(CreditCardContext creditCardContext)
        {
            _context = creditCardContext;

        }

        internal async void CalculateNewFeeAmount(int cardId, decimal transactionAmount)
        {
            var lastFee = new PaymentFee();
            lock (_context.PaymentFees)
                 lastFee = _context.PaymentFees.LastOrDefault();
            var newFeePercentage = 0.5m;
            var lastPaymentFee = 0.5m;
            var newPaymentFee = 0.5m;
            DateTime lastTimePercentageCal = DateTime.Now;
            int newId = 1;
            if (lastFee != null)
            {
                newId = lastFee.Id + 1;
                newPaymentFee = lastFee.FeePercentage;
                if (lastTimePercentageCal.AddHours(-1) > lastFee.LastTimePercentageCalculated)
                {
                    lastTimePercentageCal = DateTime.Now;
                    lastPaymentFee = GetLastPaymentFee();
                    newFeePercentage = CalculateNewFeePercentage();
                    newPaymentFee = lastPaymentFee * newFeePercentage;
                }
            }
            else
            {
                lastPaymentFee = GetLastPaymentFee();
                newFeePercentage = CalculateNewFeePercentage();
                newPaymentFee = lastPaymentFee * newFeePercentage;
            }
            
            var feeAmount = decimal.Round(newPaymentFee * transactionAmount, 2, MidpointRounding.AwayFromZero);

            var paymentFees = new PaymentFee
            {
                Id = newId,
                CardId = cardId,
                FeePercentage = newPaymentFee,
                FeeAmount = feeAmount,
                LastTimePercentageCalculated = lastTimePercentageCal,


            };
            lock (_context.PaymentFees)
                _context.PaymentFees.Add(paymentFees);

            await _context.SaveChangesAsync();

        }
        private decimal CalculateNewFeePercentage()
        {
            var ufe = UFE.GetNewFee();
            return ufe;
        }

        private decimal GetLastPaymentFee()
        {
            lock (_context.PaymentFees)
            {
                var lastFee = _context.PaymentFees.LastOrDefault();
                if (lastFee != null)
                {
                    return lastFee.FeePercentage;
                }
                return 0.1m;
            }
        }

    }
}
