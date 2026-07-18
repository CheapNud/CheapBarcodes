using CheapBarcodes.Scanning;
using Xunit;

namespace CheapBarcodes.Scanning.Tests
{
    public class GtinTests
    {
        // Canonical valid examples: EAN-13 (Stabilo), EAN-8, UPC-A
        [Theory]
        [InlineData("4006381333931")]
        [InlineData("96385074")]
        [InlineData("036000291452")]
        public void IsValid_AcceptsKnownGoodCodes(string code)
        {
            Assert.True(Gtin.IsValid(code));
        }

        [Theory]
        [InlineData("4006381333932")] // wrong check digit
        [InlineData("96385075")]
        [InlineData("400638133393")]  // 12 digits but check digit belongs to the 13-digit form
        [InlineData("40063813339")]   // invalid length
        [InlineData("40063A1333931")] // non-digit
        [InlineData("")]
        [InlineData(null)]
        public void IsValid_RejectsBadCodes(string? code)
        {
            Assert.False(Gtin.IsValid(code));
        }

        [Fact]
        public void TryNormalize_PadsToGtin14AndStaysValid()
        {
            Assert.True(Gtin.TryNormalize("4006381333931", out var gtin14));
            Assert.Equal("04006381333931", gtin14);
            Assert.True(Gtin.IsValid(gtin14));
        }

        [Fact]
        public void TryNormalize_MakesEanVariantsMatch()
        {
            Assert.True(Gtin.TryNormalize("04006381333931", out var fromGtin14));
            Assert.True(Gtin.TryNormalize("4006381333931", out var fromEan13));
            Assert.Equal(fromGtin14, fromEan13);
        }

        [Fact]
        public void TryNormalize_RejectsInvalid()
        {
            Assert.False(Gtin.TryNormalize("1234567890128X", out _));
        }

        [Fact]
        public void CalculateCheckDigit_MatchesKnownExample()
        {
            Assert.Equal(1, Gtin.CalculateCheckDigit("400638133393"));
        }
    }
}
