namespace CheapBarcodes.Scanning
{
    /// <summary>
    /// GTIN/EAN/UPC check-digit validation and normalization (GS1 mod-10).
    /// Covers EAN-8, UPC-A (12), EAN-13 and GTIN-14.
    /// </summary>
    public static class Gtin
    {
        public static bool IsValid(string? code)
        {
            if (string.IsNullOrEmpty(code) || code.Length is not (8 or 12 or 13 or 14))
            {
                return false;
            }

            foreach (var digit in code)
            {
                if (!char.IsAsciiDigit(digit))
                {
                    return false;
                }
            }

            return CalculateCheckDigit(code.AsSpan(0, code.Length - 1)) == code[^1] - '0';
        }

        /// <summary>
        /// Validates and left-pads to GTIN-14 so EAN-13/UPC-A/EAN-8 variants of the
        /// same item key match a single value. Leading zeros never affect the check
        /// digit because weights are anchored at the right.
        /// </summary>
        public static bool TryNormalize(string? code, out string gtin14)
        {
            if (!IsValid(code))
            {
                gtin14 = string.Empty;
                return false;
            }

            gtin14 = code!.PadLeft(14, '0');
            return true;
        }

        /// <summary>
        /// GS1 mod-10 check digit for the given digits (without the check digit):
        /// weights 3,1,3,1... starting from the rightmost data digit.
        /// </summary>
        public static int CalculateCheckDigit(ReadOnlySpan<char> dataDigits)
        {
            int sum = 0;
            int weight = 3;
            for (int position = dataDigits.Length - 1; position >= 0; position--)
            {
                sum += (dataDigits[position] - '0') * weight;
                weight = 4 - weight; // alternates 3,1,3,1...
            }

            return (10 - sum % 10) % 10;
        }
    }
}
