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
            Assembly library = Assembly.LoadFile(Directory.GetCurrentDirectory() + @"\" + dllName);
            List<string[]> convertedResult = null;

            // To search for the desired class
            foreach (Type type in library.GetTypes()) {
                if (type.IsClass == true) {
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
