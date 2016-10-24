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
    public class ReplicaMain {

        public static void Main(string[] args) {

            string url;
            int port;
            string routing;
            string operation;
            List<string> inputs = new List<string>();
            List<string> outputs = new List<string>();
            int i = 0;
            Object tuple;
            Operator oper;


            //############ Parse and save the function arguments ###################
            if (args.Length < 3){
                System.Console.WriteLine(" !!! The number of arguments is not correct: at least 3 expected");
                return;
            }
            
            url = args[0];
            routing = args[1];
            operation = args[2];


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



            //############ Open an input chanel ###################
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            ReplicaBuffer mo = new ReplicaBuffer();
            RemotingServices.Marshal(mo, "ReplicaBuffer",typeof(ReplicaInterface));


            /*
            while (1 == 1) {
                tuple = mo.getTuple();
                oper.


    */


            }


            //############ Start processing tuples ###################
            //TODO thread that eats the input and send to the next replica


            System.Console.WriteLine("press enter to shutdown");
            Console.ReadLine();
        }
    }
}
