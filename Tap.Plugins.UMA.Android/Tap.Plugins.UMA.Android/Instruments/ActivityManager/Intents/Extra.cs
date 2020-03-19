// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]

using System.Collections.Generic;

namespace Tap.Plugins.UMA.Android.Instruments.ActivityManager.Intents
{
    public class Extra
    {
        public ExtraType ExtraType { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public bool IsValid
        {
            get { return !string.IsNullOrWhiteSpace(Key) && (ExtraType == ExtraType.Null || !string.IsNullOrWhiteSpace(Value)); }
        }

        public Extra() { }

        public Extra(ExtraType type, string key, string value)
        {
            ExtraType = type;
            Key = key;
            Value = value;
        }

        public string ToArgument()
        {
            if (ExtraType == ExtraType.Null)
            {
                return $"{ExtraType.Argument()} {Key}";
            }
            else
            {
                return $"{ExtraType.Argument()} {Key} {Value}";
            }
        }
    }

    public class ExtraComparer : IEqualityComparer<Extra>
    {
        public bool Equals(Extra e1, Extra e2)
        {
            return (e1.ExtraType == e2.ExtraType) && equals(e1.Key, e2.Key) && equals(e1.Value, e2.Value);
        }

        public int GetHashCode(Extra e)
        {
            return $"{e.ExtraType}{e.Key}{e.Value}".GetHashCode();
        }

        private bool equals(string s1, string s2)
        {
            if (s1 == null) {
                return (s2 == null);
            }
            else
            {
                return s1.Equals(s2);
            }
        }
    }
}
