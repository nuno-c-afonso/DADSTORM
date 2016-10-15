using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessCreationService {
    class Program {
        static void Main(string[] args) {
            //TODO: This is only a test to launch this program from the PCS
            CommonClasses.ProcessCreator pc = new CommonClasses.ProcessCreator();
            pc.createReplica(null, null, null, null, null);
        }
    }
}
