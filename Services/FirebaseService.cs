using AYExpenseTracker.Models;
using System.Net.Http.Json;

namespace AYExpenseTracker.Services
{
    public class FirebaseService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly FirebaseAuthService _auth;

        public FirebaseService(HttpClient http, IConfiguration config, FirebaseAuthService auth)
        {
            _http = http;
            _baseUrl = config["Firebase:BaseUrl"]!;
            _auth = auth;
        }

        // ===========================
        // 💸 EXPENSES / INCOME
        // ===========================

        public async Task<List<Expense>> GetExpensesAsync()
        {
            if (_auth.UserId == null || _auth.IdToken == null)
                return new List<Expense>();

            var url = $"{_baseUrl}/users/{_auth.UserId}/expenses.json?auth={_auth.IdToken}";
            var result = await _http.GetFromJsonAsync<Dictionary<string, Expense>>(url);

            if (result == null)
                return new List<Expense>();

            return result.Select(kv =>
            {
                kv.Value.Id = kv.Key;
                return kv.Value;
            }).ToList();
        }

        public async Task<List<Expense>> GetOnlyExpensesAsync()
        {
            var all = await GetExpensesAsync();
            return all.Where(e => e.Type == TransactionType.Expense).ToList();
        }

        public async Task<List<Expense>> GetOnlyIncomeAsync()
        {
            var all = await GetExpensesAsync();
            return all.Where(e => e.Type == TransactionType.Income).ToList();
        }

        public async Task AddExpenseAsync(Expense exp)
        {
            if (_auth.UserId == null || _auth.IdToken == null)
                return;

            var url = $"{_baseUrl}/users/{_auth.UserId}/expenses.json?auth={_auth.IdToken}";
            var response = await _http.PostAsJsonAsync(url, exp);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (result != null && result.ContainsKey("name"))
                    exp.Id = result["name"];
            }
        }

        public async Task UpdateExpenseAsync(Expense exp)
        {
            if (_auth.UserId == null || _auth.IdToken == null || string.IsNullOrEmpty(exp.Id))
                return;

            var url = $"{_baseUrl}/users/{_auth.UserId}/expenses/{exp.Id}.json?auth={_auth.IdToken}";
            await _http.PutAsJsonAsync(url, exp);
        }

        public async Task DeleteExpenseAsync(string id)
        {
            if (_auth.UserId == null || _auth.IdToken == null || string.IsNullOrEmpty(id))
                return;

            var url = $"{_baseUrl}/users/{_auth.UserId}/expenses/{id}.json?auth={_auth.IdToken}";
            await _http.DeleteAsync(url);
        }

        // ===========================
        // 🏷️ CATEGORIES
        // ===========================

        public async Task<List<Category>> GetCategoriesAsync()
        {
            if (_auth.UserId == null || _auth.IdToken == null)
                return new List<Category>();

            var url = $"{_baseUrl}/users/{_auth.UserId}/categories.json?auth={_auth.IdToken}";
            var result = await _http.GetFromJsonAsync<Dictionary<string, Category>>(url);

            if (result == null)
                return new List<Category>();

            return result.Select(kv =>
            {
                kv.Value.Id = kv.Key;
                return kv.Value;
            }).ToList();
        }

        public async Task AddCategoryAsync(Category category)
        {
            if (_auth.UserId == null || _auth.IdToken == null)
                return;

            var url = $"{_baseUrl}/users/{_auth.UserId}/categories.json?auth={_auth.IdToken}";
            var response = await _http.PostAsJsonAsync(url, category);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (result != null && result.ContainsKey("name"))
                    category.Id = result["name"];
            }
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            if (_auth.UserId == null || _auth.IdToken == null || string.IsNullOrEmpty(category.Id))
                return;

            var url = $"{_baseUrl}/users/{_auth.UserId}/categories/{category.Id}.json?auth={_auth.IdToken}";
            await _http.PutAsJsonAsync(url, category);

            var expenses = await GetExpensesAsync();
            var relatedExpenses = expenses.Where(e => e.CategoryId == category.Id).ToList();
            foreach (var exp in relatedExpenses)
            {
                exp.Category = category.Name;
                await UpdateExpenseAsync(exp);
            }
        }

        public async Task<bool> DeleteCategoryAsync(string id)
        {
            if (_auth.UserId == null || _auth.IdToken == null || string.IsNullOrEmpty(id))
                return false;

            var expenses = await GetExpensesAsync();
            if (expenses.Any(e => e.CategoryId == id))
                return false;

            var url = $"{_baseUrl}/users/{_auth.UserId}/categories/{id}.json?auth={_auth.IdToken}";
            await _http.DeleteAsync(url);
            return true;
        }

        // ===========================
        // 🎯 SAVINGS GOALS
        // ===========================

        public async Task<List<SavingsGoal>> GetSavingsGoalsAsync()
        {
            if (_auth.UserId == null || _auth.IdToken == null)
                return new List<SavingsGoal>();

            var url = $"{_baseUrl}/users/{_auth.UserId}/savingsGoals.json?auth={_auth.IdToken}";
            var result = await _http.GetFromJsonAsync<Dictionary<string, SavingsGoal>>(url);

            if (result == null)
                return new List<SavingsGoal>();

            return result.Select(kv =>
            {
                kv.Value.Id = kv.Key;
                return kv.Value;
            }).ToList();
        }

        public async Task AddSavingsGoalAsync(SavingsGoal goal)
        {
            if (_auth.UserId == null || _auth.IdToken == null)
                return;

            var url = $"{_baseUrl}/users/{_auth.UserId}/savingsGoals.json?auth={_auth.IdToken}";
            var response = await _http.PostAsJsonAsync(url, goal);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (result != null && result.ContainsKey("name"))
                    goal.Id = result["name"];
            }
        }

        public async Task UpdateSavingsGoalAsync(SavingsGoal goal)
        {
            if (_auth.UserId == null || _auth.IdToken == null || string.IsNullOrEmpty(goal.Id))
                return;

            var url = $"{_baseUrl}/users/{_auth.UserId}/savingsGoals/{goal.Id}.json?auth={_auth.IdToken}";
            await _http.PutAsJsonAsync(url, goal);
        }

        public async Task DeleteSavingsGoalAsync(string id)
        {
            if (_auth.UserId == null || _auth.IdToken == null || string.IsNullOrEmpty(id))
                return;

            var url = $"{_baseUrl}/users/{_auth.UserId}/savingsGoals/{id}.json?auth={_auth.IdToken}";
            await _http.DeleteAsync(url);
        }

        // ===========================
        // 🔔 FCM TOKENS
        // ===========================

        public async Task SaveFcmTokenAsync(string token)
        {
            if (_auth.UserId == null || _auth.IdToken == null || string.IsNullOrEmpty(token))
                return;

            // Save to user-specific location
            var userUrl = $"{_baseUrl}/users/{_auth.UserId}/fcmTokens.json?auth={_auth.IdToken}";
            await _http.DeleteAsync(userUrl);

            var payload = new { token = token, addedAt = DateTime.UtcNow, userId = _auth.UserId };
            await _http.PostAsJsonAsync(userUrl, payload);

            // Also save to global location for Admin broadcasts
            await SaveGlobalFcmTokenAsync(token);
        }

        private async Task SaveGlobalFcmTokenAsync(string token)
        {
            if (_auth.UserId == null || _auth.IdToken == null || string.IsNullOrEmpty(token))
                return;

            // We use the token itself as a key (encoded) to avoid duplicates in the global list
            var safeTokenKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token))
                .Replace("/", "_").Replace("+", "-").Replace("=", "");
            
            var globalUrl = $"{_baseUrl}/global_tokens/{safeTokenKey}.json?auth={_auth.IdToken}";
            
            var payload = new { token = token, userId = _auth.UserId, lastUpdated = DateTime.UtcNow };
            await _http.PutAsJsonAsync(globalUrl, payload);
        }

        public async Task<List<string>> GetAllGlobalFcmTokensAsync()
        {
            if (_auth.IdToken == null) return new List<string>();

            var url = $"{_baseUrl}/global_tokens.json?auth={_auth.IdToken}";
            var result = await _http.GetFromJsonAsync<Dictionary<string, Dictionary<string, object>>>(url);

            if (result == null) return new List<string>();

            var tokens = new List<string>();
            foreach (var entry in result.Values)
            {
                if (entry.TryGetValue("token", out var tokenObj) && tokenObj is string token)
                {
                    tokens.Add(token);
                }
            }
            return tokens.Distinct().ToList();
        }
    }
}
