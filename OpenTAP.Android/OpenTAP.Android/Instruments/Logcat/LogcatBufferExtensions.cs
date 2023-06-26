// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]

using System.Collections.Generic;
using System.Linq;

namespace Tap.Plugins.UMA.Android.Instruments.Logcat
{
    public static class LogcatBufferExtensions
    {
        private static Dictionary<LogcatBuffer, string> BUFFER_NAMES;

        static LogcatBufferExtensions()
        {
            BUFFER_NAMES = new Dictionary<LogcatBuffer, string>()
            {
                { LogcatBuffer.Radio, "radio" },
                { LogcatBuffer.Events, "events" },
                { LogcatBuffer.Main, "main" },
                { LogcatBuffer.System, "system" },
                { LogcatBuffer.Crash, "crash" },
            };
        }

        public static LogcatBuffer AllBuffers()
        {
            return LogcatBuffer.Radio | LogcatBuffer.Events | LogcatBuffer.Main | LogcatBuffer.System | LogcatBuffer.Crash;
        }

        public static LogcatBuffer DefaultBuffers()
        {
            return LogcatBuffer.Main;
        }

        public static IEnumerable<string> ToBufferNames(this LogcatBuffer buffer)
        {
            return BUFFER_NAMES
                .Keys
                .Where(b => buffer.HasFlag(b)).
                Select(b => BUFFER_NAMES[b]);
        }
    }
}
