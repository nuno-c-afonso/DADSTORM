﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica
{
    public class DupOperator : Operator{
        public override string[] Operate(string[] tuple){
            return tuple;
        }
    }

}