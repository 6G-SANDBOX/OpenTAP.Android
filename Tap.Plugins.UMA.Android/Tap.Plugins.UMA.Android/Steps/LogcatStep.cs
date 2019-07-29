﻿// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]
//
// This file cannot be modified or redistributed. This header cannot be removed.

using Keysight.Tap;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Tap.Plugins.UMA.Android.Instruments;
using Tap.Plugins.UMA.Android.Instruments.Logcat;

namespace Tap.Plugins.UMA.Android.Steps
{
    [Display("Logcat",
        Groups: new string[] { "UMA", "Android" },
        Description: "Gets the logcat from an Android device.")]
    public class LogcatStep : AdbStepBase
    {
        #region Constants

        private static readonly LogcatOutputMode[] FILE_OUTPUT_MODES;

        static LogcatStep()
        {
            FILE_OUTPUT_MODES = new LogcatOutputMode[]
            {
                LogcatOutputMode.LocalFile,
                LogcatOutputMode.DeviceFile
            };
        }

        #endregion

        #region Settings

        [Display(Name: "Device ID",
            Group: "Device",
            Description: "The ID of the device whose logcat will be retrieved.",
            Order: 1.0)]
        public string DeviceId { get; set; }

        [Display(Name: "Filter tags",
            Group: "Filter",
            Description: "The tags to be filter, and their priority.",
            Order: 2.0)]
        public List<LogcatFilterPair> FilterPairs { get; set; }

        [Display(Name: "Clean filter tags", 
            Group: "Filter",
            Description: "Removes duplicated or empty entries from the list of filters.",
            Order: 2.1)]
        [Browsable(true)]
        public void CleanFilterPairs()
        {
            cleanFilterPairs();
        }

        [Display(Name: "Default filter",
            Group: "Filter",
            Description: "If enabled, filter all the tags not explicity filtered above\n" +
                "using this priority.",
            Order: 2.2)]
        public Enabled<LogcatPriority> DefaultFilterPriority { get; set; }

        [Display(Name: "Buffers",
            Group: "Filter",
            Description: "Get the logcat only from the specified buffer or buffers.\n" +
                "Default is: main.",
            Order: 2.3)]
        public LogcatBuffer Buffer { get; set; }

        [Display(Name: "Execution Mode",
            Group: "Execution",
            Description: "Whether to return the current logcat contents, or run in background:\n" +
                "- Instantaneous: return the current logcat contents\n" +
                "- Continuous: run logcat on device in background; use a 'Retrieve\n" +
                "  Background Logcat' step to stop and save the output generated\n" +
                "  by this background logcat",
            Order: 3.0)]
        public LogcatExecutionMode ExecutionMode { get; set; }

        [Display(Name: "Format",
            Group: "Output",
            Description: "The format used for the logcat output. Default: threadtime.",
            Order: 4.0)]
        public LogcatFormat Format { get; set; }

        [Display("Target",
            Group: "Output",
            Description: "Where the logcat output will be shown or saved:\n" +
                "- Log: dump to TAP log\n" +
                "- Local File: save to a file in the computer\n" +
                "- Device File: save to a file in the Android device",
            Order: 4.1)]
        [EnabledIf("ExecutionMode", LogcatExecutionMode.Instant, HideIfDisabled = true)]
        public LogcatOutputMode OutputMode { get; set; }

        [Display(Name: "Local File",
            Group: "Output",
            Description: "Write logcat to this local file.",
            Order: 4.2)]
        [FilePath]
        [EnabledIf("ExecutionMode", LogcatExecutionMode.Instant, HideIfDisabled = true)]
        [EnabledIf("OutputMode", LogcatOutputMode.LocalFile, HideIfDisabled = true)]
        public string LocalFilename { get; set; }

        [Display(Name: "Device File",
            Group: "Output",
            Description: "Write logcat to this file on the Android device. In continuous mode,\n" +
                "the temporary log will always be saved to the device. When rotating\n" +
                "log files, additional log files will have a suffix \".N\".",
            Order: 4.3)]
        [EnabledIf("SaveLogcatToDevice", true, HideIfDisabled = true)]
        public string DeviceFilename { get; set; }

        [Display("Rotate Files",
            Group: "Rotation",
            Description: "If enabled, rotate the logcat output into several files.\n" +
                "All existing files with the same name will be deleted first.",
            Order: 5.0)]
        [EnabledIf("SaveLogcatToDevice", true, HideIfDisabled = true)]
        public bool RotateFiles { get; set; }

        [Display("Rotate File Size",
            Group: "Rotation",
            Description: "Rotate the log file when it reaches this size.\n" +
                "Default: 16 Kb",
            Order: 5.1)]
        [Unit("Kb")]
        [EnabledIf("SaveLogcatToDevice", true, HideIfDisabled = true)]
        [EnabledIf("RotateFiles", true, HideIfDisabled = true)]
        public int RotateFileSize { get; set; }

        [Display("Rotate File Count",
            Group: "Rotation",
            Description: "The maximum number of rotated log files.\n" +
                "Default: 4",
            Order: 5.2)]
        [EnabledIf("SaveLogcatToDevice", true, HideIfDisabled = true)]
        [EnabledIf("RotateFiles", true, HideIfDisabled = true)]
        public int RotateFileCount { get; set; }

        [Display("Background Logcat")]
        [Output]
        [XmlIgnore]
        public BackgroundLogcat BackgroundLogcat { get; private set; }

        #endregion

        #region Helper properties

        [XmlIgnore]
        public bool SaveLogcatToDevice
        {
            get { return LogcatExecutionMode.Continuous == ExecutionMode || LogcatOutputMode.DeviceFile == OutputMode; }
        }

        [XmlIgnore]
        public bool SaveLogcatToLocal
        {
            get { return LogcatExecutionMode.Instant == ExecutionMode && LogcatOutputMode.LocalFile == OutputMode; }
        }

        #endregion

        public LogcatStep()
        {
            FilterPairs = new List<LogcatFilterPair>();

            // Default values
            DefaultFilterPriority = new Enabled<LogcatPriority> { IsEnabled = false, Value = LogcatPriority.Silent };
            Buffer = LogcatBufferExtensions.DefaultBuffers();
            Format = LogcatFormat.Threadtime;
            DeviceFilename = "$EXTERNAL_STORAGE/triangle.log";
            RotateFiles = false;
            RotateFileSize = 16;
            RotateFileCount = 4;

            // Validation rules
            Rules.Add(() => (!SaveLogcatToLocal || !string.IsNullOrWhiteSpace(LocalFilename)), "Please set a valid local file path", "LocalFilename");
            Rules.Add(() => (!SaveLogcatToDevice || !string.IsNullOrWhiteSpace(DeviceFilename)), "Please set a valid device file path", "DeviceFilename");
            Rules.Add(() => (!SaveLogcatToDevice || !RotateFiles || RotateFileSize > 0), "Please set a rotate file size larger than 0 Kb", "RotateFileSize");
            Rules.Add(() => (!SaveLogcatToDevice || !RotateFiles || RotateFileCount > 0), "Please set a rotate file count larger than zero", "RotateFileCount");
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();

            // Since TAP never calls the FilterPair setter during test plan design,
            // this is the best place where we can clean the list (in place)
            cleanFilterPairs();
        }

        public override void Run()
        {
            if (SaveLogcatToDevice && RotateFiles)
            {
                deleteExistingRotateLogFiles();
            }

            string logcatArguments = buildArguments();

            if (LogcatExecutionMode.Continuous == ExecutionMode)
            {
                AdbProcess process = Adb.ExecuteAdbBackgroundCommand(logcatArguments, DeviceId);
                BackgroundLogcat = new BackgroundLogcat(process, DeviceId, DeviceFilename, RotateFiles);
            }
            else
            {
                AdbCommandResult result = Adb.ExecuteAdbCommand(logcatArguments, DeviceId);

                if (LogcatOutputMode.LocalFile == OutputMode)
                {
                    writeOutputToFile(result, LocalFilename);
                    result.Output.Clear();
                }

                HandleResult(result);
            }
        }

        #region Private methods

        private void cleanFilterPairs()
        {
            List<LogcatFilterPair> cleaned = FilterPairs
                .FindAll(p => !string.IsNullOrWhiteSpace(p.Tag))
                .Distinct(new LogcatFilterPairTagComparer()).ToList<LogcatFilterPair>();

            int removed = FilterPairs.Count - cleaned.Count;
            if (removed != 0) { Log.Info($"Removing {removed} duplicated or empty filters."); }

            FilterPairs.Clear();
            FilterPairs.AddRange(cleaned);
        }

        private void deleteExistingRotateLogFiles()
        {
            Log.Debug($"Deleting existing log files: {DeviceFilename}*");
            AdbCommandResult result = Adb.ExecuteAdbCommand($"shell \"rm -f {DeviceFilename}*\"", DeviceId, retries: 1);
            LogAdbOutput(result);
        }

        private string buildArguments()
        {
            LogcatCommandBuilder builder = new LogcatCommandBuilder();
            builder.Filter = buildFilter();
            builder.Buffer = Buffer;
            builder.Format = Format;
            if (SaveLogcatToDevice)
            {
                builder.Filename = DeviceFilename;
                if (RotateFiles)
                {
                    builder.RotateFileSize = RotateFileSize;
                    builder.RotateFileCount = RotateFileCount;
                }
                if (LogcatExecutionMode.Continuous == ExecutionMode)
                {
                    builder.DumpAndExit = false;
                }
            }
            return builder.Build();
        }

        private LogcatFilter buildFilter()
        {
            LogcatFilter filter = new LogcatFilter();

            foreach (LogcatFilterPair pair in FilterPairs)
            {
                filter.SetTagPriority(pair.Tag, pair.Priority);
            }

            if (DefaultFilterPriority.IsEnabled)
            {
                filter.DefaultPriority = DefaultFilterPriority.Value;
            }

            return filter;
        }

        private void writeOutputToFile(AdbCommandResult result, string filename)
        {
            Log.Info($"Writing logcat to {filename}");

            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (string line in result.Output)
                {
                    writer.WriteLine(line);
                }
            }
        }


        #endregion

        #region Inner classes

        private class LogcatFilterPairTagComparer : IEqualityComparer<LogcatFilterPair>
        {
            public bool Equals(LogcatFilterPair x, LogcatFilterPair y)
            {
                return x.Tag.Equals(y.Tag);
            }

            public int GetHashCode(LogcatFilterPair obj)
            {
                return obj.Tag.GetHashCode();
            }
        }

        #endregion
    }
}
