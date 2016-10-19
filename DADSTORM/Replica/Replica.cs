using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public class Replica {

        public static void Main(string[] args) {

            if (args.Length < 3){
                System.Console.WriteLine(" !!! The number of arguments is not correct: at least 3 expected");
            }


            // Parse and save the function arguments
            string url = args[0];
            int port;
            string routing = args[1];
            string operation = args[2];
            List<string> inputs = new List<string>();
            List<string> outputs = new List<string>();
            int i = 0;

            if (args[3].Equals("-i") && !args[4].Equals("-o"))
                for (i = 4; !args[i].Equals("-o") || i < args.Length; i++)
                    inputs.Add(args[i]);
            
            else 
                System.Console.WriteLine(" !!! Input Sources expected");


            if (i <args.Length-1 && args[i].Equals("-o") )
                for(; i < args.Length; i++)
                    outputs.Add(args[i]);
            
            else
                System.Console.WriteLine(" !!! Output destinations expected");

            System.Console.WriteLine("starting repica with URL:{0} \n routing:{1} \n operation {2}"
                    , url, routing, operation);//TODO can add more information
 
            CommonClasses.UrlSpliter urlspli = new CommonClasses.UrlSpliter();
            port = int.Parse(urlspli.getPort(url));
   
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            ReplicaBuffer mo = new ReplicaBuffer();
            RemotingServices.Marshal(mo, "ReplicaBuffer",typeof(ReplicaInterface));
            
            //TODO thread that eats the input and send to the next replica


            System.Console.WriteLine("");

            //createReplica(string URL, string routing,
            //    string op, List < string > inputs, List < string > output)
            //TODO: This is only a test to launch this program from the PCS

            Console.WriteLine("Hello, world!");
            Console.ReadLine();
        }
    }
}
