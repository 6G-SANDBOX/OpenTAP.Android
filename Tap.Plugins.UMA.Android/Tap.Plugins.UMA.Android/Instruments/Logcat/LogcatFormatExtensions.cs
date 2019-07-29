// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]
//
// This file cannot be modified or redistributed. This header cannot be removed.

using System.Collections.Generic;

namespace Tap.Plugins.UMA.Android.Instruments.Logcat
{
    public static class LogcatFormatExtensions
    {
        private static Dictionary<LogcatFormat, string> FORMAT_NAMES;

        static LogcatFormatExtensions()
        {
            FORMAT_NAMES = new Dictionary<LogcatFormat, string>()
            {
                { LogcatFormat.Brief, "brief" },
                { LogcatFormat.Long, "long" },
                { LogcatFormat.Process, "process" },
                { LogcatFormat.Raw, "raw" },
                { LogcatFormat.Tag, "tag" },
                { LogcatFormat.Thread, "thread" },
                { LogcatFormat.Threadtime, "threadtime" },
                { LogcatFormat.Time, "time" }
            };
        }

        public static string ToFormatName(this LogcatFormat format)
        {
            return FORMAT_NAMES[format];
        }
    }
}
