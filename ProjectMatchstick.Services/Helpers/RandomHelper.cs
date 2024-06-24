using Godot;
using ProjectMatchstick.Services.Generation;
using System;
using System.Collections;
using System.Collections.Generic;
using static ProjectMatchstick.Services.Generation.Steps.OverlappedWfcGenerationStep;

namespace ProjectMatchstick.Services.Helpers;

public static class RandomHelper
{
    public static int SelectRandomWeighted<T>(List<T> items, Func<T, double> weightFunction, Random random)
    {
        if (items == null || items.Count == 0)
        {
            throw new ArgumentException("'items' must contain one or more elements.");
        }
        if (weightFunction == null)
        {
            throw new ArgumentException("'weightFunction' cannot be null.");
        }
        if (random == null)
        {
            throw new ArgumentException("'random' cannot be null");
        }

        /* Get prefix sum from sumWeights */

        var prefixSum = new double[items.Count];
        
        for (int i = 0; i < prefixSum.Length; i++)
        {
            double previousSum = i == 0 ? 0 : prefixSum[i - 1];
            prefixSum[i] = weightFunction(items[i]) + previousSum;
        }

        if (prefixSum[items.Count - 1] == 0)
        {
            return random.Next(items.Count);
        }

        /* Select random value from 0 to totalSum. Return the TerrainId which corresponds to this random value in the prefixSum  */

        double r = random.NextDouble() * prefixSum[prefixSum.Length - 1];
        for (int i = 0; i < prefixSum.Length; i++)
        {
            if (prefixSum[i] > r)
            {
                return i;
            }
        }

        throw new Exception($"Something went wrong in {nameof(RandomHelper)}.{nameof(SelectRandomWeighted)}");
    }
}
