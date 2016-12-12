using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public class FilterOperation : Operation {
        int fieldNumber;
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
            Console.Write(" ____{0} < {1} =", tupValue, value);
            if (tupValue.CompareTo(value) < 0){
                Console.WriteLine("true ____");
                return true;
            }
            else{
                Console.WriteLine("false ____");
                return false;
            }
        }

        public static bool moreThan(string tupValue, string value){
            Console.Write(" ____{0} > {1} =", tupValue, value);
            if (tupValue.CompareTo(value) > 0)
            {
                Console.WriteLine("true ____");
                return true;
            }
            else
            {
                Console.WriteLine("false ____");
                return false;
            }
        }

        public static bool equal(string tupValue, string value){
            Console.Write(" ____{0} == {1} =", tupValue, value);
            if (tupValue.Equals(value))
            {
                Console.WriteLine("true ____");
                return true;
            }
            else
            {
                Console.WriteLine("false ____");
                return false;
            }
        }
        public override List<string[]> Operate(string[] tuple){

            Console.WriteLine("ON FILTER OPERATION:");
            if (tuple != null)
                foreach (var a in tuple)
                    Console.WriteLine("   " + a);

            Console.WriteLine("field number -1 = "+(fieldNumber - 1));

            if (conditionCheck(tuple[fieldNumber - 1], value)) {
                Console.WriteLine("condition was checked" + value);

                List<string[]> l = new List<string[]>();
                l.Add(tuple);
                return l;
            }
            else
                return null;
        }
    }
}
