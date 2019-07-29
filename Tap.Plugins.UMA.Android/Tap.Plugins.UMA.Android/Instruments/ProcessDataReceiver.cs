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
    internal class ProcessDataReceiver
    {
        private Process process;
        private List<string> output;

        public Process Process
        {
            get
            {
                return process;
            }
        }

        public IReadOnlyCollection<string> Output
        {
            get
            {
                return output.AsReadOnly();
            }
        }

        public ProcessDataReceiver(Process process)
        {
            this.process = process;

            output = new List<string>();

            // Auto-close when process exists
            process.Exited += processExited;

            process.OutputDataReceived += processDataReceived;
            process.ErrorDataReceived += processDataReceived;
        }

        public void Close()
        {
            lock (this)
            {
                if (process != null)
                {
                    process.OutputDataReceived -= processDataReceived;
                    process.ErrorDataReceived -= processDataReceived;
                    process = null;
                }
            }
        }

        private void processDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                output.Add(e.Data);
            }
        }

        private void processExited(object sender, EventArgs e)
        {
            Close();
        }
    }
}
