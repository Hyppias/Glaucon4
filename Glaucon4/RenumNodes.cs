#region FileHeader
// Project: Glaucon4
// Filename:   RenumNodes.cs
// Last write: 4/30/2023 2:29:40 PM
// Creation:   4/24/2023 12:01:10 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System;
using System.Diagnostics;

namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {
        // Current will contain the final node number permutation.
        // It must be kept for un-permute the node numbers for output
        // Using/ not using permutation is controlled by Glaucon.Param.RenumNodes
        public int[] Current;

        // See: https://www.codeproject.com/Articles/13789/Simulated-Annealing-Example-in-C
        private int[] next;
        private Random rand = new Random();

        private void RenumNodes()
        {
            var iteration = 0;
            var alpha = 0.999;
            int initialDistance;

            // double proba;

            var temperature = 400.0;
            var epsilon = 0.001;

            Current = new int[Nodes.Count];
            next = new int[Nodes.Count];
            for (var i = 0; i < Nodes.Count; i++)
            {
                Current[i] = i;
            }

            var distance = initialDistance = ComputeDistance(Current); // initial diff

            Debug.WriteLine($"Dist = {distance}, iterations = {iteration}");

            //while the temperature didnt reach epsilon
            while (temperature > epsilon)
            {
                iteration++;

                //get the next random permutation of distances
                ComputeNext(Current, next);
                //compute the distance of the new permuted configuration
                var delta = ComputeDistance(next) - distance;
                //if the new distance is better accept it and assign it to Current,
                // to use it in the next try
                if (delta < 0)
                {
                    Assign(Current, next);
                    distance = delta + distance;
                }

                else
                {
                    var proba = rand.NextDouble();
                    //if the new distance is worse accept it but with a probability level
                    // if the probability is less than e^(-delta/temperature).
                    //otherwise the old value is kept
                    if (proba < Math.Exp(-delta / temperature))
                    {
                        Assign(Current, next);
                        distance = delta + distance;
                    }
                }

                //cooling proces on every iteration
                temperature *= alpha;
            }

            // renumbering done.
            // update the members, but only if it is better than the initial numbering:
            if (distance < initialDistance)
            {
                Debug.WriteLine($"initial diff = {initialDistance}, new diff = {distance + 1}");
                foreach (var mbr in Members)
                {
                    mbr.NodeA = Nodes[Current[mbr.NodeA.Nr]];
                    mbr.NodeB = Nodes[Current[mbr.NodeB.Nr]];
                }

                foreach (var nd in Nodes)
                {
                    nd.Nr = Current[nd.Nr];
                }
#if DEBUG
                Debug.WriteLine($"Dist = {distance}, iterations = {iteration}, Temp = {temperature}");
                foreach (var mbr in Members) //for (int i = 0; i < Members.Length ; i++)
                {
                    Debug.WriteLine($"{Current[mbr.NodeA.Nr] + 1} {Current[mbr.NodeB.Nr] + 1}");
                }
#endif
            }
        }

        // Compute the next permutation:
        // Randomly pick two (diffenent) nodes and swap them
        private void ComputeNext(int[] c, int[] n)
        {
            int i2;
            for (var i = 0; i < Nodes.Count; i++)
            {
                n[i] = c[i];
            }

            var i1 = rand.Next(0, Nodes.Count);
            do
            {
                i2 = rand.Next(0, Nodes.Count); // must differ from i1
            } while (i1 == i2);

           (n[i1], n[i2]) = (n[i2],n[i1]);

        }

        // Yes, we want to keep this change in node numbering
        private void Assign(int[] current, int[] next)
        {
            for (var i = 0; i < Nodes.Count; i++)
            {
                current[i] = next[i];
            }
        }

        // compute the maximum node number difference
        private int ComputeDistance(int[] t)
        {
            var max = 0;
            foreach (var mbr in Members)
            {
                max = Math.Max(max, Math.Abs(t[mbr.NodeA.Nr] - t[mbr.NodeB.Nr]));
            }

            return max;
        }
    }
}
