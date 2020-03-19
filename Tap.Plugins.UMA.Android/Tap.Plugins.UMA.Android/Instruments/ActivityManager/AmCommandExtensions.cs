// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]

using System;

namespace Tap.Plugins.UMA.Android.Instruments.ActivityManager
{
    public static class AmCommandExtensions
    {
        public static string Argument(this AmCommand command)
        {
            switch (command)
            {
                case AmCommand.Start: return "start";
                case AmCommand.StartService: return "startservice";
                case AmCommand.ForceStop: return "force-stop";
                case AmCommand.Broadcast: return "broadcast";
                default: throw new ArgumentException($"ActivityManager command {command} not supported.");
            }
        }
    }
}
