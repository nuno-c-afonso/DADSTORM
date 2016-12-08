using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonClasses;

namespace CommonClasses {
    [Serializable]
    public class DecisionStructure {
        private bool isFinal = false;
        private string url;

        public bool IsFinal {
            get {
                return isFinal;
            }

            set {
                isFinal = value;
            }
        }

        public string URL {
            get {
                return url;
            }

            set {
                url = value;
            }
        }

        public DecisionStructure(string url) {
            this.url = url;
        }
    }
}
