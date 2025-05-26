using System;
using System.Collections.Generic;

namespace HoneyOS
{
    public class ProcessControlBlock
    {
        public int pID;
        public int burstTime;
        public int arrivalTime;
        public int priority;
        public int memorySize;
        public status state;
        public MemorySegment Segment { get; set; }
        public List<int> PageTable { get; set; }

        public ProcessControlBlock(int pID, int burstTime, int arrivalTime, int priority, int memorySize, status state)
        {
            this.pID = pID;
            this.burstTime = burstTime;
            this.arrivalTime = arrivalTime;
            this.priority = priority;
            this.memorySize = memorySize;
            this.state = (status)state;
            this.PageTable = new List<int>();
        }

        public void PrintPCB()
        {
            Console.WriteLine($"Process ID: {pID}, BT: {burstTime}, AT: {arrivalTime}, Priority: {priority}, Memory Size: {memorySize}, State: {state}");
        }
    }

    public enum status
    {
        NEW, READY, RUNNING, TERMINATED
    }
}