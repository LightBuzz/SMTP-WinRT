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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace LightBuzz.SMTP
{
    /// <summary>
    /// Implements an SMTP client.
    /// </summary>
    public sealed class SmtpClient : IDisposable
    {
        #region Properties

        /// <summary>
        /// SMTP server.
        /// </summary>
        private string Server { get; set; }

        /// <summary>
        /// SMTP port (usually 25).
        /// </summary>
        private int Port { get; set; }

        /// <summary>
        /// Server username.
        /// </summary>
        private string Username { get; set; }

        /// <summary>
        /// Server password.
        /// </summary>
        private string Password { get; set; }

        /// <summary>
        /// Secure connection (SSL/TLS).
        /// </summary>
        private bool SSL { get; set; }

        /// <summary>
        /// Specifies whether the client is connected to the server.
        /// </summary>
        private bool IsConnected { get; set; }

        /// <summary>
        /// Specifies whether the client is authenticated
        /// </summary>
        private bool IsAuthenticated { get; set; }

        /// <summary>
        /// SMTP Socket.
        /// </summary>
        private SmtpSocket Socket { get; set; }

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new SMTP client.
        /// </summary>
        /// <param name="server">The server host.</param>
        /// <param name="port">The server port (usually 25).</param>
        public SmtpClient(string server, int port)
        {
            Server = server;
            Port = port;
        }

        /// <summary>
        /// Creates a new SMTP client.
        /// </summary>
        /// <param name="server">The server host.</param>
        /// <param name="port">The server port (usually 25).</param>
        /// <param name="ssl">Use a secure connection (SSL/TLS).</param>
        public SmtpClient(string server, int port, bool ssl)
            : this(server, port)
        {
            SSL = ssl;
        }

        /// <summary>
        /// Creates a new SMTP client.
        /// </summary>
        /// <param name="server">The server host.</param>
        /// <param name="port">The server port (usually 25).</param>
        /// <param name="ssl">Use a secure connection (SSL/TLS).</param>
        /// <param name="username">The server username.</param>
        /// <param name="password">The server password.</param>
        public SmtpClient(string server, int port, bool ssl, string username, string password)
            : this(server, port, ssl)
        {
            Username = username;
            Password = password;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Sends the specified email message.
        /// </summary>
        /// <param name="message">The email message.</param>
        /// <returns>True if the email was sent successfully. False otherwise.</returns>
        public async Task<SmtpResult> SendMailAsync(EmailMessage message)
        {
            if (!IsConnected)
            {
                await ConnectAsync();
            }

            if (!IsConnected)
            {
                return SmtpResult.ConnectionFailed;
            }

            if (!IsAuthenticated)
            {
                await AuthenticateAsync();
            }

            if (!IsAuthenticated)
            {
                return SmtpResult.AuthenticationFailed;
            }

            SmtpResponse response = await Socket.Send(string.Format("Mail From:<{0}>", Username));

            if (!response.ContainsStatus(SmtpCode.RequestedMailActionCompleted))
            {
                return SmtpResult.CouldNotCreateMail;
            }

            foreach (EmailRecipient to in message.To)
            {
                SmtpResponse responseTo = await Socket.Send(string.Format("Rcpt To:<{0}>", to.Address));

                if (!responseTo.ContainsStatus(SmtpCode.RequestedMailActionCompleted))
                {
                    break;
                }
            }

            foreach (EmailRecipient to in message.CC)
            {
                SmtpResponse responseTo = await Socket.Send(string.Format("Rcpt To:<{0}>", to.Address));

                if (!responseTo.ContainsStatus(SmtpCode.RequestedMailActionCompleted))
                {
                    break;
                }
            }

            foreach (EmailRecipient to in message.Bcc)
            {
                SmtpResponse responseTo = await Socket.Send(string.Format("Rcpt To:<{0}>", to.Address));

                if (!responseTo.ContainsStatus(SmtpCode.RequestedMailActionCompleted))
                {
                    break;
                }
            }

            SmtpResponse responseData = await Socket.Send(string.Format("Data"));

            if (!responseData.ContainsStatus(SmtpCode.StartMailInput))
            {
                return SmtpResult.CouldNotCreateMail;
            }

            SmtpResponse repsonseMessage = await Socket.Send(await BuildSmtpMailInput(message));

            if (!repsonseMessage.ContainsStatus(SmtpCode.RequestedMailActionCompleted))
            {
                return SmtpResult.CouldNotCreateMail;
            }

            SmtpResponse responseQuit = await Socket.Send("Quit");

            if (!responseQuit.ContainsStatus(SmtpCode.ServiceClosingTransmissionChannel))
            {
                return SmtpResult.CouldNotCloseTransmissionChannel;
            }

            return SmtpResult.OK;
        }

        /// <summary>
        /// [deprecated] Sends the specified email message. Use <see cref="SendMailAsync(EmailMessage)"/> instead. 
        /// </summary>
        /// <param name="message">The email message.</param>
        /// <returns>True if the email was sent successfully. False otherwise.</returns>
        public async Task<bool> SendMail(EmailMessage message)
        {
            SmtpResult result = await SendMailAsync(message);

            return result == SmtpResult.OK;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Connects to the server.
        /// </summary>
        /// <returns>True for successful connection. False otherwise.</returns>
        private async Task<bool> ConnectAsync()
        {
            try
            {
                if (IsConnected)
                {
                    Socket.Dispose();
                    IsConnected = false;
                }

                Socket = new SmtpSocket(Server, Port, SSL);

                SmtpResponse response = await Socket.EstablishConnection();

                if (response.ContainsStatus(SmtpCode.ServiceReady))
                {
                    IsConnected = true;

                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Authenticates a socket client using the specified Username and Password.
        /// </summary>
        /// <returns>True if the client was successfully authenticated. False otherwise.</returns>
        private async Task<bool> AuthenticateAsync()
        {
            if (!IsConnected)
            {
                return false;
            }

            // get the type of auth
            SmtpResponse response = await Socket.Send("EHLO " + Server);

            if (response.ContainsMessage("STARTTLS"))
            {
                SmtpResponse responseSSL = await Socket.Send("STARTTLS");

                if (responseSSL.ContainsStatus(SmtpCode.ServiceReady))
                {
                    await Socket.UpgradeToSslAsync();

                    return await AuthenticateAsync();
                }
            }

            if (response.ContainsMessage("AUTH"))
            {
                if (response.ContainsMessage("LOGIN"))
                {
                    IsAuthenticated = await AuthenticateByLoginAsync();
                }
                else if (response.ContainsMessage("PLAIN"))
                {
                    IsAuthenticated = await AuthenticateByPlainAsync();
                }
            }
            else
            {
                await Socket.Send("EHLO " + Server);

                IsAuthenticated = true;
            }

            return IsAuthenticated;
        }

        /// <summary>
        /// Authenticates a socket client using login authentication.
        /// </summary>
        /// <returns>True if the client was successfully authenticated. False otherwise.</returns>
        private async Task<bool> AuthenticateByLoginAsync()
        {
            if (!IsConnected)
            {
                return false;
            }

            SmtpResponse response = await Socket.Send("Auth Login");

            if (!response.ContainsStatus(SmtpCode.WaitingForAuthentication))
            {
                return false;
            }

            SmtpResponse responseUsername = await Socket.Send(Convert.ToBase64String(Encoding.UTF8.GetBytes(Username)));

            if (!responseUsername.ContainsStatus(SmtpCode.WaitingForAuthentication))
            {
                return false;
            }

            SmtpResponse responsePassword = await Socket.Send(Convert.ToBase64String(Encoding.UTF8.GetBytes(Password)));

            if (!responsePassword.ContainsStatus(SmtpCode.AuthenticationSuccessful))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Authenticates a socket client using plain authentication.
        /// </summary>
        /// <returns>True if the client was successfully authenticated. False otherwise.</returns>
        private async Task<bool> AuthenticateByPlainAsync()
        {
            if (!IsConnected)
            {
                return false;
            }

            SmtpResponse response = await Socket.Send("Auth Plain");

            if (!response.ContainsStatus(SmtpCode.WaitingForAuthentication))
            {
                return false;
            }

            string lineAuthentication = string.Format("{0}\0{0}\0{1}", Username, Password);

            SmtpResponse responseAuth = await Socket.Send(Convert.ToBase64String(Encoding.UTF8.GetBytes(lineAuthentication)));

            if (!responseAuth.ContainsStatus(SmtpCode.AuthenticationSuccessful))
            {
                return false;
            }

            return true;

        }

        private async Task<string> BuildSmtpMailInput(EmailMessage message)
        {
            if (Username == null)
            {
                throw new Exception("From field is missing.");
            }

            if (message.To.Count == 0 && message.CC.Count == 0 && message.Bcc.Count == 0)
            {
                throw new Exception("Recipient fields are missing (TO/CC/BCC).");
            }

            StringBuilder mailInput = new StringBuilder();

            mailInput.AppendFormat("Date: {0}{1}", DateTime.Now.ToString("ddd, dd MMM yyyy HH:mm:ss +0000"), System.Environment.NewLine);
            mailInput.AppendFormat("X-Priority: {0}{1}", message.Importance.ToXPriority(), System.Environment.NewLine);
            mailInput.AppendFormat("From: \"{0}\"<{1}>{2}", Username, Username, System.Environment.NewLine);

            mailInput.AppendFormat("To: {0}{1}", string.Join(", ", message.To.Select(r => string.Format("\"{0}\"<{1}>", r.Name, r.Address))), Environment.NewLine);
            if (message.CC.Any())
            {
                mailInput.AppendFormat("Cc: {0}{1}", string.Join(", ", message.CC.Select(r => string.Format("\"{0}\"<{1}>", r.Name, r.Address))), Environment.NewLine);
            }
            if (message.Bcc.Any())
            {
                mailInput.AppendFormat("Bcc: {0}{1}", string.Join(", ", message.Bcc.Select(r => string.Format("\"{0}\"<{1}>", r.Name, r.Address))), Environment.NewLine);
            }

            mailInput.AppendFormat("Subject: {0}{1}", message.Subject, Environment.NewLine);
            mailInput.AppendFormat("MIME-Version: 1.0{0}", Environment.NewLine);

            Guid boundary = Guid.NewGuid();
            if (message.Attachments.Any())
            {
                mailInput.AppendFormat("Content-Type: multipart/mixed; boundary=\"{0}\"{1}", boundary, Environment.NewLine);
                mailInput.AppendFormat("{0}", Environment.NewLine);
                mailInput.AppendFormat("--{0}{1}", boundary, Environment.NewLine);
            }
            if (message.Body.StartsWith("<html>"))
            {
                mailInput.AppendFormat("Content-Type: text/html; {0}", Environment.NewLine);
            }
            else
            {
                mailInput.AppendFormat("Content-Type: text/plain; charset=\"{0}\"{1}", Encoding.UTF8.WebName, Environment.NewLine);
            }

            mailInput.Append(Environment.NewLine);
            mailInput.Append(message.Body);
            mailInput.Append(Environment.NewLine);
            mailInput.Append(Environment.NewLine);

            if (message.Attachments.Any())
            {
                foreach (EmailAttachment attachment in message.Attachments)
                {
                    mailInput.AppendFormat("--{0}{1}", boundary, Environment.NewLine);
                    mailInput.AppendFormat("Content-Type: application/octet-stream;{0}", Environment.NewLine);
                    mailInput.AppendFormat("Content-Transfer-Encoding: base64{0}", Environment.NewLine);
                    if (attachment.ContentId != null)
                    {
                        mailInput.AppendFormat("Content-ID: {0}{1}", attachment.ContentId, Environment.NewLine);
                    }
                    var disposition = attachment.IsInline ? "inline" : "attachment";
                    mailInput.AppendFormat("Content-Disposition: " + disposition + "; filename= \"" + attachment.FileName + "\" {0}", Environment.NewLine);
                    mailInput.AppendFormat("Content-Disposition: attachment; filename= \"" + attachment.FileName + "\" {0}", Environment.NewLine);
                    string fileEncoded = await _encodeToBase64(attachment.Data);
                    mailInput.Append(Environment.NewLine);
                    mailInput.Append(fileEncoded);
                    mailInput.Append(Environment.NewLine);
                }
                mailInput.AppendFormat("--{0}--{1}", boundary, Environment.NewLine);
            }
            mailInput.Append(".");

            return mailInput.ToString();
        }

        /// <summary>
        /// Encode File to base 64
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>The encoded file</returns>
        private async Task<string> _encodeToBase64(IRandomAccessStreamReference data)
        {
            string encode = string.Empty;
            using (IRandomAccessStreamWithContentType stream = await data.OpenReadAsync())
            {
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);

                    byte[] bytes = new byte[reader.UnconsumedBufferLength];

                    while (reader.UnconsumedBufferLength > 0)
                    {
                        reader.ReadBytes(bytes);
                    }
                    encode = Convert.ToBase64String(bytes);
                }
            }
            return encode;
        }
        #endregion

        public void Dispose()
        {
            if (Socket != null)
            {
                Socket.Dispose();
            }
        }
    }
}