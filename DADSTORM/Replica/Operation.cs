﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica{
    public abstract class Operation {

        public abstract List<string[]> Operate(string[] tuple);
            

    }
}

