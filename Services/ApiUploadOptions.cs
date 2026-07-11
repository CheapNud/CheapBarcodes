namespace CheapBarcodes.Services
{
    /// <summary>
    /// User-configurable phone-home settings. Non-sensitive values persist via
    /// MAUI Preferences; the auth header value lives in SecureStorage.
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

        public bool IsConfigured => Uri.TryCreate(BaseUrl, UriKind.Absolute, out var parsedUrl)
            && (parsedUrl.Scheme == Uri.UriSchemeHttps || parsedUrl.Scheme == Uri.UriSchemeHttp);

        /// <summary>
        /// The auth header is only ever sent over HTTPS (see ScanApiClient).
        /// </summary>
        public bool IsHttps => Uri.TryCreate(BaseUrl, UriKind.Absolute, out var parsedUrl)
            && parsedUrl.Scheme == Uri.UriSchemeHttps;

        public static async Task<ApiUploadOptions> LoadAsync() => new()
        {
            BaseUrl = Preferences.Default.Get(BaseUrlKey, string.Empty),
            AuthHeaderName = Preferences.Default.Get(AuthHeaderNameKey, string.Empty),
            AuthHeaderValue = await SecureStorage.Default.GetAsync(AuthHeaderValueKey) ?? string.Empty,
            AutoPost = Preferences.Default.Get(AutoPostKey, false),
        };

        public async Task SaveAsync()
        {
            Preferences.Default.Set(BaseUrlKey, BaseUrl?.Trim() ?? string.Empty);
            Preferences.Default.Set(AuthHeaderNameKey, AuthHeaderName?.Trim() ?? string.Empty);
            Preferences.Default.Set(AutoPostKey, AutoPost);
            await SecureStorage.Default.SetAsync(AuthHeaderValueKey, AuthHeaderValue?.Trim() ?? string.Empty);

            // Clean up any plaintext value written by earlier builds
            Preferences.Default.Remove(AuthHeaderValueKey);
        }
    }
}
