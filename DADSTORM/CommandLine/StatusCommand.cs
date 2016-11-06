using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine {
    public class StatusCommand : Command {
        private OperatorsInfo operatorsInfo;

        public StatusCommand(OperatorsInfo opi) {
            operatorsInfo = opi;
        }

        public void execute(string[] args) {
            foreach(string name in operatorsInfo.OperatorNames) {
                OperatorBuilder opb = operatorsInfo.getOpInfo(name);
                foreach (string addr in opb.Addresses) {
                    ReplicaInterface obj = (ReplicaInterface)Activator.GetObject(typeof(ReplicaInterface), addr);
                    obj.Status();
                }
            }
        }
    }
}
