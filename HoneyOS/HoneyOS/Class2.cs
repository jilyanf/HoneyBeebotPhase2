using System;
using System.Collections.Generic;
using System.Linq;

namespace HoneyOS
{
    public class Scheduler
    {
        protected List<ProcessControlBlock> pcb_list;

        public Scheduler()
        {
            pcb_list = new List<ProcessControlBlock>();
        }

        public void AddPCB(ProcessControlBlock pcb) => this.pcb_list.Add(pcb);
        public void RemovePCB(ProcessControlBlock pcb) => this.pcb_list.Remove(pcb);

        public ProcessControlBlock Run(ProcessControlBlock process)
        {
            process.state = status.RUNNING;
            process.burstTime--;
            if (process.burstTime < 1)
            {
                process.state = status.TERMINATED;
                process.PrintPCB();
            }
            return process;
        }
    }

    public class FIFO : Scheduler
    {
        public FIFO() : base() { }

        public int GetEarliest(List<ProcessControlBlock> processes, int currentTime)
        {
            int index = -1;
            for (int i = 0; i < processes.Count; i++)
            {
                if (processes[i].arrivalTime <= currentTime)
                {
                    if (index == -1 || processes[i].arrivalTime < processes[index].arrivalTime)
                        index = i;
                }
            }
            return index;
        }
    }

    public class SJF : Scheduler
    {
        public SJF() : base() { }

        public int GetShortest(List<ProcessControlBlock> processes, int currentTime)
        {
            int index = -1;
            for (int i = 0; i < processes.Count; i++)
            {
                if (processes[i].arrivalTime <= currentTime)
                {
                    if (index == -1 || processes[i].burstTime < processes[index].burstTime)
                        index = i;
                }
            }
            return index;
        }

        public ProcessControlBlock Run(int index, ref List<ProcessControlBlock> q)
        {
            for (int i = 0; i < q.Count; i++)
            {
                if (q[i].state == status.RUNNING && q[i].pID != q[index].pID)
                    q[i].state = status.READY;
            }

            q[index].state = status.RUNNING;
            q[index].burstTime--;
            if (q[index].burstTime < 1)
            {
                q[index].state = status.TERMINATED;
                q[index].PrintPCB();
            }
            return q[index];
        }
    }

    public class PRIO : Scheduler
    {
        public PRIO() : base() { }

        public int PrioritizeProcess(List<ProcessControlBlock> processes, int currentTime)
        {
            int index = -1;
            for (int i = 0; i < processes.Count; i++)
            {
                if (processes[i].arrivalTime <= currentTime)
                {
                    if (index == -1 || processes[i].priority > processes[index].priority)
                        index = i;
                }
            }
            return index;
        }

        public ProcessControlBlock Run(int index, ref List<ProcessControlBlock> q)
        {
            for (int i = 0; i < q.Count; i++)
            {
                if (q[i].state == status.RUNNING && q[i].pID != q[index].pID)
                    q[i].state = status.READY;
            }

            q[index].state = status.RUNNING;
            q[index].burstTime--;
            if (q[index].burstTime < 1)
            {
                q[index].state = status.TERMINATED;
                q[index].PrintPCB();
            }
            return q[index];
        }
    }

    public class RRR : Scheduler
    {
        private int timeSlice;
        public RRR(int timeSlice) : base() => this.timeSlice = timeSlice;

        public bool ifTimeToQuantum(int currentTime) => currentTime != 0 && currentTime % timeSlice == 0;

        public int GetEarliest(List<ProcessControlBlock> processes, int currentTime)
        {
            int index = -1;
            for (int i = 0; i < processes.Count; i++)
            {
                if (processes[i].arrivalTime <= currentTime)
                {
                    if (index == -1 || processes[i].arrivalTime < processes[index].arrivalTime)
                        index = i;
                }
            }
            return index;
        }
    }

    public enum algo
    {
        SJF, FIFO, PRIO, RRR
    }
}