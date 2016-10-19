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
        // TODO: Check if the those IP addresses are the same as the ones given in the configuration file
        static void Main(string[] args) {
            const int PCS_RESERVED_PORT = 10000;
            const int LOGGING_PORT = 10001;

            // TODO: Process the configuration file and then reach the process creation services

            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, false);

            // TODO: Check for the incoming exceptions when the URL is not available
            foreach (string s in args) {
                Console.WriteLine("Calling PCS on address " + s);
                try
                {   
                    CommonClasses.IProcessCreator obj = (CommonClasses.IProcessCreator)Activator.GetObject(typeof(CommonClasses.IProcessCreator),
                    "tcp://" + s + ":" + PCS_RESERVED_PORT + "/ProcessCreator");

                    // TODO: Send the right arguments
                    obj.createReplica("tcp://localhost:10005", "other", "stuff", new List<string>(), new List<string>());
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    Console.WriteLine("Error with host " + s);
                    //Console.WriteLine("Exception " + e);
                }
                
            }

            Console.ReadLine();
        }
    }
}
