// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]
//
// This file cannot be modified or redistributed. This header cannot be removed.

using Keysight.Tap;

namespace Tap.Plugins.UMA.Android.Steps
{
    /// <summary>
    /// Base class for steps that use ADB.
    /// Includes advanced settings for the command execution
    /// </summary>
    public abstract class AdbCommandStepBase : AdbStepBase
    {
        [Display(Name: "Number of Retries",
                 Group: "Advanced",
                 Description: "Number of times to try to execute the command until it succeeds.\n" +
                              "Use 1 to try just once. Default: 3.",
                 Order: 99.0,
                 Collapsed: true)]
        public int Retries { get; set; }

        [Display(Name: "Retry Wait",
                 Group: "Advanced",
                 Description: "Time to wait between retries.\n" +
                              "Default: 5000 ms.",
                 Order: 99.1,
                 Collapsed: true)]
        [Unit("ms")]
        public int RetryWaitMillis { get; set; }

        [Display(Name: "Timeout",
                 Group: "Advanced",
                 Description: "Timeout for the adb command to complete, before terminating it.\n" +
                              "Default: 30000 ms.",
                 Order: 99.2,
                 Collapsed: true)]
        [Unit("ms")]
        public int TimeoutMillis { get; set; }

        public AdbCommandStepBase()
        {
            Retries = 3;
            RetryWaitMillis = 5000; // 5 seconds
            TimeoutMillis = 30000; // 30 seconds
        }

    }
}
