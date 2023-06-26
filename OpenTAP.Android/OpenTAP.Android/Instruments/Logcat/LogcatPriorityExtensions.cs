// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]

using System;

namespace Tap.Plugins.UMA.Android.Instruments.Logcat
{
    public static class LogcatPriorityExtensions
    {
        public static string ToCode(this LogcatPriority priority)
        {
            switch (priority)
            {
                case LogcatPriority.Verbose: return "V";
                case LogcatPriority.Debug: return "D";
                case LogcatPriority.Info: return "I";
                case LogcatPriority.Warning: return "W";
                case LogcatPriority.Error: return "E";
                case LogcatPriority.Fatal: return "F";
                case LogcatPriority.Silent: return "S";
                default: throw new ArgumentException($"Unknown logcat priority {priority}");
            }
        }
    }
}
