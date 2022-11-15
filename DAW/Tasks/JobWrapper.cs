using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Tasks
{
    class JobWrapper
    {
        public readonly DateTime Created = DateTime.UtcNow;
        public readonly JobPool JobPool;
        /// <summary>
        /// Higher priority for higher Priority
        /// </summary>
        public readonly int Priority;
        public readonly Action Action;
        public readonly string SyncKey;
        public readonly bool Repeat;

        public JobWrapper(string synckey, Action job, bool repeat, int priority)
        {
            SyncKey = synckey;
            Action = job;
            Repeat = repeat;
            Priority = priority;
        }

        public JobWrapper(Action job, JobPool jobPool, int priority)
        {
            Action = job;
            JobPool = jobPool;
            Priority = priority;
        }
    }
}
