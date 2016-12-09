using Replica;
using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadTuplesFromFile {
    public class TupleFileReader {

        public static void Main(string[] args) {
            string filepath = args[0];
            string routingLower = args[1].ToLower();
            string semantics = args[2];
            List<string> replicas = new List<string>();

            for (int i = 3; i < args.Length; i++)
                replicas.Add(args[i]);

            string[] lines;
            Router router;
            char[] delimiters = { '(', ')' };
            string[] splitted = routingLower.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            if (splitted[0].Equals("primary"))
                router = new PrimaryRouter(replicas, semantics);
            else if (splitted[0].Equals("random"))
                router = new RandomRouter(replicas, semantics);
            else
                router = new HashRouter(replicas, semantics, int.Parse(splitted[1]));


            lines = System.IO.File.ReadAllLines(filepath);
            lines = lines.Where(line => (line.Length > 0 && line[0] != '%')).ToArray();
            
            int counter = 0;
            foreach (string line in lines) {
                string[] tuple = getTupleFromLine(line);
                TupleWrapper t = new TupleWrapper("", "" + counter++, tuple);
                router.sendToNext(t);
            }
        }

        public static string[] getTupleFromLine(string line) {
            string[] delimiters = { ", ", " " };
            string[] tuple = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            return tuple;
        }
    }
}
