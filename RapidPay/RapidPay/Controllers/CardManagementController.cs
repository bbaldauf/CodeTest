using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RapidPay.Models;
using System.Text;
namespace RapidPay.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CardManagementController : ControllerBase
    {
        private readonly CreditCardContext _context;
        private readonly PaymentFeesController _paymentFeesController;
        public CardManagementController(CreditCardContext creditCardContext)
        {
            _context = creditCardContext;
            _paymentFeesController = new PaymentFeesController(creditCardContext);
        }

        [HttpPost(Name = "CreateCard")]
        public async Task<IActionResult> CreateCard(decimal balance)
        {
            var lastCreditCard = new CreditCard();
            lock (_context.TransactionList)
                lastCreditCard = _context.CreditCards.LastOrDefault();
            int newId = 1;
            if (lastCreditCard != null)
            {
                newId = lastCreditCard.Id + 1;
            }

            string creditCard = GetNewCardNumber(newId);
            var cardManagement = new CreditCard
            {
                Id = newId,
                CardNumber = creditCard,
                Balance = balance


            };
            lock (_context.CreditCards)
                _context.CreditCards.Add(cardManagement);
            await _context.SaveChangesAsync();

            return CreatedAtAction("CreateCard", creditCard);
        }

        [HttpGet("{creditCardNumber}")]
        public ActionResult<decimal> GetCardBalance(string creditCardNumber)
        {
            var creditCard = GetCreditCard(creditCardNumber);
            if (creditCard == null)
                return NotFound();
            return creditCard.Balance;
        }
        [HttpPut(Name = "Pay")]
        public async Task<IActionResult> Pay(string creditCardNumber, decimal payment)
        {

            var creditCard = GetCreditCard(creditCardNumber);

            if (creditCard != null)
            {
                AddTransaction(creditCard.Id, payment);
                creditCard.Balance -= payment;

                _context.Entry(creditCard).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    _paymentFeesController.CalculateNewFeeAmount(creditCard.Id, payment);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CreditCardExists(creditCardNumber))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();

            }
            return NotFound();
        }
        private void AddTransaction(int cardId, decimal payment)
        {
            var lastTransaction = new PaymentTransaction();
            lock (_context.TransactionList)
                lastTransaction = _context.TransactionList.LastOrDefault();

            int newId = 1;
            if (lastTransaction != null)
            {
                newId = lastTransaction.Id + 1;
            }

            var transaction = new PaymentTransaction
            {
                Id = newId,
                CardId = cardId,
                Date = DateTime.Now,
                TransactionAmount = payment
            };
            lock (_context.TransactionList)
                _context.TransactionList.Add(transaction);
            _context.SaveChangesAsync();
        }

        private bool TransactionExists(int id)
        {
            lock (_context.TransactionList)
                return _context.TransactionList.Any(e => e.Id == id);
        }

        
        private bool CreditCardExists(string cardNumber)
        {
            lock (_context.CreditCards)
                return _context.CreditCards.Any(e => e.CardNumber == cardNumber);
        }
        private CreditCard GetCreditCard(string cardNumber)
        {
            CreditCard? creditCard = null;
            if (cardNumber != null)
            {
                lock (_context.CreditCards)
                    creditCard = (from c in _context.CreditCards
                        where c.CardNumber == cardNumber
                        select c).FirstOrDefault();
            }
            return creditCard;
        }
        private string GetNewCardNumber(int maxId)
        {
            string bin = "55555555";
            var endingNumbers = new StringBuilder();
            if (maxId < 8)
            {
                for (int i = maxId.ToString().Length; i <= 8; i++)
                {
                    endingNumbers.Append("0");
                }
                endingNumbers.Append(maxId.ToString());
            }
            else
                endingNumbers.Append(maxId.ToString());
            return bin + endingNumbers.ToString();
        }
    }
}
