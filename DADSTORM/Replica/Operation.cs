using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica{
    public abstract class Operation {
        private HashSet<string> seenStrings = new HashSet<string>();

        public abstract List<string[]> Operate(string[] tuple);
            
        protected void processedTuple(string[] tuple) {
            foreach (string s in tuple)
                seenStrings.Add(s);
        }

        protected bool wasElementSeen(string s) {
            return seenStrings.Contains(s);
        }
    }
}

