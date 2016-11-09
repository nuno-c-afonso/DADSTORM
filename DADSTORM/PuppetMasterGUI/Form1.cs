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
using System.Windows;

namespace PuppetMasterGUI {
    public partial class Form1 : Form {
        private ReadFileByLineFiltered lineParser;
        CommonClasses.UrlSpliter urlsplitter = new CommonClasses.UrlSpliter();
        OperatorsInfo operatorsInfo = new OperatorsInfo();
        private string firstOPName;

        private bool alreadyRunConfigCommands = false;

        private string loggingLevel ="light";
        private string semantics = "at-most-once";

        string DEFAULT_CONFIG_PATH = @".\input\dadstorm.config";

        private List<string> logMessages;

        const int PCS_RESERVED_PORT = 10000;
        const int LOGGING_PORT = 10001;
        private IPAddress puppetMasterIPAddress = IPAddresses.LocalIPAddress();
        private Shell shell;
        private bool canUseCommands = true;
        private Task lastCommand = null;

        private CancellationTokenSource tokenSource2;
        private CancellationToken cancelToken;

        private void FormPuppetMaster_Load(object sender, EventArgs e) { }

        public Form1() {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(formClosing);

            PuppetMasterLog.form = this;
            tokenSource2 = new CancellationTokenSource();
            cancelToken = tokenSource2.Token;

            logMessages = new List<string>();

            TcpChannel channel = new TcpChannel(LOGGING_PORT);

            // #TODO The code below was generating a weird exception, must investigate it
            try
            {
                ChannelServices.RegisterChannel(channel, false);
                RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof(PuppetMasterLog), "log",
                    WellKnownObjectMode.Singleton);

            }
            catch (RemotingException){}

            importConfigFile(DEFAULT_CONFIG_PATH);


        }
        private void importConfigFile(string path) {
            
            // In case the input file is not on .\input\dadstorm.config at the start of GUI
            try {
                lineParser = new ReadFileByLineFiltered(path);
                textBox2.Text = string.Join("\r\n", lineParser.remainingLines());
                textBox3.Text = path.Split('\\').Last();

                // shouldn't run more than once
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

                AddMsgToLog(line);
                //ConsoleBox.AppendText(line + "\r\n");

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

            shell = new Shell(operatorsInfo);

            firstOPName = operatorsInfo.getFirstOperator();

            contactReplicas();
        }

        private void contactReplicas() {
            foreach(string opName in operatorsInfo.OperatorNames) {
                OperatorBuilder opb = operatorsInfo.getOpInfo(opName);
                string routing = null;
                string incomingRouting = null;
                List<string> output = null;
                List<string> input = null;
                List<string> operation = new List<string>();

                // Setting the common parameters between replicas
                try {
                    routing = operatorsInfo.getMyRouting(opb.Name);
                    incomingRouting = operatorsInfo.getMyIncomingRouting(opb.Name);
                    output = operatorsInfo.getOuputAddressesListOfOP(opb.Name);
                    input = operatorsInfo.getInputAddressesListOfOP(opb.Name);
                } catch (LastOperatorException) {
                    routing = "primary";
                    incomingRouting = "primary";
                    output = new List<string>();
                    input = new List<string>();
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
                            routing, incomingRouting, semantics, loggingLevel, i, operation, opb.Addresses, output, input);

                        //testReplica(opb.Addresses[i]);
                    }
                    catch (System.Net.Sockets.SocketException e) {
                        Console.WriteLine("Error with host " + address);
                        Console.WriteLine("Exception " + e);
                    }
                }
            }
        }

        //Run One Command
        private void button1_Click(object sender, EventArgs e) {
            runNextLine();
        }

        //Run All Commands
        private void button2_Click(object sender, EventArgs e) {
            asyncRunNextLine();
        }

        // To load the configuration file
        private void button3_Click(object sender, EventArgs e) {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                importConfigFile(openFileDialog1.FileName);
        }

        // To load the script file
        private void scriptButton_Click(object sender, EventArgs e) {
            OpenFileDialog openFileDialog2 = new OpenFileDialog();

            if (alreadyRunConfigCommands && openFileDialog2.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                scriptTextBox.Text = openFileDialog2.FileName;
                ReadFileByLine rfbl = new ReadFileByLine(openFileDialog2.FileName);
                char[] delimiter = { ' ' };
                string toAdd = "";

                try {
                    while (true) {
                        string command = rfbl.nextLine();
                        if (Shell.doesCommandExist(command))
                            toAdd += command + "\r\n";
                    }
                }
                catch (EOFException) { }

                if (!textBox2.Text.EndsWith("\r\n") && textBox2.Text.Length > 0)
                    textBox2.Text += "\r\n" + toAdd;
                else
                    textBox2.Text += toAdd;
            }
        }

        // To intercept the closing command
        private async void formClosing(object sender, FormClosingEventArgs e) {
            closeAllReplicas();
        }


        /*************************************************************************************
         *  These are the functions that allow running the desired operations asynchronously *
         ************************************************************************************/


        private async void runNextLine() {
            if ((textBox2.Text != null || !textBox2.Text.Equals("")) && alreadyRunConfigCommands && canUseCommands) {
                string[] delimiter = { "\r\n" };
                string[] lines = textBox2.Text.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length > 0) {

                    if (shell.Waiting > 0)
                        await Task.Run(() => waitOnPuppetMaster(shell.Waiting), cancelToken);

                    Invoke(new EditTextBoxes(ChangeTextBoxesLines)); // thread-safe access to form

                    Debug.WriteLine("calling shell for command");

                    if (lastCommand != null)
                        lastCommand.Wait();
                    lastCommand = new Task(() => shell.run(lines[0]));
                    lastCommand.Start();
                }
            }
        }

        private async void asyncRunNextLine() {
            await Task.Run(() => runNextLine(), cancelToken);
            if (!textBox2.Text.Equals("")) {
                await Task.Run(() => Thread.Sleep(100), cancelToken);
                asyncRunNextLine();
            }
        }

        private async void waitOnPuppetMaster(int time) {
            canUseCommands = false;
            Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss tt") +"- "+ time + " ms, on wait");
            Thread.Sleep(time);
            Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss tt") + "- " + time + " ms, after wait");
            shell.Waiting = 0;
            canUseCommands = true;
        }

        // needed for changing text of form in threads
        private void ChangeTextBoxesLines() {
            string[] delimiter = { "\r\n" };
            string[] lines = textBox2.Text.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            textBox1.Text = lines[0];
            textBox2.Text = string.Join("\r\n", lines.Skip(1));

            AddMsgToLog(lines[0]);
        }

        public void closeAllReplicas() {
            tokenSource2.Cancel();
            List<string> replicasNames = operatorsInfo.OperatorNames;
            List<Task> TaskList = new List<Task>();
            foreach (string name in replicasNames) {
                OperatorBuilder ob = operatorsInfo.getOpInfo(name);
                int repFactor = ob.RepFactor;

                for (int i = 0; i < repFactor; i++) {
                    string crashLine = "crash " + name + " " + i;
                    AddMsgToLog(crashLine);

                    var LastTask = new Task(() => shell.run(crashLine));
                    LastTask.Start();
                    TaskList.Add(LastTask);
                }
            }
            Task.WaitAll(TaskList.ToArray());

        }

        public void AddMsgToLog(string arg, bool replaceWithTabs = false)
        {
            if (replaceWithTabs)
                arg = arg.Replace(" ", "\t");

            string changedMsg = DateTime.Now.ToString("HH:mm:ss tt") + "  " + arg;

            logMessages.Add(changedMsg);
            ConsoleBox.AppendText(changedMsg + "\r\n");
            Debug.WriteLine("AddMsgToLog " + changedMsg);

        }
    }

    delegate void EditTextBoxes();
    delegate void CallCommands(string line);
    delegate void DelAddMsg(string mensagem, bool replace);


    public class PuppetMasterLog : MarshalByRefObject, IPuppetMasterLog {
        public static Form1 form;

        public PuppetMasterLog() { }

        public string Log(string args) {
            Debug.WriteLine("LOG was called " + args);

            //form.AddMsgToLog(args);
            form.Invoke(new DelAddMsg(form.AddMsgToLog), args, false); // thread-safe access to form
            Debug.WriteLine("DEBUG LOG " + args);
            return "YUP ITS DEBUG";
        }

    }

}
