// <copyright file="Events.cs" company="Bounz">
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
    using MIG.Config;

    public class OptionChangedEventArgs
    {
        public readonly Option Option;

        public OptionChangedEventArgs(Option option)
        {
            Option = option;
        }
    }

    public class ProcessRequestEventArgs
    {
        public readonly MigClientRequest Request;

        public ProcessRequestEventArgs(MigClientRequest request)
        {
            Request = request;
        }
    }

    public class InterfaceModulesChangedEventArgs
    {
        public readonly string Domain;

        public InterfaceModulesChangedEventArgs(string domain)
        {
            Domain = domain;
        }
    }

    public class InterfacePropertyChangedEventArgs
    {
        public readonly MigEvent EventData;

        public InterfacePropertyChangedEventArgs(MigEvent eventData)
        {
            EventData = eventData;
        }

        public InterfacePropertyChangedEventArgs(string domain, string source, string description, string propertyPath, object propertyValue)
        {
            EventData = new MigEvent(domain, source, description, propertyPath, propertyValue);
        }
    }
}
