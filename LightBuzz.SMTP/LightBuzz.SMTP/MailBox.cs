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

namespace LightBuzz.SMTP
{
    /// <summary>
    /// Mailbox is an object formed by an email address and a name
    /// </summary>
    public class MailBox
    {
        #region Members
        
        /// <summary>
        /// See <see cref="EmailAddress">Address</see>
        /// </summary>
        private string _emailAddress;

        /// <summary>
        /// See <see cref="Name">Name</see>
        /// </summary>
        private string _name;
        
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the mailbox address.
        /// </summary>
        public string EmailAddress
        {
            get
            {
                return _emailAddress;
            }
            set
            {
                _emailAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets the mailbox name.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MailBox()
        {
        }

        /// <summary>
        /// Full constructor with both objects.
        /// </summary>
        /// <param name="name">The Name of the person.</param>
        /// <param name="emailAddress">The Email address.</param>
        public MailBox(string name, string emailAddress)
        {
            _name = name;
            _emailAddress = emailAddress;
        }

        #endregion
    }
}
