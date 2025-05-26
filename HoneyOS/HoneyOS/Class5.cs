using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HoneyOS
{
    public enum PageAllocationStrategy
    {
        FirstFit,
        BestFit
    }

    public class PagedMemoryManager
    {
        public const int PageSize = 4;
        public const int TotalPages = 8;

        private bool[] pageTable;
        private Dictionary<int, List<int>> processPages;
        private PageAllocationStrategy strategy;

        public PagedMemoryManager(PageAllocationStrategy strategy = PageAllocationStrategy.FirstFit)
        {
            this.strategy = strategy;
            pageTable = new bool[TotalPages];
            processPages = new Dictionary<int, List<int>>();
        }

        public bool AllocateMemory(int processId, int memorySize, out List<int> allocatedPages)
        {
            allocatedPages = new List<int>();
            int pagesNeeded = (int)Math.Ceiling((double)memorySize / PageSize);

            List<int> freeIndices = GetFreePages(strategy);

            if (freeIndices.Count < pagesNeeded)
                return false;

            allocatedPages = freeIndices.Take(pagesNeeded).ToList();
            foreach (var index in allocatedPages)
                pageTable[index] = true;

            processPages[processId] = allocatedPages;
            return true;
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

        private List<int> GetFreePages(PageAllocationStrategy strategy)
        {
            var indices = new List<int>();

            switch (strategy)
            {
                case PageAllocationStrategy.FirstFit:
                    for (int i = 0; i < pageTable.Length; i++)
                        if (!pageTable[i]) indices.Add(i);
                    break;

                case PageAllocationStrategy.BestFit:
                    // For simple paging, BestFit is same as FirstFit (no size variance)
                    goto case PageAllocationStrategy.FirstFit;
            }

            return indices;
        }
    }
}
