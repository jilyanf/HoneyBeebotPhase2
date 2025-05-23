﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.ModelBinding;

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

        // function to determine if memory can be allocated for the process
        // returns true if memory can be allocated, and if true, memory is automatically allocated
        // returns false otherwise
        public bool AllocateMemory(int memorySize, out MemorySegment allocatedSegment)
        {
            allocatedSegment = null;
            //Find the first segment large enough to accommodate the memory request
            var segment = freeSegments.FirstOrDefault(s => s.Size >= memorySize);
            if (segment != null)
            {
                allocatedSegment = new MemorySegment(segment.Start, memorySize);
                if(segment.Size == memorySize)
                {
                    freeSegments.Remove(segment);
                }
                else
                {
                    // Reduce free segment size 
                    segment.Start += memorySize;
                    segment.Size -= memorySize;
                }
                availableMemory -= memorySize;
                return true;
            }
            return false; 
        }

        // function to deallocate memory segment allocated for a process
        public void DeallocateMemory(MemorySegment segment)
        {
            availableMemory += segment.Size;
            freeSegments.Add(segment);
            freeSegments = MergeAdjacentSegments(freeSegments); // Merge adjacent free segments
        }

        // function to get the current total free memory in the memory manager
        public int GetAvailableMemory()
        {
            return availableMemory;
        }

        // function to merge any adjacent free segments
        private List<MemorySegment> MergeAdjacentSegments(List<MemorySegment> segments)
        {
            var mergedSegments = new List<MemorySegment>();
            foreach (var segment in segments.OrderBy(s => s.Start))
            {
                if (mergedSegments.Count == 0)
                {
                    mergedSegments.Add(segment);
                }
                else
                {
                    var lastSegment = mergedSegments.Last();
                    if (lastSegment.Start + lastSegment.Size == segment.Start)
                    {
                        lastSegment.Size += segment.Size; // Merge adjacent segments
                    }
                    else
                    {
                        mergedSegments.Add(segment);
                    }
                }
            }
            return mergedSegments;
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

        // debug function, prints to console the details of a segment
        public void printSegment()
        {
            Console.WriteLine($"Start: {Start}, Size: {Size}");
        }
    }
}
