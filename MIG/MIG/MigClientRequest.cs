// <copyright file="MigClientRequest.cs" company="Bounz">
// This file is part of HomeGenie-BE Project source code.
//
// HomeGenie-BE is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// HomeGenie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with HomeGenie-BE.  If not, see http://www.gnu.org/licenses.
//
//  Project Homepage: https://github.com/Bounz/HomeGenie-BE
//
//  Forked from Homegenie by Generoso Martello gene@homegenie.it
// </copyright>

namespace MIG
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class MigClientRequest
    {
        private object responseData;

        public MigContext Context { get; }

        public MigInterfaceCommand Command { get; }

        public string RequestText;
        public byte[] RequestData;

        public object ResponseData
        {
            get
            {
                return responseData;
            }

            set
            {
                if (value != null)
                {
                    Handled = true;
                }

                responseData = value;
            }
        }

        public bool Handled = false;

        public MigClientRequest(MigContext context, MigInterfaceCommand command)
        {
            Command = command;
            Context = context;
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Status
    {
        Ok,
        Error
    }

    public class ResponseText
    {
        public string ResponseValue { get; }

        public ResponseText(string response)
        {
            ResponseValue = response;
        }
    }

    public class ResponseStatus
    {
        public Status Status { get; }

        public string Message { get; }

        public ResponseStatus(Status status, string message = "")
        {
            this.Status = status;
            this.Message = message;
        }
    }
}
