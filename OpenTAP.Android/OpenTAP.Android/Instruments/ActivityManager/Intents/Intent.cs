// Author:      Bruno Garcia Garcia <bgg@uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]

using System.Collections.Generic;

namespace Tap.Plugins.UMA.Android.Instruments.ActivityManager.Intents
{
    public class Intent
    {
        public string Action { get; set; }
        public string DataUri { get; set; }
        public string MimeType { get; set; }
        public string Category { get; set; }
        public string Component { get; set; }
        public SortedSet<IntentFlags> Flags { get; set; }
        public List<Extra> Extras { get; set; }
        public bool IsSelector { get; set; }

        public Intent()
        {
            Extras = new List<Extra>();
            Flags = new SortedSet<IntentFlags>();
        }

        public string ToArgument()
        {
            List<string> arguments = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(Action)) { arguments.Add($"-a {Action}"); }
            if (!string.IsNullOrWhiteSpace(DataUri)) { arguments.Add($"-d {DataUri}"); }
            if (!string.IsNullOrWhiteSpace(MimeType)) { arguments.Add($"-t {MimeType}"); }
            if (!string.IsNullOrWhiteSpace(Category)) { arguments.Add($"-c {Category}"); }
            if (!string.IsNullOrWhiteSpace(Component)) { arguments.Add($"-n {Component}"); }

            foreach (IntentFlags flag in Flags) { arguments.Add(flag.Argument()); }
            foreach (Extra extra in Extras) { arguments.Add(extra.ToArgument()); }

            if (IsSelector) { arguments.Add("--selector"); }

            return string.Join(" ", arguments);
        }
    }
}
