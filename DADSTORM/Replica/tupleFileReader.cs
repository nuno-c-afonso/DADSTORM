using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica
{
    public class tupleFileReader
    {
        private ReplicaInterface buffer;
        private string[] lines;

        public tupleFileReader(ReplicaInterface buffer, string filepath)
        {
            this.buffer = buffer;
            lines = System.IO.File.ReadAllLines(filepath);
            lines = lines.Where(line => (line.Length > 0 && line[0] != '%')).ToArray();
        } 

        public void feedBuffer(){
            string[] tuple;
            foreach(string line in lines){
                tuple = getTupleFromLine(line);
                buffer.addTuple(tuple);
            }

        }

        public string[] getTupleFromLine(string line){
            string[] delimiters = { ", ", " " };
            string[] tuple = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            return tuple;
        }



    }
}
