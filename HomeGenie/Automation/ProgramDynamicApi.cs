// <copyright file="ProgramDynamicApi.cs" company="Bounz">
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

namespace HomeGenie
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MIG;

    public static class ProgramDynamicApi
    {
        private static Dictionary<string, Func<object, object>> dynamicApi = new Dictionary<string, Func<object, object>>();

        public static Func<object, object> Find(string request)
        {
            Func<object, object> handler = null;
            if (dynamicApi.ContainsKey(request))
            {
                handler = dynamicApi[request];
            }

            return handler;
        }

        public static Func<object, object> FindMatching(string request)
        {
            Func<object, object> handler = null;
            for (int i = 0; i < dynamicApi.Keys.Count; i++)
            {
                if (request.StartsWith(dynamicApi.Keys.ElementAt(i)))
                {
                    handler = dynamicApi[dynamicApi.Keys.ElementAt(i)];
                    break;
                }
            }

            return handler;
        }

        public static void Register(string request, Func<object, object> handlerfn)
        {
            if (dynamicApi.ContainsKey(request))
            {
                dynamicApi[request] = handlerfn;
            }
            else
            {
                dynamicApi.Add(request, handlerfn);
            }
        }

        public static void UnRegister(string request)
        {
            if (dynamicApi.ContainsKey(request))
            {
                dynamicApi.Remove(request);
            }
        }

        public static object TryApiCall(MigInterfaceCommand command)
        {
            object response = string.Empty;

            // Dynamic Interface API
            var registeredApi = command.Domain + "/" + command.Address + "/" + command.Command;
            var handler = ProgramDynamicApi.Find(registeredApi);
            if (handler != null)
            {
                // explicit command API handlers registered in the form <domain>/<address>/<command>
                // receives only the remaining part of the request after the <command>
                var args = command.OriginalRequest.Substring(registeredApi.Length).Trim('/');
                response = handler(args);
            }
            else
            {
                handler = ProgramDynamicApi.FindMatching(command.OriginalRequest.Trim('/'));
                if (handler != null)
                {
                    // other command API handlers
                    // receives the full request string
                    response = handler(command.OriginalRequest.Trim('/'));
                }
            }

            return response;
        }
    }
}
