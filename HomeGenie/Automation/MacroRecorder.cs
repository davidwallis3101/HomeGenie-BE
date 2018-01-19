// <copyright file="MacroRecorder.cs" company="Bounz">
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

namespace HomeGenie.Automation
{
    using System;
    using System.Collections.Generic;
    using HomeGenie.Service.Constants;
    using MIG;

    public enum MacroDelayType
    {
        None = -1,
        Fixed,
        Mimic
    }

    public class MacroRecorder
    {
        // vars for macro record fn
        private bool isMacroRecordingEnabled = false;
        private List<MigInterfaceCommand> macroCommands = new List<MigInterfaceCommand>();
        private DateTime currentTimestamp = DateTime.Now;
        private double delaySeconds = 1;
        private MacroDelayType delayType = MacroDelayType.Fixed;

        // private DateTime startTimestamp = DateTime.Now;
        private ProgramManager masterControlProgram;

        public MacroRecorder(ProgramManager mcp)
        {
            masterControlProgram = mcp;
        }

        public void RecordingDisable()
        {
            // stop recording
            isMacroRecordingEnabled = false;
        }

        public void RecordingEnable()
        {
            // start recording
            macroCommands.Clear();

            // startTimestamp = currentTimestamp = DateTime.Now;
            isMacroRecordingEnabled = true;
        }

        public ProgramBlock SaveMacro(string options)
        {
            RecordingDisable();
            var program = new ProgramBlock();
            program.Name = "New Macro";
            program.Address = masterControlProgram.GeneratePid();
            program.Type = "Wizard";
            foreach (var migCommand in macroCommands)
            {
                var command = new ProgramCommand();
                command.Domain = migCommand.Domain;
                command.Target = migCommand.Address;
                command.CommandString = migCommand.Command;
                command.CommandArguments = string.Empty;
                if (!string.IsNullOrEmpty(migCommand.GetOption(0)) && migCommand.GetOption(0) != "null")
                {
                    // TODO: should we pass entire command option string? migCmd.OptionsString
                    command.CommandArguments = migCommand.GetOption(0) + (options != string.Empty && options != "null" ? "/" + options : string.Empty);
                }

                program.Commands.Add(command);
            }

            masterControlProgram.ProgramAdd(program);
            return program;
        }

        public void AddCommand(MigInterfaceCommand cmd)
        {
            double delay = 0;
            switch (delayType)
            {
            case MacroDelayType.Mimic:
                // calculate pause between current and previous command
                delay = new TimeSpan(DateTime.Now.Ticks - currentTimestamp.Ticks).TotalSeconds;
                break;

            case MacroDelayType.Fixed:
                // put a fixed pause
                delay = delaySeconds;
                break;
            }

            try
            {
                if (delay > 0 && macroCommands.Count > 0)
                {
                    // add a pause command to the macro
                    macroCommands.Add(new MigInterfaceCommand(Domains.HomeAutomation_HomeGenie + "/Automation/Program.Pause/" + delay.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                }

                macroCommands.Add(cmd);
            }
            catch
            {
                // HomeGenieService.LogEvent(Domains.HomeAutomation_HomeGenie, "migservice_ServiceRequestPostProcess(...)", ex.Message, "Exception.StackTrace", ex.StackTrace);
            }

            currentTimestamp = DateTime.Now;
        }

        public bool IsRecordingEnabled
        {
            get { return isMacroRecordingEnabled; }
        }

        public MacroDelayType DelayType
        {
            get { return delayType; }
            set { delayType = value; }
        }

        public double DelaySeconds
        {
            get { return delaySeconds; }
            set { delaySeconds = value; }
        }
    }
}
