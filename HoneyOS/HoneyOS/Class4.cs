using System;
using System.Collections.Generic;
using System.Linq;

namespace HoneyOS
{
    public class MemoryManager
    {
        protected const int TotalMemory = 32;
        protected int availableMemory;
        public List<MemorySegment> freeSegments;

        public MemoryManager()
        {
            availableMemory = TotalMemory;
            freeSegments = new List<MemorySegment> { new MemorySegment(0, TotalMemory) };
        }

        public bool AllocateMemory(int memorySize, out MemorySegment allocatedSegment)
        {
            allocatedSegment = null;
            var segment = freeSegments.FirstOrDefault(s => s.Size >= memorySize);
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
            freeSegments = freeSegments.OrderBy(s => s.Start).ToList();
            freeSegments = MergeAdjacentSegments(freeSegments);

            int freeStart = freeSegments.Sum(s => s.Size);
            freeSegments.Clear();
            if (freeStart < TotalMemory)
                freeSegments.Add(new MemorySegment(freeStart, TotalMemory - freeStart));
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