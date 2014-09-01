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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBuzz.SMTP
{
    public class EmailClient
    {
        #region Properties

        /// <summary>
        /// The server host.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// The server port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The server authentication username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The server authentication password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Secure connection used (SSL/TLS).
        /// </summary>
        public bool SSL { get; set; }

        /// <summary>
        /// From email address.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// To email address.
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// The subject line.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The message body.
        /// </summary>
        public string Message { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Sends the specified email message.
        /// </summary>
        /// <returns>True if the message was sent successfully. False otherwise.</returns>
        public async Task<bool> SendAsync()
        {
            SmtpClient client = new SmtpClient(Server, Port, SSL, Username, Password);
            SmtpMessage message = new SmtpMessage(From, To, null, Subject, Message);

            bool result = await client.SendMail(message);

            return result;
        }

        #endregion
    }
}
