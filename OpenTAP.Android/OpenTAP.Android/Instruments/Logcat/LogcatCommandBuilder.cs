// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]

using System.Collections.Generic;
using System.Linq;

namespace Tap.Plugins.UMA.Android.Instruments.Logcat
{
    /// <summary>
    /// Builder class for the arguments of a logcat command.
    /// </summary>
    /// <remarks>
    /// This class supports a subset of the <see cref="https://developer.android.com/studio/command-line/logcat.html">command line options of logcat</see>.
    /// It doesn't check the correctness of the combinations of options.
    /// </remarks>
    public class LogcatCommandBuilder
    {
        private readonly SortedSet<string> flagOptions;

        #region Command line option properties

        public LogcatBuffer Buffer { get; set; }

        public bool Clear
        {
            get { return getFlagOption("-c"); }
            set { setFlagOption("-c", value); }
        }

        public bool DumpAndExit
        {
            get { return getFlagOption("-d"); }
            set { setFlagOption("-d", value); }
        }

        public string Filename { get; set; }

        public LogcatFilter Filter { get; set; }

        public LogcatFormat? Format { get; set; }

        public int? RotateFileSize { get; set; }

        public int? RotateFileCount { get; set; }

        #endregion

        /// <summary>
        /// Creates a new builder for creating logcat commands.
        /// </summary>
        /// <remarks>
        /// The only logcat command line option set by default (besides the actual logcat defaults)
        /// is <see cref="DumpAndExit"/> (-d), so that the logcat commands returns as soon as it
        /// dumps the current contents of the logcat. It is not recommended to disable this option.
        /// </remarks>
        public LogcatCommandBuilder()
        {
            flagOptions = new SortedSet<string>();

            DumpAndExit = true;
        }

        /// <summary>
        /// Returns the arguments for adb to perform the configured logcat command, in a single string.
        /// </summary>
        /// <returns>The arguments for adb</returns>
        public string Build()
        {
            List<string> arguments = new List<string>();

            arguments.Add("logcat");

            addFlagOptions(arguments);
            addBuffer(arguments);
            addFilename(arguments);
            addFormat(arguments);
            addRotateFileSize(arguments);
            addRotateFileCount(arguments);
            addFilter(arguments);

            return string.Join(" ", arguments);
        }

        #region Static factory methods

        /// <summary>
        /// Creates a new logcat command to clear the logcat contents.
        /// </summary>
        /// <returns></returns>
        public static LogcatCommandBuilder CreateClear()
        {
            return new LogcatCommandBuilder() { Clear = true };
        }

        /// <summary>
        /// Creates a new logcat command that suppresses everything but a single tag.
        /// </summary>
        /// <param name="tag">The tag to filter</param>
        /// <param name="priority">The minimum priority of the tag filter</param>
        /// <returns></returns>
        public static LogcatCommandBuilder CreateFilterSingleTag(string tag, LogcatPriority priority = LogcatPriority.Verbose)
        {
            return new LogcatCommandBuilder() { Filter = LogcatFilter.CreateSingleTagFilter(tag, priority) };
        }

        #endregion

        #region Private methods

        private bool getFlagOption(string flag)
        {
            return flagOptions.Contains(flag);
        }

        private void setFlagOption(string flag, bool set)
        {
            if (set)
            {
                flagOptions.Add(flag);
            }
            else
            {
                flagOptions.Remove(flag);
            }
        }

        private void addFlagOptions(List<string> arguments)
        {
            arguments.AddRange(flagOptions);
        }

        private void addBuffer(List<string> arguments)
        {
            if (Buffer != 0)
            {
                arguments.AddRange(Buffer.ToBufferNames().Select(b => $"-b {b}"));
            }
        }

        private void addFilename(List<string> arguments)
        {
            if (!string.IsNullOrWhiteSpace(Filename) && Filename != "stdout")
            {
                arguments.Add($"-f {Filename}");
            }
        }

        private void addFilter(List<string> arguments)
        {
            if (Filter != null)
            {
                arguments.Add(Filter.ToAdbArguments());
            }
        }

        private void addFormat(List<string> arguments)
        {
            if (Format.HasValue)
            {
                arguments.Add($"-v {Format.Value.ToFormatName()}");
            }
        }

        private void addRotateFileSize(List<string> arguments)
        {
            if (RotateFileSize.HasValue)
            {
                arguments.Add($"-r {RotateFileSize}");
            }
        }

        private void addRotateFileCount(List<string> arguments)
        {
            if (RotateFileCount.HasValue)
            {
                arguments.Add($"-n {RotateFileCount}");
            }
        }

        #endregion
    }
}
