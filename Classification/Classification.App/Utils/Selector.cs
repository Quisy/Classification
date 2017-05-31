using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classification.App.Helpers;

namespace Classification.App.Utils
{
    public class Selector
    {
        private Database _set;

        public Selector(Database set)
        {
            _set = set;
        }


        public IList<int> Fisher(int k)
        {
            Dictionary<string, float[]> classMeans = CountClassMeans();
            Dictionary<string, float[,]> dispersionMatrixes = GenerateDispersionMatrixes(classMeans);
            List<int> bestFeatures = new List<int>();
            int maxValueFeatureId = 0;

            Dictionary<int, float> fisherResults = new Dictionary<int, float>();
            float nominator = 0f, denominator = 0f;

            float[] tempVector = null;

            foreach (var mean in classMeans)
            {
                tempVector = tempVector == null ? mean.Value : VectorHelper.SubstractVectorFromVector(tempVector, mean.Value);
            }

            foreach (var featureId in _set.FeaturesIDs)
            {
                nominator = VectorHelper.CountVectorLength(new float[] { tempVector[featureId] });

                foreach (var dispersionMatrix in dispersionMatrixes)
                {
                    denominator += dispersionMatrix.Value[featureId, featureId];
                }

                float fisherValue = nominator / denominator;

                fisherResults.Add(featureId, fisherValue);
            }

            for (int i = 0; i < k; i++)
            {
                maxValueFeatureId = fisherResults.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                bestFeatures.Add(maxValueFeatureId);
                fisherResults.Remove(maxValueFeatureId);
            }

            return bestFeatures;
        }

        public IList<int> FisherSFS(int k)
        {
            Dictionary<string, float[]> classMeans = CountClassMeans();
            Dictionary<string, float[,]> dispersionMatrixes = GenerateDispersionMatrixes(classMeans);
            List<int> bestFeatures = new List<int>();
            int maxValueFeatureId = 0;

            Dictionary<int, float> fisherResults = new Dictionary<int, float>();
            float nominator = 0f, denominator = 0f;

            float[] tempVector = null;

            foreach (var mean in classMeans)
            {
                tempVector = tempVector == null ? mean.Value : VectorHelper.SubstractVectorFromVector(tempVector, mean.Value);
            }

            foreach (var featureId in _set.FeaturesIDs)
            {
                nominator = VectorHelper.CountVectorLength(new float[] { tempVector[featureId] });

                foreach (var dispersionMatrix in dispersionMatrixes)
                {
                    denominator += dispersionMatrix.Value[featureId, featureId];
                }

                float fisherValue = nominator / denominator;

                fisherResults.Add(featureId, fisherValue);
            }

            maxValueFeatureId = fisherResults.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            bestFeatures.Add(maxValueFeatureId);
            fisherResults.Clear();

            for (int i = 1; i < k; i++)
            {
                foreach (var featureId in _set.FeaturesIDs.Where(id => !bestFeatures.Contains(id)))
                {
                    List<float[]> means = new List<float[]>();
                    foreach (var mean in classMeans)
                    {
                        float[] tempMean = new float[bestFeatures.Count + 1];

                        int j = 0;

                        for (j = 0; j < bestFeatures.Count; j++)
                        {
                            tempMean[j] = mean.Value[bestFeatures[j]];
                        }

                        tempMean[j] = mean.Value[featureId];
                        means.Add(tempMean);
                    }


                    tempVector = null;
                    foreach (var mean in means)
                    {
                        tempVector = tempVector == null
                            ? mean
                            : VectorHelper.SubstractVectorFromVector(tempVector, mean);
                    }

                    nominator = VectorHelper.CountVectorLength(tempVector);
                    denominator = 0f;

                    foreach (var dispersionMatrix in dispersionMatrixes)
                    {
                        float[,] disperionValue = dispersionMatrix.Value;
                        float[,] tempMatrix = new float[bestFeatures.Count + 1, bestFeatures.Count + 1];
                        var calculatingFeatures = bestFeatures.OrderBy(f => f).ToList();
                        calculatingFeatures.Add(featureId);
                        calculatingFeatures = calculatingFeatures.OrderBy(f => f).ToList();

                        for (int j = 0; j < calculatingFeatures.Count; j++)
                        {
                            for (int l = 0; l < calculatingFeatures.Count; l++)
                            {
                                tempMatrix[j, l] = disperionValue[calculatingFeatures[j], calculatingFeatures[l]];
                            }
                        }

                        denominator += MatrixHelper.Determinant(tempMatrix);
                    }

                    float fisherValue = nominator / denominator;

                    fisherResults.Add(featureId, fisherValue);
                }

                maxValueFeatureId = fisherResults.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                bestFeatures.Add(maxValueFeatureId);
                fisherResults.Clear();
            }

            return bestFeatures;
        }

        private Dictionary<string, float[]> CountClassMeans()
        {
            Dictionary<string, float[]> classMeans = new Dictionary<string, float[]>();

            foreach (var className in _set.ClassNames)
            {
                float[] mean = new float[_set.NoFeatures];

                var classObjects = _set.Objects.Where(o => o.ClassName.Equals(className)).ToList();

                for (int i = 0; i < _set.NoFeatures; i++)
                {
                    mean[i] = classObjects.Select(o => o.Features[i]).Average();
                }

                classMeans.Add(className, mean);
            }

            return classMeans;
        }

        private Dictionary<string, float[,]> GenerateDispersionMatrixes(Dictionary<string, float[]> classMeans)
        {
            Dictionary<string, float[,]> dispersionMatrixes = new Dictionary<string,float[,]>();

            foreach (var className in _set.ClassNames)
            {
                var classObjects = _set.Objects.Where(o => o.ClassName.Equals(className)).ToList();

                float[,] classMatrix = new float[_set.NoFeatures, classObjects.Count];

                for (int i = 0; i < _set.NoFeatures; i++)
                {
                    for (int j = 0; j < classObjects.Count; j++)
                    {
                        classMatrix[i, j] = classObjects[j].Features[i];
                    }
                }

                classMatrix = MatrixHelper.SubstractVectorFromMatrix(classMatrix, classMeans[className]);

                var dispersionMatrix = MatrixHelper.MultiplyMatrix(classMatrix, MatrixHelper.Transpose(classMatrix));
                dispersionMatrixes.Add(className, dispersionMatrix);
            }

            return dispersionMatrixes;
        }
    }
}
