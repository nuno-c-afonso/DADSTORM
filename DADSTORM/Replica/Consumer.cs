using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;



namespace Replica{

    public class Consumer{

        TcpChannel channel;
        string[] tuple;
        List<string[]> result;
        ReplicaBuffer inputBuffer;
        Operation operation;
        List<ReplicaInterface> destinationOperators;
        int numberDestinations;
        string puppetMasterUrl;                       
        string routing;                               
        string semantics;                             
        string logLevel;
        Random random;

        public Consumer(ReplicaBuffer inputBuffer, Operation opereration, List<string> outputsURL, int myselfIndex,
            string puppetMasterUrl, string routing, string semantics, string logLevel){

            //TODO missing inputs adding wen needed
            this.inputBuffer = inputBuffer;
            this.operation = opereration;
            destinationOperators = new List<ReplicaInterface>();
            numberDestinations = 0;

            //channel = new TcpChannel();
            //ChannelServices.RegisterChannel(channel, false);

            int numReplicas = outputsURL.Count;
            for (int i = 0; i < numReplicas; i++)
                if (i != myselfIndex)
                    addTupleDestination(outputsURL[i]);

            this.puppetMasterUrl = puppetMasterUrl;
            this.routing = routing;
            this.semantics = semantics;
            this.logLevel = logLevel;
            random = new Random();

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

                result = operation.Operate(tuple);
                if (result != null)
                {
                    foreach(string[] outTuple in result)
                        fowardTuple(outTuple);

                }
            }
        }


        public void fowardTuple(string[] tuple){
            //chose Replica to foward
            int destI = selectRoutingDestination(tuple);
            ReplicaInterface destination = destinationOperators[destI];

            //see the type of processing semantics send to the replixa
            if (semantics.Equals("at-most-once"))
                sendAtMostOnce(destination, tuple);
            if (semantics.Equals("at-least-once"))
                sendAtMostOnce(destination, tuple);
            if (semantics.Equals("exactly-once"))
                sendAtMostOnce(destination, tuple);
        }

        public void showStatus(){
            //TODO
        }

        public void addTupleDestination(String url){
            try{
                ReplicaInterface destination = (ReplicaInterface)Activator.GetObject(typeof(ReplicaInterface), url);
                destinationOperators.Add(destination);
                ++numberDestinations;
            }
            catch (System.Net.Sockets.SocketException e){
                Console.WriteLine("Error with host " + url);
                Console.WriteLine(e);
            }
        }

        public int selectRoutingDestination(String[] tuple) {
            if (routing.Equals("primary"))
                return 0;//the primary is always the first one to apear
            else if (routing.Equals("random")) {
                return random.Next(0, numberDestinations - 1);
            }
            else /*(routing.StartsWith("hashing("))*/ {
                int fieldnumber = int.Parse(routing[9].ToString());
                return tuple[fieldnumber].GetHashCode() % numberDestinations;//check it should be numberDestinations -
            }
            /*else
                throw new ArgumentException("the chosen routing policie doesnt exist");*/ //FIXME input validation issues
        }


        public void sendAtMostOnce(ReplicaInterface destination,string[] tuple){
            destination.addTuple(tuple);//todo make this assync
            /*RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(destination.addTuple());
            IAsyncResult RemAr = RemoteDel.BeginInvoke(null, null);*/
        }

        public void sendAtLeastOnce(ReplicaInterface destination, string[] tuple)
        {
            //TODO
        }

        public void sendExactlyOnce(ReplicaInterface destination, string[] tuple)
        {
            //TODO
        }


    }
}
