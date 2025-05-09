using System;
using System.Collections.Generic;
using System.Linq;

namespace HoneyOS
{
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
        public MemoryMode CurrentMemoryMode { get; set; } = MemoryMode.Contiguous;
        public DefragPolicy DefragmentationPolicy { get; set; } = DefragPolicy.OnDemand;

        public TaskManager()
        {
            readyQueue = new List<ProcessControlBlock>();
            jobQueue = new List<ProcessControlBlock>();
            currentTime = 0;
            taskStatus = taskStatus.PAUSE;
            memoryManager = new MemoryManager();
            pagedMemoryManager = new PagedMemoryManager();
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
                random.Next(1, 10),
                random.Next(1, 10),
                random.Next(1, 10),
                random.Next(2, 8),
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
                        if (memoryManager.AllocateMemory(pcb.memorySize, out MemorySegment segment))
                        {
                            pcb.Segment = segment;
                            allocated = true;
                        }
                        else if (DefragmentationPolicy == DefragPolicy.OnDemand && memoryManager.NeedsDefragmentation())
                        {
                            DefragmentMemory();
                            if (memoryManager.AllocateMemory(pcb.memorySize, out segment))
                            {
                                pcb.Segment = segment;
                                allocated = true;
                            }
                        }
                    }
                    else
                    {
                        if (pagedMemoryManager.AllocateMemory(pcb.pID, pcb.memorySize, out List<int> pages))
                        {
                            pcb.PageTable = pages;
                            allocated = true;
                        }
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

        public void DefragmentMemory()
        {
            var processes = readyQueue.ToList();

            foreach (var pcb in processes)
            {
                memoryManager.DeallocateMemory(pcb.Segment);
                pcb.Segment = null;
            }

            memoryManager.DefragmentMemory();

            foreach (var pcb in processes)
            {
                if (memoryManager.AllocateMemory(pcb.memorySize, out MemorySegment segment))
                {
                    pcb.Segment = segment;
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
            AdmitJobQueue();

            switch (schedulingAlgorithm)
            {
                case algo.FIFO:
                    FIFO fifo = new FIFO();
                    int index = fifo.GetEarliest(readyQueue, currentTime);
                    if (index != -1)
                    {
                        ProcessControlBlock currentProcess = readyQueue[index];
                        readyQueue[index] = fifo.Run(currentProcess);
                        if (readyQueue[index].state == status.TERMINATED)
                        {
                            if (CurrentMemoryMode == MemoryMode.Contiguous)
                                memoryManager.DeallocateMemory(readyQueue[index].Segment);
                            else
                                pagedMemoryManager.DeallocateMemory(readyQueue[index].pID);
                            readyQueue.RemoveAt(index);
                        }
                    }
                    break;
                case algo.SJF:
                    SJF sjf = new SJF();
                    index = sjf.GetShortest(readyQueue, currentTime);
                    if (index != -1)
                    {
                        readyQueue[index] = sjf.Run(index, ref readyQueue);
                        if (readyQueue[index].state == status.TERMINATED)
                        {
                            if (CurrentMemoryMode == MemoryMode.Contiguous)
                                memoryManager.DeallocateMemory(readyQueue[index].Segment);
                            else
                                pagedMemoryManager.DeallocateMemory(readyQueue[index].pID);
                            readyQueue.RemoveAt(index);
                        }
                    }
                    break;
                case algo.PRIO:
                    PRIO prio = new PRIO();
                    index = prio.PrioritizeProcess(readyQueue, currentTime);
                    if (index != -1)
                    {
                        readyQueue[index] = prio.Run(index, ref readyQueue);
                        if (readyQueue[index].state == status.TERMINATED)
                        {
                            if (CurrentMemoryMode == MemoryMode.Contiguous)
                                memoryManager.DeallocateMemory(readyQueue[index].Segment);
                            else
                                pagedMemoryManager.DeallocateMemory(readyQueue[index].pID);
                            readyQueue.RemoveAt(index);
                        }
                    }
                    break;
                case algo.RRR:
                    RRR rr = new RRR(4);
                    if (rr.ifTimeToQuantum(currentTime))
                    {
                        index = rr.GetEarliest(readyQueue, currentTime);
                        ProcessControlBlock process = new ProcessControlBlock(
                            readyQueue[index].pID,
                            readyQueue[index].burstTime,
                            currentTime,
                            readyQueue[index].priority,
                            readyQueue[index].memorySize,
                            status.READY
                        );
                        if (CurrentMemoryMode == MemoryMode.Contiguous)
                            memoryManager.DeallocateMemory(readyQueue[index].Segment);
                        else
                            pagedMemoryManager.DeallocateMemory(readyQueue[index].pID);
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
                        readyQueue.Add(process);
                    }
                    index = rr.GetEarliest(readyQueue, currentTime);
                    if (index != -1)
                    {
                        ProcessControlBlock currentProcess = readyQueue[index];
                        readyQueue[index] = rr.Run(currentProcess);
                        if (readyQueue[index].state == status.TERMINATED)
                        {
                            if (CurrentMemoryMode == MemoryMode.Contiguous)
                                memoryManager.DeallocateMemory(readyQueue[index].Segment);
                            else
                                pagedMemoryManager.DeallocateMemory(readyQueue[index].pID);
                            readyQueue.RemoveAt(index);
                        }
                    }
                    break;
            }
        }
    }

    public enum taskStatus { PLAY, PAUSE, STOP }
    public enum MemoryMode { Contiguous, Paged }
    public enum DefragPolicy { OnDemand, Periodic, Never }
}