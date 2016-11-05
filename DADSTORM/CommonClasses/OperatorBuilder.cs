using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses {
    public class OperatorBuilder {
        public string Name { get; private set; }
        public List<string> Input { get; private set; }
        public int RepFactor { get; private set; }
        public string Routing { get; private set; }
        public List<string> Addresses { get; private set; }
        public List<string> SpecificParameters { get; private set; }
        public string OperatorType { get; private set; }


        public OperatorBuilder(List<string> words) {
            Input = new List<string>();
            Addresses = new List<string>();
            SpecificParameters = new List<string>();

            int index;

            Name = words[0];

            for (index = 3; !words[index].Equals("rep"); index++)
                Input.Add(words[index]);

            // Update the index to the replication factor position
            index += 2;
            int repFactor;
            int.TryParse(words[index],out repFactor);
            RepFactor = repFactor;

            // Update the index to the routing position
            index += 2;
            Routing = words[index];

            // Update the index to the addresses position
            index += 2;
            for (; !words[index].Equals("operator"); index++)
                Addresses.Add(words[index]);

            // Update the index to the operator position
            index += 2;
            OperatorType = words[index++];
            for (; index < words.Count; index++)
                SpecificParameters.Add(words[index]);

            //find "operator" string index
            // addresses start at index 10 and end before "operator"
            // fields specific to the OPERATOR start 2 after "operator" or 1 after "spec"

            // when the operator specific parameters start
            /*int i = words.IndexOf("operator") + 2;

            Addresses = words.Skip(10).Take(repFactor).ToList();
            OperatorType = words[i];
            SpecificParameters = words.Skip(i+1).Take(words.Count - i).ToList();
            */

            Debug.WriteLine("OP: {0}\n\t input: {1} \n\t rep_factor: {2}\n\t routing: {3}\n\t addresses: {4}\n\t  Type: {5}\n\t specific paremeters: {6}"
                    , Name, string.Join(",", Input), RepFactor, Routing, string.Join(",", Addresses), OperatorType, string.Join(",", SpecificParameters) );//TODO can add more information
            
        }

        
    }
}
