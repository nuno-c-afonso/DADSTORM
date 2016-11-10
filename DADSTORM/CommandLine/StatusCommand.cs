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
            if (args.Length > 0) {
                OperatorBuilder opb;

                if ((opb = operatorsInfo.getOpInfo(args[0])) == null)
                    throw new WrongOperatorException();

                if (args.Length > 1) {
                    int repIndex;

                    if (!int.TryParse(args[1], out repIndex))
                        throw new WrongTypeOfArgException();

                    if (!(repIndex < opb.Addresses.Count && repIndex >= 0))
                        throw new IndexOutOfBoundsException();

                    string address = opb.Addresses[repIndex];
                    callSpecificReplica(address);
                }
                else
                    callAllReplicas(opb);
            }
            else
                callAllOperators();
        }

        private void callSpecificReplica(string address) {
            ReplicaInterface obj = (ReplicaInterface)Activator.GetObject(typeof(ReplicaInterface), address);
            RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(obj.Status);
            IAsyncResult RemAr = RemoteDel.BeginInvoke(null, obj);
        }

        private void callAllReplicas(OperatorBuilder opb) {
            List<Task> taskList = new List<Task>();
            for (int i = 0; i < opb.Addresses.Count; i++) {
                string address = opb.Addresses[i];
                Task lastTask = new Task(() => callSpecificReplica(address));
                lastTask.Start();
                taskList.Add(lastTask);
            }
            Task.WaitAll(taskList.ToArray());
        }

        private void callAllOperators() {
            List<Task> taskList = new List<Task>();
            foreach (string name in operatorsInfo.OperatorNames) {
                OperatorBuilder opb = operatorsInfo.getOpInfo(name);
                Task lastTask = new Task(() => callAllReplicas(opb));
                lastTask.Start();
                taskList.Add(lastTask);
            }
            Task.WaitAll(taskList.ToArray());
        }
    }
}
