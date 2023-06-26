// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]

using OpenTap;
using Tap.Plugins.UMA.Android.Instruments;
using Tap.Plugins.UMA.Android.Instruments.Logcat;

namespace Tap.Plugins.UMA.Android.Steps
{
    [Display("Retrieve Background Logcat",
        Groups: new string[] { "Android" },
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
            BackgroundLogcat logcat = BackgroundLogcat.Value;

            AdbCommandResult result = logcat.Terminate(Log);
            LogAdbOutput(result);

            string[] res = Adb.RetrieveLogcat(logcat, LocalFilename);

            foreach (string line in res)
            {
                Log.Debug(line);
            }

            if (DeleteFiles)
            {
                Adb.DeleteExistingDeviceLogcatFiles(logcat.DeviceFilename, logcat.DeviceId);
            }
        }
    }
}
