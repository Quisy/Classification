using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classification.App.Helpers
{
    public static class VectorHelper
    {

        public static float[] SubstractVectorFromVector(float[] vector1, float[] vector2)
        {
            var result = new float[vector1.Length];
            for (int i = 0; i < vector1.Length; i++)
            {
                result[i] = vector1[i] - vector2[i];
            }

            return result;
        }

        public static float CountVectorLength(float[] vector)
        {
            float result = 0f;
            foreach (var val in vector)
            {
                result += val * val;
            }
            result = (float)Math.Sqrt(result);
            return result;
        }
    }

}
