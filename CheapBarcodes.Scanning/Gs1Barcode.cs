namespace CheapBarcodes.Scanning
{
    /// <summary>
    /// A parsed GS1 barcode: every application identifier as raw text, plus typed
    /// accessors for the common warehouse fields.
    /// </summary>
    public sealed class Gs1Barcode(IReadOnlyDictionary<string, string> applicationIdentifiers)
    {
        public IReadOnlyDictionary<string, string> ApplicationIdentifiers { get; } = applicationIdentifiers;

        public string? Sscc => GetValue("00");
        public string? Gtin => GetValue("01");
        public string? BatchOrLot => GetValue("10");
        public string? SerialNumber => GetValue("21");
        public DateOnly? ProductionDate => ParseGs1Date(GetValue("11"));
        public DateOnly? ExpiryDate => ParseGs1Date(GetValue("17"));

        public int? Count => int.TryParse(GetValue("37"), out var parsedCount) ? parsedCount : null;

        private string? GetValue(string applicationIdentifier) =>
            ApplicationIdentifiers.TryGetValue(applicationIdentifier, out var rawValue) ? rawValue : null;

        /// <summary>
        /// GS1 dates are YYMMDD; DD of 00 means end of month. YY 51-99 reads as 19xx.
        /// </summary>
        private static DateOnly? ParseGs1Date(string? rawDate)
        {
            if (rawDate is not { Length: 6 })
            {
                return null;
            }

            if (!int.TryParse(rawDate.AsSpan(0, 2), out var year)
                || !int.TryParse(rawDate.AsSpan(2, 2), out var month)
                || !int.TryParse(rawDate.AsSpan(4, 2), out var day)
                || month is < 1 or > 12)
            {
                return null;
            }

            year += year >= 51 ? 1900 : 2000;

            try
            {
                return day == 0
                    ? new DateOnly(year, month, DateTime.DaysInMonth(year, month))
                    : new DateOnly(year, month, day);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }
    }
}
