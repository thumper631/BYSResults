// ***********************************************************************
// Assembly         : BYSResults
// Author           : James Thompson
// Created          : 05-08-2025
//
// Last Modified By : James Thompson
// Last Modified On : 05-08-2025
// ***********************************************************************
// <copyright file="Error.cs" company="BYSResults">
//     Copyright (c) NAIT. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BYSResults
{
    /// <summary>
    /// Represents an error associated with a failed operation.
    /// </summary>
    public class Error : IEquatable<Error>
    {
        /// <summary>
        /// Gets the error code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class with the specified message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public Error(string message) : this(string.Empty, message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class with the specified code and message.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        public Error(string code, string message)
        {
            Code = code ?? string.Empty; // Ensure Code is never null
            Message = message ?? string.Empty; //  Ensure Message is never null
        }

        /// <summary>
        ///  Override Equals to provide value equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as Error);
        }

        /// <summary>
        ///  Implement IEquatable.Equals
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Error? other)
        {
            if (other == null) return false;
            return Code == other.Code && Message == other.Message;
        }

        /// <summary>
        /// Override GetHashCode to provide a hash code consistent with Equals.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + Code.GetHashCode();
                hash = hash * 23 + Message.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        ///  Override the == operator
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(Error? left, Error? right)
        {
            return EqualityComparer<Error>.Default.Equals(left, right);
        }

        /// <summary>
        ///  Override the != operator
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(Error? left, Error? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Returns a string representation of the Error.
        /// </summary>
        /// <returns>A string representation of the Error.</returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Code))
            {
                return Message;
            }
            return $"{Code}: {Message}";
        }
    }
}
