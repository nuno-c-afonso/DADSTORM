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

namespace PuppetMasterGUI {
    public partial class Form1 : Form {
        private ReadFileByLineFiltered lineParser;
        CommonClasses.UrlSpliter urlsplitter = new CommonClasses.UrlSpliter();
       

        private string loggingLevel ="light";
        private string semantics = "at-most-once";

        private List<string> operatorNames = new List<string>();
        private Dictionary<string, string> whereToSend = new Dictionary<string, string>(); // OP1 -> OP2 cause 'OP2 input ops OP1'
        private Dictionary<string, OperatorBuilder> operatorNameToOperatorBuilderDictionary = new Dictionary<string, OperatorBuilder>();

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

                        // use a class for this 3 variables ? #TODO FIXME
                        operatorNames.Add(opb.Name);
                        whereToSend.Add(opb.Input, opb.Name);
                        operatorNameToOperatorBuilderDictionary.Add(opb.Name, opb);


                        //Use Name and Input to create a graph/map to know the next node  **step 1

                        Debug.WriteLine("created fields association " + opb.Name);
                        break;

                }

            }
            int port = 10005;
            foreach (var line in lines)
            {
                LineParser ln = new LineParser(line);
                Debug.WriteLine(ln.Words[0]);
                switch (ln.Words[0])
                {
                    case "Semantics":
                        break;
                    case "LoggingLevel":
                        break;
                    default:
                        //# TODO Call create replica here
                        OperatorBuilder opb = new OperatorBuilder(ln.Words.ToList()); //is it really necessary to rebuild this here ?

                        // get outputReplicaList
                        //      get whereToSend
                        var outList = new List<string>();

                        string nextOP;
                        if( whereToSend.TryGetValue(opb.Name, out nextOP) )
                        {
                            OperatorBuilder o = operatorNameToOperatorBuilderDictionary[nextOP]; // need to check first ?
                            outList = o.Addresses;
                        }


                        //Use Name and Input to create a graph/map to know the next node  **step 1
                        
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

                                CommonClasses.IProcessCreator obj = (CommonClasses.IProcessCreator)Activator.GetObject(typeof(CommonClasses.IProcessCreator),
                                "tcp://" + pcsAddress + ":" + PCS_RESERVED_PORT + "/ProcessCreator");

                                // TODO: Send the right arguments
                                //TODO last argument is the addresses of replicas of next operator?
                                //      if it is this should be done after the map OP1 -> OP2 is created  **step 2

                                //puts OperatorType at the start of OperatorParameters (necessary for replica main)
                                //opb.SpecificParameters.Insert(0, opb.OperatorType);

                                /*
                                var wrongList = new List<string>();
                                wrongList.Add("exemploDeOutputAddress");
                                */

                                // build this string: OPTYPE param1,param2,param3
                                // ex: FILTER 3,=,"www.tecnico.ulisboa.pt"
                                var operatorParametersComma = new List<string>();
                                operatorParametersComma.Add(opb.OperatorType);
                                operatorParametersComma.Add(string.Join(",", opb.SpecificParameters));

                                // TODO FIXME first argument being sent should be the puppetMasterUrl, it's still not
                                obj.createReplica("tcp://localhost:" + port++.ToString(), opb.Routing, semantics, loggingLevel,
                                                                   i, operatorParametersComma, opb.Addresses, outList);

                                //string args = createReplica("tcp://localhost:" + port++.ToString(), opb.Routing, semantics, loggingLevel, i, opb.SpecificParameters, opb.Addresses, new List<string>());

                                //Debug.WriteLine("REPLICA ARGS "+ args);


                            }
                            catch (System.Net.Sockets.SocketException e)
                            {
                                Console.WriteLine("Error with host " + pcsAddress);
                                //Console.WriteLine("Exception " + e);
                            }
                        }

                        break;
                }

            }

        }
        /*
        // used for debug instead of calling the processCreationService
        public string createReplica(string masterURL, string routing, string semantics, string logLevel,
            int repIndex, List<string> op, List<string> replicas, List<string> output)
        {

            string a = "";

            // Building the arguments for the main
            a = masterURL + " " + routing + " " + semantics + " " + logLevel;
            a+=" -op " + string.Join(" ", op);
            a += " -r " + repIndex +" " + string.Join(" ", replicas);
            a += " -o " + string.Join(" ", output);

            return a;
        }
        */
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string line = lineParser.nextLine();
                textBox1.Text = line;
                textBox2.Text = string.Join("\r\n", lineParser.remainingLines());
                LineParser ln = new LineParser(line);
                var words = ln.Words;

            }
            catch (EOFException)
            {
                textBox1.Text = "";
            }
            
        }

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
