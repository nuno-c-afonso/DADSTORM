using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses
{
    public class ReplicaBuffer : MarshalByRefObject, ReplicaInterface
    {
        //TODO make methods
        public void addTuple(Object tuple) { }
        public void Start() { }
        public void Interval(int time) { }
        public string Status() { return "FIXME"; }
        public void Crash() { }
        public void Freeze() { }
        public void Unfreeze() { }



        public Object getTuple() {
            Object tuple = Tuple.Create("cacaaca","blabla");
            return tuple;
        }
    }
}
