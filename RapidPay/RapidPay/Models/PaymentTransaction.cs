namespace RapidPay.Models
{
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public DateTime Date { get; set; }
        public decimal TransactionAmount { get; set; }
    }
}
