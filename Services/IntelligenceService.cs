using AYExpenseTracker.Models;

namespace AYExpenseTracker.Services
{
    public class IntelligenceService
    {
        private readonly StateService _state;

        public IntelligenceService(StateService state)
        {
            _state = state;
        }
        public string PredictCategory(string description, CategoryType type)
        {
            if (string.IsNullOrWhiteSpace(description)) return string.Empty;

            var lowerDesc = description.ToLower();
            
            // 1. Check history for exact matches
            var lastMatch = _state.Expenses
                .Where(e => e.Description?.ToLower() == lowerDesc && e.Type == (type == CategoryType.Expense ? TransactionType.Expense : TransactionType.Income))
                .OrderByDescending(e => e.Date)
                .FirstOrDefault();

            if (lastMatch != null) return lastMatch.CategoryId ?? string.Empty;

            // 2. Rule-based keywords (Mini-AI)
            if (type == CategoryType.Expense)
            {
                if (lowerDesc.Contains("food") || lowerDesc.Contains("restaurant") || lowerDesc.Contains("dinner") || lowerDesc.Contains("kfc") || lowerDesc.Contains("mcdonald"))
                    return GetCategoryIdByName("Food") ?? GetCategoryIdByName("Groceries") ?? string.Empty;
                
                if (lowerDesc.Contains("uber") || lowerDesc.Contains("careem") || lowerDesc.Contains("taxi") || lowerDesc.Contains("fuel") || lowerDesc.Contains("gas"))
                    return GetCategoryIdByName("Transport") ?? GetCategoryIdByName("Car") ?? string.Empty;

                if (lowerDesc.Contains("rent") || lowerDesc.Contains("utility") || lowerDesc.Contains("electricity") || lowerDesc.Contains("water"))
                    return GetCategoryIdByName("Housing") ?? GetCategoryIdByName("Bills") ?? string.Empty;

                if (lowerDesc.Contains("netflix") || lowerDesc.Contains("spotify") || lowerDesc.Contains("game") || lowerDesc.Contains("cinema"))
                    return GetCategoryIdByName("Entertainment") ?? string.Empty;
            }
            else
            {
                if (lowerDesc.Contains("salary") || lowerDesc.Contains("work") || lowerDesc.Contains("bonus"))
                    return GetCategoryIdByName("Salary") ?? string.Empty;
            }

            return string.Empty;
        }

        public decimal GetDailyBudget()
        {
            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;
            var daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
            var remainingDays = daysInMonth - DateTime.Today.Day + 1;

            var currentMonthIncome = _state.Expenses
                .Where(e => e.Type == TransactionType.Income && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .Sum(e => e.Amount);

            var currentMonthExpenses = _state.Expenses
                .Where(e => e.Type == TransactionType.Expense && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .Sum(e => e.Amount);

            // If no income recorded, assume a default or use total balance (simplified)
            var available = currentMonthIncome - currentMonthExpenses;
            if (available < 0) return 0;

            return available / remainingDays;
        }

        public List<string> GetAIPersonalInsights(bool isArabic)
        {
            var insights = new List<string>();
            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;
            var lastMonth = DateTime.Today.AddMonths(-1).Month;
            var lastYear = DateTime.Today.AddMonths(-1).Year;

            var dailyBudget = GetDailyBudget();
            if (dailyBudget > 0)
            {
                insights.Add(isArabic
                    ? $"💡 ميزانيتك اليومية المتبقية هي {dailyBudget:N2} KWD. حاول الالتزام بها!"
                    : $"💡 Your remaining daily budget is {dailyBudget:N2} KWD. Try to stay within this limit!");
            }

            // Insight 1: Spending Comparison
            var currentSpend = _state.Expenses
                .Where(e => e.Type == TransactionType.Expense && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .Sum(e => e.Amount);

            var lastMonthSpend = _state.Expenses
                .Where(e => e.Type == TransactionType.Expense && e.Date.Month == lastMonth && e.Date.Year == lastYear)
                .Sum(e => e.Amount);

            if (lastMonthSpend > 0)
            {
                var diff = ((currentSpend - lastMonthSpend) / lastMonthSpend) * 100;
                if (diff > 10)
                {
                    insights.Add(isArabic 
                        ? $"📈 لقد زاد إنفاقك بنسبة {diff:N0}% مقارنة بالشهر الماضي."
                        : $"📈 Your spending is {diff:N0}% higher than last month.");
                }
                else if (diff < -10)
                {
                    insights.Add(isArabic
                        ? $"📉 عمل رائع! لقد وفرت {Math.Abs(diff):N0}% مقارنة بالشهر الماضي."
                        : $"📉 Great job! You saved {Math.Abs(diff):N0}% compared to last month.");
                }
            }

            // Insight 2: High Category Spend
            var topCategoryGroup = _state.Expenses
                .Where(e => e.Type == TransactionType.Expense && e.Date.Month == currentMonth)
                .GroupBy(e => e.CategoryId)
                .Select(g => new { CategoryId = g.Key, Total = g.Sum(e => e.Amount) })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();

            if (topCategoryGroup != null)
            {
                var cat = _state.Categories.FirstOrDefault(c => c.Id == topCategoryGroup.CategoryId);
                if (cat != null && currentSpend > 0)
                {
                    var pct = (topCategoryGroup.Total / currentSpend) * 100;
                    if (pct > 40)
                    {
                        insights.Add(isArabic
                            ? $"⚠️ فئة {cat.Name} تستهلك {pct:N0}% من مصروفاتك."
                            : $"⚠️ {cat.Name} accounts for {pct:N0}% of your total spending.");
                    }
                }
            }

            // Insight 3: Savings Potential
            var income = _state.Expenses
                .Where(e => e.Type == TransactionType.Income && e.Date.Month == currentMonth)
                .Sum(e => e.Amount);
            if (income > 0)
            {
                var savingsRate = ((income - currentSpend) / income) * 100;
                if (savingsRate > 20)
                {
                    insights.Add(isArabic
                        ? "🚀 معدل ادخارك ممتار! أنت تسدد ديونك أو تبني ثروتك بسرعة."
                        : "🚀 Your savings rate is excellent! You're building wealth efficiently.");
                }
            }

            if (!insights.Any())
            {
                insights.Add(isArabic
                    ? "✨ ميزانيتك تبدو جيدة حتى الآن! استمر في التتبع."
                    : "✨ Your budget looks healthy so far! Keep tracking your expenses.");
            }

            return insights;
        }

        public List<Expense> DetectRecurringTransactions()
        {
            // Simple detection: same amount, same category, same description, appearing at least twice
            return _state.Expenses
                .Where(e => e.Type == TransactionType.Expense)
                .GroupBy(e => new { e.Amount, e.CategoryId, e.Description })
                .Where(g => g.Count() >= 2)
                .Select(g => g.First())
                .ToList();
        }

        private string? GetCategoryIdByName(string name)
        {
            return _state.Categories.FirstOrDefault(c => c.Name?.Contains(name, StringComparison.OrdinalIgnoreCase) == true)?.Id;
        }
    }
}
