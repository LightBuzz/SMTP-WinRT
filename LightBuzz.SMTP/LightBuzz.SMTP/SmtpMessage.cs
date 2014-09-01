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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBuzz.SMTP
{
    /// <summary>
    /// Implements an SMTP message.
    /// </summary>
    public class SmtpMessage
    {
        #region Constants

        readonly string DATE_FORMAT = "ddd, dd MMM yyyy HH:mm:ss +0000";

        #endregion

        #region Properties

        /// <summary>
        /// The encoding of the email.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// The email sender address.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// The list of the email receivers.
        /// </summary>
        public List<string> To { get; set; }

        /// <summary>
        /// The boundary of the email.
        /// </summary>
        public string Boundary { get; set; }

        /// <summary>
        /// The encoding used to transfer the email (7 bit, Base64 or Quoted-Printable).
        /// </summary>
        public string TransferEncoding { get; set; }

        /// <summary>
        /// The CC receivers.
        /// </summary>
        public List<string> Cc { get; set; }

        /// <summary>
        /// The BCC receivers.
        /// </summary>
        public List<string> Bcc { get; set; }

        /// <summary>
        /// The priority of the email.
        /// </summary>
        public SmtpPriority Priority { get; set; }

        /// <summary>
        /// The subject of the email.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The body of the email.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Specifies whether to use HTML.
        /// </summary>
        public bool IsHtml { get; set; }

        /// <summary>
        /// Content Type ("text/html" or "text/plain").
        /// </summary>
        public string ContentType { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new SMTP email message.
        /// </summary>
        public SmtpMessage()
        {
            Initialize();
        }

        /// <summary>
        /// Creates a new SMTP email message.
        /// </summary>
        /// <param name="from">From email address.</param>
        /// <param name="to">To email address.</param>
        /// <param name="cc">CC email address.</param>
        /// <param name="subject">Subject line.</param>
        /// <param name="body">Message body.</param>
        public SmtpMessage(string from, string to, string cc, string subject, string body)
        {
            Initialize();

            From = from;
            
            if (!string.IsNullOrEmpty(to))
            {
                To.Add(to);
            }

            if (!string.IsNullOrEmpty(cc))
            {
                Cc.Add(cc);
            }

            Subject = subject;
            Body = body;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Creates the SMTP message body.
        /// </summary>
        /// <returns>The string representation of the message.</returns>
        public string CreateMessageBody()
        {
            if (string.IsNullOrEmpty(From))
            {
                throw new Exception("From field is missing.");
            }

            if (To.Count == 0)
            {
                throw new Exception("To field is missing.");
            }

            StringBuilder message = new StringBuilder();

            message.AppendFormat("Date: {0}{1}", DateTime.Now.ToString(DATE_FORMAT), System.Environment.NewLine);
            message.AppendFormat("X-Priority: {0}{1}", ((byte)Priority).ToString(), System.Environment.NewLine);
            message.AppendFormat("From: {0}{1}", From, System.Environment.NewLine);

            message.Append("To: ");

            for (int i = 0; i < To.Count; i++)
            {
                string to = To[i];
                string separator = i == To.Count - 1 ? Environment.NewLine : ", ";

                message.AppendFormat("{0}{1}", to, separator);
            }

            message.Append("Cc: ");

            for (int i = 0; i < Cc.Count; i++)
            {
                string cc = Cc[i];
                string separator = i == Cc.Count - 1 ? Environment.NewLine : ", ";

                message.AppendFormat("{0}{1}", cc, separator);
            }

            message.AppendFormat("MIME-Version: 1.0{0}", Environment.NewLine);
            message.AppendFormat("Content-Transfer-Encoding: {0}{1}", TransferEncoding, Environment.NewLine);
            message.AppendFormat("Content-Disposition: inline{0}", Environment.NewLine);
            message.AppendFormat("Subject: {0}{1}", Subject, Environment.NewLine);

            if (IsHtml)
            {
                message.AppendFormat("Content-Type: text/html; {0}", System.Environment.NewLine);
            }
            else
            {
                message.AppendFormat("Content-Type: text/plain; charset=\"{0}\"{1}", Encoding.WebName, System.Environment.NewLine);
            }

            message.Append(Environment.NewLine);
            message.Append(Body);
            message.Append(Environment.NewLine);
            message.Append(".");

            return message.ToString();
        }

        #endregion

        #region Private methods

        private void Initialize()
        {
            To = new List<string>();
            Cc = new List<string>();
            Bcc = new List<string>();
            Priority = SmtpPriority.Normal;
            Boundary = String.Format("_MESSAGE_ID_{0}", Guid.NewGuid().ToString());
            TransferEncoding = "7bit";
            Encoding = Encoding.UTF8;
        }

        #endregion
    }
}
