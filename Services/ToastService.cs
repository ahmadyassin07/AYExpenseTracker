namespace AYExpenseTracker.Services
{
    public class ToastService
    {

        public event Action<string, string>? OnShow;

        public void ShowToast(string message, string type = "success")
        {
            OnShow?.Invoke(message, type);
        }


    }
}
