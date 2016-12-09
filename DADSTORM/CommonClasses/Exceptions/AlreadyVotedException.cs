using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses {
    [Serializable]
    public class AlreadyVotedException : ApplicationException {
        public AlreadyVotedException() { }

        public AlreadyVotedException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
		: base(info, context) { }
    }
}
