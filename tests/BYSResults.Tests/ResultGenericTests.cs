using System;
using System.Linq;
using BYSResults;
using Xunit;

namespace BYSResults.Tests
{
    public class ResultGenericTests
    {
        [Fact]
        public void Success_ShouldStoreValue()
        {
            var result = Result<int>.Success(42);

            Assert.True(result.IsSuccess);
            Assert.Equal(42, result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Failure_ShouldPropagateErrorAndDefaultValue()
        {
            var error = new Error("ERR", "failure");

            var result = Result<int>.Failure(error);

            Assert.True(result.IsFailure);
            Assert.Equal(default, result.Value);
            Assert.Contains(error, result.Errors);
        }

        [Fact]
        public void Failure_WithEnumerable_ShouldThrowWhenEmpty()
        {
            Assert.Throws<ArgumentException>(() => Result<int>.Failure(Enumerable.Empty<Error>()));
        }

        [Fact]
        public void Failure_WithEnumerable_ShouldPropagateAllErrors()
        {
            var errors = new[]
            {
                new Error("ERR1", "first"),
                new Error("ERR2", "second")
            };

            var result = Result<int>.Failure(errors);

            Assert.True(result.IsFailure);
            Assert.Equal(0, result.Value);
            Assert.Equal(errors, result.Errors);
        }

        [Fact]
        public void Combine_AllSuccess_ShouldReturnSuccess()
        {
            var first = Result<int>.Success(1);
            var second = Result<int>.Success(2);

            var combined = Result<int>.Combine(first, second);

            Assert.True(combined.IsSuccess);
            Assert.Empty(combined.Errors);
            Assert.Equal(default, combined.Value);
        }

        [Fact]
        public void Combine_WithFailure_ShouldReturnFailure()
        {
            var error = new Error("ERR", "bad");
            var combined = Result<int>.Combine(Result.Success(), Result.Failure(error));

            Assert.True(combined.IsFailure);
            Assert.Contains(error, combined.Errors);
        }

        [Fact]
        public void Combine_WithNullArray_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => Result<int>.Combine(null!));
        }

        [Fact]
        public void Combine_WithEmptyArray_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => Result<int>.Combine(Array.Empty<Result>()));
        }

        [Fact]
        public void Map_OnSuccess_ShouldTransformValue()
        {
            var result = Result<int>.Success(2);

            var mapped = result.Map(v => v * 3);

            Assert.True(mapped.IsSuccess);
            Assert.Equal(6, mapped.Value);
        }

        [Fact]
        public void Map_OnFailure_ShouldPropagateErrors()
        {
            var error = new Error("ERR", "failed");
            var result = Result<int>.Failure(error);

            var mapped = result.Map(v => v * 2);

            Assert.True(mapped.IsFailure);
            Assert.Contains(error, mapped.Errors);
        }

        [Fact]
        public void Map_OnFailure_ShouldNotInvokeSelector()
        {
            var error = new Error("ERR", "failed");
            var result = Result<int>.Failure(error);
            var invoked = false;

            result.Map(v =>
            {
                invoked = true;
                return v;
            });

            Assert.False(invoked);
        }

        [Fact]
        public void Bind_OnSuccess_ShouldReturnNewResult()
        {
            var result = Result<int>.Success(2);

            var bound = result.Bind(v => Result<string>.Success((v * 5).ToString()));

            Assert.True(bound.IsSuccess);
            Assert.Equal("10", bound.Value);
        }

        [Fact]
        public void Bind_OnSuccess_ShouldReturnFailureFromFunction()
        {
            var error = new Error("ERR", "bad");
            var result = Result<int>.Success(3);

            var bound = result.Bind(_ => Result<string>.Failure(error));

            Assert.True(bound.IsFailure);
            Assert.Contains(error, bound.Errors);
        }

        [Fact]
        public void Bind_OnFailure_ShouldPropagateErrors()
        {
            var error = new Error("ERR", "bad");
            var result = Result<int>.Failure(error);

            var bound = result.Bind(v => Result<string>.Success(v.ToString()));

            Assert.True(bound.IsFailure);
            Assert.Contains(error, bound.Errors);
        }

        [Fact]
        public void WithValue_OnSuccess_ShouldSetValue()
        {
            var result = Result<int>.Success(1);

            var sameInstance = result.WithValue(10);

            Assert.Equal(10, result.Value);
            Assert.Same(result, sameInstance);
        }

        [Fact]
        public void WithValue_OnFailure_ShouldThrow()
        {
            var result = Result<int>.Failure(new Error("ERR", "bad"));

            Assert.Throws<InvalidOperationException>(() => result.WithValue(5));
        }

        [Fact]
        public void ImplicitConversion_ShouldCreateSuccessResult()
        {
            Result<int> result = 7;

            Assert.True(result.IsSuccess);
            Assert.Equal(7, result.Value);
            Assert.Empty(result.Errors);
        }
    }
}
