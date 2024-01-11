using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphPreparing
{
    public struct Edge
    {
        public int StartNodeIndex { get; set; }
        public int EndNodeIndex { get; set; }
        public int Weight { get; set; }
        public Edge(int startNodeIndex, int endNodeIndex, int weight)
        {
            StartNodeIndex = startNodeIndex;
            EndNodeIndex = endNodeIndex;
            Weight = weight;
        }
    }
}
