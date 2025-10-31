using System;
using System.Linq;
using BYSResults;
using Xunit;

namespace BYSResults.Tests
{
    public class ResultTests
    {
        [Fact]
        public void Success_ShouldCreateSuccessfulResult()
        {
            var result = Result.Success();

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Failure_WithSingleError_ShouldCreateFailureResult()
        {
            var error = new Error("ERR001", "Something went wrong");

            var result = Result.Failure(error);

            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Single(result.Errors, error);
            Assert.Equal(error, result.FirstError);
        }

        [Fact]
        public void Failure_WithErrorEnumerable_ShouldThrowWhenEmpty()
        {
            Assert.Throws<ArgumentException>(() => Result.Failure(Enumerable.Empty<Error>()));
        }

        [Fact]
        public void Failure_WithErrorEnumerable_ShouldCreateFailureResult()
        {
            var errors = new[]
            {
                new Error("ERR001", "First"),
                new Error("ERR002", "Second")
            };

            var result = Result.Failure(errors);

            Assert.False(result.IsSuccess);
            Assert.Equal(errors.Length, result.Errors.Count);
            Assert.Contains(errors[0], result.Errors);
            Assert.Contains(errors[1], result.Errors);
        }

        [Fact]
        public void Combine_AllSuccess_ShouldReturnSuccess()
        {
            var results = new[]
            {
                Result.Success(),
                Result.Success()
            };

            var combined = Result.Combine(results);

            Assert.True(combined.IsSuccess);
            Assert.Empty(combined.Errors);
        }

        [Fact]
        public void Combine_WithNullArray_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => Result.Combine(null!));
        }

        [Fact]
        public void Combine_WithEmptyArray_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => Result.Combine(Array.Empty<Result>()));
        }

        [Fact]
        public void Combine_WithFailures_ShouldAggregateErrors()
        {
            var error1 = new Error("ERR1", "one");
            var error2 = new Error("ERR2", "two");
            var results = new[]
            {
                Result.Success(),
                Result.Failure(error1),
                Result.Failure(error2)
            };

            var combined = Result.Combine(results);

            Assert.True(combined.IsFailure);
            Assert.Equal(2, combined.Errors.Count);
            Assert.Contains(error1, combined.Errors);
            Assert.Contains(error2, combined.Errors);
        }

        [Fact]
        public void AddError_ShouldConvertSuccessToFailure()
        {
            var result = Result.Success();
            var error = new Error("ERR", "Oops");

            result.AddError(error);

            Assert.True(result.IsFailure);
            Assert.Contains(error, result.Errors);
        }

        [Fact]
        public void AddError_OnFailure_ShouldAppendError()
        {
            var original = new Error("ERR1", "existing");
            var additional = new Error("ERR2", "additional");
            var result = Result.Failure(original);

            result.AddError(additional);

            Assert.True(result.IsFailure);
            Assert.Equal(2, result.Errors.Count);
            Assert.Contains(original, result.Errors);
            Assert.Contains(additional, result.Errors);
        }

        [Fact]
        public void AddErrors_ShouldConvertSuccessToFailureAndAppendErrors()
        {
            var result = Result.Success();
            var errors = new[]
            {
                new Error("E1", "First"),
                new Error("E2", "Second")
            };

            result.AddErrors(errors);

            Assert.True(result.IsFailure);
            Assert.Equal(errors.Length, result.Errors.Count);
            Assert.Contains(errors[0], result.Errors);
            Assert.Contains(errors[1], result.Errors);
        }

        [Fact]
        public void AddErrors_OnFailure_ShouldAppendWithoutRemovingExisting()
        {
            var existing = new[] { new Error("E0", "Existing") };
            var additional = new[]
            {
                new Error("E1", "First"),
                new Error("E2", "Second")
            };
            var result = Result.Failure(existing);

            result.AddErrors(additional);

            Assert.Equal(3, result.Errors.Count);
            Assert.Contains(existing[0], result.Errors);
            Assert.Contains(additional[0], result.Errors);
            Assert.Contains(additional[1], result.Errors);
        }

        [Fact]
        public void AddError_WithException_ShouldCaptureMessagesAndConvertToFailure()
        {
            // Test that AddError(Exception) uses exception type as code and combines messages
            var exception = new InvalidOperationException("Top level error", new Exception("Inner detail"));
            var result = Result.Success();

            result.AddError(exception);

            Assert.True(result.IsFailure);
            var captured = Assert.Single(result.Errors);
            Assert.Equal("InvalidOperationException", captured.Code);
            Assert.Equal("Top level error --> Inner detail", captured.Message);
        }

        [Fact]
        public void AddError_WithExceptionWithoutInner_ShouldUseDefaultMessage()
        {
            // Test that AddError(Exception) uses exception type as code and exception message when no inner exception
            var exception = new InvalidOperationException("Top level error");
            var result = Result.Success();

            result.AddError(exception);

            Assert.True(result.IsFailure);
            var captured = Assert.Single(result.Errors);
            Assert.Equal("InvalidOperationException", captured.Code);
            Assert.Equal("Top level error", captured.Message);
        }
    }
}
