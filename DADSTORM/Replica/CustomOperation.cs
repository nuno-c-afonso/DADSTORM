using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace Replica {
    public class CustomOperation : Operation {
        private string dllName;
        private string className;
        private string methodName;

        public CustomOperation(string dllName, string className, string methodName) {
            this.dllName = dllName;
            this.className = className;
            this.methodName = methodName;
        }

        public override string[] Operate(string[] tuple) {
            
            try {
                Assembly library = Assembly.LoadFile(Directory.GetCurrentDirectory() + @"\" + dllName);
                Object o = library.CreateInstance(className);
                Type[] argsTypes = { typeof(List<string>) };

                MethodInfo mi = o.GetType().GetMethod(methodName, argsTypes);

            } catch(System.IO.FileNotFoundException e) {
                Console.WriteLine("The .dll file was not found!");
            }


            return tuple;
        }
    }
}
