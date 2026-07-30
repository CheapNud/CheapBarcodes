using CheapBarcodes.Scanning;
using Xunit;

namespace CheapBarcodes.Scanning.Tests
{
    public class IntentScannerProfileTests
    {
        [Fact]
        public void AllKnown_PresetsAreWellFormed()
        {
            var presets = IntentScannerProfile.AllKnown;

            Assert.NotEmpty(presets);
            Assert.All(presets, preset =>
            {
                Assert.NotEmpty(preset.Actions);
                Assert.All(preset.Actions, action => Assert.False(string.IsNullOrWhiteSpace(action)));

                // A profile must have at least one way to extract data
                Assert.True(preset.DataExtraKeys.Length + preset.ByteArrayExtraKeys.Length > 0);
                Assert.NotNull(preset.DataEncoding);
            });
        }

        [Fact]
        public void AllKnown_NoDuplicateActionsAcrossPresets()
        {
            var allActions = IntentScannerProfile.AllKnown.SelectMany(preset => preset.Actions).ToList();

            Assert.Equal(allActions.Count, allActions.Distinct().Count());
        }

        [Fact]
        public void LengthExtraKey_OnlySetWhenByteArrayKeysExist()
        {
            Assert.All(IntentScannerProfile.AllKnown, preset =>
            {
                if (preset.LengthExtraKey != null)
                {
                    Assert.NotEmpty(preset.ByteArrayExtraKeys);
                }
            });
        }
    }
}
