using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HoneyOS
{
    public class PagedMemoryManager
    {
        public const int PageSize = 4;
        public const int TotalPages = 8;

        private bool[] pageTable;
        private Dictionary<int, List<int>> processPages;

        public PagedMemoryManager()
        {
            pageTable = new bool[TotalPages];
            processPages = new Dictionary<int, List<int>>();
        }

        public bool AllocateMemory(int processId, int memorySize, out List<int> allocatedPages)
        {
            allocatedPages = new List<int>();
            int pagesNeeded = (int)Math.Ceiling((double)memorySize / PageSize);

            if (GetFreePageCount() < pagesNeeded) return false;

            for (int i = 0; i < pageTable.Length && allocatedPages.Count < pagesNeeded; i++)
            {
                if (!pageTable[i])
                {
                    pageTable[i] = true;
                    allocatedPages.Add(i);
                }
            }

            if (allocatedPages.Count == pagesNeeded)
            {
                processPages[processId] = allocatedPages;
                return true;
            }

            foreach (var page in allocatedPages)
                pageTable[page] = false;
            return false;
        }

        public void DeallocateMemory(int processId)
        {
            if (processPages.TryGetValue(processId, out var pages))
            {
                foreach (var page in pages)
                    pageTable[page] = false;
                processPages.Remove(processId);
            }
        }

        public int GetFreePageCount() => pageTable.Count(p => !p);

        public string GetMemoryMap()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < pageTable.Length; i++)
            {
                sb.Append(pageTable[i] ? "X " : "_ ");
                if ((i + 1) % 4 == 0) sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}