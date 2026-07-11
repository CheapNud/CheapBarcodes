using System.Net.Http.Json;

namespace CheapBarcodes.Services
{
    /// <summary>
    /// Posts scans to the user-configured API endpoint (see ApiUploadOptions).
    /// </summary>
    public class ScanApiClient(HttpClient httpClient)
    {
        public async Task PostScanAsync(string barcode, string barcodeFormat, string scanSource, DateTime timestamp, CancellationToken cancellationToken = default)
        {
            var uploadOptions = ApiUploadOptions.Load();
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

            var apiResponse = await httpClient.SendAsync(apiRequest, cancellationToken);
            apiResponse.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Returns the HTTP status code of a GET against the configured URL, for the settings page test button.
        /// </summary>
        public async Task<string> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            var uploadOptions = ApiUploadOptions.Load();
            if (!uploadOptions.IsConfigured)
            {
                return "No API URL configured";
            }

            using var apiRequest = BuildRequest(HttpMethod.Get, uploadOptions);
            var apiResponse = await httpClient.SendAsync(apiRequest, cancellationToken);
            return $"{(int)apiResponse.StatusCode} {apiResponse.StatusCode}";
        }

        private static HttpRequestMessage BuildRequest(HttpMethod method, ApiUploadOptions uploadOptions)
        {
            var apiRequest = new HttpRequestMessage(method, uploadOptions.BaseUrl);
            if (!string.IsNullOrWhiteSpace(uploadOptions.AuthHeaderName))
            {
                apiRequest.Headers.TryAddWithoutValidation(uploadOptions.AuthHeaderName, uploadOptions.AuthHeaderValue);
            }

            return apiRequest;
        }
    }
}
