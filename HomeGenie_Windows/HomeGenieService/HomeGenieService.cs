// <copyright file="HomeGenieService.cs" company="Bounz">
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

namespace HomeGenieService
{
    using System;
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.ServiceProcess;
    using System.Threading;

    class HomeGenieService : ServiceBase
    {
        private Process homegenie = null;

        public HomeGenieService()
        {
            this.ServiceName = "HomeGenie";
            this.CanHandlePowerEvent = true;
            this.CanHandleSessionChangeEvent = false;
            this.CanPauseAndContinue = false;
            this.CanShutdown = true;
            this.CanStop = true;
        }

        [RunInstaller(true)]
        public class HomeGenieInstaller : Installer
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="HomeGenieInstaller"/> class.
            /// </summary>
            public HomeGenieInstaller()
            {
                ServiceProcessInstaller serviceProcessInstaller =
                    new ServiceProcessInstaller();
                ServiceInstaller serviceInstaller = new ServiceInstaller();

                // # Service Account Information
                serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
                serviceProcessInstaller.Username = null;
                serviceProcessInstaller.Password = null;

                // # Service Information
                serviceInstaller.DisplayName = "HomeGenie Automation Server";
                serviceInstaller.StartType = ServiceStartMode.Automatic;

                // # This must be identical to the WindowsService.ServiceBase name
                // # set in the constructor of WindowsService.cs
                serviceInstaller.ServiceName = "HomeGenieService";

                this.Installers.Add(serviceProcessInstaller);
                this.Installers.Add(serviceInstaller);
            }

            static void Main(string[] args)
            {
                if (System.Environment.UserInteractive)
                {
                    string parameter = string.Concat(args);
                    switch (parameter)
                    {
                        case "--install":
                            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                            break;
                        case "--uninstall":
                            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                            break;
                    }
                }
                else
                {
                    ServiceBase.Run(new HomeGenieService());
                }
            }
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            StartHomeGenie();
        }

        protected override void OnStop()
        {
            StopHomeGenie();
            base.OnStop();
        }

        private void StartHomeGenie()
        {
            new Thread(() => { StartHomeGenieProcess(); }).Start();
        }

        private void StartHomeGenieProcess()
        {
            homegenie = new Process();
            homegenie.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HomeGenie.exe");
            homegenie.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            homegenie.StartInfo.UseShellExecute = false;
            homegenie.Start();
            homegenie.WaitForExit();
            int exitCode = homegenie.ExitCode;

            // if ExitCode is 1 then a restart was requested
            if (exitCode == 1)
            {
                StartHomeGenie();
            }
        }

        private void StopHomeGenie()
        {
            if (homegenie != null)
            {
                try
                {
                    homegenie.Kill();
                }
                catch
                {
                }
            }
        }
    }
}
