// <copyright file="ProgramHelperBase.cs" company="Bounz">
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
    using System.IO;
    using System.Net;
    using System.Threading;
    using HomeGenie.Automation.Scripting;
    using HomeGenie.Service;
    using MIG;

    public class ProgramHelperBase
    {
        protected HomeGenieService homegenie;

        public ProgramHelperBase(HomeGenieService hg)
        {
            homegenie = hg;
        }

        /// <summary>
        /// Gets the logger object.
        /// </summary>
        /// <value>The logger object.</value>
        public NLog.Logger Log
        {
            get { return MigService.Log; }
        }

        /// <summary>
        /// Playbacks a synthesized voice message from speaker.
        /// </summary>
        /// <param name="sentence">Message to output.</param>
        /// <param name="locale">Language locale string (eg. "en-US", "it-IT", "en-GB", "nl-NL",...).</param>
        /// <param name="goAsync">If true, the command will be executed asyncronously.</param>
        /// <remarks />
        /// <example>
        /// Example:
        /// <code>
        /// Program.Say("The garage door has been opened", "en-US");
        /// </code>
        /// </example>
        public ProgramHelperBase Say(string sentence, string locale = null, bool goAsync = false)
        {
            if (string.IsNullOrWhiteSpace(locale))
            {
                locale = Thread.CurrentThread.CurrentCulture.Name;
            }

            try
            {
                Utility.Say(sentence, locale, goAsync);
            }
            catch (Exception e)
            {
                HomeGenieService.LogError(e);
            }

            return this;
        }

        // these redundant method definitions are for Jint compatibility
        public ProgramHelperBase Say(string sentence)
        {
            return Say(sentence, null, false);
        }

        public ProgramHelperBase Say(string sentence, string locale)
        {
            return Say(sentence, locale, false);
        }

        /// <summary>
        /// Playbacks a wave file.
        /// </summary>
        /// <param name="waveUrl">URL of the audio wave file to play.</param>
        public ProgramHelperBase Play(string waveUrl)
        {
            try
            {
                string outputDirectory = Utility.GetTmpFolder();
                string file = Path.Combine(outputDirectory, "_wave_tmp." + Path.GetExtension(waveUrl));
                using (var webClient = new WebClient())
                {
                    byte[] audiodata = webClient.DownloadData(waveUrl);

                    if (!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }

                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }

                    var stream = File.OpenWrite(file);
                    stream.Write(audiodata, 0, audiodata.Length);
                    stream.Close();

                    webClient.Dispose();
                }

                Utility.Play(file);
            }
            catch (Exception e)
            {
                HomeGenieService.LogError(e);
            }

            return this;
        }

        /// <summary>
        /// Parses the given (api call) string as a MigInterfaceCommand object.
        /// </summary>
        /// <returns>The mig command.</returns>
        /// <param name="command">Api Command (eg. "HomeAutomation.X10/A5/Control.Level/50").</param>
        public MigInterfaceCommand ParseApiCall(string apiCall)
        {
            return new MigInterfaceCommand(apiCall);
        }

        /// <summary>
        /// Invoke an API command and get the result.
        /// </summary>
        /// <returns>The API command response.</returns>
        /// <param name="apiCommand">Any MIG/APP API command withouth the "/api/" prefix.</param>
        public object ApiCall(string apiCommand)
        {
            if (apiCommand.StartsWith("/api/"))
            {
                apiCommand = apiCommand.Substring(5);
            }

            return homegenie.InterfaceControl(new MigInterfaceCommand(apiCommand));
        }

        /// <summary>
        /// Executes a function asynchronously.
        /// </summary>
        /// <returns>
        /// The Thread object of this asynchronous task.
        /// </returns>
        /// <param name='functionBlock'>
        /// Function name or inline delegate.
        /// </param>
        public Thread RunAsyncTask(Utility.AsyncFunction functionBlock)
        {
            return Utility.RunAsyncTask(functionBlock);
        }

        /// <summary>
        /// Executes the specified Automation Program.
        /// </summary>
        /// <param name='programId'>
        /// Program name or ID.
        /// </param>
        public void Run(string programId)
        {
            Run(programId, string.Empty);
        }

        /// <summary>
        /// Executes the specified Automation Program.
        /// </summary>
        /// <param name='programId'>
        /// Program name or ID.
        /// </param>
        /// <param name='options'>
        /// Program options.
        /// </param>
        public void Run(string programId, string options)
        {
            var program = homegenie.ProgramManager.Programs.Find(p => p.Address.ToString() == programId || p.Name == programId);
            if (program != null && !program.IsRunning)
            {
                homegenie.ProgramManager.Run(program, options);
            }
        }

        /// <summary>
        /// Wait until the given program is not running.
        /// </summary>
        /// <returns>ProgramHelper.</returns>
        /// <param name="programId">Program address or name.</param>
        public ProgramHelperBase WaitFor(string programId)
        {
            var program = homegenie.ProgramManager.Programs.Find(p => p.Address.ToString() == programId || p.Name == programId);
            if (program != null)
            {
                while (!program.IsRunning)
                {
                    Thread.Sleep(1000);
                }

                Thread.Sleep(1000);
            }

            return this;
        }

        /// <summary>
        /// Returns a reference to the ProgramHelper of a program.
        /// </summary>
        /// <returns>ProgramHelper.</returns>
        /// <param name="programAddress">Program address (id).</param>
        public ProgramHelper WithAddress(int programAddress)
        {
            var program = homegenie.ProgramManager.Programs.Find(p => p.Address == programAddress);
            ProgramHelper programHelper = null;
            if (program != null)
            {
                programHelper = new ProgramHelper(homegenie, program.Address);
            }

            return programHelper;
        }

        /// <summary>
        /// Returns a reference to the ProgramHelper of a program.
        /// </summary>
        /// <returns>ProgramHelper.</returns>
        /// <param name="programName">Program name.</param>
        public ProgramHelper WithName(string programName)
        {
            var program = homegenie.ProgramManager.Programs.Find(p => p.Name.ToLower() == programName.ToLower());
            ProgramHelper programHelper = null;
            if (program != null)
            {
                programHelper = new ProgramHelper(homegenie, program.Address);
            }

            return programHelper;
        }
    }
}
