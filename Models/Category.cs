namespace AYExpenseTracker.Models
{
    public enum CategoryType { Expense, Income }

    public class Category
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public CategoryType Type { get; set; } = CategoryType.Expense;
        public string Icon { get; set; } = "bi-tag";
        public string Color { get; set; } = "#6366f1";
        public decimal BudgetLimit { get; set; } = 0;
    }
}
