using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HoneyOS
{
    public enum PageAllocationStrategy
    {
        FirstFit,
        BestFit,
        WorstFit
    }

    public enum PageReplacementStrategy
    {
        FIFO,
        LRU,
        LFU,
        Random
    }

    public class PagedMemoryManager
    {
        public const int PageSize = 4;
        public const int TotalPages = 8;

        private bool[] pageTable;
        private Dictionary<int, List<int>> processPages;
        private PageAllocationStrategy strategy;
        private PageReplacementStrategy replacementStrategy;

        // For replacement tracking
        private Dictionary<int, Queue<int>> processPageQueue; // For FIFO
        private Dictionary<int, Dictionary<int, int>> pageAccessCounts; // For LFU
        private Dictionary<int, Dictionary<int, DateTime>> pageLastAccessTimes; // For LRU

        // Monitoring statistics
        private int totalAllocations;
        private int totalDeallocations;
        private int totalPageFaults;
        private int totalPagesReplaced;
        private List<int> replacedPageIndices; // New: Tracks which pages were replaced

        public PagedMemoryManager(PageAllocationStrategy strategy = PageAllocationStrategy.FirstFit,
                                PageReplacementStrategy replacementStrategy = PageReplacementStrategy.FIFO)
        {
            this.strategy = strategy;
            this.replacementStrategy = replacementStrategy;
            pageTable = new bool[TotalPages];
            processPages = new Dictionary<int, List<int>>();

            // Initialize replacement tracking structures
            processPageQueue = new Dictionary<int, Queue<int>>();
            pageAccessCounts = new Dictionary<int, Dictionary<int, int>>();
            pageLastAccessTimes = new Dictionary<int, Dictionary<int, DateTime>>();

            // Initialize monitoring stats
            totalAllocations = 0;
            totalDeallocations = 0;
            totalPageFaults = 0;
            totalPagesReplaced = 0;
            replacedPageIndices = new List<int>();
        }

        public bool AllocateMemory(int processId, int memorySize, out List<int> allocatedPages)
        {
            allocatedPages = new List<int>();
            int pagesNeeded = (int)Math.Ceiling((double)memorySize / PageSize);

            List<int> freeIndices = GetFreePages(strategy, pagesNeeded);

            if (freeIndices.Count < pagesNeeded)
            {
                totalPageFaults++;

                if (!TryFreePagesUsingReplacement(processId, pagesNeeded - freeIndices.Count))
                    return false;

                freeIndices = GetFreePages(strategy, pagesNeeded);
                if (freeIndices.Count < pagesNeeded)
                    return false;
            }

            allocatedPages = freeIndices.Take(pagesNeeded).ToList();
            foreach (var index in allocatedPages)
                pageTable[index] = true;

            processPages[processId] = allocatedPages;

            InitializeReplacementTracking(processId, allocatedPages);

            totalAllocations++;

            return true;
        }

        public void DeallocateMemory(int processId)
        {
            if (processPages.TryGetValue(processId, out var pages))
            {
                foreach (var page in pages)
                    pageTable[page] = false;
                processPages.Remove(processId);

                processPageQueue.Remove(processId);
                pageAccessCounts.Remove(processId);
                pageLastAccessTimes.Remove(processId);

                totalDeallocations++;
            }
        }

        public void AccessPage(int processId, int pageIndex)
        {
            if (processPages.TryGetValue(processId, out var pages) && pages.Contains(pageIndex))
            {
                switch (replacementStrategy)
                {
                    case PageReplacementStrategy.LRU:
                        pageLastAccessTimes[processId][pageIndex] = DateTime.Now;
                        break;
                    case PageReplacementStrategy.LFU:
                        pageAccessCounts[processId][pageIndex]++;
                        break;
                    case PageReplacementStrategy.FIFO:
                        break;
                }
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

        private List<int> GetFreePages(PageAllocationStrategy strategy, int pagesNeeded)
        {
            var indices = new List<int>();

            switch (strategy)
            {
                case PageAllocationStrategy.FirstFit:
                    for (int i = 0; i < pageTable.Length; i++)
                        if (!pageTable[i]) indices.Add(i);
                    break;

                case PageAllocationStrategy.BestFit:
                    goto case PageAllocationStrategy.FirstFit;

                case PageAllocationStrategy.WorstFit:
                    goto case PageAllocationStrategy.FirstFit;
            }

            return indices;
        }

        private void InitializeReplacementTracking(int processId, List<int> pages)
        {
            processPageQueue[processId] = new Queue<int>(pages);

            pageAccessCounts[processId] = new Dictionary<int, int>();
            foreach (var page in pages)
                pageAccessCounts[processId][page] = 0;

            pageLastAccessTimes[processId] = new Dictionary<int, DateTime>();
            foreach (var page in pages)
                pageLastAccessTimes[processId][page] = DateTime.Now;
        }

        private bool TryFreePagesUsingReplacement(int requestingProcessId, int pagesNeeded)
        {
            foreach (var process in processPages)
            {
                if (process.Key == requestingProcessId) continue;

                if (process.Value.Count >= pagesNeeded)
                {
                    List<int> pagesToFree = SelectPagesForReplacement(process.Key, pagesNeeded);

                    foreach (var page in pagesToFree)
                    {
                        pageTable[page] = false;
                        processPages[process.Key].Remove(page);

                        if (processPageQueue.ContainsKey(process.Key))
                            processPageQueue[process.Key] = new Queue<int>(processPageQueue[process.Key].Where(x => x != page));
                        if (pageAccessCounts.ContainsKey(process.Key) && pageAccessCounts[process.Key].ContainsKey(page))
                            pageAccessCounts[process.Key].Remove(page);
                        if (pageLastAccessTimes.ContainsKey(process.Key) && pageLastAccessTimes[process.Key].ContainsKey(page))
                            pageLastAccessTimes[process.Key].Remove(page);

                        replacedPageIndices.Add(page); // Log replaced page index
                    }

                    totalPagesReplaced += pagesToFree.Count;

                    return true;
                }
            }

            return false;
        }

        private List<int> SelectPagesForReplacement(int processId, int count)
        {
            var pages = processPages[processId];
            if (pages.Count < count) return new List<int>();

            switch (replacementStrategy)
            {
                case PageReplacementStrategy.FIFO:
                    return processPageQueue[processId].Take(count).ToList();

                case PageReplacementStrategy.LRU:
                    return pageLastAccessTimes[processId]
                        .OrderBy(x => x.Value)
                        .Take(count)
                        .Select(x => x.Key)
                        .ToList();

                case PageReplacementStrategy.LFU:
                    return pageAccessCounts[processId]
                        .OrderBy(x => x.Value)
                        .Take(count)
                        .Select(x => x.Key)
                        .ToList();

                case PageReplacementStrategy.Random:
                    var rnd = new Random();
                    return pages.OrderBy(x => rnd.Next()).Take(count).ToList();

                default:
                    return pages.Take(count).ToList();
            }
        }

        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                { "TotalAllocations", totalAllocations },
                { "TotalDeallocations", totalDeallocations },
                { "TotalPageFaults", totalPageFaults },
                { "TotalPagesReplaced", totalPagesReplaced },
                { "FreePagesRemaining", GetFreePageCount() },
                { "ReplacedPageIndices", new List<int>(replacedPageIndices) }
            };
        }
    }
}
