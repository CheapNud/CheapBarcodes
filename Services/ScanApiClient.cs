using System.Net.Http.Json;

namespace CheapBarcodes.Services
{
    /// <summary>
    /// Posts scans to the user-configured API endpoint (see ApiUploadOptions).
    /// Uses its own HttpClient with redirects disabled so the auth header
    /// cannot leak to a redirect target, and only sends auth over HTTPS.
    /// </summary>
    public class ScanApiClient
    {
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);

        private readonly HttpClient _httpClient = new(new HttpClientHandler { AllowAutoRedirect = false })
        {
            Timeout = RequestTimeout,
        };

        public async Task PostScanAsync(string barcode, string barcodeFormat, string scanSource, DateTime timestamp, CancellationToken cancellationToken = default)
        {
            var uploadOptions = await ApiUploadOptions.LoadAsync();
            if (!uploadOptions.IsConfigured)
            {
                return;
            }

            using var apiRequest = BuildRequest(HttpMethod.Post, uploadOptions);
            apiRequest.Content = JsonContent.Create(new
            {
                barcode,
                format = barcodeFormat,
                source = scanSource,
                timestamp,
                device = DeviceInfo.Current.Name,
            });

            var apiResponse = await _httpClient.SendAsync(apiRequest, cancellationToken);
            apiResponse.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Returns the HTTP status code of a GET against the configured URL, for the settings page test button.
        /// </summary>
        public async Task<string> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            var uploadOptions = await ApiUploadOptions.LoadAsync();
            if (!uploadOptions.IsConfigured)
            {
                return "No valid API URL configured";
            }

            using var apiRequest = BuildRequest(HttpMethod.Get, uploadOptions);
            var apiResponse = await _httpClient.SendAsync(apiRequest, cancellationToken);
            return $"{(int)apiResponse.StatusCode} {apiResponse.StatusCode}";
        }

        private static HttpRequestMessage BuildRequest(HttpMethod method, ApiUploadOptions uploadOptions)
        {
            var apiRequest = new HttpRequestMessage(method, uploadOptions.BaseUrl);

            // Never put credentials on a cleartext connection
            if (uploadOptions.IsHttps && !string.IsNullOrWhiteSpace(uploadOptions.AuthHeaderName))
            {
                apiRequest.Headers.TryAddWithoutValidation(uploadOptions.AuthHeaderName, uploadOptions.AuthHeaderValue);
            }

            return apiRequest;
        }
    }
}
