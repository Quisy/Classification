using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using Classification.App.Helpers;
using Classification.App.Models;

namespace Classification.App.Utils
{
    public class Classifier
    {
        private readonly Database _trainingSet;
        private readonly Database _testSet;

        public MethodResult NearestNeighbourResult { get; set; }
        public MethodResult KNearestNeighbourResult { get; set; }
        public MethodResult NearestMeanResult { get; set; }
        public MethodResult KNearestMeanResult { get; set; }

        public Classifier(Database trainingSet, Database testSet)
        {
            _trainingSet = trainingSet;
            _testSet = testSet;

            NearestNeighbourResult = new MethodResult();
            KNearestNeighbourResult = new MethodResult();
            NearestMeanResult = new MethodResult();
            KNearestMeanResult = new MethodResult();
        }

        public void ClassifyNearestNeighbour()
        {
            foreach (var testObject in _testSet.Objects)
            {
                float tempValue = 0f;
                float finalValue = 0f;
                var result = new ClassificationResult { Object = testObject };

                foreach (var trainingObject in _trainingSet.Objects)
                {
                    for (int i = 0; i < testObject.FeaturesNumber; i++)
                    {
                        tempValue += (float)Math.Pow(testObject.Features[i] - trainingObject.Features[i], 2);
                    }

                    tempValue = (float)Math.Sqrt(tempValue);

                    if (finalValue == 0f || tempValue < finalValue)
                    {
                        finalValue = tempValue;
                        result.AssignedClassName = trainingObject.ClassName;
                    }
                }

                NearestNeighbourResult.ClassificationResults.Add(result);
            }
        }

        public void ClassifyKNearestNeighbour(uint k)
        {
            Dictionary<string, int> classCounter = new Dictionary<string, int>();

            foreach (string className in _trainingSet.ClassNames)
            {
                classCounter.Add(className, 0);
            }

            foreach (var testObject in _testSet.Objects)
            {
                float tempValue = 0f;
                IList<float> values = new List<float>();
                var result = new ClassificationResult { Object = testObject };

                foreach (string className in _trainingSet.ClassNames)
                {
                    classCounter[className] = 0;
                }

                foreach (var trainingObject in _trainingSet.Objects)
                {
                    for (int i = 0; i < testObject.FeaturesNumber; i++)
                    {
                        tempValue += (float)Math.Pow(testObject.Features[i] - trainingObject.Features[i], 2);
                    }

                    tempValue = (float)Math.Sqrt(tempValue);
                    values.Add(tempValue);
                }

                IList<int> minValuesIndexes = new List<int>();

                for (int i = 0; i < k; i++)
                {
                    var lowestValue = values.Where(v => !minValuesIndexes.Contains(values.IndexOf(v))).Min();
                    minValuesIndexes.Add(values.IndexOf(lowestValue));
                }

                foreach (int index in minValuesIndexes)
                {
                    classCounter[_trainingSet.Objects[index].ClassName]++;
                }

                result.AssignedClassName = classCounter.First(c => c.Value == classCounter.Values.Max()).Key;

                KNearestNeighbourResult.ClassificationResults.Add(result);
            }
        }

        public void ClassifyNearesMean()
        {
            Dictionary<string, float[]> classMeans = CountClassMeans();


            foreach (var testObject in _testSet.Objects)
            {
                var result = new ClassificationResult { Object = testObject };
                float finalValue = 0f;

                foreach (var classMean in classMeans)
                {
                    float tempValue = 0f;

                    var currentMean = classMean.Value;

                    for (int i = 0; i < testObject.FeaturesNumber; i++)
                    {
                        tempValue += (float)Math.Pow(testObject.Features[i] - currentMean[i], 2);
                    }

                    tempValue = (float)Math.Sqrt(tempValue);

                    if (finalValue == 0f || tempValue < finalValue)
                    {
                        finalValue = tempValue;
                        result.AssignedClassName = classMean.Key;
                    }
                }

                NearestMeanResult.ClassificationResults.Add(result);
            }
        }

        public void ClassifyKNearestMean(int k)
        {
            Dictionary<int, float[]> subClassesMeans = new Dictionary<int, float[]>();
            Dictionary<int, List<int>> objectsInSubClasses = new Dictionary<int, List<int>>();
            bool isOk = true;

            for (int i = 0; i < k; i++)
            {
                var classObjects = _trainingSet.Objects.Where(o => o.ClassName.Equals(_trainingSet.ClassNames[0])).ToList();
                var subClassMean = classObjects[i].Features.ToArray();
                subClassesMeans.Add(i, subClassMean);
                objectsInSubClasses.Add(i, new List<int>());
            }

            do
            {
                var tempObjectsInSubClasses = new Dictionary<int, List<int>>();

                foreach (var subClassMean in subClassesMeans)
                {
                    tempObjectsInSubClasses.Add(subClassMean.Key, new List<int>());
                }

                foreach (var trainingObject in _trainingSet.Objects)
                {
                    int tempClassKey = 0;
                    float finalValue = 0f;
                    bool first = true;

                    foreach (var subClassMean in subClassesMeans)
                    {
                        float distance =
                            VectorHelper.CountDistanceBetweenVectors(trainingObject.Features.ToArray(), subClassMean.Value);

                        if (first)
                        {
                            tempClassKey = subClassMean.Key;
                            finalValue = distance;
                            first = false;
                        }
                        else if (distance < finalValue)
                        {
                            finalValue = distance;
                            tempClassKey = subClassMean.Key;
                        }

                    }

                    tempObjectsInSubClasses[tempClassKey].Add(_trainingSet.Objects.IndexOf(trainingObject));
                }

                foreach (var obj in tempObjectsInSubClasses)
                {
                    if (!obj.Value.SequenceEqual(objectsInSubClasses[obj.Key]))
                    {
                        isOk = false;
                        objectsInSubClasses = tempObjectsInSubClasses;
                        break;
                    }

                    isOk = true;
                }

                if (!isOk)
                {
                    foreach (var obj in objectsInSubClasses)
                    {
                        var classObjects = _trainingSet.Objects.Where(o => obj.Value.Contains(_trainingSet.Objects.IndexOf(o))).ToList();

                        var mean = CountMeanOfObjects(classObjects, _trainingSet.NoFeatures);

                        subClassesMeans[obj.Key] = mean;
                    }
                }

            } while (!isOk);

            Dictionary<string, float[]> classesMeans = new Dictionary<string, float[]>();

            foreach (var className in _trainingSet.ClassNames.Skip(1).ToList())
            {
                var classObjects = _trainingSet.Objects.Where(o => o.ClassName.Equals(className)).ToList();
                classesMeans.Add(className, CountMeanOfObjects(classObjects, _trainingSet.NoFeatures));
            }

            var subClassesParentName = _trainingSet.ClassNames[0];

            foreach (var subClassMean in subClassesMeans)
            {
                classesMeans.Add($"{subClassesParentName}.{subClassMean.Key}", subClassMean.Value);
            }

            foreach (var testObject in _testSet.Objects)
            {
                var result = new ClassificationResult { Object = testObject };
                float finalValue = 0f;

                foreach (var classMean in classesMeans)
                {
                    float tempValue = 0f;

                    var currentMean = classMean.Value;

                    for (int i = 0; i < testObject.FeaturesNumber; i++)
                    {
                        tempValue += (float)Math.Pow(testObject.Features[i] - currentMean[i], 2);
                    }

                    tempValue = (float)Math.Sqrt(tempValue);

                    if (finalValue == 0f || tempValue < finalValue)
                    {
                        finalValue = tempValue;
                        result.AssignedClassName = classMean.Key.StartsWith(_trainingSet.ClassNames[0]) ? _trainingSet.ClassNames[0] : classMean.Key;
                    }
                }

                KNearestMeanResult.ClassificationResults.Add(result);
            }

        }

        private float[] CountMeanOfObjects(IList<ObjectModel> objects, int featuresNo)
        {
            float[] result = new float[featuresNo];

            for (int i = 0; i < featuresNo; i++)
            {
                result[i] = objects.Select(o => o.Features[i]).Average();
            }

            return result;
        }

        private Dictionary<string, float[]> CountClassMeans()
        {
            Dictionary<string, float[]> classMeans = new Dictionary<string, float[]>();

            foreach (var className in _trainingSet.ClassNames)
            {
                float[] mean = new float[_trainingSet.NoFeatures];

                var classObjects = _trainingSet.Objects.Where(o => o.ClassName.Equals(className)).ToList();

                for (int i = 0; i < _trainingSet.NoFeatures; i++)
                {
                    mean[i] = classObjects.Select(o => o.Features[i]).Average();
                }

                classMeans.Add(className, mean);
            }

            return classMeans;
        }



    }
}
