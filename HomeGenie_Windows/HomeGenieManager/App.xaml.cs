// <copyright file="App.xaml.cs" company="Bounz">
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

namespace HomeGenieManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using OpenSource.UPnP;

    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    ///
    public partial class App : Application
    {
        static bool createdNew;
        static Mutex m_Mutex;

        private UPnPControlPoint upnpControl;
        private Dictionary<string, UPnPDevice> upnpService;

        // Add this method override
        protected override void OnStartup(StartupEventArgs e)
        {
            m_Mutex = new Mutex(true, "HomeGenieManagerMutex", out createdNew);

            // e.Args is the string[] of command line argruments
            upnpService = new Dictionary<string, UPnPDevice>();
            upnpControl = new UPnPControlPoint();
            upnpControl.OnSearch += upnpcontrol_OnSearch;
            upnpControl.OnCreateDevice += upnpcontrol_OnCreateDevice;
            upnpControl.FindDeviceAsync("urn:schemas-upnp-org:device:HomeAutomationServer:1");
            Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnLastWindowClose;
            var myDialogWindow = new LoadingDialog();
            myDialogWindow.Show();
            Task loader = new Task(() =>
            {
                int t = 0;
                while (t < 10)
                {
                    if (upnpService.Count > 0)
                    {
                        Thread.Sleep(2000);
                        for (int s = 0; s < UPnPDevices.Count; s++)
                        {
                            var dev = UPnPDevices.ElementAt(s).Value;
                            if (dev.StandardDeviceType == "HomeAutomationServer")
                            {
                                System.Diagnostics.Process.Start(dev.PresentationURL);
                                t = 10;
                                break;
                            }
                        }
                    }
                    t++;
                    Thread.Sleep(1000);
                }

                Thread.Sleep(2000);
                myDialogWindow.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        if (!createdNew)
                        {
                            myDialogWindow.Close();
                            Application.Current.Shutdown();
                        }
                        else
                        {
                            var window = new MainWindow();
                            window.Show();
                            myDialogWindow.Close();
                        }
                    }), null);
            });
            loader.Start();
        }

        private void upnpcontrol_OnSearch(System.Net.IPEndPoint ResponseFromEndPoint, System.Net.IPEndPoint responseReceivedOnEndPoint, Uri descriptionLocation, string usn, string searchTarget, int maxAge)
        {
            upnpControl.CreateDeviceAsync(descriptionLocation, maxAge);

            // Console.WriteLine(USN + "\n" + SearchTarget);
        }

        private void upnpcontrol_OnCreateDevice(UPnPDevice device, Uri descriptionURL)
        {
            // Console.WriteLine(DescriptionURL + "\n" + Device.PresentationURL);
            lock (upnpService)
                if (!upnpService.ContainsKey(descriptionURL.ToString()))
                {
                    upnpService.Add(descriptionURL.ToString(), device);
                }
        }

        public Dictionary<string, UPnPDevice> UPnPDevices
        {
            get { return upnpService; }
        }
    }
}
