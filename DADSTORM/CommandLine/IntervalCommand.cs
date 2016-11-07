using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine {
    public class IntervalCommand : AsyncCommand {
        private OperatorsInfo operatorsInfo;

        public IntervalCommand(OperatorsInfo opi) {
            operatorsInfo = opi;
        }

        public override void execute(string[] args) {
            if (args.Length < 2)
                throw new WrongNumberOfArgsException();

            OperatorBuilder opb;
            int interval;

            if ((opb = operatorsInfo.getOpInfo(args[0])) == null)
                throw new WrongOperatorException();

            if (!int.TryParse(args[1], out interval))
                throw new WrongTypeOfArgException();

            foreach (string addr in opb.Addresses) {
                ReplicaInterface obj = (ReplicaInterface)Activator.GetObject(typeof(ReplicaInterface), addr);
                RemoteAsyncDelegateWithTime RemoteDel = new RemoteAsyncDelegateWithTime(obj.Interval);
                IAsyncResult RemAr = RemoteDel.BeginInvoke(interval, null, obj);
            }
        }
    }
}
