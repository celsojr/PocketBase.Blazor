using System;
using System.Text.Json;
using FluentResults;
using PocketBase.Blazor.Exceptions;

namespace PocketBase.Blazor.Extensions
{
    /// <summary>
    /// Helpers to inspect failed PocketBase requests.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Returns the underlying <see cref="PocketBaseException"/> of a failed request, when available.
        /// </summary>
        public static PocketBaseException? GetPocketBaseException(this IResultBase result)
        {
            if (result is null)
                return null;

            foreach (IError error in result.Errors)
            {
                if (error is ExceptionalError exceptional && exceptional.Exception is PocketBaseException pbe)
                    return pbe;
            }

            return null;
        }

        /// <summary>
        /// Returns the HTTP status code of a failed request, when available.
        /// </summary>
        public static int? GetStatusCode(this IResultBase result)
            => result.GetPocketBaseException()?.Status;

        /// <summary>
        /// Returns the multi-factor authentication session id when PocketBase requires a second
        /// auth method (HTTP 401 with an <c>mfaId</c> payload), otherwise <c>null</c>.
        /// </summary>
        /// <example>
        /// <code>
        /// Result&lt;AuthResponse&gt; first = await pb.Collection("users").AuthWithPasswordAsync(email, password);
        /// if (first.IsFailed &amp;&amp; first.GetMfaId() is string mfaId)
        /// {
        ///     // ... prompt the user for the second factor ...
        ///     Result&lt;AuthResponse&gt; done = await pb.Collection("users")
        ///         .AuthWithOtpAsync(otpId, code, mfaId: mfaId);
        /// }
        /// </code>
        /// </example>
        public static string? GetMfaId(this IResultBase result)
            => result.GetPocketBaseException()?.MfaId;

        /// <summary>
        /// Returns the deserialized <c>data</c> object of a failed request, when available.
        /// </summary>
        public static JsonElement? GetErrorData(this IResultBase result)
            => result.GetPocketBaseException()?.Data;
    }
}