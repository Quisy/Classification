using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Classification.App.Helpers
{
    public static class MatrixHelper
    {

        public static float[,] MultiplyMatrix(float[,] a, float[,] b)
        {
            int rA = a.GetLength(0);
            int cA = a.GetLength(1);
            int rB = b.GetLength(0);
            int cB = b.GetLength(1);
            float temp = 0;
            float[,] result = new float[rA, cB];
            if (cA != rB)
            {
                // Matrix cannot be multiplied
                return null;
            }
            else
            {
                for (int i = 0; i < rA; i++)
                {
                    for (int j = 0; j < cB; j++)
                    {
                        temp = 0;
                        for (int k = 0; k < cA; k++)
                        {
                            temp += a[i, k] * b[k, j];
                        }
                        result[i, j] = temp;
                    }
                }
                return result;
            }
        }

        public static float[,] SubstractVectorFromMatrix(float[,] matrix, float[] vector)
        {
            int w = matrix.GetLength(0);
            int h = matrix.GetLength(1);
            float[,] result = new float[w, h];

            for (int i = 0; i < w; i++)
            {
                float val = vector[i];

                for (int j = 0; j < h; j++)
                {
                    result[i, j] = matrix[i, j] - val;
                }

            }

            return result;
        }

        public static float[,] Transpose(float[,] matrix)
        {
            int w = matrix.GetLength(0);
            int h = matrix.GetLength(1);

            float[,] result = new float[h, w];

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    result[j, i] = matrix[i, j];
                }
            }

            return result;
        }

        public static float Determinant(float[,] a)
        {
            int n = a.GetLength(0);
            int i, j, k;
            float det = 0;
            for (i = 0; i < n - 1; i++)
            {
                for (j = i + 1; j < n; j++)
                {
                    if (a[i, i] == 0)
                        det = 0;
                    else
                        det = a[j, i] / a[i, i];
                    for (k = i; k < n; k++)
                        a[j, k] = a[j, k] - det * a[i, k]; // HERE
                }
            }
            det = 1;
            for (i = 0; i < n; i++)
                det = det * a[i, i];
            return det;
        }
    }
}
