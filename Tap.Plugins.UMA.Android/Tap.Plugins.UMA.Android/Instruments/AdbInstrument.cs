// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file cannot be modified or redistributed. This header cannot be removed.

using OpenTap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tap.Plugins.UMA.Android.Instruments.Logcat;

namespace Tap.Plugins.UMA.Android.Instruments
{
    [Display("adb Instrument",
        Group: "UMA",
        Description: "Instrument for executing commands through Android's adb tool")]
    public class AdbInstrument : Instrument
    {
        #region Settings

        [Display(Name: "adb Executable",
            Group: "adb",
            Description: "Path to the adb executable to be used",
            Order: 1.0)]
        [FilePath]
        public string AdbPath { get; set; }

        [Display(Name: "Remote adb",
            Group: "Remote",
            Description: "Select to connect to a remote adb server",
            Order: 2.0)]
        public bool Remote { get; set; }

        [Display(Name: "Host",
            Group: "Remote",
            Description: "Host of the remote adb server.",
            Order: 2.1)]
        [EnabledIf("Remote", true, HideIfDisabled = true)]
        public string RemoteHost { get; set; }

        [Display(Name: "Port",
            Group: "Remote",
            Description: "Port of the remote adb server. The default adb port is 5037.",
            Order: 2.2)]
        [EnabledIf("Remote", true, HideIfDisabled = true)]
        public int RemotePort { get; set; }

        #endregion

        #region Constants

        private const int PROCESS_TIMEOUT_MILLIS = 10000; // 10 seconds
        private const int PROCESS_LONG_TIMEOUT_MILLIS = 30000; // 30 seconds
        private const int RETRY_WAIT_MILLIS = 5000; // 5 seconds

        #endregion

        #region Fields

        private List<AdbProcess> backgroundCommands;

        #endregion

        public AdbInstrument()
        {
            Name = "ADB";

            backgroundCommands = new List<AdbProcess>();

            // Default values
            AdbPath = @"C:\Android\adb.exe";
            Remote = false;
            RemotePort = 5037;

            // Validation rules
            Rules.Add(() => (File.Exists(AdbPath)), "Please select a valid path to the adb executable", "AdbPath");
            Rules.Add(() => (!Remote || !string.IsNullOrWhiteSpace(RemoteHost)), "Please set a valid host", "RemoteHost");
            Rules.Add(() => (!Remote || RemotePort > 0), "Please set a valid port", "RemotePort");
        }

        public override void Close()
        {
            terminateDanglingBackgroundCommands();

            base.Close();
        }

        /// <summary>
        /// Execute an adb command.
        /// </summary>
        /// <param name="arguments">The arguments for adb</param>
        /// <param name="deviceId">The ID of the device that this command is targeted to; set to <c>null</c> if the command is not sent to a device</param>
        /// <param name="timeoutMillis">A timeout in milliseconds to wait before terminating adb</param>
        /// <param name="retries">Number of times to try to execute the command until it succeeds; use 1 to execute just once</param>
        /// <param name="retryWaitMillis">Time in milliseconds to wait between retries</param>
        /// <returns>The result of executing the adb command</returns>
        public AdbCommandResult ExecuteAdbCommand(string arguments, string deviceId = null, int timeoutMillis = PROCESS_TIMEOUT_MILLIS,
            int retries = 3, int retryWaitMillis = RETRY_WAIT_MILLIS)
        {
            string completeArguments = prepareArguments(arguments, deviceId);
            return executeCommandWithRetries(AdbPath, completeArguments, timeoutMillis, retries, retryWaitMillis);
        }

        /// <summary>
        /// Execute an adb command in background.
        /// </summary>
        /// <remarks>
        /// This method returns as soon as a new process with the adb command has been started.
        /// The command can either terminate on its own, or be forced to terminate.
        /// The latter is the appropriate action for one of the typical use cases: executing logcat in background on the device.
        /// </remarks>
        /// <param name="arguments">The arguments for adb</param>
        /// <param name="deviceId">The ID of the device that this command is targeted to; set to <c>null</c> if the command is not sent to a device</param>
        /// <returns>A handler for the background process</returns>
        public AdbProcess ExecuteAdbBackgroundCommand(string arguments, string deviceId = null)
        {
            string completeArguments = prepareArguments(arguments, deviceId);
            return executeCommandBackground(AdbPath, completeArguments);
        }

        /// <summary>
        /// Copies a local file to an Android device.
        /// </summary>
        /// <param name="localFile">The local file to copy</param>
        /// <param name="remoteFile">The path in the Android device where the file will be copied to</param>
        /// <param name="deviceId">The ID of the device where the file will be copied to; can be <c>null</c> if there is only one device connected</param>
        /// <param name="timeoutMillis">A timeout in milliseconds to wait before terminating adb</param>
        /// <param name="retries">Number of times to try to execute the command until it succeeds; use 1 to execute just once</param>
        /// <param name="retryWaitMillis">Time in milliseconds to wait between retries</param>
        /// <returns>The result of executing the adb command</returns>
        public AdbCommandResult Push(string localFile, string remoteFile, string deviceId = null, int timeoutMillis = PROCESS_LONG_TIMEOUT_MILLIS,
            int retries = 3, int retryWaitMillis = RETRY_WAIT_MILLIS)
        {
            string arguments = string.Format("push \"{0}\" \"{1}\"", localFile, remoteFile);
            return ExecuteAdbCommand(arguments, deviceId, timeoutMillis, retries, retryWaitMillis);
        }

        /// <summary>
        /// Copies a remote file from an Android device.
        /// </summary>
        /// <param name="remoteFile">The remote file in an Android device to copy</param>
        /// <param name="localFile">The local path where the remote file will be copied to</param>
        /// <param name="deviceId">The ID of the device where the file will be copied from; can be <c>null</c> if there is only one device connected</param>
        /// <param name="timeoutMillis">A timeout in milliseconds to wait before terminating adb</param>
        /// <param name="retries">Number of times to try to execute the command until it succeeds; use 1 to execute just once</param>
        /// <param name="retryWaitMillis">Time in milliseconds to wait between retries</param>
        /// <returns>The result of executing the adb command</returns>
        public AdbCommandResult Pull(string remoteFile, string localFile, string deviceId = null, int timeoutMillis = PROCESS_LONG_TIMEOUT_MILLIS,
            int retries = 3, int retryWaitMillis = RETRY_WAIT_MILLIS)
        {
            string arguments = string.Format("pull \"{0}\" \"{1}\"", remoteFile, localFile);
            return ExecuteAdbCommand(arguments, deviceId, timeoutMillis, retries, retryWaitMillis);
        }

        /// <summary>
        /// Installs a local APK file on an Android device.
        /// </summary>
        /// <param name="localFile">The local APK file to install</param>
        /// <param name="options">Options for the adb install command</param>
        /// <param name="deviceId">The ID of the device where the APK will be installed on; can be <c>null</c> if there is only one device connected</param>
        /// <param name="timeoutMillis">A timeout in milliseconds to wait before terminating adb</param>
        /// <param name="retries">Number of times to try to execute the command until it succeeds; use 1 to execute just once</param>
        /// <param name="retryWaitMillis">Time in milliseconds to wait between retries</param>
        /// <returns>The result of executing the adb command</returns>
        public AdbCommandResult Install(string localFile, AdbInstallOption options = 0, string deviceId = null, int timeoutMillis = PROCESS_LONG_TIMEOUT_MILLIS,
            int retries = 3, int retryWaitMillis = RETRY_WAIT_MILLIS)
        {
            string arguments = string.Format("install {0} \"{1}\"", options.ToAdbInstallFlags(), localFile);
            return ExecuteAdbCommand(arguments, deviceId, timeoutMillis, retries, retryWaitMillis);
        }

        /// <summary>
        /// Uninstalls an application from an Android device.
        /// </summary>
        /// <param name="package">The name of the package to uninstall</param>
        /// <param name="deviceId">The ID of the device where the file will be copied from; can be <c>null</c> if there is only one device connected</param>
        /// <param name="timeoutMillis">A timeout in milliseconds to wait before terminating adb</param>
        /// <param name="retries">Number of times to try to execute the command until it succeeds; use 1 to execute just once</param>
        /// <param name="retryWaitMillis">Time in milliseconds to wait between retries</param>
        /// <returns>The result of executing the adb command</returns>
        public AdbCommandResult Uninstall(string package, string deviceId = null, int timeoutMillis = PROCESS_LONG_TIMEOUT_MILLIS,
            int retries = 3, int retryWaitMillis = RETRY_WAIT_MILLIS)
        {
            string arguments = string.Format("uninstall {0}", package);
            return ExecuteAdbCommand(arguments, deviceId, timeoutMillis, retries, retryWaitMillis);
        }

        /// <summary>
        /// Reboots an Android device.
        /// </summary>
        /// <param name="deviceId">The ID of the device to reboot; can be <c>null</c> if there is only one device connected</param>
        /// <param name="timeoutMillis">A timeout in milliseconds to wait before terminating adb</param>
        /// <param name="retries">Number of times to try to execute the command until it succeeds; use 1 to execute just once</param>
        /// <param name="retryWaitMillis">Time in milliseconds to wait between retries</param>
        /// <returns>The result of executing the adb command</returns>
        public AdbCommandResult Reboot(string deviceId = null, int timeoutMillis = PROCESS_TIMEOUT_MILLIS,
            int retries = 3, int retryWaitMillis = RETRY_WAIT_MILLIS)
        {
            string arguments = "reboot";
            return ExecuteAdbCommand(arguments, deviceId, timeoutMillis, retries, retryWaitMillis);
        }

        public AdbCommandResult Dumpsys(string service, string deviceId = null)
        {
            string arguments = $"shell dumpsys {service}";

            return ExecuteAdbCommand(arguments, deviceId);
        }

        #region Command execution

        private string prepareArguments(string arguments, string deviceId)
        {
            List<string> argumentList = new List<string>();

            if (Remote)
            {
                argumentList.Add(string.Format("-H {0} -P {1}", RemoteHost, RemotePort));
            }

            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                argumentList.Add(string.Format("-s {0}", deviceId));
            }

            argumentList.Add(arguments);

            return string.Join(" ", argumentList);
        }

        private AdbCommandResult executeCommandWithRetries(string path, string arguments, int timeoutMillis, int retries, int retryWaitMillis)
        {
            AdbCommandResult result = executeCommand(path, arguments, timeoutMillis);
            for (int i = 1; i < retries && !result.Success; i++)
            {
                Log.Warning($"Error while executing command ({i}/{retries}); retrying after {retryWaitMillis} ms...");
                TapThread.Sleep(retryWaitMillis);
                result = executeCommand(path, arguments, timeoutMillis);
            }
            return result;
        }

        private AdbCommandResult executeCommand(string path, string arguments, int timeoutMillis = PROCESS_TIMEOUT_MILLIS)
        {
            AdbProcess adbProcess = createAdbProcess(path, arguments);
            return executeProcessForeground(adbProcess, timeoutMillis);
        }

        private AdbProcess executeCommandBackground(string path, string arguments)
        {
            AdbProcess adbProcess = createAdbProcess(path, arguments);
            executeProcessBackground(adbProcess);
            return adbProcess;
        }

        private AdbProcess createAdbProcess(string path, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = path,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            Process process = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            ProcessDataReceiver receiver = new ProcessDataReceiver(process);

            return new AdbProcess(path, arguments, process, receiver);
        }

        private AdbCommandResult executeProcessForeground(AdbProcess adbProcess, int timeoutMillis = PROCESS_TIMEOUT_MILLIS)
        {
            Log.Debug($"Executing: {adbProcess}");

            bool success = false;
            Process process = adbProcess.Process;

            try
            {
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                bool finished = process.WaitForExit(timeoutMillis);
                if (!finished)
                {
                    Log.Error("Timeout while executing {0}", adbProcess);
                }
                else if (process.ExitCode != 0)
                {
                    Log.Error("Process returned error exit code: {0}", process.ExitCode);
                }
                else
                {
                    success = true;
                }
            }
            catch (Exception e)
            {
                Log.Error("Error while executing executing {0}", adbProcess);
                Log.Error(e);
            }
            finally
            {
                process.Close();
            }

            AdbCommandResult result = new AdbCommandResult()
            {
                Success = success,
                Output = new List<string>(adbProcess.Receiver.Output)
            };

            return result;
        }

        private void executeProcessBackground(AdbProcess adbProcess)
        {
            Log.Debug($"Executing in background: {adbProcess}");

            addBackgroundCommand(adbProcess);

            adbProcess.Process.Exited += ((sender, e) => removeBackgroundCommand(adbProcess));
            adbProcess.Process.Start();
            adbProcess.Process.BeginErrorReadLine();
            adbProcess.Process.BeginOutputReadLine();
        }

        public void LogAdbOutput(AdbCommandResult result)
        {
            Log.Debug("------ Start of adb output ------");
            foreach (string line in result.Output)
            {
                Log.Debug(line);
            }
            Log.Debug("------- End of adb output -------");
        }

        #endregion

        #region Logcat handling

        public AdbCommandResult DeleteExistingDeviceLogcatFiles(string filename, string deviceId)
        {
            Log.Debug($"Deleting existing log files: {filename}*");
            AdbCommandResult result = this.ExecuteAdbCommand($"shell \"rm -f {filename}*\"", deviceId, retries: 1);
            return result;
        }

        public AdbCommandResult ExecuteLogcat(string deviceId = null,
            LogcatFilter filter = null, LogcatBuffer buffer = LogcatBuffer.Main, LogcatFormat format = LogcatFormat.Threadtime)
        {
            LogcatCommandBuilder builder = baseBuilder(filter, buffer, format);
            builder.DumpAndExit = true;

            return this.ExecuteAdbCommand(builder.Build(), deviceId);
        }

        public BackgroundLogcat ExecuteBackgroundLogcat(string deviceFilename, string deviceId = null,
            LogcatFilter filter = null, LogcatBuffer buffer = LogcatBuffer.Main, LogcatFormat format = LogcatFormat.Threadtime)
        {
            LogcatCommandBuilder builder = baseBuilder(filter, buffer, format);
            builder.Filename = deviceFilename;
            builder.RotateFileSize = 16384; // 16Mb
            builder.RotateFileCount = 8;
            builder.DumpAndExit = false;

            AdbProcess process = this.ExecuteAdbBackgroundCommand(builder.Build(), deviceId);
            return new BackgroundLogcat(process, deviceId, deviceFilename, rotateFiles: true);
        }

        private LogcatCommandBuilder baseBuilder(LogcatFilter filter, LogcatBuffer buffer, LogcatFormat format)
        {
            return new LogcatCommandBuilder { Filter = filter ?? new LogcatFilter(), Buffer = buffer, Format = format };
        }

        public string[] RetrieveLogcat(BackgroundLogcat logcat, string localFilename = null)
        {
            string[] res = new string[] { };

            IEnumerable<string> deviceFiles = getAvailableLogFiles(logcat.DeviceFilename);

            if (deviceFiles.Count() != 0)
            {
                string tempFolder = createTempFolder();

                try
                {
                    IEnumerable<string> localFiles = pullLogFiles(tempFolder, deviceFiles);
                    res = combineLogFiles(localFiles, localFilename);
                }
                finally
                {
                    Directory.Delete(tempFolder, true);
                }
            }
            else
            {
                Log.Warning($"No log files found ({logcat.DeviceFilename})");
            }

            return res;
        }

        private IEnumerable<string> getAvailableLogFiles(string fileName)
        {
            string listfiles = fileName + "*";
            Regex filenameRegex = new Regex($"{fileName}(\\.(\\d+))?");

            AdbCommandResult result = this.ExecuteAdbCommand($"shell \"ls {listfiles}\"");
            List<string> files = new List<string>();

            if (result.Success)
            {
                foreach (string line in result.Output) { files.Add(line.Trim()); }
            }

            return files.OrderBy((f) => getFileNumber(f, filenameRegex)).Reverse();
        }

        private int getFileNumber(string filename, Regex regex)
        {
            int num = int.MinValue;
            Match match = regex.Match(filename);
            if (match.Success && match.Groups[2].Success)
            {
                num = int.Parse(match.Groups[2].Value);
            }
            return num;
        }

        private string createTempFolder()
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
                AdbCommandResult result = this.Pull(deviceFile, localFile);
                if (result.Success)
                {
                    localFiles.Add(localFile);
                }
                else
                {
                    Log.Error($"Could not pull {deviceFile}");
                    LogAdbOutput(result);
                }
            }

            return localFiles;
        }

        private string[] combineLogFiles(IEnumerable<string> logFiles, string filename)
        {
            List<string> res = new List<string>();

            foreach (string logFile in logFiles)
            {
                using (TextReader sourceStream = File.OpenText(logFile))
                {
                    string line;
                    while ((line = sourceStream.ReadLine()) != null)
                    {
                        res.Add(line);
                    }
                }
            }

            if (!string.IsNullOrEmpty(filename))
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

            return res.ToArray();
        }

        #endregion

        #region Background commands handling

        private void terminateDanglingBackgroundCommands()
        {
            if (backgroundCommands.Count > 0)
            {
                Log.Warning($"Closing {backgroundCommands.Count} dangling background command(s)");
            }

            IReadOnlyList<AdbProcess> danglingCommands = new List<AdbProcess>(backgroundCommands).AsReadOnly();
            foreach (AdbProcess command in danglingCommands)
            {
                try
                {
                    command.Terminate();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private void addBackgroundCommand(AdbProcess adbProcess)
        {
            lock (backgroundCommands)
            {
                backgroundCommands.Add(adbProcess);
                Log.Debug($"Added new background command; {backgroundCommands.Count} background command(s)");
            }
        }

        private void removeBackgroundCommand(AdbProcess adbProcess)
        {
            lock (backgroundCommands)
            {
                backgroundCommands.Remove(adbProcess);
                Log.Debug($"Removed background command; {backgroundCommands.Count} background command(s)");
            }
        }

        #endregion
    }
}
