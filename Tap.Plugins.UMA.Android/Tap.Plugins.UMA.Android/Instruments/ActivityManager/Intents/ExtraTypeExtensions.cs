// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]
//
// This file cannot be modified or redistributed. This header cannot be removed.

using System;

namespace Tap.Plugins.UMA.Android.Instruments.ActivityManager.Intents
{
    public static class ExtraTypeExtensions
    {
        public static string Argument(this ExtraType type)
        {
            switch (type)
            {
                case ExtraType.Null: return "--esn";
                case ExtraType.String: return "--es";
                case ExtraType.Boolean: return "--ez";
                case ExtraType.Int: return "--ei";
                case ExtraType.Long: return "--el";
                case ExtraType.Float: return "--ef";
                case ExtraType.Uri: return "--eu";
                case ExtraType.Component: return "--ecn";
                case ExtraType.IntList: return "--eia";
                case ExtraType.LongList: return "--ela";
                case ExtraType.FloatList: return "--efa";
                default: throw new ArgumentException($"Extra type {type} not recognized.");
            }
        }

    }
}
