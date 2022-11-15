using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DAW.Tasks
{
    public class JobPool
    {
        JobHandler mJobHandler;
        int mPriority;
        internal int TotalJobs;
        internal int CompletedJobs;
        internal int ProcessingJobs;

        ManualResetEvent mManualResentEvent = new ManualResetEvent(true);

        public JobPool(JobHandler jobHandler, int priority = 0)
        {
            mJobHandler = jobHandler;
            mPriority = priority;
        }

        public void AddJob(Action job)
        {
            JobWrapper jw = new JobWrapper(job, this, mPriority);

            lock (this)
            {
                TotalJobs++;
                mManualResentEvent.Reset();
            }

            mJobHandler.AddJob(jw);
        }

        public void FinishJobs()
        {
            while (true)
            {
                if (CompletedJobs == TotalJobs)
                    break;

                if (!mJobHandler.DoOneJob())
                    mManualResentEvent.WaitOne();
            }
        }

        internal void OnJobCompleted()
        {
            lock (this)
            {
                CompletedJobs++;
                ProcessingJobs--;
            }

            if (CompletedJobs == TotalJobs)
                mManualResentEvent.Set();
        }

        internal void OnJobStarted()
        {
            lock (this)
            {
                ProcessingJobs++;
            }
        }
    }
}
