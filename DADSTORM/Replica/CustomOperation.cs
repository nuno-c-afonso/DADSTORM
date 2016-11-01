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

        public CustomOperation(List<string> replicasURL, int myselfIndex,
            string dllName, string className, string methodName) : base(replicasURL, myselfIndex) {
            this.dllName = dllName;
            this.className = className;
            this.methodName = methodName;
        }

        // Remember: The class name should be the complete one, which has the namespace
        public override List<string[]> Operate(string[] tuple) {
            Assembly library = Assembly.LoadFile(Directory.GetCurrentDirectory() + @"\" + dllName);
            Object o = library.CreateInstance(className);
            if (o != null) {
                Type[] argsTypes = { typeof(List<string>) };

                MethodInfo mi = o.GetType().GetMethod(methodName, argsTypes);
                if (mi != null)
                    return (List<string[]>) mi.Invoke(o, new Object[] { new List<string>(tuple) });

                else
                    throw new MethodNotFoundException();
            }

            else
                throw new ClassNotFoundException();
        }
    }
}
