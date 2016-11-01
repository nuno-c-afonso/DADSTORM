using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica{
    public interface Operation {
        List<string[]> Operate(string[] tuple);
    }
}

