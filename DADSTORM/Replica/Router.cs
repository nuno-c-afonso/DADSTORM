using System.Collections.Generic;

namespace Replica {
    public interface Router {
        void sendToNext(List<string[]> tuples);
    }
}