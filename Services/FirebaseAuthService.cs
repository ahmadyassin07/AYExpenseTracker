using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace AYExpenseTracker.Services
{
    public class FirebaseAuthService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public string? UserId { get; private set; }
        public string? Email { get; private set; }
        public string? IdToken { get; private set; }
        public string RefreshToken { get; set; } = string.Empty; // NEW
        private const string firebaseApiKey = "AIzaSyBclK8VdNQN8GUFVyADmX_--bWJSFb7-Dk";

        public FirebaseAuthService(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }

       
        public async Task<(bool Success, string? ErrorCode)> SignUpAsync(string email, string password)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={firebaseApiKey}";
            var payload = new { email, password, returnSecureToken = true };

            var response = await _http.PostAsJsonAsync(url, payload);

            if (response.IsSuccessStatusCode)
                return (true, null);

            var error = await response.Content.ReadFromJsonAsync<FirebaseErrorResponse>();

            // Firebase returns error codes like EMAIL_EXISTS
            return (false, error?.error?.message);
        }

        private class FirebaseErrorResponse
        {
            public FirebaseError error { get; set; } = new();
        }

        private class FirebaseError
        {
            public string message { get; set; } = string.Empty;
        }




       




        public async Task<(bool success, bool verified)> SignInAsync(string email, string password)
        {
            try
            {
                var payload = new
                {
                    email,
                    password,
                    returnSecureToken = true
                };

                string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={firebaseApiKey}";
                var result = await _http.PostAsJsonAsync(url, payload);
                if (!result.IsSuccessStatusCode) return (false, false);

                var data = await result.Content.ReadFromJsonAsync<FirebaseAuthResponse>();
                if (data == null) return (false, false);

                // Check if email is verified
                bool verified = await IsEmailVerifiedAsync(data.IdToken);
                if (!verified)
                {
                    // Do NOT store IdToken/UserId yet
                    // Send verification email again just in case
                    await SendEmailVerificationAsync(data.IdToken);
                    return (false, false);
                }

                // Email is verified → now we save tokens
                UserId = data.LocalId;
                Email = email;
                IdToken = data.IdToken;
                RefreshToken = data.RefreshToken; // NEW
                await SaveToStorage();

                return (true, true);
            }
            catch
            {
                return (false, false);
            }
        }













        // ------------------ EMAIL VERIFICATION ------------------
        public async Task<bool> SendEmailVerificationAsync(string idToken)
        {
            try
            {
                var payload = new
                {
                    requestType = "VERIFY_EMAIL",
                    idToken
                };

                string url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={firebaseApiKey}";
                var result = await _http.PostAsJsonAsync(url, payload);
                return result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ------------------ CHECK IF EMAIL VERIFIED ------------------
        private async Task<bool> IsEmailVerifiedAsync(string idToken)
        {
            try
            {
                var url = $"https://identitytoolkit.googleapis.com/v1/accounts:lookup?key={firebaseApiKey}";
                var payload = new { idToken };

                var response = await _http.PostAsJsonAsync(url, payload);
                if (!response.IsSuccessStatusCode) return false;

                var data = await response.Content.ReadFromJsonAsync<LookupResponse>();
                return data?.Users?.FirstOrDefault()?.EmailVerified ?? false;
            }
            catch
            {
                return false;
            }
        }

        // ------------------ PASSWORD RESET ------------------
        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                var payload = new
                {
                    requestType = "PASSWORD_RESET",
                    email
                };

                string url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={firebaseApiKey}";
                var result = await _http.PostAsJsonAsync(url, payload);
                return result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ------------------ LOG OUT ------------------
        public async Task SignOutAsync()
        {
            UserId = null;
            Email = null;
            IdToken = null;

            await _js.InvokeVoidAsync("localStorage.removeItem", "userId");
            await _js.InvokeVoidAsync("localStorage.removeItem", "email");
            await _js.InvokeVoidAsync("localStorage.removeItem", "idToken");
        }

        // ------------------ LOAD FROM LOCAL STORAGE ------------------
        //public async Task LoadFromStorageAsync()
        //{
        //    UserId = await _js.InvokeAsync<string>("localStorage.getItem", "userId");
        //    Email = await _js.InvokeAsync<string>("localStorage.getItem", "email");
        //    IdToken = await _js.InvokeAsync<string>("localStorage.getItem", "idToken");
        //}
        public async Task LoadFromStorageAsync()
        {
            UserId = await _js.InvokeAsync<string>("localStorage.getItem", "userId");
            Email = await _js.InvokeAsync<string>("localStorage.getItem", "email");
            IdToken = await _js.InvokeAsync<string>("localStorage.getItem", "idToken");
            RefreshToken = await _js.InvokeAsync<string>("localStorage.getItem", "refreshToken"); // NEW
        }
        // ------------------ SAVE TO LOCAL STORAGE ------------------



        public async Task SaveToStorage()
        {
            if (!string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(IdToken) && !string.IsNullOrEmpty(Email))
            {
                await _js.InvokeVoidAsync("localStorage.setItem", "userId", UserId);
                await _js.InvokeVoidAsync("localStorage.setItem", "email", Email);
                await _js.InvokeVoidAsync("localStorage.setItem", "idToken", IdToken);
                if (!string.IsNullOrEmpty(RefreshToken))
                    await _js.InvokeVoidAsync("localStorage.setItem", "refreshToken", RefreshToken); // NEW
            }
        }


        public async Task<bool> RefreshIdTokenAsync()
        {
            if (string.IsNullOrEmpty(RefreshToken))
                return false;

            try
            {
                var data = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", RefreshToken }
        };

                var response = await _http.PostAsync(
                    $"https://securetoken.googleapis.com/v1/token?key={firebaseApiKey}",
                    new FormUrlEncodedContent(data));

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                IdToken = result["id_token"].ToString();
                RefreshToken = result["refresh_token"].ToString();
                UserId = result["user_id"].ToString();

                await SaveToStorage();

                return true;
            }
            catch
            {
                return false;
            }
        }



    

        // ------------------ Helper Classes ------------------
        private class FirebaseAuthResponse
        {
            public string IdToken { get; set; } = string.Empty;
            public string LocalId { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty; // NEW
        }

        private class LookupResponse
        {
            public List<UserInfo>? Users { get; set; }
        }

        private class UserInfo
        {
            public bool EmailVerified { get; set; }
        }

    }



}
