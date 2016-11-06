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
using System.Threading;

namespace PuppetMasterGUI {
    public partial class Form1 : Form {
        private ReadFileByLineFiltered lineParser;
        CommonClasses.UrlSpliter urlsplitter = new CommonClasses.UrlSpliter();
        OperatorsInfo operatorsInfo = new OperatorsInfo();
        private string firstOPName;
        private string lastOPName;

        private bool alreadyRunConfigCommands = false;

        private string loggingLevel ="light";
        private string semantics = "at-most-once";

        string DEFAULT_CONFIG_PATH = @".\input\dadstorm.config";

        private List<string> logMessages;


        const int PCS_RESERVED_PORT = 10000;
        const int LOGGING_PORT = 10001;
        private IPAddress puppetMasterIPAddress = IPAddresses.LocalIPAddress();

        public Form1() {
            InitializeComponent();

            PuppetMasterLog.form = this;
            logMessages = new List<string>();

            // TODO: Process the configuration file and then reach the process creation services
            TcpChannel channel = new TcpChannel(LOGGING_PORT);
            
            // The code below was generating a weird exception, must investigate it
            //#TODO
            try
            {
                ChannelServices.RegisterChannel(channel, false);
                RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof(PuppetMasterLog), "log",
                    WellKnownObjectMode.Singleton);


            }
            catch (RemotingException ex)
            {

            }

            importConfigFile(DEFAULT_CONFIG_PATH);


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


            if (alreadyRunConfigCommands)
                return;

            alreadyRunConfigCommands = true;
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
                        operatorsInfo.addNewOP(opb);
                        Debug.WriteLine("created fields association " + opb.Name);
                        break;

                }

            }
            int port;
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

                        // if Input of operator is not an operator it means that we have the first Operator
                        if (!operatorsInfo.isOperator(opb.Input))
                        {
                            firstOPName = opb.Name;
                        }

                        if(operatorsInfo.getNextOpInfo(opName) != null) { 
                            opb.MyRouting = operatorsInfo.getNextOpInfo(opName).PreviousRouting;
                            operatorsInfo.swapOperatorBuilder(opName, opb); // update with MyRouting info

                        }
                        else
                        {
                            lastOPName = opb.Name;
                            opb.MyRouting = "primary"; // if no routing is present, use primary
                        }

                        var outList = operatorsInfo.getOuputListOfOP(opb.Name);

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
                            //string pcsAddress = "localhost";
                            string address = urlsplitter.getAdress(opb.Addresses[i]);
                            Console.WriteLine("Calling PCS on address " + address);
                            try
                            {
                                
                                port = PCS_RESERVED_PORT;
                                CommonClasses.IProcessCreator obj = (CommonClasses.IProcessCreator)Activator.GetObject(typeof(CommonClasses.IProcessCreator),
                                "tcp://" + address + ":" + port + "/ProcessCreator");

                                // TODO FIXME first argument being sent should be the puppetMasterUrl, it's still not
                                obj.createReplica("tcp://" + puppetMasterIPAddress.ToString() + ":" + LOGGING_PORT.ToString(), opb.MyRouting, semantics, loggingLevel,
                                                                   i, operatorParametersComma, opb.Addresses, outList);
                        
                                          
                                //testReplica(opb.Addresses[i]);

                            }
                            catch (System.Net.Sockets.SocketException e)
                            {
                                Console.WriteLine("Error with host " + address);
                                Console.WriteLine("Exception " + e);
                            }

                        }

                        break;
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
                            //TODO FIXME best way to call ? (delegates vs invoke vs thread)
                            new Thread(() => testReplica(address)).Start();
                            //testReplica(address);
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

                this.Invoke(new CallCommands(run), new object[] { line });
                //this.Invoke(new DelAddMsg(this.AddMsgToLog), args);
                //run(line);

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


        public void AddMsgToLog(string args)
        {
            logMessages.Add("time| " + args);
            ConsoleBox.AppendText(args + "\r\n");
            Debug.WriteLine("in form DEBUG LOG " + args);

        }

        public void testReplica(string address)
        {
            // test status 
            CommonClasses.ReplicaInterface obj3 = (CommonClasses.ReplicaInterface)Activator.GetObject(typeof(CommonClasses.ReplicaInterface),
            address);

            Debug.WriteLine("OH WOW " + obj3.Status());
            //obj3.Status();

        }

        private void FormPuppetMaster_Load(object sender, EventArgs e)
        {

        }

    }

    delegate void CallCommands(string line);
    delegate void DelAddMsg(string mensagem);


    public class PuppetMasterLog : MarshalByRefObject, IPuppetMasterLog
    {
        public static Form1 form;

        public PuppetMasterLog() { }

        public string Log(string args)
        {
            Debug.WriteLine("LOG was called " + args);

            //form.AddMsgToLog(args);
            form.Invoke(new DelAddMsg(form.AddMsgToLog), args); // thread-safe access to form
            Debug.WriteLine("DEBUG LOG " + args);
            return "YUP ITS DEBUG";
        }

    }

}
