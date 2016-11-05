using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public class RandomRouter : Router {
        public RandomRouter(List<string> output, string semantics) : base(output, semantics) { }

        public override string calculateNext(string[] tuple) {
            Random r = new Random();

            // This type of random does not include the upper bound
            return NextOperator[r.Next(0, NextOperator.Count)];
        }
    }
}
