// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]
//
// This file cannot be modified or redistributed. This header cannot be removed.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTap;

namespace Tap.Plugins.UMA.Android.Instruments
{
    [Flags]
    public enum AdbInstallOption
    {
        [Display("Forward Lock Application")]
        ForwardLockApplication = 1,
        [Display("Replace Existing Application")]
        ReplaceExistingApplication = 2,
        [Display("Allow Test Packages")]
        AllowTestPackages = 4,
        [Display("Install Application On SD Card")]
        InstallApplicationOnSdCard = 8,
        [Display("Allow Version Code Downgrade")]
        AllowVersionCodeDowngrade = 16,
        [Display("Grant All Runtime Permissions")]
        GrantAllRuntimePermissions = 32
    }
}
