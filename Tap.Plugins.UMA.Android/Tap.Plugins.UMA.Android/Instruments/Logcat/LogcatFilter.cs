// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tap.Plugins.UMA.Android.Instruments.Logcat
{
    /// <summary>
    /// A logcat filter definition, with support for multiple tag-priority pairs.
    /// </summary>
    public class LogcatFilter
    {
        private readonly Dictionary<string, LogcatPriority> tags;

        public LogcatPriority? DefaultPriority { get; set; }

        public LogcatFilter()
        {
            tags = new Dictionary<string, LogcatPriority>();
        }

        public void SetTagPriority(string tag, LogcatPriority priority)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentException("Cannot set priority for a null or empty tag name");
            }

            if (string.IsNullOrWhiteSpace(tag) || tag == "*")
            {
                DefaultPriority = priority;
            }
            else
            {
                tags[tag] = priority;
            }
        }

        public void UnsetTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentException("Cannot unset priority for a null or empty tag name");
            }

            if (string.IsNullOrWhiteSpace(tag) || tag == "*")
            {
                DefaultPriority = null;
            }
            else
            {
                tags.Remove(tag);
            }
        }

        public LogcatPriority? GetTagPriority(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentException("Cannot unset priority for a null or empty tag name");
            }

            LogcatPriority priority;
            bool found = tags.TryGetValue(tag, out priority);

            return found ? (LogcatPriority?)priority : null;
        }

        public string ToAdbArguments()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(string.Join(" ", tags.Select(p => toArgument(p.Key, p.Value))));
            if (DefaultPriority != null)
            {
                builder.Append(" ");
                builder.Append(toArgument("*", DefaultPriority.Value));
            }
            return builder.ToString().Trim();
        }

        #region Static factory methods

        public static LogcatFilter CreateSingleTagFilter(string tag, LogcatPriority priority = LogcatPriority.Verbose)
        {
            LogcatFilter filter = new LogcatFilter();
            filter.SetTagPriority(tag, priority);
            filter.DefaultPriority = LogcatPriority.Silent;
            return filter;
        }

        #endregion

        #region Private methods

        private string toArgument(string tag, LogcatPriority priority)
        {
            return $"{tag}:{priority.ToCode()}";
        }

        #endregion
    }
}
