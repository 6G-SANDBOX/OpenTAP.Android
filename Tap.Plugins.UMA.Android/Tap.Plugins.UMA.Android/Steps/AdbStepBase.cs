// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]
//
// This file cannot be modified or redistributed. This header cannot be removed.

using Keysight.Tap;
using Tap.Plugins.UMA.Android.Instruments;

namespace Tap.Plugins.UMA.Android.Steps
{
    public abstract class AdbStepBase : TestStep
    {
        #region Settings

        [Display(Name: "adb",
            Group: "adb",
            Description: "adb Instrument.",
            Order: 0.0)]
        public AdbInstrument Adb { get; set; }

        #endregion

        public AdbStepBase()
        {

        }

        /// <summary>
        /// Sets the verdict to <c>Verdict.Fail</c> if the command was not successful, and logs its output.
        /// </summary>
        /// <param name="result">The results from an adb command</param>
        /// <returns><c>true</c> if the given result was sucessful; <c>false</c> otherwise</returns>
        protected bool HandleResult(AdbCommandResult result)
        {
            if (!result.Success)
            {
                UpgradeVerdict(Verdict.Fail);
            }

            LogAdbOutput(result);

            return result.Success;
        }

        protected void LogAdbOutput(AdbCommandResult result)
        {
            Log.Debug("Command was {0}", result.Success ? "succesful" : "not sucessful");

            Log.Debug("------ Start of adb output ------");
            foreach (string line in result.Output)
            {
                Log.Debug(line);
            }
            Log.Debug("------- End of adb output -------");
        }
    }
}
