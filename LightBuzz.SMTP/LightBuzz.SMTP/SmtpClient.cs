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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace LightBuzz.SMTP
{
    /// <summary>
    /// Implements an SMTP client.
    /// </summary>
    public class SmtpClient
    {
        #region Properties

        /// <summary>
        /// SMTP server.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// SMTP port (usually 25).
        /// </summary>
        public int Port { get; set; }
        
        /// <summary>
        /// Server username.
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Server password.
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Secure connection (SSL/TLS).
        /// </summary>
        public bool SSL { get; set; }
        
        /// <summary>
        /// Specifies whether the client is connected to the server.
        /// </summary>
        public bool IsConnected { get; set; }
        
        /// <summary>
        /// Specifies whether the client is authenticated
        /// </summary>
        public bool IsAuthenticated { get; set; }
        
        /// <summary>
        /// SMTP Socket.
        /// </summary>
        public SmtpSocket Socket { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new SMTP client.
        /// </summary>
        public SmtpClient()
        {
        }

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
        /// Connects to the server.
        /// </summary>
        /// <returns>True for successful connection. False otherwise.</returns>
        public async Task<bool> Connect()
        {
            try
            {
                if (IsConnected)
                {
                    Socket.Close();
                    IsConnected = false;
                }

                Socket = new SmtpSocket(Server, Port, SSL, Username, Password);

                SmtpResponse response = await Socket.EstablishConnection();

                if (response.Contains(SmtpCode.ServiceReady))
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
        public async Task<bool> Authenticate()
        {
            if (!IsConnected)
            {
                throw new Exception("Client is not connected");
            }

            // get the type of auth
            SmtpResponse response = await Socket.Send("EHLO " + Server);

            if (response.Contains("STARTTLS"))
            {
                SmtpResponse responseSSL = await Socket.Send("STARTTLS");

                if (responseSSL.Contains(SmtpCode.ServiceReady))
                {
                    await Socket.UpgradeToSslAsync();

                    return await Authenticate();
                }
            }

            if (response.Contains("AUTH"))
            {
                if (response.Contains("LOGIN"))
                {
                    IsAuthenticated = await AuthenticateByLogin();
                }
                else if (response.Contains("PLAIN"))
                {
                    IsAuthenticated = await AuthenticateByPlain();
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
        public async Task<bool> AuthenticateByLogin()
        {
            if (!IsConnected)
            {
                return false;
            }

            SmtpResponse response = await Socket.Send("Auth Login");

            if (!response.Contains(SmtpCode.WaitingForAuthentication))
            {
                return false;
            }

            SmtpResponse responseUsername = await Socket.Send(Convert.ToBase64String(Encoding.UTF8.GetBytes(Username)));

            if (!responseUsername.Contains(SmtpCode.WaitingForAuthentication))
            {
                return false;
            }

            SmtpResponse responsePassword = await Socket.Send(Convert.ToBase64String(Encoding.UTF8.GetBytes(Password)));

            if (!responsePassword.Contains(SmtpCode.AuthenticationSuccessful))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Authenticates a socket client using plain authentication.
        /// </summary>
        /// <returns>True if the client was successfully authenticated. False otherwise.</returns>
        public async Task<Boolean> AuthenticateByPlain()
        {
            if (!IsConnected)
            {
                return false;
            }

            SmtpResponse response = await Socket.Send("Auth Plain");

            if (!response.Contains(SmtpCode.WaitingForAuthentication))
            {
                return false;
            }

            string lineAuthentication = string.Format("{0}\0{0}\0{1}", Username, Password);

            SmtpResponse responseAuth = await Socket.Send(Convert.ToBase64String(Encoding.UTF8.GetBytes(lineAuthentication)));

            if (!responseAuth.Contains(SmtpCode.AuthenticationSuccessful))
            {
                return false;
            }

            return true;

        }

        /// <summary>
        /// Sends the specified email message.
        /// </summary>
        /// <param name="message">The email message.</param>
        /// <returns>True if the email was sent successfully. False otherwise.</returns>
        public async Task<bool> SendMail(SmtpMessage message)
        {
            if (!IsConnected)
            {
                await Connect();
            }

            if (!IsConnected)
            {
                throw new Exception("Can't connect to the SMTP server.");
            }

            if (!IsAuthenticated)
            {
                await Authenticate();
            }

            SmtpResponse response = await Socket.Send(string.Format("Mail From:<{0}>", message.From));

            if (!response.Contains(SmtpCode.RequestedMailActionCompleted))
            {
                return false;
            }

            foreach (string to in message.To)
            {
                SmtpResponse responseTo = await Socket.Send(String.Format("Rcpt To:<{0}>", to));

                if (!responseTo.Contains(SmtpCode.RequestedMailActionCompleted))
                {
                    break;
                }
            }

            SmtpResponse responseData = await Socket.Send(String.Format("Data"));

            if (!responseData.Contains(SmtpCode.StartMailInput))
            {
                return false;
            }

            SmtpResponse repsonseMessage = await Socket.Send(message.CreateMessageBody());

            if (!repsonseMessage.Contains(SmtpCode.RequestedMailActionCompleted))
            {
                return false;
            }

            SmtpResponse responseQuit = await Socket.Send("Quit");

            if (!responseQuit.Contains(SmtpCode.ServiceClosingTransmissionChannel))
            {
                return false;
            }
            
            return true;
        }

        #endregion
    }
}
