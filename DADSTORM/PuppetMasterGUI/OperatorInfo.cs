using CommandLine;
using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMasterGUI
{
    public class OperatorsInfo
    {
        private Dictionary<string, Command> commands;

        private List<string> operatorNames = new List<string>();
        private Dictionary<string, string> whereToSend = new Dictionary<string, string>(); // OP1 -> OP2 cause 'OP2 input ops OP1'
        private Dictionary<string, OperatorBuilder> operatorNameToOperatorBuilderDictionary = new Dictionary<string, OperatorBuilder>();

        public List<string> OperatorNames
        {
            get
            {
                return operatorNames;
            }
        }

        public OperatorsInfo() {
            init();
        }

        public OperatorsInfo(Dictionary<string, Command> newCommands) {
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

        public void run(string line)
        {
            string first_word;
            line = line.ToLower();
             
            char[] delimiters = { ' ' };

            List<string> args = line.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries).ToList();

            first_word = (args.Count == 0) ? "" : args[0]; //.ToLower()

            Command c;
            if (commands.TryGetValue(first_word, out c)) // Returns true.
            {
                Console.WriteLine("\tFound command: " + c);
            }
            else
            {
                Console.WriteLine("\tUnknown Command: " + first_word);
                return;
            }
            // Command c = commands[arg[0]];
            args.RemoveAt(0);
            //c.execute(args);
        }

        public void consoleLoop() {
            bool leave = false;
            string prompt = "> ";
            string line;

            Console.WriteLine("Puppet Master Shell ('exit' to leave)");
            char[] delimiters = { ' ' };

            while (!leave) {
                
                Console.Write(prompt);
                line = Console.ReadLine();
                run(line);
                line.ToLower();

                // TODO: Finish the class
            }
        }

        public bool isOperator(string opName) {
            return operatorNames.Contains(opName);
        }

        public void addNewOP(OperatorBuilder opb)
        {
            operatorNames.Add(opb.Name.ToLower());

            foreach(string s in opb.Input) {
                string sLower = s.ToLower();
                if (!whereToSend.ContainsKey(sLower))
                    whereToSend.Add(sLower, opb.Name.ToLower());
            }

            if (!operatorNameToOperatorBuilderDictionary.ContainsKey(opb.Name.ToLower()))
                operatorNameToOperatorBuilderDictionary.Add(opb.Name.ToLower(), opb);
        }

        public List<string> getOuputAddressesListOfOP(string opName) {
            OperatorBuilder nextOpBuilder = getNextOpInfo(opName.ToLower());
            return nextOpBuilder.Addresses;
        }

        public string getMyRouting(string opName) {
            OperatorBuilder nextOpBuilder = getNextOpInfo(opName.ToLower());
            return nextOpBuilder.PreviousRouting;
        }

        public OperatorBuilder getNextOpInfo(string opName) {
            OperatorBuilder nextOpBuilder = null;

            string nextOP;
            if (whereToSend.TryGetValue(opName.ToLower(), out nextOP)) {
                nextOpBuilder = operatorNameToOperatorBuilderDictionary[nextOP]; // need to check 
            }
            else
                throw new LastOperatorException();

            return nextOpBuilder;
        }

        public OperatorBuilder getOpInfo(string opName) {
            OperatorBuilder nextOpBuilder = null;
            operatorNameToOperatorBuilderDictionary.TryGetValue(opName.ToLower(), out nextOpBuilder);
            return nextOpBuilder;
        }

        public string getFirstOperator() {
            Dictionary<string, string>.ValueCollection valueColl = whereToSend.Values;

            foreach (string op in operatorNames)
                if (!valueColl.Contains<string>(op))
                    return op;

            return null;
        }

    }
}
