using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine {
    public class CrashCommand : AsyncCommand {
        private OperatorsInfo operatorsInfo;

        public CrashCommand(OperatorsInfo opi) {
            operatorsInfo = opi;
        }

        public override void execute(string[] args) {
            if (args.Length < 2)
                throw new WrongNumberOfArgsException();

            OperatorBuilder opb;
            int repIndex;

            if ((opb = operatorsInfo.getOpInfo(args[0])) == null)
                throw new WrongOperatorException();

            if (!int.TryParse(args[1], out repIndex))
                throw new WrongTypeOfArgException();

            if (!(repIndex < opb.Addresses.Count && repIndex >= 0))
                throw new IndexOutOfBoundsException();

            ReplicaInterface obj = (ReplicaInterface)Activator.GetObject(typeof(ReplicaInterface), opb.Addresses[repIndex]);
            RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(obj.Crash);
            IAsyncResult RemAr = RemoteDel.BeginInvoke(null, obj);
        }
    }
}
