// ***********************************************************************
// Assembly         : BYSResults
// Author           : James Thompson
// Created          : 05-08-2025
//
// Last Modified By : James Thompson
// Last Modified On : 05-08-2025
// ***********************************************************************
// <copyright file="ResultT.cs" company="BYSResults">
//     Copyright (c) NAIT. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace BYSResults
{
    /// <summary>
    /// Represents the outcome of an operation that may or may not return a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value returned by the operation.</typeparam>
    public class Result<T> : Result
    {
        /// <summary>
        /// Gets the value returned by the operation.  This will be the default value for <typeparamref name="T"/>
        /// if the operation failed.  Use <see cref="IsSuccess"/> to determine if a value is actually available.
        /// </summary>
        public T? Value { get; protected set; } // Changed to protected set

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{T}"/> class.
        /// </summary>
        public Result() : this(default) { } // Added default constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{T}"/> class. This constructor is protected; use the static factory methods.
        /// </summary>
        protected Result(T? value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a successful result with the specified value.
        /// </summary>
        /// <param name="value">The value returned by the operation.</param>
        /// <returns>A successful result with the value.</returns>
        public static Result<T> Success(T value)
        {
            return new Result<T>(value) { IsSuccess = true };
        }

        /// <summary>
        /// Creates a failure result with the specified error.
        /// </summary>
        /// <param name="error">The error associated with the failure.</param>
        /// <returns>A failure result.</returns>
        public new static Result<T> Failure(Error error)
        {
            return new Result<T>(default) { IsSuccess = false, Errors = new List<Error> { error } };
        }

        /// <summary>
        /// Creates a failure result with the specified errors.
        /// </summary>
        /// <param name="errors">The errors associated with the failure.</param>
        /// <returns>A failure result.</returns>
        public new static Result<T> Failure(IEnumerable<Error> errors)
        {
            if (errors == null || !errors.Any())
            {
                throw new ArgumentException("At least one error must be provided for a failure result.", nameof(errors));
            }
            return new Result<T>(default) { IsSuccess = false, Errors = errors.ToList() };
        }

        /// <summary>
        /// Creates a failure result with the specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>A failure result.</returns>
        public new static Result<T> Failure(string message) => Failure(new Error(message));

        /// <summary>
        /// Creates a failure result with the specified error code and message.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <returns>A failure result.</returns>
        public new static Result<T> Failure(string code, string message) => Failure(new Error(code, message));

        /// <summary>
        /// Executes a function and returns a successful result with its return value, or a failure result if an exception is thrown.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>A successful result with the function's return value, or a failure result with the exception details.</returns>
        public static Result<T> Try(Func<T> func)
        {
            try
            {
                return Success(func());
            }
            catch (Exception ex)
            {
                return new Result<T>().AddError(ex);
            }
        }

        /// <summary>
        /// Creates a new Result, combining the results of multiple other Results.
        /// If all input results are successful, the new result is also successful.
        /// If any input result is a failure, the new result is also a failure, and its errors are the combined errors of all input results.
        /// </summary>
        /// <param name="results">The Results to combine.</param>
        /// <returns>A new Result representing the combined outcome.</returns>
        public static new Result<T> Combine(params Result[] results)
        {
            if (results == null || results.Length == 0)
            {
                throw new ArgumentException("At least one result must be provided to combine.", nameof(results));
            }

            var errors = results.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToList();
            if (errors.Any())
            {
                return new Result<T>(default) { IsSuccess = false, Errors = errors };
            }

            return Success(default!); //  default! to suppress nullable warning.  If all results are success, then the value doesn't matter for the combine.
        }

        /// <summary>
        ///  Implicit conversion from a value of type T to a successful Result<T>
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Result<T>(T value) => Success(value);

        /// <summary>
        /// Maps the value of a successful result to a new value using the specified function.
        /// If the result is a failure, the original failure is propagated.
        /// </summary>
        /// <typeparam name="TNext">The type of the new value.</typeparam>
        /// <param name="func">The function to apply to the value.</param>
        /// <returns>A new result with the mapped value, or the original failure.</returns>
        public Result<TNext> Map<TNext>(Func<T, TNext> func)
        {
            if (IsFailure)
            {
                return Result<TNext>.Failure(Errors);
            }
            return Result<TNext>.Success(func(Value!)); // Use the null-forgiving operator because IsSuccess is checked.
        }

        /// <summary>
        /// Binds the value of a successful result to a new result using the specified function.
        /// If the result is a failure, the original failure is propagated.
        /// </summary>
        /// <typeparam name="TNext">The type of the value of the new result.</typeparam>
        /// <param name="func">The function to apply to the value, which returns a new result.</param>
        /// <returns>The result returned by the function, or the original failure.</returns>
        public Result<TNext> Bind<TNext>(Func<T, Result<TNext>> func)
        {
            if (IsFailure)
            {
                return Result<TNext>.Failure(Errors);
            }
            return func(Value!);  // Use the null-forgiving operator because IsSuccess is checked.
        }

        /// <summary>
        /// Adds an error to the result.  This method is only effective on a failure result.
        /// </summary>
        /// <param name="error">The error to add.</param>
        /// <returns>The Result instance with the added error.</returns>
        public new Result<T> AddError(Error error)
        {
            base.AddError(error);
            return this;
        }

        /// <summary>
        /// Adds errors to the result.  This method is only effective on a failure result.
        /// </summary>
        /// <param name="errors">The errors to add.</param>
        /// <returns>The Result instance with the added errors.</returns>
        public new Result<T> AddErrors(IEnumerable<Error> errors)
        {
            base.AddErrors(errors);
            return this;
        }

        /// <summary>
        /// Sets the value of the result.  This should only be called on a successful result.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>The Result instance with the value set.</returns>
        public Result<T> WithValue(T value)
        {
            if (IsSuccess)
            {
                Value = value;
            }
            else
            {
                throw new InvalidOperationException("Cannot set the value of a failed result.  The result must be successful.");
            }

            return this;
        }

        /// <summary>
        /// Gets the value if successful, or returns the specified default value if failed.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the result is a failure.</param>
        /// <returns>The result value if successful, otherwise the default value.</returns>
        public T GetValueOr(T defaultValue)
        {
            return IsSuccess ? Value! : defaultValue;
        }

        /// <summary>
        /// Gets the value if successful, or returns the value from the specified function if failed.
        /// </summary>
        /// <param name="defaultValueFunc">A function that returns the default value if the result is a failure.</param>
        /// <returns>The result value if successful, otherwise the value from the function.</returns>
        public T GetValueOr(Func<T> defaultValueFunc)
        {
            return IsSuccess ? Value! : defaultValueFunc();
        }

        /// <summary>
        /// Returns this result if successful, or the specified alternative result if failed.
        /// </summary>
        /// <param name="alternativeResult">The alternative result to return if this result is a failure.</param>
        /// <returns>This result if successful, otherwise the alternative result.</returns>
        public Result<T> OrElse(Result<T> alternativeResult)
        {
            return IsSuccess ? this : alternativeResult;
        }

        /// <summary>
        /// Returns this result if successful, or the result from the specified function if failed.
        /// </summary>
        /// <param name="alternativeResultFunc">A function that returns an alternative result if this result is a failure.</param>
        /// <returns>This result if successful, otherwise the result from the function.</returns>
        public Result<T> OrElse(Func<Result<T>> alternativeResultFunc)
        {
            return IsSuccess ? this : alternativeResultFunc();
        }

        /// <summary>
        /// Executes an action with the value without modifying the result. Useful for side effects like logging.
        /// </summary>
        /// <param name="action">The action to execute with the value if successful.</param>
        /// <returns>The same result instance.</returns>
        public Result<T> Tap(Action<T> action)
        {
            if (IsSuccess)
            {
                action(Value!);
            }
            return this;
        }

        /// <summary>
        /// Executes an action if the result is a failure, without modifying the result.
        /// </summary>
        /// <param name="action">The action to execute if failed, receiving the errors.</param>
        /// <returns>The same result instance.</returns>
        public new Result<T> TapOnFailure(Action<IReadOnlyList<Error>> action)
        {
            if (IsFailure)
            {
                action(Errors);
            }
            return this;
        }

        /// <summary>
        /// Executes a function and returns a new result if this result is successful.
        /// </summary>
        /// <param name="func">The function to execute if successful, receiving the value.</param>
        /// <returns>A new result from the function if successful, otherwise this failure.</returns>
        public Result<T> OnSuccess(Func<T, Result<T>> func)
        {
            if (IsSuccess)
            {
                return func(Value!);
            }
            return this;
        }

        /// <summary>
        /// Executes a function and returns a new result if this result is a failure.
        /// </summary>
        /// <param name="func">The function to execute if failed, receiving the errors.</param>
        /// <returns>A new result from the function if failed, otherwise this success.</returns>
        public Result<T> OnFailure(Func<IReadOnlyList<Error>, Result<T>> func)
        {
            if (IsFailure)
            {
                return func(Errors);
            }
            return this;
        }

        /// <summary>
        /// Ensures that a condition on the value is met, otherwise adds an error and converts to failure.
        /// </summary>
        /// <param name="predicate">The condition that must be true for the value.</param>
        /// <param name="error">The error to add if the condition is false.</param>
        /// <returns>This result if the condition is met, otherwise a failure result with the error.</returns>
        public Result<T> Ensure(Func<T, bool> predicate, Error error)
        {
            if (IsFailure)
            {
                return this;
            }

            if (!predicate(Value!))
            {
                return AddError(error);
            }

            return this;
        }

        /// <summary>
        /// Ensures that a condition on the value is met, otherwise adds an error and converts to failure.
        /// </summary>
        /// <param name="predicate">The condition that must be true for the value.</param>
        /// <param name="errorMessage">The error message if the condition is false.</param>
        /// <returns>This result if the condition is met, otherwise a failure result with the error.</returns>
        public Result<T> Ensure(Func<T, bool> predicate, string errorMessage)
        {
            return Ensure(predicate, new Error(errorMessage));
        }

        /// <summary>
        /// Asynchronously maps the value of a successful result to a new value using the specified function.
        /// If the result is a failure, the original failure is propagated.
        /// </summary>
        /// <typeparam name="TNext">The type of the new value.</typeparam>
        /// <param name="func">The async function to apply to the value.</param>
        /// <returns>A task containing a new result with the mapped value, or the original failure.</returns>
        public async Task<Result<TNext>> MapAsync<TNext>(Func<T, Task<TNext>> func)
        {
            if (IsFailure)
            {
                return Result<TNext>.Failure(Errors);
            }
            var result = await func(Value!);
            return Result<TNext>.Success(result);
        }

        /// <summary>
        /// Asynchronously binds the value of a successful result to a new result using the specified function.
        /// If the result is a failure, the original failure is propagated.
        /// </summary>
        /// <typeparam name="TNext">The type of the value of the new result.</typeparam>
        /// <param name="func">The async function to apply to the value, which returns a new result.</param>
        /// <returns>A task containing the result returned by the function, or the original failure.</returns>
        public async Task<Result<TNext>> BindAsync<TNext>(Func<T, Task<Result<TNext>>> func)
        {
            if (IsFailure)
            {
                return Result<TNext>.Failure(Errors);
            }
            return await func(Value!);
        }

        /// <summary>
        /// Asynchronously executes a function and returns a successful result with its return value, or a failure result if an exception is thrown.
        /// </summary>
        /// <param name="func">The async function to execute.</param>
        /// <returns>A task containing a successful result with the function's return value, or a failure result with the exception details.</returns>
        public static async Task<Result<T>> TryAsync(Func<Task<T>> func)
        {
            try
            {
                var result = await func();
                return Success(result);
            }
            catch (Exception ex)
            {
                return new Result<T>().AddError(ex);
            }
        }

        /// <summary>
        /// Asynchronously executes an action with the value without modifying the result. Useful for side effects like logging.
        /// </summary>
        /// <param name="action">The async action to execute with the value if successful.</param>
        /// <returns>A task containing the same result instance.</returns>
        public async Task<Result<T>> TapAsync(Func<T, Task> action)
        {
            if (IsSuccess)
            {
                await action(Value!);
            }
            return this;
        }

        /// <summary>
        /// Executes one of two functions depending on the result state.
        /// </summary>
        /// <param name="onSuccess">The function to execute if the result is successful, receiving the value.</param>
        /// <param name="onFailure">The function to execute if the result is a failure.</param>
        public void Match(Action<T> onSuccess, Action<IReadOnlyList<Error>> onFailure)
        {
            if (IsSuccess)
            {
                onSuccess(Value!);
            }
            else
            {
                onFailure(Errors);
            }
        }

        /// <summary>
        /// Executes one of two functions depending on the result state and returns a value.
        /// </summary>
        /// <typeparam name="TReturn">The type of the return value.</typeparam>
        /// <param name="onSuccess">The function to execute if the result is successful, receiving the value.</param>
        /// <param name="onFailure">The function to execute if the result is a failure.</param>
        /// <returns>The value returned by either function.</returns>
        public TReturn Match<TReturn>(Func<T, TReturn> onSuccess, Func<IReadOnlyList<Error>, TReturn> onFailure)
        {
            if (IsSuccess)
            {
                return onSuccess(Value!);
            }
            else
            {
                return onFailure(Errors);
            }
        }
    }
}
