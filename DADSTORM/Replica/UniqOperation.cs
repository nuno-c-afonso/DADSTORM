﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public class UniqOperation : Operation {
        private int fieldNumber;
        public UniqOperation(List<string> replicasURL, int myselfIndex, int fieldNumber)
            : base(replicasURL, myselfIndex) {
            this.fieldNumber = fieldNumber;
        }

        public override List<string[]> Operate(string[] tuple) {
            string element = tuple[fieldNumber];

            if (wasElementSeen(element))
                return null;

            bool wasInOther = false;
            foreach(string s in OtherReplicas) {
                Operation o = getGeneralReplica(s);
                if (wasInOther = o.wasElementSeen(element))
                    break;
            }

            if (wasInOther)
                return null;
           
            List<string[]> l = new List<string[]>();
            l.Add(tuple);
            return l;
        }
    }
}
