// Author:      Bruno Garcia Garcia <bgg@uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]

using System.Collections.Generic;
using Tap.Plugins.UMA.Android.Instruments.ActivityManager.Intents;

namespace Tap.Plugins.UMA.Android.Instruments.ActivityManager
{
    public class AmCommandBuilder
    {
        public AmCommand Command { get; set; }

        public bool Debug { get; set; }
        public bool Wait { get; set; }
        public bool StopApp { get; set; }
        public string User { get; set; }

        public Intent Intent { get; set; }
        public string Package { get; set; }

        #region Constructors

        public AmCommandBuilder() { }

        public AmCommandBuilder(AmCommand command)
        {
            Command = command;
        }

        #endregion

        public string Build()
        {
            List<string> arguments = new List<string>() { "shell", "am", Command.Argument() };

            if (Command == AmCommand.Start)
            {
                if (Debug) { arguments.Add("-D"); }
                if (Wait) { arguments.Add("-W"); }
                if (StopApp) { arguments.Add("-S"); }
            }

            if (Command == AmCommand.Start || Command == AmCommand.StartService || Command == AmCommand.Broadcast)
            {
                if (!string.IsNullOrWhiteSpace(User)) { arguments.Add($"--user {User}"); }
            }

            if (Command == AmCommand.ForceStop)
            {
                arguments.Add(Package);
            }
            else
            {
                if (Intent != null) { arguments.Add(Intent.ToArgument()); }
            }

            return string.Join(" ", arguments);
        }

        #region Factory helper methods

        public static AmCommandBuilder Start(Intent intent, bool debug = false, bool wait = false, bool forceStop = false, string user = null)
        {
            return new AmCommandBuilder(AmCommand.Start)
            {
                Intent = intent,
                Debug = debug,
                Wait = wait,
                StopApp = forceStop,
                User = user
            };
        }

        public static AmCommandBuilder StartService(Intent intent, string user = null)
        {
            return new AmCommandBuilder(AmCommand.StartService)
            {
                Intent = intent,
                User = user
            };
        }

        public static AmCommandBuilder ForceStop(string package)
        {
            return new AmCommandBuilder(AmCommand.ForceStop) { Package = package };
        }

        public static AmCommandBuilder BroadCast(Intent intent, string user = null)
        {
            return new AmCommandBuilder(AmCommand.Broadcast)
            {
                Intent = intent,
                User = user
            };
        }

        #endregion
    }
}
