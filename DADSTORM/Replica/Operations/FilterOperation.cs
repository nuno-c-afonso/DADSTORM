using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica
{
    public class FilterOperation : Operation
    {
        int fieldNumber;
        string condition;
        string value;
        delegate bool checkcondition(string tupValue, string value);
        checkcondition conditionCheck;

        public FilterOperation(string field_number,string condition,string value) {
            fieldNumber = int.Parse(field_number);
            this.value = value;
            if (condition.Equals("<"))
                conditionCheck = new checkcondition(lessThan);
            if (condition.Equals(">"))
                conditionCheck = new checkcondition(moreThan);
            if (condition.Equals("="))
                conditionCheck = new checkcondition(equal);
        }

        public static bool lessThan(string tupValue ,string  value ) {
            if (tupValue.CompareTo(value) < 0)
                return true;
            else
                return false;
        }

        public static bool moreThan(string tupValue, string value){
            if (tupValue.CompareTo(value) > 0)
                return true;
            else
                return false;
        }

        public static bool equal(string tupValue, string value){
            if (tupValue.CompareTo(value) == 0)
                return true;
            else
                return false;
        }
        public override List<string[]> Operate(string[] tuple){
            if (conditionCheck(tuple[fieldNumber], value)) {
                List<string[]> l = new List<string[]>();
                l.Add(tuple);
                return l;
            }
            else
                return null;
        }
    }
}
