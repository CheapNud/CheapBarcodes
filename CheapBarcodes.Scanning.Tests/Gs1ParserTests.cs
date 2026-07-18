using CheapBarcodes.Scanning;
using Xunit;

namespace CheapBarcodes.Scanning.Tests
{
    public class Gs1ParserTests
    {
        private const string Gs = "\u001d";

        [Fact]
        public void TryParse_FixedLengthAis()
        {
            Assert.True(Gs1Parser.TryParse("010400638133393117260731", out var gs1));
            Assert.Equal("04006381333931", gs1.Gtin);
            Assert.Equal(new DateOnly(2026, 7, 31), gs1.ExpiryDate);
        }

        [Fact]
        public void TryParse_StripsSymbologyIdentifier()
        {
            Assert.True(Gs1Parser.TryParse("]C10104006381333931", out var gs1));
            Assert.Equal("04006381333931", gs1.Gtin);
        }

        [Fact]
        public void TryParse_VariableLengthWithFnc1Separator()
        {
            Assert.True(Gs1Parser.TryParse($"10ABC123{Gs}21SER0001", out var gs1));
            Assert.Equal("ABC123", gs1.BatchOrLot);
            Assert.Equal("SER0001", gs1.SerialNumber);
        }

        [Fact]
        public void TryParse_VariableLengthRunsToEnd()
        {
            Assert.True(Gs1Parser.TryParse("010400638133393110LOT42", out var gs1));
            Assert.Equal("04006381333931", gs1.Gtin);
            Assert.Equal("LOT42", gs1.BatchOrLot);
        }

        [Fact]
        public void TryParse_CountAndSscc()
        {
            Assert.True(Gs1Parser.TryParse($"00123456789012345675{Gs}3724", out var gs1));
            Assert.Equal("123456789012345675", gs1.Sscc);
            Assert.Equal(24, gs1.Count);
        }

        [Fact]
        public void TryParse_MeasureAiTakesFourDigitIdentifier()
        {
            Assert.True(Gs1Parser.TryParse("3102001250", out var gs1));
            Assert.Equal("001250", gs1.ApplicationIdentifiers["3102"]);
        }

        [Fact]
        public void TryParse_DateEndOfMonthAndCentury()
        {
            Assert.True(Gs1Parser.TryParse("17990600", out var withDayZero));
            Assert.Equal(new DateOnly(1999, 6, 30), withDayZero.ExpiryDate);

            Assert.True(Gs1Parser.TryParse("11500101", out var century));
            Assert.Equal(new DateOnly(2050, 1, 1), century.ProductionDate);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("ABC")]           // no AI
        [InlineData("01123")]         // GTIN too short
        [InlineData("10")]            // variable AI with empty value
        [InlineData("5012345678900")] // plain EAN-13 (no leading AI) is not an element string
        public void TryParse_RejectsNonGs1Input(string? raw)
        {
            Assert.False(Gs1Parser.TryParse(raw, out _));
        }
    }
}
