namespace CheapBarcodes.Services
{
    /// <summary>
    /// User-configurable phone-home settings, persisted via MAUI Preferences.
    /// </summary>
    public class ApiUploadOptions
    {
        private const string BaseUrlKey = "api_base_url";
        private const string AuthHeaderNameKey = "api_auth_header_name";
        private const string AuthHeaderValueKey = "api_auth_header_value";
        private const string AutoPostKey = "api_auto_post";

        public string BaseUrl { get; set; } = string.Empty;
        public string AuthHeaderName { get; set; } = string.Empty;
        public string AuthHeaderValue { get; set; } = string.Empty;
        public bool AutoPost { get; set; }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(BaseUrl);

        public static ApiUploadOptions Load() => new()
        {
            BaseUrl = Preferences.Default.Get(BaseUrlKey, string.Empty),
            AuthHeaderName = Preferences.Default.Get(AuthHeaderNameKey, string.Empty),
            AuthHeaderValue = Preferences.Default.Get(AuthHeaderValueKey, string.Empty),
            AutoPost = Preferences.Default.Get(AutoPostKey, false),
        };

        public void Save()
        {
            Preferences.Default.Set(BaseUrlKey, BaseUrl?.Trim() ?? string.Empty);
            Preferences.Default.Set(AuthHeaderNameKey, AuthHeaderName?.Trim() ?? string.Empty);
            Preferences.Default.Set(AuthHeaderValueKey, AuthHeaderValue?.Trim() ?? string.Empty);
            Preferences.Default.Set(AutoPostKey, AutoPost);
        }
    }
}
