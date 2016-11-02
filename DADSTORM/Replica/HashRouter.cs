using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public class HashRouter : Router {
        private int hashArg;

        public HashRouter(List<string> output, string semantics, int hashArg) : base(output, semantics) {
            this.hashArg = hashArg;
        }

        public override string calculateNext(string[] tuple) {
            return NextOperator[tuple[hashArg].GetHashCode() % NextOperator.Count];
        }
    }
}
