// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]

using System;
using System.Collections.Generic;

namespace Tap.Plugins.UMA.Android.Instruments.ActivityManager.Intents
{
    public static class IntentFlagsExtensions
    {
        private static readonly Dictionary<IntentFlags, string> flagToArgument;

        static IntentFlagsExtensions()
        {
            flagToArgument = new Dictionary<IntentFlags, string>
            {
                {IntentFlags.GrantReadUriPermission, "--grant-read-uri-permission" },
                {IntentFlags.GrantWriteUriPermission, "--grant-write-uri-permission" },
                {IntentFlags.DebugLogResolution, "--debug-log-resolution" },
                {IntentFlags.ExcludeStoppedPackages, "--exclude-stopped-packages" },
                {IntentFlags.IncludeStoppedPackages, "--include-stopped-packages" },
                {IntentFlags.ActivityBroughtToFront, "--activity-brought-to-front" },
                {IntentFlags.ActivityClearTop, "--activity-clear-top" },
                {IntentFlags.ActivityClearWhenTaskReset, "--activity-clear-when-task-reset" },
                {IntentFlags.ActivityExcludeFromRecents, "--activity-exclude-from-recents" },
                {IntentFlags.ActivityLaunchedFromHistory, "--activity-launched-from-history" },
                {IntentFlags.ActivityMultipleTask, "--activity-multiple-task" },
                {IntentFlags.ActivityNoAnimation, "--activity-no-animation" },
                {IntentFlags.ActivityNoHistory, "--activity-no-history" },
                {IntentFlags.ActivityNoUserAction, "--activity-no-user-action" },
                {IntentFlags.ActivityPreviousIsTop, "--activity-previous-is-top" },
                {IntentFlags.ActivityReorderToFront, "--activity-reorder-to-front" },
                {IntentFlags.ActivityResetTaskIfNeeded, "--activity-reset-task-if-needed" },
                {IntentFlags.ActivitySingleTop, "--activity-single-top" },
                {IntentFlags.ActivityClearTask, "--activity-clear-task" },
                {IntentFlags.ActivityTaskOnHome, "--activity-task-on-home" },
                {IntentFlags.ReceiverRegisterOnly, "--receiver-registered-only" },
                {IntentFlags.ReceiverReplacePending, "--receiver-replace-pending" }
            };
        }

        public static string Argument(this IntentFlags flag)
        {
            if (!flagToArgument.ContainsKey(flag)) { throw new ArgumentException($"Intent flag {flag} not recognized."); }

            return flagToArgument[flag];
        }
    }
}
