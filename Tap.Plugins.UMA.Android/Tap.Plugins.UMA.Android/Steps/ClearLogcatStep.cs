// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]
//
// This file cannot be modified or redistributed. This header cannot be removed.

using OpenTap;
using Tap.Plugins.UMA.Android.Instruments;

namespace Tap.Plugins.UMA.Android.Steps
{
    [Display("Clear logcat",
        Groups: new string[] { "UMA", "Android" },
        Description: "Clears the logcat from an Android device.")]
    public class ClearLogcatStep : AdbStepBase
    {

        [Display(Name: "Device ID",
            Group: "Device",
            Description: "The ID of the device whose logcat will be cleared.",
            Order: 1.0)]
        public string DeviceId { get; set; }

        public override void Run()
        {
            AdbCommandResult result = Adb.ExecuteAdbCommand("logcat -c", DeviceId);

            HandleResult(result);
        }
    }
}
