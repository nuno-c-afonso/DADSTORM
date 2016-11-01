using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica
{
    public class DupOperation : Operation{
        public DupOperation(List<string> replicasURL, int myselfIndex) :
            base(replicasURL, myselfIndex) { }

        public override List<string[]> Operate(string[] tuple){
            List<string[]> l = new List<string[]>();
            l.Add(tuple);
            return l;
        }
    }

}
