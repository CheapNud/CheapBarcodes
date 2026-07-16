using System.Text.Json;

namespace CheapBarcodes.Services
{
    /// <summary>
    /// Offline queue for phone-home: failed posts persist in Preferences and are
    /// retried before the next post, when connectivity returns, or on demand.
    /// Queue order is preserved - pending scans always upload before new ones.
    /// </summary>
    public class ScanUploadQueue
    {
        private const string QueueKey = "scan_upload_queue";
        // ponytail: hard cap, oldest entries drop first; raise if real deployments overflow
        private const int MaxQueuedScans = 500;

        private readonly ScanApiClient _scanApi;
        private readonly SemaphoreSlim _flushLock = new(1, 1);

        public ScanUploadQueue(ScanApiClient scanApi)
        {
            _scanApi = scanApi;
            Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        }

        public int PendingCount => LoadQueue().Count;

        /// <summary>
        /// Drains any pending scans, then posts the new one; on failure the scan
        /// is queued. Returns whether the new scan was posted directly and how
        /// many scans remain queued.
        /// </summary>
        public async Task<(bool Posted, int Pending)> PostOrEnqueueAsync(ApiUploadOptions uploadOptions, QueuedScan scan)
        {
            await _flushLock.WaitAsync();
            try
            {
                var queue = LoadQueue();
                queue.Add(scan);
                queue = await DrainAsync(uploadOptions, queue);
                SaveQueue(queue);
                return (!queue.Contains(scan), queue.Count);
            }
            finally
            {
                _flushLock.Release();
            }
        }

        /// <summary>
        /// Attempts to upload everything currently queued. Returns the remaining count.
        /// </summary>
        public async Task<int> TryFlushAsync(ApiUploadOptions uploadOptions)
        {
            await _flushLock.WaitAsync();
            try
            {
                var queue = await DrainAsync(uploadOptions, LoadQueue());
                SaveQueue(queue);
                return queue.Count;
            }
            finally
            {
                _flushLock.Release();
            }
        }

        private async Task<List<QueuedScan>> DrainAsync(ApiUploadOptions uploadOptions, List<QueuedScan> queue)
        {
            while (queue.Count > 0)
            {
                var nextScan = queue[0];
                try
                {
                    await _scanApi.PostScanAsync(uploadOptions, nextScan.Barcode, nextScan.Format, nextScan.Source, nextScan.Timestamp);
                    queue.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Queue drain stopped: {ex.Message}");
                    break;
                }
            }

            // Enforce the cap after draining so a long outage loses the oldest scans, not the newest
            while (queue.Count > MaxQueuedScans)
            {
                queue.RemoveAt(0);
            }

            return queue;
        }

        private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            try
            {
                if (e.NetworkAccess != NetworkAccess.Internet)
                {
                    return;
                }

                var uploadOptions = await ApiUploadOptions.LoadAsync();
                if (uploadOptions.IsConfigured && PendingCount > 0)
                {
                    await TryFlushAsync(uploadOptions);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connectivity flush failed: {ex.Message}");
            }
        }

        private static List<QueuedScan> LoadQueue()
        {
            try
            {
                return JsonSerializer.Deserialize<List<QueuedScan>>(
                    Preferences.Default.Get(QueueKey, "[]")) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

        private static void SaveQueue(List<QueuedScan> queue)
        {
            Preferences.Default.Set(QueueKey, JsonSerializer.Serialize(queue));
        }
    }

    public class QueuedScan
    {
        public string Barcode { get; set; } = "";
        public string Format { get; set; } = "";
        public string Source { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
