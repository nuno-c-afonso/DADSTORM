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

namespace PuppetMasterGUI {
    public partial class Form1 : Form {
        private ReadFileByLineFiltered lineParser;
        CommonClasses.UrlSpliter urlsplitter = new CommonClasses.UrlSpliter();
        OperatorsInfo operatorsInfo = new OperatorsInfo();


        private string loggingLevel ="light";
        private string semantics = "at-most-once";


        const int PCS_RESERVED_PORT = 10000;
        const int LOGGING_PORT = 10001;

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
        private void importConfigFile(string path)
        {
            //incase the input file is not on .\input\dadstorm.config at the start of GUI
            try
            {
                string text = System.IO.File.ReadAllText(path);

                //textBox2.Text = text;
                //textBox1.Text = text.Replace("\n", Environment.NewLine);

                lineParser = new ReadFileByLineFiltered(path);
                textBox2.Text = string.Join("\r\n", lineParser.remainingLines());
                textBox3.Text = path.Split('\\').Last();

                // shouldn't run more than once even if we load another config file #TODO
                runConfigCommands(lineParser.getConfigCommandsLines());

                textBox2.Text = string.Join("\r\n", lineParser.remainingLines());
            }
            catch (System.IO.DirectoryNotFoundException)
            {

            }

        }

        private void runConfigCommands(List<string> lines)
        {
            foreach (var line in lines)
            {
                ConsoleBox.AppendText(line+ "\r\n");
                LineParser ln = new LineParser(line);
                Debug.WriteLine(ln.Words[0]);
                switch (ln.Words[0])
                {
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
                        operatorsInfo.AddNewOP(opb);
                        Debug.WriteLine("created fields association " + opb.Name);
                        break;

                }

            }
            int port = 11005;
            foreach (var line in lines)
            {
                // get first word
                var m = Regex.Match(line, @"^\w+");
                switch (m.Value)
                {
                    case "Semantics":
                        break;
                    case "LoggingLevel":
                        break;
                    default:

                        string opName = m.Value;
                        //# TODO Call create replica here ??
                        OperatorBuilder opb = operatorsInfo.getOpInfo(opName);

                        //OperatorBuilder opb = new OperatorBuilder(ln.Words.ToList()); //is it really necessary to rebuild this here ?

                        var outList = operatorsInfo.getOuputListOfOP(opb.Name);

                         //puts OperatorType at the start of OperatorParameters (necessary for replica main)
                        //opb.SpecificParameters.Insert(0, opb.OperatorType);

                        // build this string: OPTYPE param1,param2,param3
                        // ex: FILTER 3,=,"www.tecnico.ulisboa.pt"
                        var operatorParametersComma = new List<string>();
                        operatorParametersComma.Add(opb.OperatorType);
                        operatorParametersComma.Add(string.Join(",", opb.SpecificParameters));

                        Debug.WriteLine("\n### "+ opb.Name);
                        Debug.WriteLine("-- nextAddresses " + string.Join(",", outList));


                        for (int i = 0; i < opb.RepFactor; i++)
                        {
                            Console.WriteLine("- Replica number " + i);
                            Console.WriteLine("Machine Address: {0}\t port: {1}", urlsplitter.getAdress(opb.Addresses[i]), urlsplitter.getPort(opb.Addresses[i]));
                            // TODO FIXME pcsAddress should be urlsplitter.getAdress(opb.Addresses[i])
                            // but for now i can only create on localhost, not sure how to use addresses from configfile i do not control
                            string pcsAddress = "localhost";
                            Console.WriteLine("Calling PCS on address " + pcsAddress);
                            try
                            {
                                port = PCS_RESERVED_PORT;
                                CommonClasses.IProcessCreator obj = (CommonClasses.IProcessCreator)Activator.GetObject(typeof(CommonClasses.IProcessCreator),
                                "tcp://" + pcsAddress + ":" + port + "/ProcessCreator");

                                // TODO FIXME first argument being sent should be the puppetMasterUrl, it's still not
                                obj.createReplica("tcp://localhost:" + port.ToString(), opb.Routing, semantics, loggingLevel,
                                                                   i, operatorParametersComma, opb.Addresses, outList);

                            }
                            catch (System.Net.Sockets.SocketException e)
                            {
                                Console.WriteLine("Error with host1 " + pcsAddress);
                                Console.WriteLine("Exceptio1n " + e);
                            }

                            try
                            {

                                

                            }
                            catch (System.Net.Sockets.SocketException e)
                            {
                                Console.WriteLine("Error with host2 " + pcsAddress);
                                Console.WriteLine("Exception2 " + e);
                            }
                        }

                        break;
                }

            }

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

            switch (list[0])
            {
                case "start":
                    Debug.WriteLine("Param1: {0}", list[1]);
                    break;
                case "interval":
                    Debug.WriteLine("Param1: {0} |Param2: {1}", list[1], list[2]);
                    break;
                case "status":
                    break;
                case "crash":
                    Debug.WriteLine("Param1: {0} |Param2: {1}", list[1], list[2]);
                    break;
                case "freeze":
                    Debug.WriteLine("Param1: {0} |Param2: {1}", list[1], list[2]);
                    break;
                case "unfreeze":
                    Debug.WriteLine("Param1: {0} |Param2: {1}", list[1], list[2]);
                    break;
                case "wait":
                    Debug.WriteLine("Param1: {0}", list[1]);            
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
