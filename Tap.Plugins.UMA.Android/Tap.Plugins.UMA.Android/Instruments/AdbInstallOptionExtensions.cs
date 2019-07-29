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

namespace Tap.Plugins.UMA.Android.Instruments
{
    public static class AdbInstallOptionExtensions
    {
        public static readonly Dictionary<Enum, string> OptionsToFlags;

        static AdbInstallOptionExtensions()
        {
            OptionsToFlags = new Dictionary<Enum, string>();
            OptionsToFlags[AdbInstallOption.ForwardLockApplication] = "-l";
            OptionsToFlags[AdbInstallOption.ReplaceExistingApplication] = "-r";
            OptionsToFlags[AdbInstallOption.AllowTestPackages] = "-t";
            OptionsToFlags[AdbInstallOption.InstallApplicationOnSdCard] = "-s";
            OptionsToFlags[AdbInstallOption.AllowVersionCodeDowngrade] = "-d";
            OptionsToFlags[AdbInstallOption.GrantAllRuntimePermissions] = "-g";
        }

        public static string ToAdbInstallFlags(this AdbInstallOption options)
        {
            List<string> flags = new List<string>();

            IEnumerable<Enum> allOptions = Enum.GetValues(typeof(AdbInstallOption)).Cast<Enum>();
            foreach (AdbInstallOption option in allOptions.Where(options.HasFlag).Where(OptionsToFlags.ContainsKey))
            {
                flags.Add(OptionsToFlags[option]);
            }

            return string.Join(" ", flags);
        }
    }
}
