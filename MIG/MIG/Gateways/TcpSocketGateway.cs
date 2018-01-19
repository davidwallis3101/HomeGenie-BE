// <copyright file="TcpSocketGateway.cs" company="Bounz">
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

namespace MIG.Gateways
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MIG.Config;
    using MIG.Utility;

    public class TcpSocketGateway : IMigGateway
    {
        private const int bufferLength = 1024;

        private TcpServerChannel server;

        private int servicePort = 4502;

        public event PreProcessRequestEventHandler PreProcessRequest;

        public event PostProcessRequestEventHandler PostProcessRequest;

        public List<Option> Options { get; set; }

        public void OnSetOption(Option option)
        {
            if (option.Name.Equals("Port"))
            {
                int.TryParse(option.Value, out servicePort);
            }
        }

        public void OnInterfacePropertyChanged(object sender, InterfacePropertyChangedEventArgs args)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            server.SendAll(encoding.GetBytes(MigService.JsonSerialize(args)));
        }

        public bool Start()
        {
            bool success = false;

            server = NetworkConnectivity.CreateTcpServerChannel("server");

            try
            {
                server.Connect(servicePort);
                server.ChannelClientConnected += server_ChannelClientConnected;
                server.ChannelClientDisconnected += server_ChannelClientDisconnected;
                server.DataReceived += server_DataReceived;
                server.ExceptionOccurred += server_ExceptionOccurred;
                success = true;
            }
            catch (Exception e)
            {
                MigService.Log.Error(e);
            }

            return success;
        }

        public void Stop()
        {
            server.ChannelClientConnected -= server_ChannelClientConnected;
            server.ChannelClientDisconnected -= server_ChannelClientDisconnected;
            server.DataReceived -= server_DataReceived;
            server.ExceptionOccurred -= server_ExceptionOccurred;
            server.Disconnect();
        }

        public void ProcessRequest(ServerDataEventArgs args)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            var message = encoding.GetString(args.Data, 0, args.DataLength);
            var migContext = new MigContext(ContextSource.TcpSocketGateway, args);
            var migRequest = new MigClientRequest(migContext, new MigInterfaceCommand(message));

            OnPreProcessRequest(migRequest);

            if (!migRequest.Handled)
            {
                OnPostProcessRequest(migRequest);
            }
        }

        private void server_ChannelClientConnected(object sender, ServerConnectionEventArgs args)
        {
            server.Receive(bufferLength, args.ClientId);
        }

        private void server_DataReceived(object sender, ServerDataEventArgs args)
        {
            server.Receive(bufferLength, (int)args.ClientId);
            if (args.DataLength > 0)
            {
                ProcessRequest(args);
            }
        }

        private void server_ChannelClientDisconnected(object sender, ServerConnectionEventArgs args)
        {
        }

        private void server_ExceptionOccurred(object sender, System.IO.ErrorEventArgs e)
        {
        }

        protected virtual void OnPreProcessRequest(MigClientRequest request)
        {
            if (request != null && PreProcessRequest != null)
            {
                PreProcessRequest(this, new ProcessRequestEventArgs(request));
            }
        }

        protected virtual void OnPostProcessRequest(MigClientRequest request)
        {
            if (request != null && PostProcessRequest != null)
            {
                PostProcessRequest(this, new ProcessRequestEventArgs(request));
            }
        }
    }
}
