// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]

using System;

namespace Tap.Plugins.UMA.Android.Instruments.Logcat
{
    [Flags]
    public enum LogcatBuffer
    {
        Radio = 1,
        Events = 2,
        Main = 4,
        System = 8,
        Crash = 16
    }
}
