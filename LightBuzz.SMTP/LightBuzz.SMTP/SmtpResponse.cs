//
// Copyright (c) LightBuzz, Inc.
// All rights reserved.
//
// http://lightbuzz.com
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
// FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
// COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
// BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS
// OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED
// AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY
// WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//
// Based on: http://bit.ly/1q4focT by Sebastien Pertus
//

using System.Collections.Generic;
using System.Linq;

namespace LightBuzz.SMTP
{
    /// <summary>
    /// Implements an SMTP response.
    /// </summary>
    internal class SmtpResponse
    {
        #region Properties

        /// <summary>
        /// The SMTP values.
        /// </summary>
        public IList<KeyValuePair<SmtpCode, string>> Values { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of SmtpResponse.
        /// </summary>
        public SmtpResponse()
        {
            Values = new List<KeyValuePair<SmtpCode, string>>(); 
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Determines whether the SMTP response contains the specified status code.
        /// </summary>
        /// <param name="status">The SMTP code to check.</param>
        /// <returns>True if the response contains the code. False otherwise.</returns>
        public bool ContainsStatus(SmtpCode status)
        {
            if (Values.Count == 0)
            {
                return false;
            }

            return Values.Any(kvp => kvp.Key == status);
        }

        /// <summary>
        /// Specifies whether the SMTP response contains the specified message.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <returns>True if the response contains the message. False otherwise.</returns>
        public bool ContainsMessage(string message)
        {
            if (Values.Count == 0)
            {
                return false;
            }

            return Values.Any(kvp => kvp.Value.Contains(message));
        }

        #endregion
    }
}
