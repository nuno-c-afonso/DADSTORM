﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace ProcessCreationService {
    class ProcessCreationService {
        static void Main(string[] args) {
            const int PCS_RESERVED_PORT = 10000;

            TcpChannel channel = new TcpChannel(PCS_RESERVED_PORT);
            ChannelServices.RegisterChannel(channel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(CommonClasses.ProcessCreator),
                "ProcessCreator", WellKnownObjectMode.Singleton);

            System.Console.WriteLine("Waiting on port "+ PCS_RESERVED_PORT);
            System.Console.WriteLine("Press <enter> to exit.");
            System.Console.ReadLine();
        }
    }
}
