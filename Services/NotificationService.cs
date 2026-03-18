using Microsoft.JSInterop;

namespace AYExpenseTracker.Services
{
    public class NotificationService
    {
        private readonly IJSRuntime _js;

        public NotificationService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<string> RequestPermissionAsync()
        {
            try
            {
                return await _js.InvokeAsync<string>("notificationHelper.requestPermission");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error requesting notification permission: {ex.Message}");
                return "denied";
            }
        }

        public async Task SendNotificationAsync(string title, string body, string icon = "wallet.png")
        {
            try
            {
                await _js.InvokeVoidAsync("notificationHelper.sendNotification", title, new
                {
                    body = body,
                    icon = icon,
                    badge = icon,
                    vibrate = new[] { 100, 50, 100 }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending notification: {ex.Message}");
            }
        }
    }
}
