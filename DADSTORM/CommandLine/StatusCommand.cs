using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommandLine {
    public class StatusCommand : AsyncCommand {
        private OperatorsInfo operatorsInfo;

        public StatusCommand(OperatorsInfo opi) {
            operatorsInfo = opi;
        }

        public override void execute(string[] args) {
            foreach(string name in operatorsInfo.OperatorNames) {
                OperatorBuilder opb = operatorsInfo.getOpInfo(name);
                foreach (string addr in opb.Addresses) {

                    ReplicaInterface obj = (ReplicaInterface)Activator.GetObject(typeof(ReplicaInterface), addr);
                    RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(obj.Status);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(null, obj);

                    //obj.Status(); // TODO use thread for this call ?
                    //new Thread(() => obj.Status()).Start();
                }
            }
        }
    }
}
