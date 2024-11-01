using Microsoft.EntityFrameworkCore;

namespace RapidPay.Models
{
    public class CreditCardContext : DbContext
    {
        public CreditCardContext(DbContextOptions<CreditCardContext> options) : base(options)
        {

        }
        public DbSet<CreditCard> CreditCards { get; set; } = null!;
        public DbSet<PaymentTransaction> TransactionList { get; set; } = null!;

        public DbSet<PaymentFee> PaymentFees { get; set; } = null!;
        
    }
}
