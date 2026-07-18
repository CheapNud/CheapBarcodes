using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CheapBarcodes.Services
{
    /// <summary>
    /// Posts scans to the user-configured API endpoint (see ApiUploadOptions).
    /// Uses its own HttpClient with redirects disabled so credentials cannot leak
    /// to a redirect target, and only sends auth over HTTPS. When a token endpoint
    /// is configured, OAuth client-credentials takes precedence over header auth,
    /// with an in-memory token cache and a single forced refresh on 401.
    /// </summary>
    public class ScanApiClient(ILogger<ScanApiClient>? logger = null)
    {
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan TokenExpirySafetyMargin = TimeSpan.FromSeconds(60);

        private readonly HttpClient _httpClient = new(new HttpClientHandler { AllowAutoRedirect = false })
        {
            Timeout = RequestTimeout,
        };

        private string? _cachedToken;
        private DateTime _tokenExpiresAt = DateTime.MinValue;

        public async Task PostScanAsync(ApiUploadOptions uploadOptions, string barcode, string barcodeFormat, string scanSource, DateTimeOffset timestamp, CancellationToken cancellationToken = default)
        {
            if (!uploadOptions.IsConfigured)
            {
                return;
            }

            var apiResponse = await SendWithAuthAsync(uploadOptions, () =>
            {
                var apiRequest = new HttpRequestMessage(HttpMethod.Post, uploadOptions.BaseUrl);
                apiRequest.Content = JsonContent.Create(new
                {
                    barcode,
                    format = barcodeFormat,
                    source = scanSource,
                    timestamp,
                    device = DeviceInfo.Current.Name,
                });
                return apiRequest;
            }, cancellationToken);

            apiResponse.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Returns the HTTP status code of a GET against the configured URL, for the settings page test button.
        /// </summary>
        public async Task<string> TestConnectionAsync(ApiUploadOptions uploadOptions, CancellationToken cancellationToken = default)
        {
            if (!uploadOptions.IsConfigured)
            {
                return "No valid API URL configured";
            }

            var apiResponse = await SendWithAuthAsync(uploadOptions,
                () => new HttpRequestMessage(HttpMethod.Get, uploadOptions.BaseUrl), cancellationToken);
            return $"{(int)apiResponse.StatusCode} {apiResponse.StatusCode}";
        }

        /// <summary>
        /// Sends the request with the applicable auth attached; on 401 with OAuth
        /// active, refreshes the token once and retries. requestFactory is invoked
        /// per attempt because a request message cannot be reused after sending.
        /// </summary>
        private async Task<HttpResponseMessage> SendWithAuthAsync(ApiUploadOptions uploadOptions, Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken)
        {
            var apiResponse = await SendOnceAsync(uploadOptions, requestFactory, forceTokenRefresh: false, cancellationToken);

            if (apiResponse.StatusCode == HttpStatusCode.Unauthorized && uploadOptions.IsOAuthConfigured && uploadOptions.IsHttps)
            {
                apiResponse.Dispose();
                apiResponse = await SendOnceAsync(uploadOptions, requestFactory, forceTokenRefresh: true, cancellationToken);
            }

            return apiResponse;
        }

        private async Task<HttpResponseMessage> SendOnceAsync(ApiUploadOptions uploadOptions, Func<HttpRequestMessage> requestFactory, bool forceTokenRefresh, CancellationToken cancellationToken)
        {
            using var apiRequest = requestFactory();

            // Never put credentials on a cleartext connection
            if (uploadOptions.IsHttps)
            {
                if (uploadOptions.IsOAuthConfigured)
                {
                    var accessToken = await GetAccessTokenAsync(uploadOptions, forceTokenRefresh, cancellationToken);
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        apiRequest.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(uploadOptions.AuthHeaderName))
                {
                    apiRequest.Headers.TryAddWithoutValidation(uploadOptions.AuthHeaderName, uploadOptions.AuthHeaderValue);
                }
            }

            return await _httpClient.SendAsync(apiRequest, cancellationToken);
        }

        private async Task<string?> GetAccessTokenAsync(ApiUploadOptions uploadOptions, bool forceRefresh, CancellationToken cancellationToken)
        {
            if (!forceRefresh && _cachedToken != null && DateTime.UtcNow < _tokenExpiresAt)
            {
                return _cachedToken;
            }

            try
            {
                var tokenFields = new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = uploadOptions.ClientId,
                    ["client_secret"] = uploadOptions.ClientSecret,
                };

                if (!string.IsNullOrWhiteSpace(uploadOptions.Scope))
                {
                    tokenFields["scope"] = uploadOptions.Scope;
                }

                using var tokenResponse = await _httpClient.PostAsync(uploadOptions.TokenEndpoint,
                    new FormUrlEncodedContent(tokenFields), cancellationToken);
                tokenResponse.EnsureSuccessStatusCode();

                using var tokenJson = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync(cancellationToken));
                _cachedToken = tokenJson.RootElement.GetProperty("access_token").GetString();

                var expiresInSeconds = tokenJson.RootElement.TryGetProperty("expires_in", out var expiresElement)
                    ? expiresElement.GetInt32()
                    : 3600;
                _tokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds) - TokenExpirySafetyMargin;

                return _cachedToken;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Token acquisition failed");
                _cachedToken = null;
                return null;
            }
        }
    }
}
