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
using Windows.Storage;

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
        public MailBox From { get; set; }

        /// <summary>
        /// The list of the email receivers.
        /// </summary>
        public List<MailBox> To { get; set; }

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
        public List<MailBox> Cc { get; set; }

        /// <summary>
        /// The BCC receivers.
        /// </summary>
        public List<MailBox> Bcc { get; set; }

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
        /// <summary>
        /// Indicates that the email has an attacment
        /// </summary>
        public bool HasAttachments { get; set; }
        /// <summary>
        /// The atthacment files to attach to the email
        /// </summary>
        public List<string> AttachedFiles { get; set; }

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
        /// <param name="from">From Mail Box object.</param>
        /// <param name="to">To Mail Box object.</param>
        /// <param name="cc">CC Mailbox object.</param>
        /// <param name="subject">Subject line.</param>
        /// <param name="body">Message body.</param>
        /// <param name="filesToAttach">Files that has to be attached</param>
        public SmtpMessage(MailBox from, MailBox to, MailBox cc, string subject, string body, List<string> filesToAttach=null)
        {
            Initialize();

            From = from;
            
            if (!String.IsNullOrEmpty(to.EmailAddress))
            {
                To.Add(to);
            }

            if (cc != null && !string.IsNullOrEmpty(cc.EmailAddress))
            {
                Cc.Add(cc);
            }

            Subject = subject;
            Body = body;
            if (filesToAttach != null)
            {
                HasAttachments = true;
                AttachedFiles = filesToAttach;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Creates the SMTP message body.
        /// </summary>
        /// <returns>The string representation of the message.</returns>
        public async Task<string> CreateMessageBody()
        {
            if (From==null)
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
            message.AppendFormat("From: \"{0}\"<{1}>{2}", From.Name,From.EmailAddress, System.Environment.NewLine);

            message.Append("To: ");

            for (int i = 0; i < To.Count; i++)
            {
                MailBox to = To[i];
                string separator = i == To.Count - 1 ? Environment.NewLine : ", ";

                message.AppendFormat("\"{0}\"<{1}>{2}", to.Name,to.EmailAddress, separator);
            }
            if (Cc.Count>0) message.Append("Cc: ");
            for (int i = 0; i < Cc.Count; i++)
            {
                MailBox cc = Cc[i];
                string separator = i == Cc.Count - 1 ? Environment.NewLine : ", ";

                message.AppendFormat("\"{0}\"<{1}>{2}", cc.Name,cc.EmailAddress, separator);
            }
            if (Cc.Count > 0) message.AppendFormat("{0}", Environment.NewLine);
            message.AppendFormat("Subject: {0}{1}", Subject, Environment.NewLine);
            message.AppendFormat("MIME-Version: 1.0{0}", Environment.NewLine);
            if (HasAttachments)
            {
                message.AppendFormat("Content-Type: multipart/mixed; boundary=\"{0}\"{1}", Boundary, Environment.NewLine);
                message.AppendFormat("{0}",Environment.NewLine);
                message.AppendFormat("--{0}{1}", Boundary, Environment.NewLine);
            }
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
            message.Append(Environment.NewLine);
            if (HasAttachments)
            {
                foreach (string file in AttachedFiles)
                {
                    message.AppendFormat("--{0}{1}", Boundary, Environment.NewLine);
                    message.AppendFormat("Content-Type: application/octet-stream;{0}", Environment.NewLine);
                    message.AppendFormat("Content-Transfer-Encoding: base64{0}", Environment.NewLine);
                    string filename = _findFileName(file);
                    message.AppendFormat("Content-Disposition: attachment; filename= \"" + filename + "\" {0}", Environment.NewLine);
                    string fileEncoded = await _encodeToBase64(file);
                    message.Append(Environment.NewLine);
                    message.Append(fileEncoded);
                    message.Append(Environment.NewLine);
                }
                message.AppendFormat("--{0}--{1}", Boundary, Environment.NewLine);
            }
            message.Append(".");

            return message.ToString();
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Encode File to base 64
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>The encoded file</returns>
        private async Task<string> _encodeToBase64(string filePath)
        {
            string encode = String.Empty;
            if (!string.IsNullOrEmpty(filePath))
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
                string fileText = await FileIO.ReadTextAsync(file);
                byte[] bytes = new byte[fileText.Length * sizeof(char)];
                System.Buffer.BlockCopy(fileText.ToCharArray(), 0, bytes, 0, bytes.Length);
                encode = Convert.ToBase64String(bytes);
            }
            return encode;
        }

        /// <summary>
        /// Extract file Name from path
        /// </summary>
        /// <param name="filePath">The file path to process</param>
        /// <returns>
        /// The file name with the extension
        /// </returns>
        private string _findFileName(string filePath)
        {
            int position = filePath.LastIndexOf('/');
            if (position == -1) position = filePath.LastIndexOf('\\');
            if (position == -1)
            {
                throw new Exception("Cannot find attached file");
            }
            string fileName= filePath.Substring(position+1, filePath.Length - position -1);
            return fileName;
        }
        /// <summary>
        /// Inizialization of the message
        /// </summary>
        private void Initialize()
        {
            To = new List<MailBox>();
            Cc = new List<MailBox>();
            Bcc = new List<MailBox>();
            Priority = SmtpPriority.Normal;
            Boundary = String.Format("KkK170891tpbkKk__FV_KKKkkkjjwq");
            TransferEncoding = "7bit";
            Encoding = Encoding.UTF8;
        }

        #endregion
    }
}
