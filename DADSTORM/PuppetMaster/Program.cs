using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster {
    class Program {
        // The arguments are the IP addresses of the Process Creation Services, ordered by the respective operators
        static void Main(string[] args) {
            const int PCS_RESERVED_PORT = 10000;

            // TODO: Process the configuration file and then reach the process creation services

            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, false);

            // TODO: Check for the incoming exceptions when the URL is not available
            foreach (string s in args) {
                CommonClasses.IProcessCreator obj = (CommonClasses.IProcessCreator)Activator.GetObject(typeof(CommonClasses.IProcessCreator),
                "tcp://" + s + ":" + PCS_RESERVED_PORT + "/ProcessCreator");

                obj.createReplica(null, null, null, null, null);
            }

            Console.ReadLine();
        }
    }
}
