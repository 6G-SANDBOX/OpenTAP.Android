// Author:      Bruno Garcia Garcia <bgg@uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]

using System;

namespace Tap.Plugins.UMA.Android.Instruments.ActivityManager.Intents
{
    public enum IntentFlags
    {
        GrantReadUriPermission,
        GrantWriteUriPermission,
        DebugLogResolution,
        ExcludeStoppedPackages,
        IncludeStoppedPackages,
        ActivityBroughtToFront,
        ActivityClearTop,
        ActivityClearWhenTaskReset,
        ActivityExcludeFromRecents,
        ActivityLaunchedFromHistory,
        ActivityMultipleTask,
        ActivityNoAnimation,
        ActivityNoHistory,
        ActivityNoUserAction,
        ActivityPreviousIsTop,
        ActivityReorderToFront,
        ActivityResetTaskIfNeeded,
        ActivitySingleTop,
        ActivityClearTask,
        ActivityTaskOnHome,
        ReceiverRegisterOnly,
        ReceiverReplacePending
    }
}
