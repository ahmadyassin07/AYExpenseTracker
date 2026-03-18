using System.ComponentModel.DataAnnotations;

namespace AYExpenseTracker.Models
{
    public enum TransactionType { Expense, Income }
    public enum RecurringFrequency { None, Daily, Weekly, Monthly, Yearly }
    public enum PaymentMethod { Cash, CreditCard, DebitCard, BankTransfer, Online, Other }

    public class Expense
    {
        public string? Id { get; set; }  // Firebase key
        public string Title { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Category { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TransactionType Type { get; set; } = TransactionType.Expense;
        public bool IsRecurring { get; set; } = false;
        public RecurringFrequency RecurringFrequency { get; set; } = RecurringFrequency.None;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    }
}
