namespace CheapBarcodes.Services
{
    /// <summary>
    /// User-configurable phone-home settings. Non-sensitive values persist via
    /// MAUI Preferences; the auth header value and OAuth client secret live in
    /// SecureStorage.
    /// </summary>
    public class ApiUploadOptions
    {
        private const string BaseUrlKey = "api_base_url";
        private const string AuthHeaderNameKey = "api_auth_header_name";
        private const string AuthHeaderValueKey = "api_auth_header_value";
        private const string AutoPostKey = "api_auto_post";
        private const string TokenEndpointKey = "api_token_endpoint";
        private const string ClientIdKey = "api_client_id";
        private const string ClientSecretKey = "api_client_secret";
        private const string ScopeKey = "api_scope";

        public string BaseUrl { get; set; } = string.Empty;
        public string AuthHeaderName { get; set; } = string.Empty;
        public string AuthHeaderValue { get; set; } = string.Empty;
        public bool AutoPost { get; set; }

        // OAuth client-credentials add-on: activates when a token endpoint is set
        public string TokenEndpoint { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;

        public bool IsConfigured => Uri.TryCreate(BaseUrl, UriKind.Absolute, out var parsedUrl)
            && (parsedUrl.Scheme == Uri.UriSchemeHttps || parsedUrl.Scheme == Uri.UriSchemeHttp);

        /// <summary>
        /// The auth header is only ever sent over HTTPS (see ScanApiClient).
        /// </summary>
        public bool IsHttps => Uri.TryCreate(BaseUrl, UriKind.Absolute, out var parsedUrl)
            && parsedUrl.Scheme == Uri.UriSchemeHttps;

        /// <summary>
        /// OAuth requires an HTTPS token endpoint (the client secret travels in the
        /// token request) plus client id and secret.
        /// </summary>
        public bool IsOAuthConfigured => Uri.TryCreate(TokenEndpoint, UriKind.Absolute, out var tokenUrl)
            && tokenUrl.Scheme == Uri.UriSchemeHttps
            && !string.IsNullOrWhiteSpace(ClientId)
            && !string.IsNullOrWhiteSpace(ClientSecret);

        public static async Task<ApiUploadOptions> LoadAsync() => new()
        {
            BaseUrl = Preferences.Default.Get(BaseUrlKey, string.Empty),
            AuthHeaderName = Preferences.Default.Get(AuthHeaderNameKey, string.Empty),
            AuthHeaderValue = await SecureStorage.Default.GetAsync(AuthHeaderValueKey) ?? string.Empty,
            AutoPost = Preferences.Default.Get(AutoPostKey, false),
            TokenEndpoint = Preferences.Default.Get(TokenEndpointKey, string.Empty),
            ClientId = Preferences.Default.Get(ClientIdKey, string.Empty),
            ClientSecret = await SecureStorage.Default.GetAsync(ClientSecretKey) ?? string.Empty,
            Scope = Preferences.Default.Get(ScopeKey, string.Empty),
        };

        public async Task SaveAsync()
        {
            Preferences.Default.Set(BaseUrlKey, BaseUrl?.Trim() ?? string.Empty);
            Preferences.Default.Set(AuthHeaderNameKey, AuthHeaderName?.Trim() ?? string.Empty);
            Preferences.Default.Set(AutoPostKey, AutoPost);
            Preferences.Default.Set(TokenEndpointKey, TokenEndpoint?.Trim() ?? string.Empty);
            Preferences.Default.Set(ClientIdKey, ClientId?.Trim() ?? string.Empty);
            Preferences.Default.Set(ScopeKey, Scope?.Trim() ?? string.Empty);
            await SecureStorage.Default.SetAsync(AuthHeaderValueKey, AuthHeaderValue?.Trim() ?? string.Empty);
            await SecureStorage.Default.SetAsync(ClientSecretKey, ClientSecret?.Trim() ?? string.Empty);

            // Clean up any plaintext value written by earlier builds
            Preferences.Default.Remove(AuthHeaderValueKey);
        }
    }
}
