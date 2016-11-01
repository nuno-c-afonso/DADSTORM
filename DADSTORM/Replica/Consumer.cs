using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Replica{

    public class Consumer{

        string[] tuple;
        List<string[]> result;
        ReplicaBuffer inputBuffer;
        Operation opereration;

        public Consumer(ReplicaBuffer inputBuffer, Operation opereration)
        {

            //TODO missing inputs adding wen needed
            this.inputBuffer = inputBuffer;
            this.opereration = opereration;

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
            //see the type of routing used by the desyination

            //chose Replica to foward
            //TODO

            //send to that replixa
            //TODO
        }

        public void showStatus(){
            //TODO
        }
    }
}
