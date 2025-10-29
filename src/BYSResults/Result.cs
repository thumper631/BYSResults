// ***********************************************************************
// Assembly         : BYSResults
// Author           : James Thompson
// Created          : 05-08-2025
//
// Last Modified By : James Thompson
// Last Modified On : 06-01-2025
// ***********************************************************************
// <copyright file="Result.cs" company="BYSResults">
//     Copyright (c) NAIT. All rights reserved.
// </copyright>
// <summary>
//  1.1.4
//  - Updated GetInnerException to handle null InnerException
//  1.1.3
//  - Added Revision History to readme.md
//  1.1.1 - 1.1.2
//  -  Correct issues with readme.md
//  1.1.0
//  - Added AddError(Exception exception)
//  -   added the ability to accept an exception as a error.   
//  1.0.0
//  Initial creation.
// </summary>
// ***********************************************************************

using System;
using System.Linq;

namespace BYSResults
{
    /// <summary>
    /// Represents the outcome of an operation, either success or failure, potentially with a value and/or errors.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets the list of errors associated with the operation.  This will be empty for a successful result.
        /// </summary>
        public IReadOnlyList<Error> Errors { get; protected set; }

        /// <summary>
        /// Gets the first error, if any.  If there are no errors, returns null.
        /// </summary>
        public Error? FirstError => Errors.FirstOrDefault();

        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class.
        /// </summary>
        public Result()
        {
            Errors = new List<Error>(); // Initialize Errors as a mutable list
            IsSuccess = true; // Set IsSuccess to true by default
        }

        /// <summary>
        /// Creates a successful result with no value.
        /// </summary>
        /// <returns>A successful result.</returns>
        public static Result Success()
        {
            return new Result { IsSuccess = true };
        }

        /// <summary>
        /// Creates a failure result with the specified error.
        /// </summary>
        /// <param name="error">The error associated with the failure.</param>
        /// <returns>A failure result.</returns>
        public static Result Failure(Error error)
        {
            return new Result { IsSuccess = false, Errors = new List<Error> { error } }; // Initialize with a new list
        }

        /// <summary>
        /// Creates a failure result with the specified errors.
        /// </summary>
        /// <param name="errors">The errors associated with the failure.</param>
        /// <returns>A failure result.</returns>
        public static Result Failure(IEnumerable<Error> errors)
        {
            if (errors == null || !errors.Any())
            {
                throw new ArgumentException("At least one error must be provided for a failure result.", nameof(errors));
            }
            return new Result { IsSuccess = false, Errors = errors.ToList() }; // Initialize with a new list
        }

        /// <summary>
        /// Creates a failure result with the specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>A failure result.</returns>
        public static Result Failure(string message) => Failure(new Error(message));

        /// <summary>
        /// Creates a failure result with the specified error code and message.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <returns>A failure result.</returns>
        public static Result Failure(string code, string message) => Failure(new Error(code, message));

        /// <summary>
        /// Executes an action and returns a successful result, or a failure result if an exception is thrown.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>A successful result if the action completes, or a failure result with the exception details.</returns>
        public static Result Try(Action action)
        {
            try
            {
                action();
                return Success();
            }
            catch (Exception ex)
            {
                return new Result().AddError(ex);
            }
        }

        /// <summary>
        /// Creates a new Result, combining the results of multiple other Results.
        /// If all input results are successful, the new result is also successful.
        /// If any input result is a failure, the new result is also a failure, and its errors are the combined errors of all input results.
        /// </summary>
        /// <param name="results">The Results to combine.</param>
        /// <returns>A new Result representing the combined outcome.</returns>
        public static Result Combine(params Result[] results)
        {
            if (results == null || results.Length == 0)
            {
                throw new ArgumentException("At least one result must be provided to combine.", nameof(results));
            }

            var errors = results.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToList(); // Collect errors into a list.
            if (errors.Any())
            {
                return new Result { IsSuccess = false, Errors = errors }; // Return a new Result with the combined errors
            }

            return Success();
        }

        /// <summary>
        /// Adds an error to the result.  This method is only effective on a failure result.
        /// </summary>
        /// <param name="error">The error to add.</param>
        /// <returns>The Result instance with the added error.</returns>
        public Result AddError(Error error)
        {
            if (IsSuccess) //check for isSuccess
            {
                IsSuccess = false;
            }

            if (Errors is List<Error> errorList) //check the type of errors
            {
                errorList.Add(error);
            }
            else
            {
                //if it is not a list, create a new list.
                Errors = new List<Error>(Errors) { error };
            }
            return this;
        }

        /// <summary>
        /// Adds errors to the result.  This method is only effective on a failure result.
        /// </summary>
        /// <param name="errors">The errors to add.</param>
        /// <returns>The Result instance with the added errors.</returns>
        public Result AddErrors(IEnumerable<Error> errors)
        {
            if (IsSuccess) //check for isSuccess
            {
                IsSuccess = false;
            }

            if (Errors is List<Error> errorList)
            {
                errorList.AddRange(errors);
            }
            else
            {
                Errors = new List<Error>(Errors).Concat(errors).ToList();
            }
            return this;
        }

        /// <summary>
        /// Adds an exception message to the result.  This method is only effective on a failure result.
        /// </summary>
        /// <param name="exception">exception error to add.</param>
        /// <returns>The Result instance with the added exception.</returns>
        public Result AddError(Exception exception)
        {

            if (IsSuccess) //check for isSuccess
            {
                IsSuccess = false;
            }

            Error error = new Error(exception.Message, GetInnerException(exception.InnerException).Message.ToString());


            if (Errors is List<Error> errorList) //check the type of errors
            {
                errorList.Add(error);
            }
            else
            {
                //if it is not a list, create a new list.
                Errors = new List<Error>(Errors) { error };
            }
            return this;
        }
        /// <summary>
        /// Gets the inner exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns>System.Exception.</returns>
        private Exception GetInnerException(System.Exception? ex)
        {
            if (ex == null)
            {
                return new Exception("No inner exception available.");
            }

            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            return ex;
        }

        /// <summary>
        /// Executes an action without modifying the result. Useful for side effects like logging.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>The same result instance.</returns>
        public Result Tap(Action action)
        {
            action();
            return this;
        }

        /// <summary>
        /// Executes an action if the result is successful, without modifying the result.
        /// </summary>
        /// <param name="action">The action to execute if successful.</param>
        /// <returns>The same result instance.</returns>
        public Result TapOnSuccess(Action action)
        {
            if (IsSuccess)
            {
                action();
            }
            return this;
        }

        /// <summary>
        /// Executes an action if the result is a failure, without modifying the result.
        /// </summary>
        /// <param name="action">The action to execute if failed, receiving the errors.</param>
        /// <returns>The same result instance.</returns>
        public Result TapOnFailure(Action<IReadOnlyList<Error>> action)
        {
            if (IsFailure)
            {
                action(Errors);
            }
            return this;
        }

        /// <summary>
        /// Executes an action and returns a new result if this result is successful.
        /// </summary>
        /// <param name="action">The action to execute if successful.</param>
        /// <returns>A new result from the action if successful, otherwise this failure.</returns>
        public Result OnSuccess(Func<Result> action)
        {
            if (IsSuccess)
            {
                return action();
            }
            return this;
        }

        /// <summary>
        /// Executes an action and returns a new result if this result is a failure.
        /// </summary>
        /// <param name="action">The action to execute if failed, receiving the errors.</param>
        /// <returns>A new result from the action if failed, otherwise this success.</returns>
        public Result OnFailure(Func<IReadOnlyList<Error>, Result> action)
        {
            if (IsFailure)
            {
                return action(Errors);
            }
            return this;
        }

        /// <summary>
        /// Ensures that a condition is met, otherwise adds an error and converts to failure.
        /// </summary>
        /// <param name="predicate">The condition that must be true.</param>
        /// <param name="error">The error to add if the condition is false.</param>
        /// <returns>This result if the condition is met, otherwise a failure result with the error.</returns>
        public Result Ensure(Func<bool> predicate, Error error)
        {
            if (IsFailure)
            {
                return this;
            }

            if (!predicate())
            {
                return AddError(error);
            }

            return this;
        }

        /// <summary>
        /// Ensures that a condition is met, otherwise adds an error and converts to failure.
        /// </summary>
        /// <param name="predicate">The condition that must be true.</param>
        /// <param name="errorMessage">The error message if the condition is false.</param>
        /// <returns>This result if the condition is met, otherwise a failure result with the error.</returns>
        public Result Ensure(Func<bool> predicate, string errorMessage)
        {
            return Ensure(predicate, new Error(errorMessage));
        }

        /// <summary>
        /// Executes one of two functions depending on the result state.
        /// </summary>
        /// <param name="onSuccess">The function to execute if the result is successful.</param>
        /// <param name="onFailure">The function to execute if the result is a failure.</param>
        public void Match(Action onSuccess, Action<IReadOnlyList<Error>> onFailure)
        {
            if (IsSuccess)
            {
                onSuccess();
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
        /// <param name="onSuccess">The function to execute if the result is successful.</param>
        /// <param name="onFailure">The function to execute if the result is a failure.</param>
        /// <returns>The value returned by either function.</returns>
        public TReturn Match<TReturn>(Func<TReturn> onSuccess, Func<IReadOnlyList<Error>, TReturn> onFailure)
        {
            if (IsSuccess)
            {
                return onSuccess();
            }
            else
            {
                return onFailure(Errors);
            }
        }

    }
}
