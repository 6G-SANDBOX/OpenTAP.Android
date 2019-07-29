// Author:      Alberto Salmerón Moreno<salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]
//
// This file cannot be modified or redistributed. This header cannot be removed.

using Tap.Plugins.UMA.Android.Instruments;
using System;
using Keysight.Tap;

namespace Tap.Plugins.UMA.Android.Instruments.Logcat
{
    public class BackgroundLogcat
    {
        public AdbProcess AdbProcess { get; private set; }
        public string DeviceId { get; private set; }
        public string DeviceFilename { get; private set; }
        public bool RotateFiles { get; private set; }
        public DateTime StartTime { get; private set; }

        public BackgroundLogcat(AdbProcess adbProcess, string deviceId, string deviceFilename, bool rotateFiles)
        {
            AdbProcess = adbProcess;
            DeviceId = deviceId;
            DeviceFilename = deviceFilename;
            RotateFiles = rotateFiles;
            StartTime = DateTime.Now;
        }

        public AdbCommandResult Terminate(TraceSource log = null)
        {
            if (AdbProcess.HasFinished && log != null)
            {
                log.Warning("Background process already terminated");
            }
            else
            {
                if (log != null) { log.Info("Terminating background process"); }
                AdbProcess.Terminate();
            }
            return AdbProcess.Result;
        }
    }
}
