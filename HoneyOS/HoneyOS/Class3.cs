using System;
using System.Collections.Generic;
using System.Linq;

namespace HoneyOS
{
    public enum taskStatus
    {
        PLAY, PAUSE, STOP
    }

    public class TaskManager
    {
        public List<ProcessControlBlock> readyQueue;
        public List<ProcessControlBlock> jobQueue;
        public taskStatus taskStatus;
        private static int nextPID = 0;
        public int currentTime;
        public algo schedulingAlgorithm;
        public MemoryManager memoryManager;
        public PagedMemoryManager pagedMemoryManager;

        // Memory configuration properties
        public MemoryMode CurrentMemoryMode { get; private set; }
        public DefragPolicy DefragmentationPolicy { get; private set; }
        public AllocationStrategy AllocationStrategy { get; private set; }
        public DefragmentationStrategy DefragStrategy { get; private set; }
        public PageAllocationStrategy PageAllocationStrategy { get; private set; }
        public PageReplacementStrategy PageReplacementStrategy { get; private set; }

        private int defragCounter = 0;
        private const int DEFRAG_INTERVAL = 5; // For periodic defragmentation

        public int TotalContextSwitches { get; private set; } = 0;
        public int TotalProcessesCompleted { get; private set; } = 0;
        public Dictionary<algo, int> SchedulingDecisions = new Dictionary<algo, int>();

        public TaskManager(
            MemoryMode memoryMode,
            DefragPolicy defragPolicy,
            AllocationStrategy allocStrategy,
            DefragmentationStrategy defragStrategy,
            PageAllocationStrategy pageAllocStrategy,
            PageReplacementStrategy pageReplStrategy)
        {
            readyQueue = new List<ProcessControlBlock>();
            jobQueue = new List<ProcessControlBlock>();
            currentTime = 0;
            taskStatus = taskStatus.PAUSE;

            // Set configuration
            CurrentMemoryMode = memoryMode;
            DefragmentationPolicy = defragPolicy;
            AllocationStrategy = allocStrategy;
            DefragStrategy = defragStrategy;
            PageAllocationStrategy = pageAllocStrategy;
            PageReplacementStrategy = pageReplStrategy;

            // Initialize appropriate memory manager
            if (CurrentMemoryMode == MemoryMode.Contiguous)
            {
                memoryManager = new MemoryManager(AllocationStrategy, DefragStrategy);
            }
            else
            {
                pagedMemoryManager = new PagedMemoryManager(PageAllocationStrategy, PageReplacementStrategy);
            }

            // Initialize scheduling decision counters
            foreach (algo algorithm in Enum.GetValues(typeof(algo)))
            {
                SchedulingDecisions[algorithm] = 0;
            }
        }

        public Dictionary<string, object> GetStatistics()
        {
            var stats = new Dictionary<string, object>();

            // Memory statistics
            if (CurrentMemoryMode == MemoryMode.Contiguous)
            {
                var memStats = memoryManager.GetStatistics();
                foreach (var kvp in memStats)
                {
                    stats[$"Memory_{kvp.Key}"] = kvp.Value;
                }
            }
            else
            {
                var pageStats = pagedMemoryManager.GetStatistics();
                foreach (var kvp in pageStats)
                {
                    stats[$"Paging_{kvp.Key}"] = kvp.Value;
                }
            }

            // Scheduling statistics
            stats["CurrentTime"] = currentTime;
            stats["ReadyProcesses"] = readyQueue.Count;
            stats["WaitingProcesses"] = jobQueue.Count;
            stats["TotalContextSwitches"] = TotalContextSwitches;
            stats["TotalProcessesCompleted"] = TotalProcessesCompleted;

            // Add scheduling algorithm decisions
            foreach (var kvp in SchedulingDecisions)
            {
                stats[$"Scheduling_{kvp.Key}"] = kvp.Value;
            }

            return stats;
        }

        public void GenerateProcesses(int numProcesses)
        {
            Random random = new Random();
            for (int i = 0; i < numProcesses; i++)
            {
                ProcessControlBlock pcb = CreateProcess(nextPID++, random);
                jobQueue.Add(pcb);
            }
        }

        private ProcessControlBlock CreateProcess(int pID, Random random)
        {
            return new ProcessControlBlock(
                pID,
                random.Next(1, 10),    // Priority
                random.Next(1, 10),    // Burst time
                random.Next(0, 5),     // Arrival time
                random.Next(2, 8),     // Memory size
                status.NEW
            );
        }

        private void AdmitJobQueue()
        {
            jobQueue.Sort((pcb1, pcb2) => pcb1.arrivalTime.CompareTo(pcb2.arrivalTime));

            foreach (var pcb in jobQueue.ToList())
            {
                if (pcb.arrivalTime <= currentTime)
                {
                    bool allocated = false;

                    if (CurrentMemoryMode == MemoryMode.Contiguous)
                    {
                        allocated = TryAllocateContiguous(pcb);

                        // Handle defragmentation if allocation failed
                        if (!allocated && DefragmentationPolicy == DefragPolicy.OnDemand &&
                            memoryManager.NeedsDefragmentation())
                        {
                            DefragmentMemory();
                            allocated = TryAllocateContiguous(pcb);
                        }
                    }
                    else // Paged mode
                    {
                        allocated = TryAllocatePaged(pcb);
                    }

                    if (allocated)
                    {
                        pcb.state = status.READY;
                        readyQueue.Add(pcb);
                        jobQueue.Remove(pcb);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private bool TryAllocateContiguous(ProcessControlBlock pcb)
        {
            if (memoryManager.AllocateMemory(pcb.memorySize, out MemorySegment segment))
            {
                pcb.Segment = segment;
                return true;
            }
            return false;
        }

        private bool TryAllocatePaged(ProcessControlBlock pcb)
        {
            if (pagedMemoryManager.AllocateMemory(pcb.pID, pcb.memorySize, out List<int> pages))
            {
                pcb.PageTable = pages;
                return true;
            }
            return false;
        }

        public void DefragmentMemory()
        {
            if (CurrentMemoryMode != MemoryMode.Contiguous) return;

            var processes = readyQueue.ToList();
            var savedStates = processes.ToDictionary(p => p.pID, p => p.state);

            // Deallocate all memory
            foreach (var pcb in processes)
            {
                memoryManager.DeallocateMemory(pcb.Segment);
                pcb.Segment = null;
                pcb.state = status.READY;
            }

            // Perform defragmentation
            memoryManager.DefragmentMemory();

            // Reallocate memory
            foreach (var pcb in processes)
            {
                if (memoryManager.AllocateMemory(pcb.memorySize, out MemorySegment segment))
                {
                    pcb.Segment = segment;
                    pcb.state = savedStates[pcb.pID];
                }
                else
                {
                    pcb.state = status.NEW;
                    readyQueue.Remove(pcb);
                    jobQueue.Add(pcb);
                }
            }
        }

        public void Execute()
        {
            currentTime++;

            // Check for periodic defragmentation
            if (CurrentMemoryMode == MemoryMode.Contiguous &&
                DefragmentationPolicy == DefragPolicy.Periodic)
            {
                defragCounter++;
                if (defragCounter >= DEFRAG_INTERVAL && memoryManager.NeedsDefragmentation())
                {
                    DefragmentMemory();
                    defragCounter = 0;
                }
            }

            AdmitJobQueue();

            // Process scheduling
            switch (schedulingAlgorithm)
            {
                case algo.FIFO:
                    ExecuteFIFO();
                    break;
                case algo.SJF:
                    ExecuteSJF();
                    break;
                case algo.PRIO:
                    ExecutePRIO();
                    break;
                case algo.RRR:
                    ExecuteRRR();
                    break;
            }
        }

        private void ExecuteFIFO()
        {
            SchedulingDecisions[algo.FIFO]++;
            FIFO fifo = new FIFO();
            int index = fifo.GetEarliest(readyQueue, currentTime);
            if (index != -1)
            {
                TotalContextSwitches++;
                ProcessControlBlock currentProcess = readyQueue[index];
                readyQueue[index] = fifo.Run(currentProcess);
                if (readyQueue[index].state == status.TERMINATED)
                {
                    TotalProcessesCompleted++;
                    HandleProcessTermination(readyQueue[index]);
                    readyQueue.RemoveAt(index);
                }
            }
        }

        private void ExecuteSJF()
        {
            SchedulingDecisions[algo.SJF]++;
            SJF sjf = new SJF();
            int index = sjf.GetShortest(readyQueue, currentTime);
            if (index != -1)
            {
                TotalContextSwitches++;
                readyQueue[index] = sjf.Run(index, ref readyQueue);
                if (readyQueue[index].state == status.TERMINATED)
                {
                    TotalProcessesCompleted++;
                    HandleProcessTermination(readyQueue[index]);
                    readyQueue.RemoveAt(index);
                }
            }
        }

        private void ExecutePRIO()
        {
            SchedulingDecisions[algo.PRIO]++;
            PRIO prio = new PRIO();
            int index = prio.PrioritizeProcess(readyQueue, currentTime);
            if (index != -1)
            {
                TotalContextSwitches++;
                readyQueue[index] = prio.Run(index, ref readyQueue);
                if (readyQueue[index].state == status.TERMINATED)
                {
                    TotalProcessesCompleted++;
                    HandleProcessTermination(readyQueue[index]);
                    readyQueue.RemoveAt(index);
                }
            }
        }

        private void ExecuteRRR()
        {
            SchedulingDecisions[algo.RRR]++;
            RRR rr = new RRR(4);
            if (rr.ifTimeToQuantum(currentTime))
            {
                int index = rr.GetEarliest(readyQueue, currentTime);
                if (index != -1)
                {
                    TotalContextSwitches++;
                    ProcessControlBlock process = new ProcessControlBlock(
                        readyQueue[index].pID,
                        readyQueue[index].burstTime,
                        currentTime,
                        readyQueue[index].priority,
                        readyQueue[index].memorySize,
                        status.READY
                    );

                    TotalProcessesCompleted++;
                    HandleProcessTermination(readyQueue[index]);
                    readyQueue.RemoveAt(index);

                    if (CurrentMemoryMode == MemoryMode.Contiguous)
                    {
                        if (memoryManager.AllocateMemory(process.memorySize, out MemorySegment segment))
                            process.Segment = segment;
                    }
                    else
                    {
                        if (pagedMemoryManager.AllocateMemory(process.pID, process.memorySize, out List<int> pages))
                            process.PageTable = pages;
                    }

                    readyQueue.Add(rr.Run(process));
                }
            }
        }

        private void HandleProcessTermination(ProcessControlBlock process)
        {
            if (CurrentMemoryMode == MemoryMode.Contiguous)
            {
                memoryManager.DeallocateMemory(process.Segment);
            }
            else
            {
                pagedMemoryManager.DeallocateMemory(process.pID);
            }
        }
    }
}