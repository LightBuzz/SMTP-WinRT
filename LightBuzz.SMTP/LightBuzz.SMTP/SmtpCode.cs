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


namespace LightBuzz.SMTP
{
    /// <summary>
    /// Enumerates the available SMTP codes.
    /// (Source: http://www.ietf.org/rfc/rfc2821.txt)
    /// </summary>
    internal enum SmtpCode : int
    {
        None = 0,
        SystemStatus = 211,
        HelpMessage = 214,
        ServiceReady = 220,
        ServiceClosingTransmissionChannel = 221,
        AuthenticationSuccessful = 235,
        RequestedMailActionCompleted = 250,
        UserNotLocalWillForwardTo = 251,
        CannotVerifyUserButWillAcceptMessage = 252,
        WaitingForAuthentication = 334,
        StartMailInput = 354,
        ServiceNotAvailable = 421,
        MailboxBusy = 450,
        RequestedError = 451,
        InsufficientSystemStorage = 452,
        SyntaxError = 500,
        SyntaxErrorInParameters = 501,
        CommandNotImplemented = 502,
        BadSequenceCommand = 503,
        CommandParameterNotImplemented = 504,
        MailboxUnavailable = 550,
        UserNotLocalTryOther = 551,
        ExceededStorageAllocation = 552,
        MailboxNameNotAllowed = 553,
        TransactionFailed = 554,
    }
}
