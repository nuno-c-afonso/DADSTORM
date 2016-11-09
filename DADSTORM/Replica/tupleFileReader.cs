using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Replica {
    public class TupleFileReader {
        private ReplicaObject buffer;
        private string[] lines;
        private Router router;

        public TupleFileReader(ReplicaObject buffer, string filepath, string routing, string semantics, List<string> replicas) {
            this.buffer = buffer;

            string routingLower = routing.ToLower();
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
        } 

        public void feedBuffer() {
            string[] tuple;
            while (!buffer.Started)
                Thread.Sleep(100);
            foreach(string line in lines){
                tuple = getTupleFromLine(line);
                router.sendToNext(tuple);
                //buffer.addTuple(tuple);
            }

        }

        public string[] getTupleFromLine(string line){
            string[] delimiters = { ", ", " " };
            string[] tuple = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            return tuple;
        }



    }
}
