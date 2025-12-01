using System;
using System.Collections.Generic;
using System.Text;

namespace Memory_Exploper
{
    internal class LeakyApp
    {
        private static List<object> _cache = new();

        public void Run()
        {
            for(int i =0; i<10000; i++)
            {
                var data = new byte[1024];
                _cache.Add(data);
            }
        }
    }
}
