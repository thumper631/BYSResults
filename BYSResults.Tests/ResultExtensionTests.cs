using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BYSResults;
using Xunit;

namespace BYSResults.Tests
{
    /// <summary>
    /// Unit tests for new Result pattern extension methods.
    /// Tests Match, Try, GetValueOr, OrElse, Tap, OnSuccess, OnFailure, Ensure, and async methods.
    /// </summary>
    public class ResultExtensionTests
    {
        #region Match Tests

        [Fact]
        public void Result_Match_OnSuccess_ShouldExecuteSuccessAction()
        {
            // Arrange
            var result = Result.Success();
            var executed = false;

            // Act
            result.Match(
                onSuccess: () => executed = true,
                onFailure: _ => executed = false
            );

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void Result_Match_OnFailure_ShouldExecuteFailureAction()
        {
            // Arrange
            var error = new Error("TEST", "Error message");
            var result = Result.Failure(error);
            IReadOnlyList<Error>? capturedErrors = null;

            // Act
            result.Match(
                onSuccess: () => { },
                onFailure: errors => capturedErrors = errors
            );

            // Assert
            Assert.NotNull(capturedErrors);
            Assert.Contains(error, capturedErrors);
        }

        [Fact]
        public void Result_MatchWithReturn_ShouldReturnCorrectValue()
        {
            // Arrange
            var successResult = Result.Success();
            var failureResult = Result.Failure("Error");

            // Act
            var successValue = successResult.Match(
                onSuccess: () => "success",
                onFailure: _ => "failure"
            );
            var failureValue = failureResult.Match(
                onSuccess: () => "success",
                onFailure: _ => "failure"
            );

            // Assert
            Assert.Equal("success", successValue);
            Assert.Equal("failure", failureValue);
        }

        [Fact]
        public void ResultT_Match_OnSuccess_ShouldReceiveValue()
        {
            // Arrange
            var result = Result<int>.Success(42);
            int capturedValue = 0;

            // Act
            result.Match(
                onSuccess: value => capturedValue = value,
                onFailure: _ => { }
            );

            // Assert
            Assert.Equal(42, capturedValue);
        }

        [Fact]
        public void ResultT_MatchWithReturn_ShouldTransformValue()
        {
            // Arrange
            var result = Result<int>.Success(10);

            // Act
            var output = result.Match(
                onSuccess: value => value * 2,
                onFailure: _ => 0
            );

            // Assert
            Assert.Equal(20, output);
        }

        #endregion

        #region Try Tests

        [Fact]
        public void Result_Try_WithSuccessfulAction_ShouldReturnSuccess()
        {
            // Act
            var result = Result.Try(() => { });

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Result_Try_WithException_ShouldReturnFailure()
        {
            // Test that Try catches exceptions and converts them to errors with exception type as code
            // Act
            var result = Result.Try(() => throw new InvalidOperationException("Test error"));

            // Assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Equal("InvalidOperationException", result.FirstError?.Code);
            Assert.Contains("Test error", result.FirstError?.Message);
        }

        [Fact]
        public void ResultT_Try_WithSuccessfulFunc_ShouldReturnValue()
        {
            // Act
            var result = Result<int>.Try(() => 42);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void ResultT_Try_WithException_ShouldReturnFailure()
        {
            // Test that Try<T> catches exceptions and uses exception type as error code
            // Act
            var result = Result<string>.Try(() => throw new ArgumentException("Invalid argument"));

            // Assert
            Assert.True(result.IsFailure);
            Assert.Null(result.Value);
            Assert.Equal("ArgumentException", result.FirstError?.Code);
            Assert.Contains("Invalid argument", result.FirstError?.Message);
        }

        #endregion

        #region GetValueOr and OrElse Tests

        [Fact]
        public void GetValueOr_OnSuccess_ShouldReturnValue()
        {
            // Arrange
            var result = Result<int>.Success(42);

            // Act
            var value = result.GetValueOr(0);

            // Assert
            Assert.Equal(42, value);
        }

        [Fact]
        public void GetValueOr_OnFailure_ShouldReturnDefault()
        {
            // Arrange
            var result = Result<int>.Failure("Error");

            // Act
            var value = result.GetValueOr(99);

            // Assert
            Assert.Equal(99, value);
        }

        [Fact]
        public void GetValueOr_WithFunc_OnFailure_ShouldCallFunc()
        {
            // Arrange
            var result = Result<string>.Failure("Error");
            var funcCalled = false;

            // Act
            var value = result.GetValueOr(() =>
            {
                funcCalled = true;
                return "default";
            });

            // Assert
            Assert.True(funcCalled);
            Assert.Equal("default", value);
        }

        [Fact]
        public void OrElse_OnSuccess_ShouldReturnOriginal()
        {
            // Arrange
            var result = Result<int>.Success(42);
            var alternative = Result<int>.Success(99);

            // Act
            var output = result.OrElse(alternative);

            // Assert
            Assert.Equal(42, output.Value);
        }

        [Fact]
        public void OrElse_OnFailure_ShouldReturnAlternative()
        {
            // Arrange
            var result = Result<int>.Failure("Error");
            var alternative = Result<int>.Success(99);

            // Act
            var output = result.OrElse(alternative);

            // Assert
            Assert.True(output.IsSuccess);
            Assert.Equal(99, output.Value);
        }

        [Fact]
        public void OrElse_WithFunc_OnFailure_ShouldCallFunc()
        {
            // Arrange
            var result = Result<int>.Failure("Error");
            var funcCalled = false;

            // Act
            var output = result.OrElse(() =>
            {
                funcCalled = true;
                return Result<int>.Success(100);
            });

            // Assert
            Assert.True(funcCalled);
            Assert.Equal(100, output.Value);
        }

        #endregion

        #region Tap Tests

        [Fact]
        public void Result_Tap_ShouldExecuteAction()
        {
            // Arrange
            var result = Result.Success();
            var executed = false;

            // Act
            var output = result.Tap(() => executed = true);

            // Assert
            Assert.True(executed);
            Assert.Same(result, output);
        }

        [Fact]
        public void Result_TapOnSuccess_OnSuccess_ShouldExecute()
        {
            // Arrange
            var result = Result.Success();
            var executed = false;

            // Act
            result.TapOnSuccess(() => executed = true);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void Result_TapOnSuccess_OnFailure_ShouldNotExecute()
        {
            // Arrange
            var result = Result.Failure("Error");
            var executed = false;

            // Act
            result.TapOnSuccess(() => executed = true);

            // Assert
            Assert.False(executed);
        }

        [Fact]
        public void Result_TapOnFailure_OnFailure_ShouldExecute()
        {
            // Arrange
            var error = new Error("ERR", "message");
            var result = Result.Failure(error);
            IReadOnlyList<Error>? capturedErrors = null;

            // Act
            result.TapOnFailure(errors => capturedErrors = errors);

            // Assert
            Assert.NotNull(capturedErrors);
            Assert.Contains(error, capturedErrors);
        }

        [Fact]
        public void ResultT_Tap_OnSuccess_ShouldReceiveValue()
        {
            // Arrange
            var result = Result<int>.Success(42);
            int capturedValue = 0;

            // Act
            var output = result.Tap(value => capturedValue = value);

            // Assert
            Assert.Equal(42, capturedValue);
            Assert.Same(result, output);
        }

        [Fact]
        public void ResultT_Tap_OnFailure_ShouldNotExecute()
        {
            // Arrange
            var result = Result<int>.Failure("Error");
            var executed = false;

            // Act
            result.Tap(_ => executed = true);

            // Assert
            Assert.False(executed);
        }

        #endregion

        #region OnSuccess and OnFailure Tests

        [Fact]
        public void Result_OnSuccess_OnSuccess_ShouldExecuteAndReturnNewResult()
        {
            // Arrange
            var result = Result.Success();
            var newResult = Result.Success();

            // Act
            var output = result.OnSuccess(() => newResult);

            // Assert
            Assert.Same(newResult, output);
        }

        [Fact]
        public void Result_OnSuccess_OnFailure_ShouldReturnOriginal()
        {
            // Arrange
            var result = Result.Failure("Error");

            // Act
            var output = result.OnSuccess(() => Result.Success());

            // Assert
            Assert.Same(result, output);
        }

        [Fact]
        public void Result_OnFailure_OnFailure_ShouldExecuteAndReturnNewResult()
        {
            // Arrange
            var result = Result.Failure("Error");
            var newResult = Result.Success();

            // Act
            var output = result.OnFailure(errors => newResult);

            // Assert
            Assert.Same(newResult, output);
        }

        [Fact]
        public void ResultT_OnSuccess_ShouldChainTransformation()
        {
            // Arrange
            var result = Result<int>.Success(10);

            // Act
            var output = result.OnSuccess(value => Result<int>.Success(value * 2));

            // Assert
            Assert.True(output.IsSuccess);
            Assert.Equal(20, output.Value);
        }

        [Fact]
        public void ResultT_OnFailure_OnFailure_ShouldReturnRecoveredResult()
        {
            // Arrange
            var result = Result<int>.Failure("Error");

            // Act
            var output = result.OnFailure(errors => Result<int>.Success(42));

            // Assert
            Assert.True(output.IsSuccess);
            Assert.Equal(42, output.Value);
        }

        #endregion

        #region Ensure Tests

        [Fact]
        public void Result_Ensure_WithTruePredicate_ShouldRemainSuccess()
        {
            // Arrange
            var result = Result.Success();

            // Act
            var output = result.Ensure(() => true, "Should not fail");

            // Assert
            Assert.True(output.IsSuccess);
        }

        [Fact]
        public void Result_Ensure_WithFalsePredicate_ShouldBecomeFailure()
        {
            // Arrange
            var result = Result.Success();

            // Act
            var output = result.Ensure(() => false, "Validation failed");

            // Assert
            Assert.True(output.IsFailure);
            Assert.Contains("Validation failed", output.FirstError?.Message);
        }

        [Fact]
        public void Result_Ensure_OnExistingFailure_ShouldRemainFailure()
        {
            // Arrange
            var result = Result.Failure("Original error");

            // Act
            var output = result.Ensure(() => true, "Another check");

            // Assert
            Assert.True(output.IsFailure);
            Assert.Single(output.Errors);
        }

        [Fact]
        public void ResultT_Ensure_WithValidValue_ShouldRemainSuccess()
        {
            // Arrange
            var result = Result<int>.Success(10);

            // Act
            var output = result.Ensure(value => value > 0, "Value must be positive");

            // Assert
            Assert.True(output.IsSuccess);
            Assert.Equal(10, output.Value);
        }

        [Fact]
        public void ResultT_Ensure_WithInvalidValue_ShouldBecomeFailure()
        {
            // Arrange
            var result = Result<int>.Success(-5);

            // Act
            var output = result.Ensure(value => value > 0, "Value must be positive");

            // Assert
            Assert.True(output.IsFailure);
            Assert.Contains("Value must be positive", output.FirstError?.Message);
        }

        [Fact]
        public void ResultT_Ensure_CanChainMultipleValidations()
        {
            // Arrange
            var result = Result<int>.Success(15);

            // Act
            var output = result
                .Ensure(value => value > 0, "Must be positive")
                .Ensure(value => value < 100, "Must be less than 100")
                .Ensure(value => value % 5 == 0, "Must be divisible by 5");

            // Assert
            Assert.True(output.IsSuccess);
        }

        [Fact]
        public void ResultT_Ensure_ChainedValidations_ShouldAccumulateErrors()
        {
            // Arrange
            var result = Result<int>.Success(-5);

            // Act
            var output = result
                .Ensure(value => value > 0, "Must be positive")
                .Ensure(value => value < 100, "Must be less than 100");

            // Assert
            Assert.True(output.IsFailure);
            Assert.Single(output.Errors); // Only first validation fails
        }

        #endregion

        #region Async Tests

        [Fact]
        public async Task ResultT_MapAsync_OnSuccess_ShouldTransformValue()
        {
            // Arrange
            var result = Result<int>.Success(5);

            // Act
            var output = await result.MapAsync(async value =>
            {
                await Task.Delay(1);
                return value * 2;
            });

            // Assert
            Assert.True(output.IsSuccess);
            Assert.Equal(10, output.Value);
        }

        [Fact]
        public async Task ResultT_MapAsync_OnFailure_ShouldPropagateFailure()
        {
            // Arrange
            var error = new Error("ERR", "Failed");
            var result = Result<int>.Failure(error);

            // Act
            var output = await result.MapAsync(async value =>
            {
                await Task.Delay(1);
                return value * 2;
            });

            // Assert
            Assert.True(output.IsFailure);
            Assert.Contains(error, output.Errors);
        }

        [Fact]
        public async Task ResultT_BindAsync_OnSuccess_ShouldChainResults()
        {
            // Arrange
            var result = Result<int>.Success(5);

            // Act
            var output = await result.BindAsync(async value =>
            {
                await Task.Delay(1);
                return Result<string>.Success($"Value: {value}");
            });

            // Assert
            Assert.True(output.IsSuccess);
            Assert.Equal("Value: 5", output.Value);
        }

        [Fact]
        public async Task ResultT_BindAsync_OnFailure_ShouldPropagateFailure()
        {
            // Arrange
            var error = new Error("ERR", "Failed");
            var result = Result<int>.Failure(error);

            // Act
            var output = await result.BindAsync(async value =>
            {
                await Task.Delay(1);
                return Result<string>.Success($"Value: {value}");
            });

            // Assert
            Assert.True(output.IsFailure);
            Assert.Contains(error, output.Errors);
        }

        [Fact]
        public async Task ResultT_TryAsync_WithSuccess_ShouldReturnValue()
        {
            // Act
            var result = await Result<int>.TryAsync(async () =>
            {
                await Task.Delay(1);
                return 42;
            });

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public async Task ResultT_TryAsync_WithException_ShouldReturnFailure()
        {
            // Test that TryAsync catches exceptions and uses exception type as error code
            // Act
            var result = await Result<int>.TryAsync(async () =>
            {
                await Task.Delay(1);
                throw new InvalidOperationException("Async error");
            });

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("InvalidOperationException", result.FirstError?.Code);
            Assert.Contains("Async error", result.FirstError?.Message);
        }

        [Fact]
        public async Task ResultT_TapAsync_OnSuccess_ShouldExecute()
        {
            // Arrange
            var result = Result<int>.Success(42);
            int capturedValue = 0;

            // Act
            var output = await result.TapAsync(async value =>
            {
                await Task.Delay(1);
                capturedValue = value;
            });

            // Assert
            Assert.Equal(42, capturedValue);
            Assert.Same(result, output);
        }

        [Fact]
        public async Task ResultT_TapAsync_OnFailure_ShouldNotExecute()
        {
            // Arrange
            var result = Result<int>.Failure("Error");
            var executed = false;

            // Act
            await result.TapAsync(async value =>
            {
                await Task.Delay(1);
                executed = true;
            });

            // Assert
            Assert.False(executed);
        }

        #endregion
    }
}
