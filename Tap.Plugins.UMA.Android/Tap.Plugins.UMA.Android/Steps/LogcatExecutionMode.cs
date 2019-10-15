// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]
//
// This file cannot be modified or redistributed. This header cannot be removed.

using OpenTap;

namespace Tap.Plugins.UMA.Android.Steps
{
    public enum LogcatExecutionMode
    {
        [Display("Instant - Current Logcat Contents")]
        Instant,
        [Display("Continuous - Run Logcat in Background")]
        Continuous
    }
}
