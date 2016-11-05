using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public class PrimaryRouter : Router {
        public PrimaryRouter(List<string> output, string semantics) : base(output, semantics) { }

        public override string calculateNext(string[] tuple) {
            return NextOperator[0];
        }
    }
}
