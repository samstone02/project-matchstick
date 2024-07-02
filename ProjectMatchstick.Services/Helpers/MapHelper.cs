using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ProjectMatchstick.Services.Helpers;

public static class MapHelper
{
    public static Dictionary<Vector2I, T> GetSubmap<T>(Dictionary<Vector2I, T> map, Vector2I pos, int size)
    {
        var submap = new Dictionary<Vector2I, T>();

        for (var i = 0; i < size; i++)
        {
            for (var j = 0; j < size; j++)
            {
                var isFound = map.TryGetValue(new(pos.X + i, pos.Y + j), out var value);

                if (!isFound)
                {
                    return null;
                }

                submap[new(i, j)] = value;
            }
        }

        return submap;
    }

    public static T[,] ToMatrix<T>(Dictionary<Vector2I, T> map)
    {
        int maxX = 0, maxY = 0;

        foreach (var pos in map.Keys)
        {
            if (pos.X < 0 || pos.Y < 0)
            {
                throw new NotImplementedException("Negative positions are not yet implemented");
            }

            if (pos.X > maxX)
            {
                maxX = pos.X;
            }

            if (pos.Y > maxY)
            {
                maxY = pos.Y;
            }
        }

        var matrix = new T[maxX + 1, maxY + 1];

        foreach (var pos in map.Keys)
        {
            matrix[pos.X, pos.Y] = map[pos];
        }

        return matrix;
    }
}
