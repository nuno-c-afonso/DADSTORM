using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
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
            Operation oper;                               //the operatin that this replica will make   
            Consumer consumingOperator;                   //the thread that will be handling the tuples and sending them tho the next ones

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


            //############ creating an operator of the wanted type ############

            switch (operation[0])
            {
                case "UNIQ":
                    oper = new UniqOperation(replicasUrl, replicaIndex, int.Parse(operation[1]));
                    break;
                case "COUNT":
                    oper = new CountOperation(replicasUrl, replicaIndex);
                    break;
                case "DUP":
                    oper = new DupOperation(replicasUrl, replicaIndex);
                    break;
                case "FILTER":
                    oper = new FilterOperation(replicasUrl, replicaIndex, operation[1], operation[2], operation[3]);
                    break;
                case "CUSTOM":
                    oper = new CustomOperation(replicasUrl, replicaIndex, operation[1], operation[2], operation[3]);
                    break;
                default:
                    System.Console.WriteLine("the type of operation {0} is not known", operation);
                    return;
            }



            //############ Open an input channel ###################
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            ReplicaBuffer inputBuffer = new ReplicaBuffer();
            RemotingServices.Marshal(inputBuffer, "op",typeof(ReplicaInterface));


            //############ Create a consumer of the buffer ###################

            consumingOperator = new Consumer(inputBuffer,oper, outputs, replicaIndex, PuppetMasterUrl, routing, semantics, logLevel);

            //############ Start processing tuples ###################//CHECK
            ThreadStart ts = new ThreadStart(consumingOperator.Operate);
            Thread t = new Thread(ts);
            t.Start();
            t.Join();//FIXMEshould we wait?


            System.Console.WriteLine("press enter to shutdown");
            Console.ReadLine();
        }
    }
}
