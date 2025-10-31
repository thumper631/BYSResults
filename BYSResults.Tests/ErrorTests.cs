using BYSResults;
using Xunit;

namespace BYSResults.Tests
{
    public class ErrorTests
    {
        [Fact]
        public void Constructor_ShouldNormalizeNullValues()
        {
            var error = new Error(null!, null!);

            Assert.Equal(string.Empty, error.Code);
            Assert.Equal(string.Empty, error.Message);
        }

        [Fact]
        public void Equals_ShouldReturnTrueForSameValues()
        {
            var first = new Error("CODE", "Message");
            var second = new Error("CODE", "Message");

            Assert.True(first.Equals(second));
            Assert.True(first == second);
            Assert.False(first != second);
        }

        [Fact]
        public void Equals_ShouldReturnFalseForDifferentValues()
        {
            var first = new Error("CODE", "Message");
            var second = new Error("OTHER", "Message");

            Assert.False(first.Equals(second));
            Assert.False(first == second);
            Assert.True(first != second);
        }

        [Fact]
        public void Equals_ShouldReturnFalseForNull()
        {
            var error = new Error("CODE", "Message");

            Assert.False(error.Equals(null));
        }

        [Fact]
        public void GetHashCode_ShouldMatchForEquivalentErrors()
        {
            var first = new Error("CODE", "Message");
            var second = new Error("CODE", "Message");

            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public void ToString_ShouldIncludeCodeWhenPresent()
        {
            var error = new Error("CODE", "Message");

            Assert.Equal("CODE: Message", error.ToString());
        }

        [Fact]
        public void ToString_ShouldReturnMessageWhenCodeMissing()
        {
            var error = new Error("Message");

            Assert.Equal("Message", error.ToString());
        }
    }
}
