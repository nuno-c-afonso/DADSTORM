using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace Replica{

    public class Consumer{

        TcpChannel channel;
        string[] tuple;
        List<string[]> result;
        ReplicaBuffer inputBuffer;
        Operation opereration;
        List<ReplicaInterface> nextOperators;
        int numberDestinations;
        string puppetMasterUrl;                       
        string routing;                               
        string semantics;                             
        string logLevel;

        public Consumer(ReplicaBuffer inputBuffer, Operation opereration, List<string> outputsURL, int myselfIndex,
            string puppetMasterUrl, string routing, string semantics, string logLevel){

            //TODO missing inputs adding wen needed
            this.inputBuffer = inputBuffer;
            this.opereration = opereration;
            nextOperators = new List<ReplicaInterface>();
            numberDestinations = 0;

            channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, false);

            int numReplicas = outputsURL.Count;
            for (int i = 0; i < numReplicas; i++)
                if (i != myselfIndex)
                    addTupleDestination(outputsURL[i]);

            this.puppetMasterUrl = puppetMasterUrl;
            this.routing = routing;
            this.semantics = semantics;
            this.logLevel = logLevel;

        }


        public void Operate()
        {
            while (inputBuffer.Crashed == false)
            {
                //see if it is feezed
                inputBuffer.checkFreeze();

                //see if needs to show his status
                if (inputBuffer.StatusRequested)
                    showStatus();

                //wait the defined time between processing
                Thread.Sleep(inputBuffer.WaitingTime*1000);

                //get tuple from the buffer
                tuple = inputBuffer.getTuple();

                result = opereration.Operate(tuple);
                if (result != null)
                {
                    foreach(string[] outTuple in result)
                        fowardTuple(outTuple);

                }
            }
        }


        public void fowardTuple(string[] tuple){
            //see the type os semantics used

            //see the type of routing used by the desyination

            //chose Replica to foward
            //TODO

            //send to that replixa
            //TODO
        }

        public void showStatus(){
            //TODO
        }

        public void addTupleDestination(String url){
            try{
                ReplicaInterface destination = (ReplicaInterface)Activator.GetObject(typeof(ReplicaInterface), url);
                nextOperators.Add(destination);
                ++numberDestinations;
            }
            catch (System.Net.Sockets.SocketException e){
                Console.WriteLine("Error with host " + url);
                Console.WriteLine(e);
            }
        }


    }
}
