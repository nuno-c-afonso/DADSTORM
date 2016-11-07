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

        public override List<string[]> Operate(string[] tuple) { 
            string[] fileLocation = Directory.GetFiles(Directory.GetCurrentDirectory(), dllName, System.IO.SearchOption.AllDirectories);
            Assembly library = Assembly.LoadFile(fileLocation[0]);
            List<string[]> convertedResult = null;
            Console.WriteLine("dllName:|{0}|, className:|{1}|, methodName|{2}|", dllName, className, methodName);
            // To search for the desired class
            foreach (Type type in library.GetTypes()) {
                if (type.IsClass == true) {
                    Console.WriteLine("type.IsClass == true type.fullname{0}", type.FullName);
                    if (type.FullName.EndsWith("." + className)) {
                        // create an instance of the object
                        object o = Activator.CreateInstance(type);

                        // Dynamically Invoke the method
                        List<string> l = new List<string>(tuple);
                        object[] args = new object[] { l };

                        object resultObject = type.InvokeMember(methodName,
                          BindingFlags.Default | BindingFlags.InvokeMethod, null, o, args);

                        IList<IList<string>> result = (IList<IList<string>>) resultObject;

                        foreach (IList<string> t in result) {
                            string[] tupleArray = new string[t.Count];
                            for (int i = 0; i < t.Count; i++)
                                tupleArray[i] = t[i];
                            convertedResult.Add(tupleArray);
                        }

                        return convertedResult;
                    }
                }
            }

            throw new CouldNotInvokeMethodException();
        }
    }
}
