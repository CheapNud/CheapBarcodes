namespace CheapBarcodes.Services
{
    /// <summary>
    /// Logging settings. Read synchronously at startup (logger is built before the
    /// app runs), so these live in Preferences - a Seq ingestion key is a
    /// low-privilege write-only telemetry credential, unlike the API auth values.
    /// </summary>
    public class LoggingOptions
    {
        private const string SeqUrlKey = "log_seq_url";
        private const string SeqApiKeyKey = "log_seq_api_key";

        public string SeqUrl { get; set; } = string.Empty;
        public string SeqApiKey { get; set; } = string.Empty;

        public bool IsSeqConfigured => Uri.TryCreate(SeqUrl, UriKind.Absolute, out var parsedUrl)
            && (parsedUrl.Scheme == Uri.UriSchemeHttps || parsedUrl.Scheme == Uri.UriSchemeHttp);

        public static string LogDirectory => Path.Combine(FileSystem.AppDataDirectory, "logs");

        public static LoggingOptions Load() => new()
        {
            SeqUrl = Preferences.Default.Get(SeqUrlKey, string.Empty),
            SeqApiKey = Preferences.Default.Get(SeqApiKeyKey, string.Empty),
        };

        public void Save()
        {
            Preferences.Default.Set(SeqUrlKey, SeqUrl?.Trim() ?? string.Empty);
            Preferences.Default.Set(SeqApiKeyKey, SeqApiKey?.Trim() ?? string.Empty);
        }
    }
}
