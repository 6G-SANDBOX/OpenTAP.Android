// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]

using System;
using OpenTap;
using Tap.Plugins.UMA.Android.Instruments;

namespace Tap.Plugins.UMA.Android.Steps
{
    [Display("adb Command",
        Groups: new string[] { "Android" },
        Description: "Send a command using Android's adb tool")]
    public class AdbCommandStep : AdbCommandStepBase
    {
        #region Settings

        [Display(Name: "Command",
            Group: "Command",
            Description: "A set of pre-defined adb commands. Each one takes a different set of parameters:\n" +
                "- Custom: send any adb command with the given arguments\n" +
                "- Push: upload a local file to an Android device\n" +
                "- Pull: download a remote file from an Android device",
            Order: 1.0)]
        public AdbCommand Command { get; set; }

        [Display(Name: "Device ID",
            Group: "Command",
            Description: "The ID of the device to which this command will be sent.\n" +
                "This is not required for commands that are not sent to\n" +
                "a device (e.g. 'adb devices'), or when a single device\n" +
                "is connected.",
            Order: 1.1)]
        public string DeviceId { get; set; }

        [Display(Name: "Arguments",
            Group: "Command",
            Description: "The arguments that will be passed to adb, as is.",
            Order: 1.2)]
        [EnabledIf("Command", AdbCommand.Custom, HideIfDisabled = true)]
        public string Arguments { get; set; }

        [Display(Name: "Local file",
            Group: "Command",
            Description: "A local file to push or install, or where to save a pull.",
            Order: 1.3)]
        [FilePath]
        [EnabledIf("Command", AdbCommand.Push, AdbCommand.Pull, AdbCommand.Install, HideIfDisabled = true)]
        public string LocalFile { get; set; }

        [Display(Name: "Remote file",
            Group: "Command",
            Description: "A remote file to pull, or where to save a push.",
            Order: 1.4)]
        [EnabledIf("Command", AdbCommand.Push, AdbCommand.Pull, HideIfDisabled = true)]
        public string RemoteFile { get; set; }

        [Display(Name: "Install options",
            Group: "Command",
            Description: "Options for adb install command.",
            Order: 1.5)]
        [EnabledIf("Command", AdbCommand.Install, HideIfDisabled = true)]
        public AdbInstallOption AdbInstallOptions { get; set; }

        [Display(Name: "Package name",
            Group: "Command",
            Description: "Name of the package used in the command.",
            Order: 1.6)]
        [EnabledIf("Command", AdbCommand.Uninstall, AdbCommand.StartApp, HideIfDisabled = true)]
        public string Package { get; set; }

        #endregion

        public AdbCommandStep()
        {
            // Default values
            Command = AdbCommand.Custom;
            Arguments = "devices";
            AdbInstallOptions = AdbInstallOption.ReplaceExistingApplication | AdbInstallOption.AllowVersionCodeDowngrade;

            // Validation rules
            Rules.Add(() => (Command != AdbCommand.Custom || !string.IsNullOrWhiteSpace(Arguments)), "adb requires at least one argument", "Arguments");
            Rules.Add(() => ((Command != AdbCommand.Push && Command != AdbCommand.Pull && Command != AdbCommand.Install) || !string.IsNullOrWhiteSpace(LocalFile)), "Please set a valid local file", "LocalFile");
            Rules.Add(() => ((Command != AdbCommand.Push && Command != AdbCommand.Pull) || !string.IsNullOrWhiteSpace(RemoteFile)), "Please set a valid local file", "RemoteFile");
            Rules.Add(() => (Command != AdbCommand.Uninstall && Command != AdbCommand.StartApp || !string.IsNullOrWhiteSpace(Package)), "Please set a package name", "Package");
            Rules.Add(() => (Retries > 0), "Please set a number of retries greater than zero", "Retries");
            Rules.Add(() => (RetryWaitMillis > 0), "Please set a retry wait greater than zero", "RetryWaitMillis");
            Rules.Add(() => (TimeoutMillis > 0), "Please set a timeout greater than zero", "TimeoutMillis");
        }

        public override void Run()
        {
            AdbCommandResult result = executeCommand();
            HandleResult(result);
        }

        #region Private methods

        private AdbCommandResult executeCommand()
        {
            switch (Command)
            {
                case AdbCommand.Custom:
                    return Adb.ExecuteAdbCommand(Arguments, DeviceId, TimeoutMillis, Retries, RetryWaitMillis);
                case AdbCommand.Push:
                    return Adb.Push(LocalFile, RemoteFile, DeviceId, TimeoutMillis, Retries, RetryWaitMillis);
                case AdbCommand.Pull:
                    return Adb.Pull(RemoteFile, LocalFile, DeviceId, TimeoutMillis, Retries, RetryWaitMillis);
                case AdbCommand.Install:
                    return Adb.Install(LocalFile, AdbInstallOptions, DeviceId, TimeoutMillis, Retries, RetryWaitMillis);
                case AdbCommand.Uninstall:
                    return Adb.Uninstall(Package, DeviceId, TimeoutMillis, Retries, RetryWaitMillis);
                case AdbCommand.Reboot:
                    return Adb.Reboot(DeviceId, TimeoutMillis, Retries, RetryWaitMillis);
                case AdbCommand.StartApp:
                    return Adb.ExecuteAdbCommand(
                        $"shell monkey -p {Package} -c android.intent.category.LAUNCHER 1", DeviceId, TimeoutMillis, Retries, RetryWaitMillis);
                default:
                    throw new ArgumentException("Unsupported adb command: {0}", Enum.GetName(typeof(AdbCommand), Command));
            }
        }

        #endregion
    }
}
