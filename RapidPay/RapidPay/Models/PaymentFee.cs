namespace RapidPay.Models
{
    public class PaymentFee
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public decimal FeePercentage { get; set; }
        public decimal FeeAmount { get; set; }

        public DateTime LastTimePercentageCalculated { get; set; }

    }
}
