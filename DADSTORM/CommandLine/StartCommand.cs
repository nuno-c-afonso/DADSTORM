using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine {
    public class StartCommand : Command {
        private bool wasStarted;
        private OperatorsInfo operatorsInfo;

        public StartCommand(OperatorsInfo opi) {
            wasStarted = false;
            operatorsInfo = opi;
        }

        public void execute(string[] args) {
            if(!wasStarted) {
                if (args.Length == 0)
                    throw new WrongNumberOfArgsException();

                OperatorBuilder opb;
                if((opb = operatorsInfo.getOpInfo(args[0])) == null)
                    throw new WrongOperatorException();

                foreach(string addr in opb.Addresses) {
                    ReplicaInterface obj = (ReplicaInterface) Activator.GetObject(typeof(ReplicaInterface), addr);
                    obj.Start();
                }

                wasStarted = true;
            }
        }
    }
}
