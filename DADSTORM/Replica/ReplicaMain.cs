using CommonClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            System.Console.WriteLine("Program args:\n" + string.Join(" ", args));

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
            List<string> inputs = new List<string>();       // To store the replica's inputs
            List<Thread> fileReaders = new List<Thread>();
            int port;                                     // To store the port in which the service will be available

            int i;
            Operation oper;                               //the operation that this replica will make   
            ReplicaObject consumingOperator;              //the thread that will be handling the tuples and sending them tho the next ones

            //############ Parse and save the function arguments ###################
            PuppetMasterUrl = args[0];
            routing = args[1];
            semantics = args[2];
            logLevel = args[3];

            i = 4; // index of -o argument
            while (!args[++i].Equals("-r"))
                operation.Add(args[i]);

            // index 0: FILTER
            // index 1: 3,=,"www.tecnico.ulisboa.pt"

            if (operation.Count > 2)
                throw new FormatException();
            else if (operation.Count == 2)
            {
                var fields = operation[1];
                var newList = fields.Split(',').ToList();
                newList.Insert(0, operation[0]);
                operation = newList;

            }

            /*
            // FIXME TODO not sure what this is for
            // has index out of bound exception

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
            */
            
            replicaIndex = int.Parse(args[++i]);
            while (!args[++i].Equals("-o"))
                replicasUrl.Add(args[i]);

            while (!args[++i].Equals("-i"))
                outputs.Add(args[i]);

            /*
             *  TODO: Check the case when there's no more inputs
             */
            int argsSize = args.Length;
            while (++i < argsSize)
                inputs.Add(args[i]);






            CommonClasses.UrlSpliter urlspli = new CommonClasses.UrlSpliter();
            port = int.Parse(urlspli.getPort(replicasUrl[replicaIndex]));

            //FIXME just for debug
            System.Console.WriteLine("VARS.. PuppetMasterUrl: {0}\n\t routing: {1} \n\t semantics: {2}\n\t logLevel: {3}\n\t replicaIndex: {4}\n\t  port: {5}\n\t operation: {6}\n\t replicasUrl: {7}\n\t outputs: {8} \n\t inputs: {9}"
                    , PuppetMasterUrl, routing, semantics, logLevel, replicaIndex, port ,string.Join(",\n\t", operation),string.Join(",\n\t", replicasUrl), string.Join(",\n\t", outputs), string.Join(",\n\t", inputs));

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
                    oper = new DupOperation();
                    break;
                case "FILTER":
                    oper = new FilterOperation(operation[1], operation[2], operation[3]);
                    break;
                case "CUSTOM":
                    oper = new CustomOperation(operation[1], operation[2], operation[3]);
                    break;
                case "TOFILE":
                    oper = new OutputOperation();
                    break;
                default:
                    System.Console.WriteLine("the type of operation {0} is not known", operation);
                    return;
            }
            Console.WriteLine("1-Registering TCP chanel");

            //############ Open an input channel ###################
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            Console.WriteLine("2-Creating Buffer");

            //############ Create a  buffer ###################

            consumingOperator = new ReplicaObject(PuppetMasterUrl, routing, semantics, logLevel, oper, outputs,
                                                    replicasUrl[replicaIndex], operation[0]);

            RemotingServices.Marshal(consumingOperator, "op", typeof(ReplicaInterface));

            Console.WriteLine("4-If needed creating File reader");

            foreach (string input in inputs)
                if (input.EndsWith(".dat") || input.EndsWith(".data")){
                    tupleFileReader fr = new tupleFileReader(consumingOperator, input);
                    ThreadStart tstart = new ThreadStart(fr.feedBuffer);
                    Thread th = new Thread(tstart);
                    th.Start();
                    fileReaders.Add(th);
                }

            Console.WriteLine("5-Start processing tuples");

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
