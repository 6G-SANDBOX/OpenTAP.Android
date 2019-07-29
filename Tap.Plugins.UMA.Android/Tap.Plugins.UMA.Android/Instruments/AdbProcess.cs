// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712. [2016 - 2018]
//
// This file cannot be modified or redistributed. This header cannot be removed.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tap.Plugins.UMA.Android.Instruments
{
    public class AdbProcess
    {
        public string Executable { get; private set; }
        public string Arguments { get; private set; }
        internal Process Process { get; private set; }
        internal ProcessDataReceiver Receiver { get; private set; }

        public bool HasFinished
        {
            get { return Process == null || Process.HasExited; }
        }

        public AdbCommandResult Result { get; private set; }

        internal AdbProcess(string executable, string arguments, Process process, ProcessDataReceiver receiver)
        {
            Executable = executable;
            Arguments = arguments;
            Process = process;
            Receiver = receiver;
        }

        public AdbCommandResult Terminate()
        {
            killProcess();
            Result = getResult();
            freeProcess();
            return Result;
        }

        public override string ToString()
        {
            return $"{Executable} {Arguments}".Trim();
        }

        private void killProcess()
        {
            try
            {
                Process.Kill();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private void freeProcess()
        {
            Process.Close();
            Process = null;
        }

        private AdbCommandResult getResult()
        {
            return new AdbCommandResult()
            {
                Success = Process.HasExited && Process.ExitCode != 0, // Assume that killing the background process is what we wanted
                Output = new List<string>(Receiver.Output)
            };
        }
    }
}
