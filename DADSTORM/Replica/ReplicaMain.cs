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

            //TODO remove
            for (int xcd = 0; xcd < args.Length; xcd++)
                System.Console.WriteLine(args[xcd]);

            /*  This information will be recovered from the Main arguments
             *  Structure:
             *  <PuppetMasterUrl> <routing> <semantics> <logLevel> -op <operation>
             *  -r <replicaIndex> <replica1> <replica2> ... <replica-n>
             *  -o <output1> <output2> ... <output-n>
             */
            string PuppetMasterUrl;                       // To store the Puppet Master's URL
            string routing;                               // To store the routing type
            string semantics;                             // To store the tuple processing semantics
            string logLevel;                              // To store the desired logging level
            int replicaIndex;                             // To store the position where the current replica's URL is
            List<string> operation = new List<string>();  // To store the desired operation for the replica
            List<string> replicasUrl = new List<string>();// To store the URLs for all replicas
            List<string> outputs = new List<string>();    // To store the replica's outputsB
            int port;                                     // To store the port in which the service will be available

            int i;
            string[] tuple;
            List<string[]> result;
            Operation oper;

            //############ Parse and save the function arguments ###################
            PuppetMasterUrl = args[0];
            routing = args[1];
            semantics = args[2];
            logLevel = args[3];

            // FIXME: The case with " inside the strings is not considered
            string incomplete = "";
            for (i = 5; !args[i].Equals("-r"); i++) {
                if (args[i][0] == '"' && args[i][args[i].Length - 1] == '"')
                    operation.Add(args[i]);
                else {
                    if (args[i][0] == '"')
                        incomplete = args[i];
                    else {
                        incomplete += " " + args[i];
                        if (args[i][args[i].Length - 1] == '"')
                            operation.Add(incomplete);
                    }
                }
            }

            replicaIndex = int.Parse(args[++i]);
            while (!args[++i].Equals("-o"))
                replicasUrl.Add(args[i]);

            /*
             *  TODO: Check the case when there's no more outputs
             */
            int argsSize = args.Length;
            while (++i < argsSize)
                outputs.Add(args[i]);

            CommonClasses.UrlSpliter urlspli = new CommonClasses.UrlSpliter();
            port = int.Parse(urlspli.getPort(replicasUrl[replicaIndex]));

            /* FIXME: This was the previous version
            if (args.Length < 3){
                System.Console.WriteLine("The number of arguments is not correct: at least 3 expected  !!! ");
                return;
            }

            operation = args[2];//last of the last version



            if (args[3].Equals("-i") && !args[4].Equals("-o"))
                for (i = 4; !args[i].Equals("-o") || i < args.Length; i++)
                    inputs.Add(args[i]);
            
            else 
                System.Console.WriteLine("Expected Input Sources !!!");


            if (i <args.Length-1 && args[i].Equals("-o") )
                for(; i < args.Length; i++)
                    outputs.Add(args[i]);
            
            else
                System.Console.WriteLine("Expected output destinations  !!! ");

            System.Console.WriteLine("starting repica with URL:{0} \n routing:{1} \n operation {2}"
                    , url, routing, operation);//TODO can add more information
 
            CommonClasses.UrlSpliter urlspli = new CommonClasses.UrlSpliter();
            port = int.Parse(urlspli.getPort(url));
            */

            //############ creating an operator of the wanted type ############

            switch (operation[0])
            {
                case "UNIQ":
                    oper = new UniqOperation();
                    break;
                case "COUNT":
                    oper = new CountOperation();
                    break;
                case "DUP":
                    oper = new DupOperation();
                    break;
                case "FILTER":
                    oper = new FilterOperation(operation[1], operation[2], operation[3]);
                    break;
                default:
                    System.Console.WriteLine("the type of operation {0} is not known", operation);
                    return;
            }



            //############ Open an input channel ###################
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            ReplicaBuffer input = new ReplicaBuffer();
            RemotingServices.Marshal(input, "ReplicaBuffer",typeof(ReplicaInterface));

            
            //############ Create a consumer of the buffer ###################//CHECK
            
            //Should do like this or create an object to do it ??????????????????
            
            //############ Start processing tuples ###################//CHECK
            while (input.Crashed == false) {
                //see if it is feezed
                input.checkFreeze();

                //get tuple from the buffer
                tuple = input.getTuple();

                //see its own type of routin,
                // if is primary and is the primary one select who will handle it
                //else continue


                result = oper.Operate(tuple);
                if (result != null)
                    result = null;//do nothing

                //see the type of routing used by the desyination
                
                //chose Replica to foward
                //TODO

                //send to that replixa
                //TODO
                
                //wait the defined time between processing
 


            }





            System.Console.WriteLine("press enter to shutdown");
            Console.ReadLine();
        }
    }
}
