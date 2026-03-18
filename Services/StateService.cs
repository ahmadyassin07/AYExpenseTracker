using AYExpenseTracker.Models;
using Microsoft.JSInterop;

namespace AYExpenseTracker.Services
{
    public class StateService
    {
        private readonly FirebaseService _firebase;
        private readonly ToastService _toast;
        private readonly NotificationService _notifications;

        public List<Expense> Expenses { get; private set; } = new();
        public List<Category> Categories { get; private set; } = new();
        public List<SavingsGoal> SavingsGoals { get; private set; } = new();

        public event Action? OnChange;
        public bool IsLoading { get; private set; }

        public StateService(FirebaseService firebase, ToastService toast, NotificationService notifications)
        {
            _firebase = firebase;
            _toast = toast;
            _notifications = notifications;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        private async Task CheckBudgetAlerts(Expense expense)
        {
            if (expense.Type != TransactionType.Expense) return;

            var category = Categories.FirstOrDefault(c => c.Id == expense.CategoryId);
            if (category == null || category.BudgetLimit <= 0) return;

            var spentThisMonth = Expenses
                .Where(e => e.CategoryId == expense.CategoryId && 
                            e.Type == TransactionType.Expense &&
                            e.Date.Month == DateTime.Today.Month && 
                            e.Date.Year == DateTime.Today.Year)
                .Sum(e => e.Amount);

            if (spentThisMonth > category.BudgetLimit)
            {
                await _notifications.SendNotificationAsync(
                    "Budget Exceeded! ⚠️", 
                    $"You've spent {spentThisMonth:N2} KWD on {category.Name}, exceeding your {category.BudgetLimit:N2} KWD limit."
                );
            }
            else if (spentThisMonth > category.BudgetLimit * 0.8m)
            {
                await _notifications.SendNotificationAsync(
                    "Budget Alert! 🔔", 
                    $"You've reached 80% of your budget for {category.Name} ({spentThisMonth:N2} / {category.BudgetLimit:N2} KWD)."
                );
            }
        }

        public async Task InitializeAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            NotifyStateChanged();

            try
            {
                var expensesTask = _firebase.GetExpensesAsync();
                var categoriesTask = _firebase.GetCategoriesAsync();
                var goalsTask = _firebase.GetSavingsGoalsAsync();

                await Task.WhenAll(expensesTask, categoriesTask, goalsTask);

                Expenses = expensesTask.Result;
                Categories = categoriesTask.Result;
                SavingsGoals = goalsTask.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing state: {ex.Message}");
                _toast.ShowToast("Error loading data from server.", "danger");
            }
            finally
            {
                IsLoading = false;
                NotifyStateChanged();
            }
        }

        // --- EXPENSES ---
        public async Task AddExpenseAsync(Expense expense)
        {
            // Optimistic Update
            expense.Id = Guid.NewGuid().ToString(); // Temp ID
            Expenses.Insert(0, expense);
            NotifyStateChanged();

            try
            {
                await _firebase.AddExpenseAsync(expense);
                // Firebase updates the Id in the object
                await CheckBudgetAlerts(expense);
            }
            catch (Exception)
            {
                Expenses.Remove(expense);
                _toast.ShowToast("Failed to add transaction. Please try again.", "danger");
                NotifyStateChanged();
            }
        }

        public async Task UpdateExpenseAsync(Expense expense)
        {
            var index = Expenses.FindIndex(e => e.Id == expense.Id);
            if (index == -1) return;

            var oldExpense = Expenses[index];
            Expenses[index] = expense;
            NotifyStateChanged();

            try
            {
                await _firebase.UpdateExpenseAsync(expense);
                await CheckBudgetAlerts(expense);
            }
            catch (Exception)
            {
                Expenses[index] = oldExpense;
                _toast.ShowToast("Failed to update transaction.", "danger");
                NotifyStateChanged();
            }
        }

        public async Task DeleteExpenseAsync(string id)
        {
            var expense = Expenses.FirstOrDefault(e => e.Id == id);
            if (expense == null) return;

            Expenses.Remove(expense);
            NotifyStateChanged();

            try
            {
                await _firebase.DeleteExpenseAsync(id);
            }
            catch (Exception)
            {
                Expenses.Add(expense);
                _toast.ShowToast("Failed to delete transaction.", "danger");
                NotifyStateChanged();
            }
        }

        // --- CATEGORIES ---
        public async Task AddCategoryAsync(Category category)
        {
            if (string.IsNullOrEmpty(category.Id)) category.Id = Guid.NewGuid().ToString();
            Categories.Add(category);
            NotifyStateChanged();

            try
            {
                await _firebase.AddCategoryAsync(category);
            }
            catch (Exception)
            {
                Categories.Remove(category);
                NotifyStateChanged();
            }
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            var index = Categories.FindIndex(c => c.Id == category.Id);
            if (index == -1) return;

            var oldCat = Categories[index];
            Categories[index] = category;
            NotifyStateChanged();

            try
            {
                await _firebase.UpdateCategoryAsync(category);
            }
            catch (Exception)
            {
                Categories[index] = oldCat;
                _toast.ShowToast("Failed to update category.", "danger");
                NotifyStateChanged();
            }
        }

        public async Task<bool> DeleteCategoryAsync(string id)
        {
            var category = Categories.FirstOrDefault(c => c.Id == id);
            if (category == null) return false;

            // Check if category is used
            if (Expenses.Any(e => e.CategoryId == id))
            {
                _toast.ShowToast("Cannot delete a category that is in use.", "warning");
                return false;
            }

            Categories.Remove(category);
            NotifyStateChanged();

            try
            {
                bool deleted = await _firebase.DeleteCategoryAsync(id);
                if (!deleted)
                {
                    Categories.Add(category);
                    NotifyStateChanged();
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                Categories.Add(category);
                NotifyStateChanged();
                return false;
            }
        }

        // --- SAVINGS GOALS ---
        public async Task AddSavingsGoalAsync(SavingsGoal goal)
        {
            goal.Id = Guid.NewGuid().ToString();
            SavingsGoals.Add(goal);
            NotifyStateChanged();

            try
            {
                await _firebase.AddSavingsGoalAsync(goal);
            }
            catch (Exception)
            {
                SavingsGoals.Remove(goal);
                _toast.ShowToast("Failed to add goal.", "danger");
                NotifyStateChanged();
            }
        }

        public async Task UpdateSavingsGoalAsync(SavingsGoal goal)
        {
            var index = SavingsGoals.FindIndex(g => g.Id == goal.Id);
            if (index == -1) return;

            var oldGoal = SavingsGoals[index];
            SavingsGoals[index] = goal;
            NotifyStateChanged();

            try
            {
                await _firebase.UpdateSavingsGoalAsync(goal);
            }
            catch (Exception)
            {
                SavingsGoals[index] = oldGoal;
                _toast.ShowToast("Failed to update goal.", "danger");
                NotifyStateChanged();
            }
        }

        public async Task DeleteSavingsGoalAsync(string id)
        {
            var goal = SavingsGoals.FirstOrDefault(g => g.Id == id);
            if (goal == null) return;

            SavingsGoals.Remove(goal);
            NotifyStateChanged();

            try
            {
                await _firebase.DeleteSavingsGoalAsync(id);
            }
            catch (Exception)
            {
                SavingsGoals.Add(goal);
                _toast.ShowToast("Failed to delete goal.", "danger");
                NotifyStateChanged();
            }
        }

        public async Task RefreshAsync()
        {
            await InitializeAsync();
        }
    }
}
