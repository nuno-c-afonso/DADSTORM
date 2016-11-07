using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses {
    public class OperatorsInfo {
        private List<string> operatorNames;
        private Dictionary<string, string> whereToSend; // OP1 -> OP2 cause 'OP2 input ops OP1'
        private Dictionary<string, OperatorBuilder> operatorNameToOperatorBuilderDictionary;

        public List<string> OperatorNames
        {
            get
            {
                return operatorNames;
            }
        }

        public OperatorsInfo() {
            operatorNames = new List<string>();
            whereToSend = new Dictionary<string, string>();
            operatorNameToOperatorBuilderDictionary = new Dictionary<string, OperatorBuilder>();
        }

        public bool isOperator(string opName) {
            return operatorNames.Contains(opName);
        }

        public void addNewOP(OperatorBuilder opb)
        {
            operatorNames.Add(opb.Name.ToLower());

            foreach(string s in opb.Input) {
                string sLower = s.ToLower();
                if (!whereToSend.ContainsKey(sLower))
                    whereToSend.Add(sLower, opb.Name.ToLower());
            }

            if (!operatorNameToOperatorBuilderDictionary.ContainsKey(opb.Name.ToLower()))
                operatorNameToOperatorBuilderDictionary.Add(opb.Name.ToLower(), opb);
        }

        public List<string> getOuputAddressesListOfOP(string opName) {
            OperatorBuilder nextOpBuilder = getNextOpInfo(opName.ToLower());
            return nextOpBuilder.Addresses;
        }

        public List<string> getInputAddressesListOfOP(string opName)
        {
            OperatorBuilder OpBuilder = getOpInfo(opName.ToLower());
            return OpBuilder.Input;
        }

        public string getMyRouting(string opName) {
            OperatorBuilder nextOpBuilder = getNextOpInfo(opName.ToLower());
            return nextOpBuilder.PreviousRouting;
        }

        public OperatorBuilder getNextOpInfo(string opName) {
            OperatorBuilder nextOpBuilder = null;

            string nextOP;
            if (whereToSend.TryGetValue(opName.ToLower(), out nextOP)) {
                nextOpBuilder = operatorNameToOperatorBuilderDictionary[nextOP]; // need to check 
            }
            else
                throw new LastOperatorException();

            return nextOpBuilder;
        }

        public OperatorBuilder getOpInfo(string opName) {
            OperatorBuilder nextOpBuilder = null;
            if (operatorNameToOperatorBuilderDictionary.TryGetValue(opName.ToLower(), out nextOpBuilder))
                return nextOpBuilder;
            return null;
        }

        public string getFirstOperator() {
            foreach (string op in operatorNames) {
                bool isFirst = true;

                foreach (KeyValuePair<string, string> entry in whereToSend) {
                    if (op.Equals(entry.Value) && operatorNames.Contains(entry.Key)) {
                        isFirst = false;
                        break;
                    }
                }

                if (isFirst)
                    return op;
            }

            return null;
        }

    }
}
