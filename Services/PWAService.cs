using Microsoft.JSInterop;

namespace AYExpenseTracker.Services
{
    public class PWAService : IDisposable
    {
        private readonly IJSRuntime _js;
        private DotNetObjectReference<PWAService>? _objRef;

        public bool IsInstallable { get; private set; }
        public bool IsUpdateAvailable { get; private set; }
        public bool IsiOS { get; private set; }
        public bool IsStandalone { get; private set; }
        public string NotificationPermission { get; private set; } = "default";

        public bool IsNotificationEnabled => NotificationPermission == "granted";

        public event Action? OnChange;

        public PWAService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task InitializeAsync()
        {
            _objRef = DotNetObjectReference.Create(this);
            await _js.InvokeVoidAsync("initPwa", _objRef);
        }

        [JSInvokable]
        public void SetInstallable(bool installable)
        {
            IsInstallable = installable;
            NotifyStateChanged();
        }

        [JSInvokable]
        public void SetUpdateAvailable(bool available)
        {
            IsUpdateAvailable = available;
            NotifyStateChanged();
        }

        [JSInvokable]
        public void SetPlatformInfo(bool isIos, bool isStandalone)
        {
            IsiOS = isIos;
            IsStandalone = isStandalone;
            NotifyStateChanged();
        }

        [JSInvokable]
        public void SetNotificationPermission(string permission)
        {
            NotificationPermission = permission;
            NotifyStateChanged();
        }

        public async Task InstallAsync()
        {
            if (IsInstallable)
            {
                var success = await _js.InvokeAsync<bool>("triggerPwaInstall");
                if (success)
                {
                    IsInstallable = false;
                    NotifyStateChanged();
                }
            }
        }

        public async Task UpdateAsync()
        {
            await _js.InvokeVoidAsync("triggerPwaUpdate");
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public void Dispose()
        {
            _objRef?.Dispose();
        }
    }
}
