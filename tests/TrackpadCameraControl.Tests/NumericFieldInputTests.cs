using TrackpadCameraControl;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class NumericFieldInputTests
    {
        [Theory]
        [InlineData("1..5", "1.5")]
        [InlineData("1.2.3", "1.23")]
        [InlineData("abc12.3xy", "12.3")]
        [InlineData("-0.5", "-0.5")]
        [InlineData(".5", ".5")]
        [InlineData("1,", "1,")]
        [InlineData("1,,2", "1,2")]
        public void SanitizePartialFloatText_FiltersInvalid(string input, string expected)
        {
            Assert.Equal(expected, NumericFieldInput.SanitizePartialFloatText(input));
        }

        [Theory]
        [InlineData("1.25", true)]
        [InlineData("1..5", false)]
        [InlineData("-", false)]
        [InlineData("1.", false)]
        [InlineData(".", false)]
        [InlineData("nope", false)]
        public void IsCompleteFloatText_ClassicalRules(string text, bool complete)
        {
            Assert.Equal(complete, NumericFieldInput.IsCompleteFloatText(text));
        }

        [Fact]
        public void TryParseFloatText_RejectsDoubleDecimal()
        {
            Assert.False(NumericFieldInput.TryParseFloatText("1..5", out _));
        }

        [Fact]
        public void TryParseFloatText_ParsesInvariant()
        {
            Assert.True(NumericFieldInput.TryParseFloatText("1.25", out float v));
            Assert.Equal(1.25f, v);
        }

        [Fact]
        public void ModOptions_TryParseFloat_UsesStrictCompleteRules()
        {
            Assert.False(ModOptions.TryParseFloat("1..5", out _));
            Assert.True(ModOptions.TryParseFloat("2.5", out float v));
            Assert.Equal(2.5f, v);
        }
    }
}
