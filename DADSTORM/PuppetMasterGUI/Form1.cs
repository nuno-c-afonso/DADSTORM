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

        private void runConfigCommands(List<string> lines)
        {
            int port = 10005;
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

                        Debug.WriteLine(opb.Name);

                        for (int i = 0; i < opb.RepFactor; i++)
                        {
                            string s = "localhost";
                            Console.WriteLine("Calling PCS on address " + s);
                            try
                            {
                                CommonClasses.IProcessCreator obj = (CommonClasses.IProcessCreator)Activator.GetObject(typeof(CommonClasses.IProcessCreator),
                                "tcp://" + s + ":" + PCS_RESERVED_PORT + "/ProcessCreator");

                                // TODO: Send the right arguments
                                //TODO last argument is the addresses of replicas of next operator?
                                //      if it is this should be done after the map OP1 -> OP2 is created  **step 2
                                

                                obj.createReplica("tcp://localhost:" + port++.ToString(), opb.Routing, semantics, loggingLevel,
                                                                   i, opb.SpecificParameters, opb.Addresses, new List<string>());
                            }
                            catch (System.Net.Sockets.SocketException e)
                            {
                                Console.WriteLine("Error with host " + s);
                                //Console.WriteLine("Exception " + e);
                            }
                        }
                        
                        break;
                }

            }

        }


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
