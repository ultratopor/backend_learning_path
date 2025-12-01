using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Swedzerland_Cheese
{
    internal class LohFragmentation
    {
        private List<byte[]> _largeObjects = new List<byte[]>();

        public void Run()
        {
            Console.WriteLine("Starting LOH Fragmentation Demo...");
            long size = 0;
            // Allocate large objects (greater than 85,000 bytes)
            for (int i = 0; i < 100; i++)
            {
                byte[] largeObject = new byte[90000]; // 90,000 bytes
                _largeObjects.Add(largeObject);
                size = Process.GetCurrentProcess().PrivateMemorySize64;
                Console.WriteLine($"Allocated large object {i + 1}: {largeObject.Length} bytes. Process memory: {size}");
            }
            // Simulate fragmentation by removing some objects
            for (int i = 0; i < 50; i += 2)
            {
                _largeObjects[i] = null; // Remove every second object
                size = Process.GetCurrentProcess().PrivateMemorySize64;
                Console.WriteLine($"Removed large object {i + 1}. Process memory: {size}");
            }
            // Force garbage collection to see the effect of fragmentation
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var veryLargeObject = new byte[100000];
            size = Process.GetCurrentProcess().PrivateMemorySize64;
            Console.WriteLine($"LOH Fragmentation Demo completed. Process memory: {size}");
            Console.ReadKey();
        }
    }
}
