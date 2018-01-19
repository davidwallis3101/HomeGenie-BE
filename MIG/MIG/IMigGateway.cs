// <copyright file="IMigGateway.cs" company="Bounz">
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
    using System.Collections.Generic;
    using MIG.Config;

    public static class GatewayExtension
    {
        public static string GetName(this IMigGateway gateway)
        {
            return gateway.GetType().Name;
        }

        public static Option GetOption(this IMigGateway gateway, string option)
        {
            if (gateway.Options != null)
            {
                return gateway.Options.Find(o => o.Name == option);
            }

            return null;
        }

        public static void SetOption(this IMigGateway gateway, string option, string value)
        {
            MigService.Log.Debug("{0}: {1}={2}", gateway.GetName(), option, value);
            var opt = gateway.GetOption(option);
            if (opt == null)
            {
                opt = new Option(option);
                gateway.Options.Add(opt);
            }

            opt.Value = value;
            gateway.OnSetOption(opt);
        }
    }

    public interface IMigGateway
    {
        event PreProcessRequestEventHandler PreProcessRequest;

        event PostProcessRequestEventHandler PostProcessRequest;

        void OnSetOption(Option option);

        void OnInterfacePropertyChanged(object sender, InterfacePropertyChangedEventArgs args);

        List<Option> Options { get; set; }

        bool Start();

        void Stop();
    }
}
