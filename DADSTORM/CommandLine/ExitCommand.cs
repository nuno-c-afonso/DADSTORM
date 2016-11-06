using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine {
    public class ExitCommand : Command {
        private Shell running;

        public ExitCommand(Shell s) {
            running = s;
        }

        public void execute(string[] args) {
            running.Leave = true;
        }
    }
}
