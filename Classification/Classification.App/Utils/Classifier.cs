using System;
using System.Collections.Generic;
using System.Linq;
using Classification.App.Models;

namespace Classification.App.Utils
{
    public class Classifier
    {
        private Database _trainingSet;
        private Database _testSet;

        public MethodResult NearestNeighbourResult { get; set; }
        public MethodResult KNearestNeighbourResult { get; set; }

        public Classifier(Database trainingSet, Database testSet)
        {
            _trainingSet = trainingSet;
            _testSet = testSet;

            NearestNeighbourResult = new MethodResult();
            KNearestNeighbourResult = new MethodResult();
        }

        public void ClassifyNearestNeighbour()
        {
            foreach (var testObject in _testSet.Objects)
            {
                float tempValue = 0f;
                float finalValue = 0f;
                var result = new ClassificationResult {Object = testObject};

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
    }
}
