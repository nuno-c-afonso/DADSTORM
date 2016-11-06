using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine {
    public class WaitCommand : Command {
        private Shell running;

        public WaitCommand(Shell s) {
            running = s;
        }

        public void execute(string[] args) {
            int converted;

            if (args.Length == 0)
                throw new WrongNumberOfArgsException();

            if (!int.TryParse(args[0], out converted))
                throw new WrongTypeOfArgException();

            running.Waiting = converted;
        }
    }
}
