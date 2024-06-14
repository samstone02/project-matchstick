namespace ProjectMatchstick.Services.Helpers;

public static class MatrixHelper
{
    public static T[,] Clone<T>(T[,] matrix)
    {
        var clone = new T[matrix.GetLength(0), matrix.GetLength(1)];
        Array.Copy(matrix, clone, matrix.Length);
        return clone;
    } 

    public static T[,] GetSubmatrix<T>(T[,] matrix, int x, int y, int n)
    {
        var submatrix = new T[n, n];

        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                submatrix[i, j] = matrix[x + i, y + j];
            }
        }

        return submatrix;
    }

    public static T[,] RotateClockwise<T>(T[,] matrix, int quarterTurns)
    {
        quarterTurns %= 4;

        int n = matrix.GetLength(0);

        while (quarterTurns > 0)
        {
            int layer = 0;

            while (layer < n / 2)
            {
                int rowA = layer;
                int colA = layer;

                int rowB = 0;
                int colB = n - layer - 1;

                int rowC = n - layer - 1;
                int colC = n - layer - 1;

                int rowD = n - layer - 1;
                int colD = layer;

                int focus = layer;

                while (focus < n - layer - 1)
                {
                    T temp = matrix[rowD, colD];
                    matrix[rowD, colD] = matrix[rowC, colC];
                    matrix[rowC, colC] = matrix[rowB, colB];
                    matrix[rowB, colB] = matrix[rowA, colA];
                    matrix[rowA, colA] = temp;

                    colA++;
                    rowB++;
                    colC--;
                    rowD--;

                    focus++;
                }

                layer++;
            }
            quarterTurns--;
        }

        return matrix;
    }
}
