using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses
{
    public class OperatorBuilder
    {
        public string Name { get; private set; }
        public string Input { get; private set; }
        public int RepFactor { get; private set; }
        public string PreviousRouting { get; private set; }
        public string MyRouting { get; set; }
        public List<string> Addresses { get; private set; }
        public List<string> SpecificParameters { get; private set; }
        public string OperatorType { get; private set; }


        public OperatorBuilder(List<string> words)
        {
            Name = words[0].ToLower();
            Input = words[3].ToLower();

            int repFactor;
            int.TryParse(words[6],out repFactor);
            RepFactor = repFactor;

            PreviousRouting = words[8];


            //find "operator" string index
            // addresses start at index 10 and end before "operator"
            // fields specific to the OPERATOR start 2 after "operator" or 1 after "spec"

            // when the operator specific paremeters start
            int i = words.IndexOf("operator") + 2;

            Addresses = words.Skip(10).Take(repFactor).ToList();
            OperatorType = words[i];
            SpecificParameters = words.Skip(i+1).Take(words.Count - i).ToList();
            

            //Debug.WriteLine("OP: {0}\n\t input: {1} \n\t rep_factor: {2}\n\t routing: {3}\n\t addresses: {4}\n\t  Type: {5}\n\t specific paremeters: {6}"
            //        , Name, Input, RepFactor, Routing, string.Join(",", Addresses), OperatorType, string.Join(",", SpecificParameters) );//TODO can add more information
            
        }

        
    }
}
