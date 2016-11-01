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

        public bool doesCommandExist(string command)
        {
            char[] delimiters = { ' ' };
            command.ToLower();

            string[] arg = command.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);

            command = (arg.Length == 0) ? "" : arg[0].ToLower();

            return commands.ContainsKey(command);

        }

        private void init() {
            commands = new Dictionary<string, Command>();

            commands.Add("start", new StartCommand());
            commands.Add("interval", new IntervalCommand());
            commands.Add("status", new StatusCommand());
            commands.Add("crash", new CrashCommand());
            commands.Add("freeze", new FreezeCommand());
            commands.Add("unfreeze", new UnfreezeCommand());
            commands.Add("wait", new WaitCommand());
            commands.Add("", new ZeroLengthStringCommand());
        }

        public void execute() {
            bool leave = false;
            string prompt = "> ";
            string line, first_word;

            Console.WriteLine("Puppet Master Shell ('exit' to leave)");


            while (!leave) {
                char[] delimiters = { ' ' };
                Console.Write(prompt);
                line = Console.ReadLine();
                line.ToLower();

                string[] arg = line.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);

                first_word = (arg.Length == 0) ? "" : arg[0].ToLower();

                Command c;
                if (commands.TryGetValue(first_word, out c)) // Returns true.
                {
                    Console.WriteLine(c);
                }
                else
                {
                    Console.WriteLine("Unknown Command: "+ first_word);
                    continue;
                }
                // Command c = commands[arg[0]];

                // TODO: Finish the class
            }
        }
    }
}
