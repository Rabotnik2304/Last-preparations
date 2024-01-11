using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphPreparing
{
    public class Graph
    {
        public int NodeCount => nodeCount;
        public string[] NodesNames => nodesNames;
        public int[][] AdjectiveMatrix => adjactiveMatrix;
        public bool IsOriented => isOriented;

        public Edge[] Edges
        {
            get => edges;
            set => edges = value;
        }
        private Edge[] edges;
        private readonly int nodeCount;
        private readonly string[] nodesNames;
        private readonly int[][] adjactiveMatrix;
        private readonly bool isOriented;

        public Graph(int vertexCount, string[] graphNodesNames, int[][] graphAdjecsiveMatrix, bool graphIsOriented)
        {
            nodeCount = vertexCount;
            nodesNames = graphNodesNames;
            adjactiveMatrix = graphAdjecsiveMatrix;
            isOriented = graphIsOriented;

            if (!isOriented)
            {
                MatrixRecovery();
            }

            edges = GetEdges().ToArray();
        }

        public void MatrixRecovery()
        {
            for (int i = 0; i < nodeCount; i++)
            {
                for (int j = 0; j < nodeCount; j++)
                {
                    if (adjactiveMatrix[i][j] > 0)
                    {
                        adjactiveMatrix[j][i] = adjactiveMatrix[i][j];
                    }
                }
            }
        }
        private List<Edge> GetEdges()
        {
            List<Edge> edges = new List<Edge>();
            bool[] visitedNodes = new bool[nodeCount];

            for (int y = 0; y < adjactiveMatrix.Length; y++)
            {
                for (int x = 0; x < adjactiveMatrix[y].Length; x++)
                {
                    if (x == y || (!isOriented && visitedNodes[x]))
                    {
                        continue;
                    }
                    if (adjactiveMatrix[y][x] <= 0)
                    {
                        continue;
                    }
                    edges.Add(new Edge(y, x, adjactiveMatrix[y][x]));
                }
                visitedNodes[y] = true;
            }
            return edges;
        }
        public void Print()
        {
            Console.Write("  ");
            for (int i = 0; i < nodeCount; i++)
            {
                Console.Write(nodesNames[i] + " ");
            }
            Console.WriteLine();

            for (int i = 0; i < nodeCount; i++)
            {
                Console.Write(nodesNames[i] + " ");
                for (int j = 0; j < nodeCount; j++)
                {
                    Console.Write(adjactiveMatrix[i][j] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
