﻿using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine {
    public class FreezeCommand : Command {
        private OperatorsInfo operatorsInfo;

        public FreezeCommand(OperatorsInfo opi) {
            operatorsInfo = opi;
        }

        public void execute(string[] args) {
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
            obj.Freeze();
        }
    }
}
