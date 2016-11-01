using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public class CountOperation : Operation {
        public CountOperation(List<string> replicasURL, int myselfIndex)
            : base(replicasURL, myselfIndex) { }

        public override List<string[]> Operate(string[] tuple) {
            //TODO implement
            throw new NotImplementedException();
        }
    }
}
