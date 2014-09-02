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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace LightBuzz.SMTP
{
    /// <summary>
    /// Implements an SMTP socket.
    /// </summary>
    public class SmtpSocket
    {
        #region Constants

        private const int BUFFER_LENGTH = 1024;

        #endregion

        #region Members

        private HostName _host;
        private StreamSocket _socket;
        private DataReader _reader;
        private DataWriter _writer;
        private int _port;
        private bool _ssl;
        private string _username;
        private string _password;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of SmtpSocket.
        /// </summary>
        /// <param name="server">Server host name.</param>
        /// <param name="port">Port (usually 25).</param>
        /// <param name="ssl">SSL/TLS support.</param>
        public SmtpSocket(string server, int port, bool ssl)
        {
            _host = new HostName(server);
            _socket = new StreamSocket();
            _port = port;
            _ssl = ssl;
        }

        /// <summary>
        /// Creates a new instance of SmtpSocket.
        /// </summary>
        /// <param name="server">Server host name.</param>
        /// <param name="port">Port (usually 25).</param>
        /// <param name="ssl">SSL/TLS support.</param>
        /// <param name="username">Server username.</param>
        /// <param name="password">Server password.</param>
        public SmtpSocket(string server, int port, bool ssl, string username, string password)
        {
            _host = new HostName(server);
            _socket = new StreamSocket();
            _port = port;
            _ssl = ssl;
            _username = username;
            _password = password;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Establishes a connection between the server and the client.
        /// </summary>
        /// <returns>The SMTP response from the server.</returns>
        public async Task<SmtpResponse> EstablishConnection()
        {
            try
            {
                _reader = new DataReader(_socket.InputStream);
                _reader.InputStreamOptions = InputStreamOptions.Partial;
                _writer = new DataWriter(_socket.OutputStream);

                SocketProtectionLevel protection = _ssl ? SocketProtectionLevel.Tls10 : SocketProtectionLevel.PlainSocket;

                await _socket.ConnectAsync(_host, _port.ToString(), protection);

                return await GetResponse("Connect");
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Upgrades to a secure connection.
        /// </summary>
        /// <returns></returns>
        public async Task UpgradeToSslAsync()
        {
            try
            {
                await _socket.UpgradeToSslAsync(SocketProtectionLevel.Tls10, _host);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Sends the specified string command using the socket connection.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <returns>The returned SMTP response.</returns>
        public async Task<SmtpResponse> Send(string command)
        {
            return await Send(Encoding.UTF8.GetBytes(command + System.Environment.NewLine), command);
        }

        /// <summary>
        /// Sends the specified binary command using the socket connection.
        /// </summary>
        /// <param name="bytes">The byte array to send.</param>
        /// <param name="command">The command to send.</param>
        /// <returns></returns>
        public async Task<SmtpResponse> Send(byte[] bytes, string command)
        {
            try
            {
                _writer.WriteBytes(bytes);

                await _writer.StoreAsync();
                
                return await GetResponse(command);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Closes the socket connection.
        /// </summary>
        public virtual void Close()
        {
            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }

            if (_writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }
        }

        #endregion

        #region Private methods

        private async Task<SmtpResponse> GetResponse(string from)
        {
            SmtpResponse response = new SmtpResponse();
            bool isStartingLine = true;
            StringBuilder stringBuilder = null;
            int charLen = 3;
            Boolean endOfStream = false;
            SmtpCode code = SmtpCode.None;
            string codeStr = string.Empty;

            try
            {
                stringBuilder = new StringBuilder();

                while (!endOfStream)
                {
                    // There is a Strange beahvior when the bufferLength is exactly the same size as the inputStream
                    await _reader.LoadAsync(BUFFER_LENGTH);

                    charLen = Math.Min((int)_reader.UnconsumedBufferLength, BUFFER_LENGTH);

                    if (charLen == 0)
                    {
                        endOfStream = true;
                        break;
                    }

                    // If charLen < bufferLength, it's end of stream
                    if (charLen < BUFFER_LENGTH)
                        endOfStream = true;

                    // get the current position
                    int charPos = 0;

                    // Read the buffer
                    byte[] buffer = new byte[charLen];
                    _reader.ReadBytes(buffer);

                    do
                    {
                        // get the character
                        char chr = (char)buffer[charPos];

                        // if it's starting point, we can read the first 3 chars.
                        if (isStartingLine)
                        {

                            codeStr += chr;

                            // Get the code
                            if (codeStr.Length == 3)
                            {
                                int codeInt;
                                if (int.TryParse(codeStr, out codeInt))
                                    code = (SmtpCode)codeInt;

                                // next 
                                isStartingLine = false;
                            }

                        }
                        else if (chr == '\r' || chr == '\n')
                        {
                            // Advance 1 byte to get the '\n' if not at the end of the buffer
                            if (chr == '\r' && charPos < (charLen - 1))
                            {
                                charPos++;
                                chr = (char)buffer[charPos];
                            }
                            if (chr == '\n')
                            {
                                KeyValuePair<SmtpCode, String> r = new KeyValuePair<SmtpCode, string>
                                    (code, stringBuilder.ToString());

                                response.Values.Add(r);

                                Debug.WriteLine("{0}{1}", ((int)code).ToString(), stringBuilder.ToString());

                                stringBuilder = new StringBuilder();
                                code = SmtpCode.None;
                                codeStr = string.Empty;
                                isStartingLine = true;
                            }
                        }
                        else
                        {
                            stringBuilder.Append(chr);
                        }

                        charPos++;

                    } while (charPos < charLen);
                }
            }
            catch
            {
                throw;
            }

            return response;
        }

        private async Task<List<string>> GetResponse2()
        {
            List<String> lines = new List<string>();
            using (MemoryStream ms = await GetResponseStream())
            {
                using (StreamReader sr = new StreamReader(ms))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();

                        if (String.IsNullOrEmpty(line))
                            break;

                        lines.Add(line);
                    }
                }
            }

            return lines;
        }

        private async Task<MemoryStream> GetResponseStream()
        {
            MemoryStream ms = new MemoryStream();

            while (true)
            {
                await _reader.LoadAsync(BUFFER_LENGTH);

                if (_reader.UnconsumedBufferLength == 0) { break; }

                Int32 index = 0;
                while (_reader.UnconsumedBufferLength > 0)
                {
                    ms.WriteByte(_reader.ReadByte());
                    index = index + 1;
                }

                if (index == 0 || index < BUFFER_LENGTH)
                    break;
            }

            ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }

        #endregion
    }
}
