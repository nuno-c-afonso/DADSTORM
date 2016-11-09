using CommonClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommandLine {
    public class Shell {
        private Dictionary<string, Command> commands;
        private OperatorsInfo operatorInfo;
        private bool leave;
        private int waiting;  // Measured in milliseconds

        public static readonly string[] commandsNames = { "start", "interval", "status",
            "crash", "freeze", "unfreeze", "wait" };

        public bool Leave {
            set {
                leave = value;
            }
        }

        public int Waiting {
            set {
                waiting = value;
            }

            get {
                return waiting;
            }
        }

        public Shell(OperatorsInfo opi) {
            init(opi);
        }

        private void init(OperatorsInfo opi) {
            operatorInfo = opi;
            leave = false;
            waiting = 0;
            commands = new Dictionary<string, Command>();

            commands.Add("start", new StartCommand(opi));
            commands.Add("interval", new IntervalCommand(opi));
            commands.Add("status", new StatusCommand(opi));
            commands.Add("crash", new CrashCommand(opi));
            commands.Add("freeze", new FreezeCommand(opi));
            commands.Add("unfreeze", new UnfreezeCommand(opi));
            commands.Add("wait", new WaitCommand(this));
        }

        public Command getCommand(string s) {
            Command c;

            if (commands.TryGetValue(s, out c))
                return c;

            return null;
        }

        public static bool doesCommandExist(string command) {
            char[] delimiters = { ' ' };
            string s = command.ToLower();

            string[] arg = s.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);

            s = (arg.Length == 0) ? "" : arg[0];

            return commandsNames.Contains(s);
        }

        public void run(string line) {
            char[] delimiters = { ' ' };
            string[] arg = line.ToLower().Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);

            if (arg.Length == 0)
                return;

            Command c = getCommand(arg[0]);

            if (c != null) {
                try {
                    c.execute(commandArgs(arg));
                }
                catch (WrongNumberOfArgsException) {
                    Console.WriteLine("The number of the given arguments is wrong. Please try again.");
                }
                catch (WrongTypeOfArgException) {
                    Console.WriteLine("The type of the given arguments is wrong. Please try again.");
                }
                catch (WrongOperatorException) {
                    Console.WriteLine("The name of the given operator is wrong. Please try again.");
                }
                catch (IndexOutOfBoundsException) {
                    Console.WriteLine("The index of the given operator is wrong. Please try again.");
                }
            }

            else
                Console.WriteLine("Unknown Command: " + arg[0]);
        }


        // Used to get the subarray containing only the command args
        private string[] commandArgs(string[] lineSplitted) {
            List<string> l = new List<string>();

            for (int i = 1; i < lineSplitted.Length; i++)
                l.Add(lineSplitted[i]);

            return l.ToArray();
        }
    }
}
