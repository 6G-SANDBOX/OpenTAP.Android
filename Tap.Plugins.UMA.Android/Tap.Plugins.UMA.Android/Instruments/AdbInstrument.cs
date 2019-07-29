// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file cannot be modified or redistributed. This header cannot be removed.

using Keysight.Tap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Tap.Plugins.UMA.Android.Instruments
{
    [Display("adb Instrument",
        Group: "UMA",
        Description: "Instrument for executing commands through Android's adb tool")]
    [ShortName("ADB")]
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

        /// <summary>
        /// Sets or unsets the airplane mode of an Android device.
        /// </summary>
        /// <remarks>
        /// This method uses several adb commands to set or unset the airplane mode. The caller can
        /// set the timeout that will be applied to each command, but not to the whole process.
        /// </remarks>
        /// <param name="enable"><c>true</c> to enable airplane mode; <c>false</c> to disable it</param>
        /// <param name="deviceId">The ID of the device; can be <c>null</c> if there is only one device connected</param>
        /// <param name="timeoutMillis">A timeout in milliseconds to wait before terminating each adb command</param>
        /// <returns>The result of executing the adb command</returns>
        public AdbCommandResult SetAirplaneMode(bool enable, string deviceId = null, int timeoutMillis = PROCESS_LONG_TIMEOUT_MILLIS)
        {
            AdbCommandResult settingResult = setAirplaneModeSetting(deviceId, enable);
            if (!settingResult.Success)
            {
                return settingResult;
            }

            AdbCommandResult intentResult = sendAirplaneModeIntent(deviceId, enable);

            // Insert output from first command before the output from the second
            intentResult.Output.Insert(0, Environment.NewLine);
            intentResult.Output.InsertRange(0, settingResult.Output);

            return intentResult;
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
                TestPlan.Sleep(retryWaitMillis);
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

        #region Airplane mode

        private AdbCommandResult setAirplaneModeSetting(string deviceId, bool enable, int timeoutMillis = PROCESS_TIMEOUT_MILLIS)
        {
            string arguments = string.Format("shell settings put global airplane_mode_on {0}", enable ? "1" : "0");
            return ExecuteAdbCommand(arguments, deviceId, timeoutMillis);
        }

        private AdbCommandResult sendAirplaneModeIntent(string deviceId, bool enable, int timeoutMillis = PROCESS_LONG_TIMEOUT_MILLIS)
        {
            string arguments = string.Format("shell am broadcast -a android.intent.action.AIRPLANE_MODE --ez state {0}", enable ? "true" : "false");
            return ExecuteAdbCommand(arguments, deviceId, timeoutMillis);
        }

        #endregion
    }
}
