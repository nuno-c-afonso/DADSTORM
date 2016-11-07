using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine {
    public abstract class AsyncCommand : Command {
        public delegate void RemoteAsyncDelegate();
        public delegate void RemoteAsyncDelegateWithTime(int t);

        public abstract void execute(string[] args);
    }
}
