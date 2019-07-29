// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]
//
// This file cannot be modified or redistributed. This header cannot be removed.

using Keysight.Tap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Tap.Plugins.UMA.Android.Instruments;
using Tap.Plugins.UMA.Android.Instruments.Logcat;

namespace Tap.Plugins.UMA.Android.Steps
{
    [Display("Retrieve Background Logcat",
        Groups: new string[] { "UMA", "Android" },
        Description: "Gets the logcat from background process running in an Android device, and stops it.")]
    public class RetrieveBackgroundLogcatStep : AdbStepBase
    {
        #region Settings

        [Display("Background Logcat",
            Group: "Logcat",
            Description: "The source of the background logcat to stop and retrieve.",
            Order: 1.0)]
        public Input<BackgroundLogcat> BackgroundLogcat { get; set; }

        [Display("Delete Device Files",
            Group: "Logcat",
            Description: "Delete the device temporary log file(s) after logcat has been retrieved.",
            Order: 1.1)]
        public bool DeleteFiles { get; set; }

        [Display(Name: "Local File",
            Group: "Output",
            Description: "Write the retrieved logcat to this local file.",
            Order: 2.0)]
        [FilePath]
        public string LocalFilename { get; set; }

        #endregion

        public RetrieveBackgroundLogcatStep()
        {
            // Validation rules
            Rules.Add(() => (BackgroundLogcat != null), "Please select a background logcat output from a Logcat step", "BackgroundLogcat");
            Rules.Add(() => (BackgroundLogcat == null || LogcatExecutionMode.Continuous == ((LogcatStep)BackgroundLogcat.Step).ExecutionMode), "Selected step is not configured to run logcat in background", "BackgroundLogcat");
            Rules.Add(() => (!string.IsNullOrWhiteSpace(LocalFilename)), "Please set a valid device file path", "OutputFilename");
        }

        public override void Run()
        {
            terminateBackgroundLogcat();
            retrieveLogcat();
            if (DeleteFiles)
            {
                deleteExistingLogFiles();
            }
        }

        private void terminateBackgroundLogcat()
        {
            if (BackgroundLogcat.Value.AdbProcess.HasFinished)
            {
                // We expected the process to be still running
                Log.Warning("Background process already terminated");
            }
            else
            {
                Log.Debug("Terminating background process");
                BackgroundLogcat.Value.AdbProcess.Terminate();
            }

            AdbCommandResult result = BackgroundLogcat.Value.AdbProcess.Result;

            LogAdbOutput(result);
        }

        private void retrieveLogcat()
        {
            if (BackgroundLogcat.Value.RotateFiles)
            {
                retrieveRotatedLogcat();
            }
            else
            {
                retrieveSingleLogFile();
            }
        }

        private void retrieveSingleLogFile()
        {
            Log.Info($"Pulling logcat into {LocalFilename}");
            Adb.Pull(BackgroundLogcat.Value.DeviceFilename, LocalFilename, BackgroundLogcat.Value.DeviceId);
        }

        #region Rotated logcat

        private void retrieveRotatedLogcat()
        {
            IEnumerable<string> deviceFiles = getAvailableLogFiles();

            if (deviceFiles.Any())
            {
                string tempDirectory = createTemporaryDirectory();

                try
                {
                    IEnumerable<string> localFiles = pullLogFiles(tempDirectory, deviceFiles);
                    combineLogFiles(localFiles, LocalFilename);
                }
                finally
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
            else
            {
                Log.Warning("No log files found");
            }
        }

        private IEnumerable<string> getAvailableLogFiles()
        {
            string filesToList = BackgroundLogcat.Value.DeviceFilename;
            if (BackgroundLogcat.Value.RotateFiles)
            {
                filesToList += "*";
            }

            AdbCommandResult result = Adb.ExecuteAdbCommand($"shell \"ls {filesToList}\"");
            List<string> deviceFiles = new List<string>();
            if (result.Success)
            {
                foreach (string line in result.Output)
                {
                    deviceFiles.Add(line.Trim());
                }
            }
            else
            {
                HandleResult(result);
            }

            return deviceFiles.OrderBy(getFileNumber).Reverse();
        }

        private string createTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(path);
            return path;
        }

        private IEnumerable<string> pullLogFiles(string tempDirectory, IEnumerable<string> deviceFiles)
        {
            Log.Info($"Pulling log files: {string.Join(", ", deviceFiles)}");

            List<string> localFiles = new List<string>(deviceFiles.Count());

            foreach (string deviceFile in deviceFiles)
            {
                string localFile = Path.Combine(tempDirectory, Path.GetFileName(deviceFile));
                AdbCommandResult result = Adb.Pull(deviceFile, localFile);
                if (result.Success)
                {
                    localFiles.Add(localFile);
                }
                else
                {
                    Log.Warning($"Could not pull {deviceFile}");
                    LogAdbOutput(result);
                }
            }

            return localFiles;
        }

        private void combineLogFiles(IEnumerable<string> logFiles, string filename)
        {
            Log.Info($"Combining log files into {filename}");

            using (Stream targetStream = File.Open(filename, FileMode.Create))
            {
                foreach (string logFile in logFiles)
                {
                    using (Stream sourceStream = File.OpenRead(logFile))
                    {
                        sourceStream.CopyTo(targetStream);
                    }
                }
            }
        }

        private int getFileNumber(string filename)
        {
            int num = Int32.MinValue;
            Regex filenameRegex = new Regex($"{BackgroundLogcat.Value.DeviceFilename}(\\.(\\d+))?");
            Match match = filenameRegex.Match(filename);
            if (match.Success && match.Groups[2].Success)
            {
                num = int.Parse(match.Groups[2].Value);
            }
            return num;
        }

        #endregion

        private void deleteExistingLogFiles()
        {
            string filename = BackgroundLogcat.Value.DeviceFilename;
            if (BackgroundLogcat.Value.RotateFiles)
            {
                filename += "*";
            }
            Log.Debug($"Deleting existing log files: {filename}");
            AdbCommandResult result = Adb.ExecuteAdbCommand($"shell \"rm -f {filename}\"", BackgroundLogcat.Value.DeviceId, retries: 1);
            LogAdbOutput(result);
        }
    }
}
