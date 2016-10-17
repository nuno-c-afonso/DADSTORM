using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine
{
    public class Shell {
        private Dictionary<string, Command> commands;

        public Shell() {
            init();
        }

        public Shell(Dictionary<string, Command> newCommands) {
            init();
            commands = commands.Concat(newCommands).ToDictionary(k => k.Key, v => v.Value);
        }

        private void init() {
            commands.Add("start", new StartCommand());
            commands.Add("interval", new IntervalCommand());
            commands.Add("status", new StatusCommand());
            commands.Add("crash", new CrashCommand());
            commands.Add("freeze", new FreezeCommand());
            commands.Add("unfreeze", new UnfreezeCommand());
        }

        public void execute() {
            bool leave = false;
            string prompt = "> ";
            string line;

            Console.WriteLine("Puppet Master Shell ('exit' to leave)");
            Console.WriteLine(prompt);

            while (!leave) {
                char[] delimiters = { ' ' };
                line = Console.ReadLine();

                string[] arg = line.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);
                Command c = commands[arg[0]];

                // TODO: Finish the class
            }
        }
    }
}
