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
            string[] tuple;
            string[] result;
            //Operator oper;
            Operator oper = new DupOperator();//put the one before and create the one that should b instead

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
