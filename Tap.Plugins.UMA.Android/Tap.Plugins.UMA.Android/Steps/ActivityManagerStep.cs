// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]
//
// This file cannot be modified or redistributed. This header cannot be removed.

using OpenTap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Tap.Plugins.UMA.Android.Instruments;
using Tap.Plugins.UMA.Android.Instruments.ActivityManager;
using Tap.Plugins.UMA.Android.Instruments.ActivityManager.Intents;

namespace Tap.Plugins.UMA.Android.Steps
{
    [Display("Activity Manager",
             Groups: new string[] { "Triangle", "Android" },
             Description: "Sends commands to the device's Activity Manager.")]
    public class ActivityManagerStep : AdbCommandStepBase
    {
        #region Settings

        [Display(Name: "Device ID",
                 Group: "Command",
                 Description: "The ID of the device to which this command will be sent.\n" +
                    "This is not required for commands that are not sent to\n" +
                    "a device (e.g. 'adb devices'), or when a single device\n" +
                    "is connected.",
                 Order: 1.0)]
        public string DeviceId { get; set; }

        [Display(Name: "Command",
                 Group: "Command",
                 Description: "Command type that will be sent to the activity manager. Available commads are:\n" +
                    " - Start: Start an Activity specified by intent.\n" +
                    " - StartService: Start the Service specified by intent.\n" +
                    " - ForceStop: Force stop everything associated with the specified package.\n" +
                    " - Broadcast: Issue a broadcast intent.",
                 Order: 1.1)]
        public AmCommand Command { get; set; }

        #region Intent settings

        [Display(Name: "Component", Group: "Intent", Order: 2.0)]
        [EnabledIf("IntentRequired", true, HideIfDisabled = true)]
        public Enabled<string> Component { get; set; }

        [Display(Name: "Action", Group: "Intent", Order: 2.1)]
        [EnabledIf("IntentRequired", true, HideIfDisabled = true)]
        public Enabled<string> Action { get; set; }

        [Display(Name: "Data URI", Group: "Intent", Order: 2.2)]
        [EnabledIf("IntentRequired", true, HideIfDisabled = true)]
        public Enabled<string> DataUri { get; set; }

        [Display(Name: "MIME Type", Group: "Intent", Order: 2.3)]
        [EnabledIf("IntentRequired", true, HideIfDisabled = true)]
        public Enabled<string> MimeType { get; set; }

        [Display(Name: "Category", Group: "Intent", Order: 2.4)]
        [EnabledIf("IntentRequired", true, HideIfDisabled = true)]
        public Enabled<string> Category { get; set; }

        [Display(Name: "Selector", Group: "Intent", Order: 2.5,
                 Description: "Requires the use of 'Data URI' and 'MIME Type' options to set the intent data and type.")]
        [EnabledIf("IntentRequired", true, HideIfDisabled = true)]
        public bool Selector { get; set; }

        [Display(Name: "Flags", Group: "Intent", Order: 2.6)]
        [EnabledIf("IntentRequired", true, HideIfDisabled = true)]
        public SortedSet<IntentFlags> Flags { get; set; }

        [Display(Name: "Intent Extras", Group: "Intent", Order: 2.7)]
        [EnabledIf("IntentRequired", true, HideIfDisabled = true)]
        public List<Extra> Extras { get; set; }

        [Display(Name: "Clean Extras", Group: "Intent", Order: 2.8,
                 Description: "Removes duplicated or invalid entries from the list of extras.")]
        [EnabledIf("IntentRequired", true, HideIfDisabled = true)]
        [Browsable(true)]
        public void CleanExtras()
        {
            cleanExtras();
        }
        
        #endregion

        #region Package settings

        [Display(Name: "Package", Group: "Package", Order: 2.0)]
        [EnabledIf("IntentRequired", false, HideIfDisabled = true)]
        public string Package { get; set; }

        #endregion

        #region Other options

        [Display(Name: "Debug", Group: "Options", Order: 3.0)]
        [EnabledIf("Command", AmCommand.Start, HideIfDisabled = true)]
        public bool Debug { get; set; }

        [Display(Name: "Wait", Group: "Options", Order: 3.1)]
        [EnabledIf("Command", AmCommand.Start, HideIfDisabled = true)]
        public bool Wait { get; set; }

        [Display(Name: "Force Stop", Group: "Options", Order: 3.2)]
        [EnabledIf("Command", AmCommand.Start, HideIfDisabled = true)]
        public bool ForceStop { get; set; }

        [Display(Name: "User", Group: "Options", Order: 3.3)]
        [EnabledIf("Command", AmCommand.Start, AmCommand.StartService, AmCommand.Broadcast, HideIfDisabled = true)]
        public string User { get; set; }

        #endregion

        #endregion

        #region Helper properties

        public bool IntentRequired { get { return Command != AmCommand.ForceStop; } }

        #endregion

        public ActivityManagerStep()
        {
            Extras = new List<Extra>();
            Flags = new SortedSet<IntentFlags>();

            // Defaults
            Component = new Enabled<string> { IsEnabled = true, Value = "" };
            Action = new Enabled<string> { IsEnabled = true, Value = "" };
            DataUri = new Enabled<string> { IsEnabled = false, Value = "" };
            MimeType= new Enabled<string> { IsEnabled = false, Value = "" };
            Category = new Enabled<string> { IsEnabled = false, Value = "" };

            // Validation rules
            Rules.Add(() => !IntentRequired || !Action.IsEnabled || !string.IsNullOrWhiteSpace(Action.Value), "Please specify the intent Action", "Action");
            Rules.Add(() => !IntentRequired || !DataUri.IsEnabled || !string.IsNullOrWhiteSpace(DataUri.Value), "Please specify the intent Data URI", "DataUri");
            Rules.Add(() => !IntentRequired || !MimeType.IsEnabled || !string.IsNullOrWhiteSpace(MimeType.Value), "Please specify the intent MIME Type", "MimeType");
            Rules.Add(() => !IntentRequired || !Category.IsEnabled || !string.IsNullOrWhiteSpace(Category.Value), "Please specify the intent Category", "Category");
            Rules.Add(() => !IntentRequired || !Component.IsEnabled || !string.IsNullOrWhiteSpace(Component.Value), "Please specify the intent Component", "Component");
            Rules.Add(() => !IntentRequired || Selector == false || 
                             (DataUri.IsEnabled && !string.IsNullOrWhiteSpace(DataUri.Value) &&
                              MimeType.IsEnabled && !string.IsNullOrWhiteSpace(MimeType.Value)), 
                             "Selector requires DataUri and MimeType to be specified.", "Selector");
            Rules.Add(() => IntentRequired || !string.IsNullOrWhiteSpace(Package), "Please specify the package name", "Package");
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();

            cleanExtras(); // Ensure that the list of extras do not containt invalid or duplicated entries
        }

        public override void Run()
        {
            AdbCommandResult result = executeCommand();
            HandleResult(result);
        }

        private AdbCommandResult executeCommand()
        {
            AmCommandBuilder builder;
            Intent intent = null;

            if (IntentRequired)
            {
                intent = new Intent();
                if (Action.IsEnabled) { intent.Action = Action.Value; }
                if (DataUri.IsEnabled) { intent.DataUri = DataUri.Value; }
                if (MimeType.IsEnabled) { intent.MimeType = MimeType.Value; }
                if (Category.IsEnabled) { intent.Category = Category.Value; }
                if (Component.IsEnabled) { intent.Component = Component.Value; }
                intent.Flags = Flags;
                intent.IsSelector = Selector;
                intent.Extras = Extras;
            }

            switch (Command)
            {
                case AmCommand.Start: builder = AmCommandBuilder.Start(intent, Debug, Wait, ForceStop, User); break;
                case AmCommand.StartService: builder = AmCommandBuilder.StartService(intent, User); break;
                case AmCommand.ForceStop: builder = AmCommandBuilder.ForceStop(Package); break;
                case AmCommand.Broadcast: builder = AmCommandBuilder.BroadCast(intent, User); break;
                default: throw new ArgumentException($"ActivityManager command {Command} not supported.");
            }
            
            return Adb.ExecuteAdbCommand(builder.Build(), DeviceId, TimeoutMillis, Retries, RetryWaitMillis);
        }

        private void cleanExtras()
        {
            List<Extra> cleaned = Extras
                .FindAll(e => e.IsValid)
                .Distinct(new ExtraComparer()).ToList();

            int removed = Extras.Count - cleaned.Count;
            if (removed != 0) { Log.Info($"Removing {removed} duplicated or invalid extras."); }

            Extras = cleaned;
        }
    }
}
