using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DAW.Tasks
{
    public class JobHandler
    {
        public Action<Exception> JobExceptionHandler;

        int mMaxConcurrentJobs;
        TimeSpan mMinTimeBetweenJobs;

        bool mIsStopped = false;
        public readonly object LOCK = new object();

        SortedDictionary<int, List<JobWrapper>> mJobQueues = new SortedDictionary<int, List<JobWrapper>>();
        List<JobWrapper> mProcessingJobs = new List<JobWrapper>();
        Dictionary<string, string> mSyncKeysInUse = new Dictionary<string, string>();

        public bool IsProcessing => mProcessingJobs.Count > 0;

        Timer mTimer;

        DateTime mLastJobEnded;
        ManualResetEvent mManualResentEvent = new ManualResetEvent(true);

        public JobHandler(int maxConcurrentJobs, int minSecondsBetweenJobs = 0)
        {
            mMaxConcurrentJobs = maxConcurrentJobs;
            mMinTimeBetweenJobs = TimeSpan.FromSeconds(minSecondsBetweenJobs);
            mTimer = new Timer(delegate (object state)
            {
                ProcessNextJob();
            });
        }

        public void Stop()
        {
            mIsStopped = true;
        }

        public void Start()
        {
            mIsStopped = false;
            ProcessNextJob();
        }

        public void SetMaxConcurrentJobs(int maxConcurrentJobs)
        {
            mMaxConcurrentJobs = maxConcurrentJobs;
        }

        public void FinishJobs()
        {
            mManualResentEvent.WaitOne();
        }

        public bool DoJobs()
        {
            return ProcessNextJob(true);
        }

        // Returns true if a job was done
        public bool DoOneJob()
        {
            return ProcessNextJob(true);
        }

        public void AddJob(Action job, int priority = 0, bool repeat = false)
        {
            AddJob(null, job, priority, repeat);
        }

        public void AddJob(string key, Action job, int priority = 0, bool repeat = false)
        {
            AddJob(new JobWrapper(key, job, repeat, -priority));
        }

        internal void AddJob(JobWrapper jw)
        {
            lock (LOCK)
            {
                mManualResentEvent.Reset();
                AddJobWrapper(jw);
            }

            // Only start jobs asynchronously
            ProcessNextJob(false);
        }

        void AddJobWrapper(JobWrapper jw)
        {
            List<JobWrapper> jobQueue;
            if (!mJobQueues.TryGetValue(jw.Priority, out jobQueue))
                mJobQueues.Add(jw.Priority, jobQueue = new List<JobWrapper>());
            jobQueue.Add(jw);
        }

        bool ProcessNextJob(bool? synchronous = false, bool onlyOneJob = false)
        {
            JobWrapper nextJob;
            bool result = false;

            while (true)
            {
                nextJob = GetNextJobToProcess(synchronous == true);

                if (nextJob == null)
                    break;

                result = true;

                if (synchronous == false || (synchronous == true && HasMoreJobs()))
                {
                    JobWrapper job = nextJob;
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        DoJob(job);
                        OnJobCompleted(job);
                        ProcessNextJob();
                    });
                }
                else
                {
                    DoJob(nextJob);
                    OnJobCompleted(nextJob);
                    if (onlyOneJob)
                        break;
                }
            }
            return result;
        }

        void DoJob(JobWrapper job)
        {
            try
            {
                job.Action();
            }
            catch (Exception ex)
            {
                try
                {
                    JobExceptionHandler?.Invoke(ex);
                }
                catch { }
            }
        }

        bool SetTimer()
        {
            TimeSpan timeSinceLastJob = DateTime.UtcNow.Subtract(mLastJobEnded);
            TimeSpan timeUntilNextJob = mMinTimeBetweenJobs - timeSinceLastJob;

            if (timeUntilNextJob.Ticks > 0)
            {
                mTimer.Change((int)timeUntilNextJob.TotalMinutes + 1, Timeout.Infinite);
                return true;
            }

            return false;
        }

        JobWrapper GetNextJobToProcess(bool disregardMaxConcurrentJobs = false)
        {
            JobWrapper result = null;

            lock (LOCK)
            {
                if (mJobQueues.Count == 0 ||
                mIsStopped ||
                (!disregardMaxConcurrentJobs && mMaxConcurrentJobs > 0 && mProcessingJobs.Count >= mMaxConcurrentJobs))
                {
                    // No job or processing has been stopped
                    // Max concurrent jobs ceiling has been hit
                }
                else if (mMinTimeBetweenJobs.Ticks > 0 && SetTimer())
                {
                    // Timer is set
                }
                else
                {
                    int? toRemove = null;

                    foreach (var kvp in mJobQueues)
                    {
                        List<JobWrapper> jobQueue = kvp.Value;

                        int index = jobQueue.FindIndex(jw =>
                            (string.IsNullOrEmpty(jw.SyncKey) || !mSyncKeysInUse.ContainsKey(jw.SyncKey)));

                        if (index > -1)
                        {
                            result = jobQueue[index];

                            jobQueue.RemoveAt(index);
                            if (jobQueue.Count == 0)
                                toRemove = kvp.Key;

                            mProcessingJobs.Add(result);
                            if (result.JobPool != null)
                            {

                            }

                            if (!string.IsNullOrEmpty(result.SyncKey))
                                mSyncKeysInUse.Add(result.SyncKey, result.SyncKey);

                            break;
                        }
                    }
                    if (toRemove.HasValue)
                    {
                        mJobQueues.Remove(toRemove.Value);
                    }
                }
            }

            return result;
        }

        bool HasMoreJobs()
        {
            bool result = false;

            lock (LOCK)
            {
                if (mJobQueues.Count == 0 ||
                mIsStopped ||
                (mMaxConcurrentJobs > 0 && mProcessingJobs.Count >= mMaxConcurrentJobs))
                {

                }
                else if (mMinTimeBetweenJobs.Ticks > 0 && SetTimer())
                {

                }
                else
                {
                    foreach (var kvp in mJobQueues)
                    {
                        List<JobWrapper> jobQueue = kvp.Value;

                        int index = jobQueue.FindIndex(jw =>
                            (string.IsNullOrEmpty(jw.SyncKey) || !mSyncKeysInUse.ContainsKey(jw.SyncKey)));

                        if (index > -1)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        void OnJobCompleted(JobWrapper job)
        {
            lock (LOCK)
            {
                mLastJobEnded = DateTime.UtcNow;
                mProcessingJobs.Remove(job);

                if (job.SyncKey != null)
                    mSyncKeysInUse.Remove(job.SyncKey);

                job.JobPool?.OnJobCompleted();

                if (!job.Repeat)
                {
                    if (mJobQueues.Count == 0 && mProcessingJobs.Count == 0)
                        mManualResentEvent.Set();
                }
                else
                {
                    AddJobWrapper(job);
                }
            }
        }
    }
}
