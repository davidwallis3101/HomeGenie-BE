// <copyright file="ArduinoAppFactory.cs" company="Bounz">
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

namespace HomeGenie.Automation.Engines
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using HomeGenie.Automation.Scripting;

    public static class ArduinoAppFactory
    {
        public static List<ProgramError> CompileSketch(string sketchFileName, string sketchMakefile)
        {
            List<ProgramError> errors = new List<ProgramError>();

            string fileIno = Path.GetFileName(sketchFileName);

            // run make
            var processInfo = new ProcessStartInfo("make", string.Empty);
            processInfo.WorkingDirectory = Path.GetDirectoryName(sketchFileName);
            processInfo.RedirectStandardOutput = false;
            processInfo.RedirectStandardInput = false;
            processInfo.RedirectStandardError = true;
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = true;
            using (Process process = Process.Start(processInfo))
            {
                using (StreamReader reader = process.StandardError)
                {
                    string errorOutput = string.Empty;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] lineParts = line.Split(':');

                        // TODO: here should parse errors and warnings
                        if (line.StartsWith(fileIno + ":") && lineParts.Length > 4)
                        {
                            int errorRow = 0;
                            int errorColumn = 0;
                            if (lineParts[3].Contains("error") && int.TryParse(lineParts[1], out errorRow) && int.TryParse(
                                lineParts[2],
                                out errorColumn))
                            {
                                var errorDetail = new string[lineParts.Length - 4];
                                Array.Copy(lineParts, 4, errorDetail, 0, errorDetail.Length);
                                errors.Add(new ProgramError()
                                {
                                    Line = errorRow,
                                    Column = errorColumn,
                                    ErrorMessage = lineParts[3] + ": " + string.Join(": ", errorDetail),
                                    ErrorNumber = "110",
                                    CodeBlock = CodeBlockEnum.CR
                                });
                            }
                        }
                        else if (line.StartsWith("Makefile:") && lineParts.Length > 2)
                        {
                            int errorRow = 0;
                            if (int.TryParse(lineParts[1], out errorRow))
                            {
                                errors.Add(new ProgramError()
                                {
                                    Line = errorRow,
                                    Column = 0,
                                    ErrorMessage = line,
                                    ErrorNumber = "120",
                                    CodeBlock = CodeBlockEnum.TC
                                });
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(line) && !line.Contains("depends.mk:") && (line.Contains("error") || line.Contains(" *** ")))
                        {
                            errorOutput += line + "\n";
                        }

                        Console.WriteLine(line);
                    }

                    if (errors.Count == 0 && !string.IsNullOrWhiteSpace(errorOutput))
                    {
                        errors.Add(new ProgramError()
                        {
                            Line = 0,
                            Column = 0,
                            ErrorMessage = errorOutput, // "Build failure: please check the Makefile; ensure BOARD_TAG is correct and ARDUINO_LIBS is referencing libraries needed by this sketch.\n\n" + errorOutput,
                            ErrorNumber = "130",
                            CodeBlock = CodeBlockEnum.CR
                        });
                    }
                }
            }

            /*
             TODO: Possibly add support for rt debugging and arduino output logging
             TODO: Implement an "arduino-hg-interop" C library
             NOTE: Issue "apt-get install arduino-mk" on the hosting platform to get needed tools for this task
            */

            return errors;
        }

        public static string UploadSketch(string sketchDirectory)
        {
            string errorOutput = string.Empty;
            var processInfo = new ProcessStartInfo("empty", "-f -L uploadres.txt make upload");
            processInfo.WorkingDirectory = sketchDirectory;
            processInfo.RedirectStandardOutput = false;
            processInfo.RedirectStandardInput = false;
            processInfo.RedirectStandardError = true;
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = true;
            using (Process process = Process.Start(processInfo))
            {
                using (StreamReader reader = process.StandardError)
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            errorOutput += line + "\n";
                        }
                    }

                    Console.WriteLine(errorOutput);
                }
            }

            try
            {
                string[] outputFile = File.ReadAllText(Path.Combine(sketchDirectory, "uploadres.txt")).Split('\n');
                for (int l = 0; l < outputFile.Length; l++)
                {
                    if (outputFile[l].StartsWith("avrdude") && outputFile[l].IndexOf(":") > 0 || outputFile[l].Contains(" *** "))
                    {
                        string logLine = outputFile[l].Substring(outputFile[l].IndexOf(":") + 1);
                        errorOutput += logLine + "\n";
                    }
                }

                File.Delete(Path.Combine(sketchDirectory, "uploadres.txt"));
            }
catch
            {
            }

            // if (!String.IsNullOrWhiteSpace(errorOutput))
            // {
            //    throw(new IOException(errorOutput));
            // }
            return errorOutput;
        }

        public static string GetSketchFile(string address)
        {
            string filename = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "programs",
                "arduino",
                address,
                "sketch_" + address + ".cpp");
            return filename;
        }

        public static bool IsValidProjectFile(string filename)
        {
            bool isValid = !Path.GetFileName(filename).StartsWith("sketch_") &&
                Path.GetFileName(filename) != "Makefile" &&
                (filename.EndsWith(".cpp")
                || filename.EndsWith(".c")
                || filename.EndsWith(".h")
                || !filename.Contains("."));
            return isValid;
        }
    }
}
