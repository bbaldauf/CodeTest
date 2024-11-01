using Humanizer;

namespace RapidPay.Services
{
    public static class UFE
    {
        public static decimal GetNewFee()
        {
            Random random = new Random();

            var returnFee = (decimal)(random.NextDouble() * (2 - 0) + 0);

            return returnFee;
        }
    }
}
