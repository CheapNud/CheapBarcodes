namespace CheapBarcodes.Scanning
{
    /// <summary>
    /// Parses GS1-128 / GS1 DataMatrix element strings into application identifiers.
    /// Handles FNC1/GS separators (ASCII 29, \u001d) and scanner symbology prefixes (]C1 etc.).
    /// </summary>
    public static class Gs1Parser
    {
        private const char Fnc1 = '\u001d';

        // ponytail: curated AI table covering the common warehouse set, not the full
        // GS1 general specification - extend the tables when a real code needs it.
        // Value = fixed data length; -1 = variable (terminated by FNC1 or end).
        private static readonly Dictionary<string, int> KnownAis = new()
        {
            ["00"] = 18, // SSCC
            ["01"] = 14, // GTIN
            ["02"] = 14, // GTIN of contained items
            ["11"] = 6,  // production date
            ["12"] = 6,  // due date
            ["13"] = 6,  // packaging date
            ["15"] = 6,  // best before
            ["16"] = 6,  // sell by
            ["17"] = 6,  // expiry
            ["20"] = 2,  // internal variant
            ["10"] = -1, // batch/lot
            ["21"] = -1, // serial
            ["22"] = -1, // consumer product variant
            ["30"] = -1, // count (variable measure)
            ["37"] = -1, // count of contained items
            ["240"] = -1, // additional product id
            ["241"] = -1, // customer part number
            ["250"] = -1, // secondary serial
            ["251"] = -1, // reference to source entity
            ["400"] = -1, // customer order number
            ["401"] = -1, // consignment number
            ["402"] = -1, // shipment number
            ["403"] = -1, // routing code
            ["410"] = 13, ["411"] = 13, ["412"] = 13, ["413"] = 13,
            ["414"] = 13, ["415"] = 13, ["416"] = 13, ["417"] = 13, // GLNs
            ["420"] = -1, // ship-to postal code
            ["90"] = -1, ["91"] = -1, ["92"] = -1, ["93"] = -1, ["94"] = -1,
            ["95"] = -1, ["96"] = -1, ["97"] = -1, ["98"] = -1, ["99"] = -1, // internal
        };

        // Metric measure AIs are 4 digits: 3-digit prefix + decimal-position digit,
        // always 6 data characters (e.g. 3102 = net weight kg, 2 decimals)
        private static readonly HashSet<string> MeasurePrefixes =
        [
            "310", "311", "312", "313", "314", "315", "316",
            "320", "321", "322", "323", "324", "325", "326", "327", "328", "329",
            "330", "331", "332", "333", "334", "335", "336",
        ];

        public static bool TryParse(string? rawBarcode, out Gs1Barcode gs1Barcode)
        {
            gs1Barcode = new Gs1Barcode(new Dictionary<string, string>());

            if (string.IsNullOrWhiteSpace(rawBarcode))
            {
                return false;
            }

            var elementString = rawBarcode;

            // Scanner symbology identifiers like ]C1 (GS1-128), ]d2 (DataMatrix), ]Q3 (QR)
            if (elementString.StartsWith(']') && elementString.Length > 3)
            {
                elementString = elementString[3..];
            }

            var identifiers = new Dictionary<string, string>();
            int position = 0;

            while (position < elementString.Length)
            {
                // FNC1 separators terminate variable-length fields; skip them
                if (elementString[position] == Fnc1)
                {
                    position++;
                    continue;
                }

                if (!TryMatchIdentifier(elementString, position, out var identifier, out var dataLength))
                {
                    return false;
                }

                position += identifier.Length;

                string dataValue;
                if (dataLength > 0)
                {
                    if (position + dataLength > elementString.Length)
                    {
                        return false;
                    }

                    dataValue = elementString.Substring(position, dataLength);
                    position += dataLength;
                }
                else
                {
                    int separatorIndex = elementString.IndexOf(Fnc1, position);
                    int endIndex = separatorIndex < 0 ? elementString.Length : separatorIndex;
                    dataValue = elementString[position..endIndex];
                    position = endIndex;
                }

                if (dataValue.Length == 0)
                {
                    return false;
                }

                identifiers[identifier] = dataValue;
            }

            if (identifiers.Count == 0)
            {
                return false;
            }

            gs1Barcode = new Gs1Barcode(identifiers);
            return true;
        }

        private static bool TryMatchIdentifier(string elementString, int position, out string identifier, out int dataLength)
        {
            // Metric measure AIs: 4 digits, fixed 6 data characters
            if (position + 4 <= elementString.Length
                && MeasurePrefixes.Contains(elementString.Substring(position, 3))
                && char.IsAsciiDigit(elementString[position + 3]))
            {
                identifier = elementString.Substring(position, 4);
                dataLength = 6;
                return true;
            }

            for (int identifierLength = 2; identifierLength <= 3; identifierLength++)
            {
                if (position + identifierLength <= elementString.Length)
                {
                    var candidate = elementString.Substring(position, identifierLength);
                    if (KnownAis.TryGetValue(candidate, out dataLength))
                    {
                        identifier = candidate;
                        return true;
                    }
                }
            }

            identifier = string.Empty;
            dataLength = 0;
            return false;
        }
    }
}
