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
        public string PreviousRouting { get; private set; }
        public List<string> Addresses { get; private set; }
        public List<string> SpecificParameters { get; private set; }
        public string OperatorType { get; private set; }

        public List<string> OutputAddresses { get; set; }


        public OperatorBuilder(List<string> words) {
            Input = new List<string>();
            Addresses = new List<string>();
            SpecificParameters = new List<string>();
            OutputAddresses = new List<string>();

            int index;

            Name = words[0].ToLower();

            for (index = 3; !words[index].ToLower().Equals("rep"); index++)
                Input.Add(words[index]);

            // Update the index to the replication factor position
            index += 2;
            int repFactor;
            int.TryParse(words[index], out repFactor);
            RepFactor = repFactor;

            // Update the index to the routing position
            index += 2;
            PreviousRouting = words[index].ToLower();

            // Update the index to the addresses position
            index += 2;
            for (; !words[index].ToLower().Equals("operator"); index++)
                Addresses.Add(words[index]);

            // Update the index to the operator position
            index += 2;
            OperatorType = words[index++].ToUpper();
            for (; index < words.Count; index++)
                SpecificParameters.Add(words[index]);
        }

        
    }
}
