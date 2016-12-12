using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public class HashRouter : Router {
        private int hashArg;

        public HashRouter(List<string> output, string semantics, int hashArg) : base(output, semantics) {
            this.hashArg = hashArg - 1; // hashing(1) needs to check position 0
        }

        public override string calculateNext(string[] tuple) {

            /* // FOR DEBUG of HASH ROUTER
            Console.WriteLine("on calculating nex3t ");
            Console.WriteLine("tuple ");
            foreach (var a in tuple)
                Console.WriteLine(" " + a);

            Console.WriteLine( hashArg);

            Console.WriteLine("NextOperator ");
            foreach (var a in NextOperator)
                Console.WriteLine(" " + a);
            Console.WriteLine("NextOperator.Count " + NextOperator.Count);
            Console.WriteLine(tuple[hashArg]);
            */


            int hashcode = tuple[hashArg].GetHashCode();
            Console.WriteLine("hashcode "+hashcode);
            hashcode = Math.Abs(hashcode);
            Console.WriteLine("Math.Abs(hashcode) " + hashcode);

            Console.WriteLine(hashcode % NextOperator.Count);
            return NextOperator[hashcode % NextOperator.Count];
        }
    }
}
