using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileManipulator;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Diagnostics;
using System.Runtime.Remoting;
using CommandLine;
using CommonClasses;
using System.Text.RegularExpressions;
using System.Net;

namespace PuppetMasterGUI {
    public partial class Form1 : Form {
        private ReadFileByLineFiltered lineParser;
        CommonClasses.UrlSpliter urlsplitter = new CommonClasses.UrlSpliter();
        OperatorsInfo operatorsInfo = new OperatorsInfo();
        private string firstOPName;

        private bool alreadyRunConfigCommands = false;

        private string loggingLevel ="light";
        private string semantics = "at-most-once";


        const int PCS_RESERVED_PORT = 10000;
        const int LOGGING_PORT = 10001;
        private IPAddress puppetMasterIPAddress = IPAddresses.LocalIPAddress();

        public Form1() {
            InitializeComponent();

            string DEFAULT_CONFIG_PATH = @".\input\dadstorm.config";
            importConfigFile(DEFAULT_CONFIG_PATH);


            // TODO: Process the configuration file and then reach the process creation services

            TcpChannel channel = new TcpChannel();
            
            // The code below was generating a weird exception, must investigate it
            //#TODO
            try
            {
                ChannelServices.RegisterChannel(channel, false);
              
            }
            catch (RemotingException ex)
            {

            }

        }
        private void importConfigFile(string path) {
            //incase the input file is not on .\input\dadstorm.config at the start of GUI
            try {
                //string text = System.IO.File.ReadAllText(path);

                //textBox2.Text = text;
                //textBox1.Text = text.Replace("\n", Environment.NewLine);

                lineParser = new ReadFileByLineFiltered(path);
                textBox2.Text = string.Join("\r\n", lineParser.remainingLines());
                textBox3.Text = path.Split('\\').Last();

                // shouldn't run more than once even if we load another config file #TODO
                
                runConfigCommands(lineParser.getConfigCommandsLines());

                textBox2.Text = string.Join("\r\n", lineParser.remainingLines());
            }
            catch (System.IO.DirectoryNotFoundException) {

            }

        }

        private void runConfigCommands(List<string> lines) {
            if (alreadyRunConfigCommands)
                return;

            alreadyRunConfigCommands = true;
            foreach (var line in lines) {
                ConsoleBox.AppendText(line + "\r\n");
                LineParser ln = new LineParser(line);
                Debug.WriteLine(ln.Words[0]);
                switch (ln.Words[0]) {
                    case "Semantics":
                        semantics = ln.Words[1];
                        break;
                    case "LoggingLevel":
                        loggingLevel = ln.Words[1];
                        break;
                    default:
                        //# TODO Call create replica here
                        OperatorBuilder opb = new OperatorBuilder(ln.Words.ToList());

                        //Use Name and Input to create a graph/map to know the next node  **step 1
                        operatorsInfo.addNewOP(opb);
                        Debug.WriteLine("created fields association " + opb.Name);
                        break;
                }

            }

            firstOPName = operatorsInfo.getFirstOperator();

            contactReplicas();
        }

        private void contactReplicas() {
            foreach(string opName in operatorsInfo.OperatorNames) {
                OperatorBuilder opb = operatorsInfo.getOpInfo(opName);
                string routing = null;
                List<string> output = null;
                List<string> operation = new List<string>();

                // Setting the common parameters between replicas
                try {
                    routing = operatorsInfo.getMyRouting(opb.Name);
                    output = operatorsInfo.getOuputAddressesListOfOP(opb.Name);
                }
                catch (LastOperatorException e) {
                    routing = "primary";
                    output = new List<string>();
                }

                operation.Add(opb.OperatorType);
                operation.Add(string.Join(",", opb.SpecificParameters));

                // Contacting all the operator's PCS
                for (int i = 0; i < opb.RepFactor; i++) {
                    Console.WriteLine("- Replica number " + i);
                    Console.WriteLine("Machine Address: {0}\t port: {1}", urlsplitter.getAddress(opb.Addresses[i]), urlsplitter.getPort(opb.Addresses[i]));

                    // TODO FIXME pcsAddress should be urlsplitter.getAdress(opb.Addresses[i])
                    // but for now i can only create on localhost, not sure how to use addresses from configfile i do not control
                    //string pcsAddress = "localhost";
                    string address = urlsplitter.getAddress(opb.Addresses[i]);
                    Console.WriteLine("Calling PCS on address " + address);

                    try {
                        CommonClasses.IProcessCreator obj = (CommonClasses.IProcessCreator)Activator.GetObject(typeof(CommonClasses.IProcessCreator),
                        "tcp://" + address + ":" + PCS_RESERVED_PORT + "/ProcessCreator");

                        // TODO FIXME first argument being sent should be the puppetMasterUrl, it's still not
                        obj.createReplica("tcp://" + puppetMasterIPAddress.ToString() + ":" + LOGGING_PORT.ToString(),
                            routing, semantics, loggingLevel, i, operation, opb.Addresses, output);

                        // test status 
                        int port = int.Parse(urlsplitter.getPort(opb.Addresses[i]));
                        CommonClasses.ReplicaInterface obj2 = (CommonClasses.ReplicaInterface)Activator.GetObject(typeof(CommonClasses.ReplicaInterface),
                        "tcp://" + address + ":" + port + "/op");

                        Debug.WriteLine("OH WOW " + obj2.Status());
                    }
                    catch (System.Net.Sockets.SocketException e) {
                        Console.WriteLine("Error with host " + address);
                        Console.WriteLine("Exception " + e);
                    }
                }
            }
        }

        private CommonClasses.ReplicaInterface getRemoteObject(string opName, int replicaIndex = 0)
        {
            var opInfo = operatorsInfo.getOpInfo(opName);
            string address = opInfo.Addresses[replicaIndex];

            return (CommonClasses.ReplicaInterface)Activator.GetObject(
                        typeof(CommonClasses.ReplicaInterface),
                        address
                        );
        }

        private void run(string line) {
            line = line.ToLower();

            var list = line.Split(' ').ToList();
            // get first word
            /*
            var m = Regex.Match(line, @"\w+|\d+"); // use .NextMatch().Value
            Debug.WriteLine(m.Value);
            */
            Debug.WriteLine(list[0]);
            CommonClasses.ReplicaInterface obj = null;

            switch (list[0])
            {
                case "start":
                    obj = getRemoteObject(list[1]);
                    Debug.WriteLine("Param1: {0}", list[1]);
                    obj.Start();
                    break;
                case "interval":
                    obj = getRemoteObject(list[1]);
                    Debug.WriteLine("Param1: {0} |Param2: {1}", list[1], list[2]);
                    obj.Interval(int.Parse(list[2]));
                    break;
                case "status":
                    foreach (var opName in operatorsInfo.OperatorNames)
                    {
                        var opInfo = operatorsInfo.getOpInfo(opName);

                        foreach (var address in opInfo.Addresses)
                        {
                            obj = (CommonClasses.ReplicaInterface)Activator.GetObject(
                                typeof(CommonClasses.ReplicaInterface),
                                address
                                );
                            obj.Status();

                        }
                    }
                    break;
                case "crash":
                    obj = getRemoteObject(list[1], int.Parse(list[2]));
                    Debug.WriteLine("Param1: {0} |Param2: {1}", list[1], list[2]);
                    obj.Crash();
                    break;
                case "freeze":
                    obj = getRemoteObject(list[1], int.Parse(list[2]));
                    Debug.WriteLine("Param1: {0} |Param2: {1}", list[1], list[2]);
                    obj.Freeze();
                    break;
                case "unfreeze":
                    obj = getRemoteObject(list[1], int.Parse(list[2]));
                    Debug.WriteLine("Param1: {0} |Param2: {1}", list[1], list[2]);
                    obj.Unfreeze();
                    break;
                case "wait":
                    //#TODO wait x seconds on puppetMaster
                    Debug.WriteLine("Param1: {0}", list[1]);   //[1] == time in ms         
                    break;
                default:
                    Debug.WriteLine("Oops, didn't find the command");
                    break;
            }

        }

        //Run One Command
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string line = lineParser.nextLine();
                textBox1.Text = line;
                textBox2.Text = string.Join("\r\n", lineParser.remainingLines());
                run(line);

            }
            catch (EOFException)
            {
                textBox1.Text = "";
            }
            
        }

        //Run All Commands
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                while (true)
                {
                    string line = lineParser.nextLine();
                    textBox1.Text = line;
                }

            }
            catch (EOFException)
            {
                textBox2.Text = string.Join("\r\n", lineParser.remainingLines());
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                importConfigFile(openFileDialog1.FileName);
            }

        }
    }
}
