namespace AYExpenseTracker.Models
{
    public class SavingsGoal
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime TargetDate { get; set; } = DateTime.Now.AddYears(1);
        public string Icon { get; set; } = "bi-piggy-bank";
        public string Color { get; set; } = "#10b981";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Computed
        public decimal ProgressPercent => TargetAmount > 0 ? Math.Min(100m, CurrentAmount / TargetAmount * 100m) : 0m;
        public decimal RemainingAmount => Math.Max(0m, TargetAmount - CurrentAmount);
        public bool IsCompleted => CurrentAmount >= TargetAmount;
        public string GetFormattedTimeRemaining(bool isArabic)
        {
            if (IsCompleted) return isArabic ? "مكتمل" : "Completed";
            
            var today = DateTime.Today;
            if (TargetDate <= today) return isArabic ? "اليوم!" : "Today!";

            int years = TargetDate.Year - today.Year;
            int months = TargetDate.Month - today.Month;
            int days = TargetDate.Day - today.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(today.AddMonths(months).Year, today.AddMonths(months).Month);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            var parts = new List<string>();
            if (years > 0) parts.Add(years + (isArabic ? (years == 1 ? " سنة" : " سنوات") : (years == 1 ? " Year" : " Years")));
            if (months > 0) parts.Add(months + (isArabic ? (months == 1 ? " شهر" : " شهور") : (months == 1 ? " Month" : " Months")));
            if (days > 0 || (years == 0 && months == 0)) parts.Add(days + (isArabic ? (days == 1 ? " يوم" : " أيام") : (days == 1 ? " Day" : " Days")));

            return string.Join(isArabic ? " و " : ", ", parts);
        }

        public decimal MonthlySavingsRequired
        {
            get
            {
                var today = DateTime.Today;
                var totalMonths = ((TargetDate.Year - today.Year) * 12) + TargetDate.Month - today.Month;
                if (TargetDate.Day < today.Day && totalMonths > 0) totalMonths--; // Adjust if we haven't reached the target day this month
                
                return totalMonths > 0 ? RemainingAmount / totalMonths : RemainingAmount;
            }
        }

    }
}
