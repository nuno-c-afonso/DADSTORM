using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine {
    public class StartCommand : AsyncCommand {
        private OperatorsInfo operatorsInfo;

        public StartCommand(OperatorsInfo opi) {
            operatorsInfo = opi;
        }

        public override void execute(string[] args) {
            if (args.Length == 0)
                throw new WrongNumberOfArgsException();

            OperatorBuilder opb;
            if((opb = operatorsInfo.getOpInfo(args[0])) == null)
                throw new WrongOperatorException();

            foreach(string addr in opb.Addresses) {
                ReplicaInterface obj = (ReplicaInterface) Activator.GetObject(typeof(ReplicaInterface), addr);

                RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(obj.Start);
                IAsyncResult RemAr = RemoteDel.BeginInvoke(null, obj);
            }
        }
    }
}
