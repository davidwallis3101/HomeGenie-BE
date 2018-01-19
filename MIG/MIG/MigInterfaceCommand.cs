// <copyright file="MigInterfaceCommand.cs" company="Bounz">
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
    using System;

    public class MigInterfaceCommand
    {
        private string[] options = new string[0];

        public string Domain { get; set; }

        public string Address { get; set; }

        public string Command { get; }

        /// <summary>
        /// The full unparsed original request string.
        /// </summary>
        public string OriginalRequest { get; }

        public MigInterfaceCommand(string request)
        {
            OriginalRequest = request;
            try
            {
                var requests = request.Trim('/').Split(new char[] { '/' }, StringSplitOptions.None);

                // At least two elements required for a valid command
                if (requests.Length > 1)
                {
                    Domain = requests[0];
                    if (Domain == "html")
                    {
                        return;
                    }
                    else if (requests.Length > 2)
                    {
                        Address = requests[1];
                        Command = requests[2];
                        if (requests.Length > 3)
                        {
                            options = new string[requests.Length - 3];
                            Array.Copy(requests, 3, options, 0, requests.Length - 3);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MigService.Log.Error(e);
            }
        }

        public string GetOption(int index)
        {
            var option = string.Empty;
            if (index < options.Length)
            {
                option = Uri.UnescapeDataString(options[index]);
            }

            return option;
        }

        public string OptionsString
        {
            get
            {
                var optiontext = string.Empty;
                for (var o = 0; o < options.Length; o++)
                {
                    optiontext += options[o] + "/";
                }

                return optiontext;
            }
        }
    }
}
