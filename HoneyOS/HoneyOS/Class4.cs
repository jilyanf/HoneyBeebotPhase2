using System;
using System.Collections.Generic;
using System.Linq;

namespace HoneyOS
{
    public enum AllocationStrategy
    {
        FirstFit,
        BestFit,
        WorstFit
    }

    public enum DefragmentationStrategy
    {
        SimpleMerge,
        MoveBlocksToStart
    }

    public enum MemoryMode
    {
        Contiguous,
        Paged
    }

    public enum DefragPolicy
    {
        OnDemand,
        Periodic,
        Never
    }

    public class MemoryManager
    {
        protected const int TotalMemory = 32;
        protected int availableMemory;
        public List<MemorySegment> freeSegments;
        private AllocationStrategy allocationStrategy;
        private DefragmentationStrategy defragStrategy;

        public MemoryManager(AllocationStrategy strategy = AllocationStrategy.FirstFit,
                             DefragmentationStrategy defrag = DefragmentationStrategy.SimpleMerge)
        {
            allocationStrategy = strategy;
            defragStrategy = defrag;
            availableMemory = TotalMemory;
            freeSegments = new List<MemorySegment> { new MemorySegment(0, TotalMemory) };
        }

        public bool AllocateMemory(int memorySize, out MemorySegment allocatedSegment)
        {
            allocatedSegment = null;
            MemorySegment segment = null;

            switch (allocationStrategy)
            {
                case AllocationStrategy.FirstFit:
                    segment = freeSegments.FirstOrDefault(s => s.Size >= memorySize);
                    break;
                case AllocationStrategy.BestFit:
                    segment = freeSegments.Where(s => s.Size >= memorySize).OrderBy(s => s.Size).FirstOrDefault();
                    break;
                case AllocationStrategy.WorstFit:
                    segment = freeSegments.Where(s => s.Size >= memorySize).OrderByDescending(s => s.Size).FirstOrDefault();
                    break;
            }

            if (segment != null)
            {
                allocatedSegment = new MemorySegment(segment.Start, memorySize);
                if (segment.Size == memorySize)
                {
                    freeSegments.Remove(segment);
                }
                else
                {
                    segment.Start += memorySize;
                    segment.Size -= memorySize;
                }
                availableMemory -= memorySize;
                return true;
            }

            return false;
        }

        public void DeallocateMemory(MemorySegment segment)
        {
            availableMemory += segment.Size;
            freeSegments.Add(segment);
            if (defragStrategy == DefragmentationStrategy.SimpleMerge)
                freeSegments = MergeAdjacentSegments(freeSegments);
        }

        public int GetAvailableMemory() => availableMemory;

        public bool NeedsDefragmentation()
        {
            if (freeSegments.Count <= 1) return false;

            freeSegments = freeSegments.OrderBy(s => s.Start).ToList();
            for (int i = 1; i < freeSegments.Count; i++)
            {
                if (freeSegments[i].Start != freeSegments[i - 1].Start + freeSegments[i - 1].Size)
                    return true;
            }
            return false;
        }

        public void DefragmentMemory()
        {
            switch (defragStrategy)
            {
                case DefragmentationStrategy.SimpleMerge:
                    freeSegments = MergeAdjacentSegments(freeSegments);
                    break;
                case DefragmentationStrategy.MoveBlocksToStart:
                    freeSegments = MergeAdjacentSegments(freeSegments);
                    int usedMemory = TotalMemory - GetAvailableMemory();
                    freeSegments.Clear();
                    freeSegments.Add(new MemorySegment(usedMemory, TotalMemory - usedMemory));
                    break;
            }
        }

        private List<MemorySegment> MergeAdjacentSegments(List<MemorySegment> segments)
        {
            var merged = new List<MemorySegment>();
            foreach (var segment in segments.OrderBy(s => s.Start))
            {
                if (merged.Count == 0)
                {
                    merged.Add(segment);
                }
                else
                {
                    var last = merged.Last();
                    if (last.Start + last.Size == segment.Start)
                    {
                        last.Size += segment.Size;
                    }
                    else
                    {
                        merged.Add(segment);
                    }
                }
            }
            return merged;
        }
    }

    public class MemorySegment
    {
        public int Start { get; set; }
        public int Size { get; set; }

        public MemorySegment(int start, int size)
        {
            Start = start;
            Size = size;
        }

        public void printSegment() => Console.WriteLine($"Start: {Start}, Size: {Size}");
    }
}
