using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Classification.App.Models;

namespace Classification.App.Utils
{
    public class Database
    {
        public IList<ObjectModel> Objects { get; private set; }
        public Dictionary<string, int> ClassCounters { get; set; }
        public IList<string> ClassNames { get; set; }
        public IList<int> FeaturesIDs { get; set; }
        public int NoClass => ClassNames.Count;
        public int NoObjects => Objects.Count;
        public int NoFeatures => FeaturesIDs.Count;

        public Database()
        {
            Objects = new List<ObjectModel>();
            ClassCounters = new Dictionary<string, int>();
            ClassNames = new List<string>();
            FeaturesIDs = new List<int>();
        }

        public bool AddObject(ObjectModel obj)
        {
            if (NoFeatures > 0 && NoFeatures != obj.FeaturesNumber)
                return false;

            Objects.Add(obj);

            if (ClassCounters.ContainsKey(obj.ClassName))
            {
                ClassCounters[obj.ClassName]++;
            }
            else
            {
                ClassCounters.Add(obj.ClassName, 1);
                ClassNames.Add(obj.ClassName);
            }

            return true;
        }

        public bool RemoveObject(ObjectModel obj)
        {
            Objects.Remove(obj);
            ClassCounters[obj.ClassName]--;

            return true;
        }

        public bool Load(string fileName)
        {
            this.Clear();

            var lines = File.ReadAllLines(fileName);
            if (lines.Length == 0)
                return false;

            string firstLine = lines[0];

            int classFeaturesNumber = int.Parse(firstLine.Substring(0, firstLine.IndexOf(',')));

            int temppos = firstLine.IndexOf(',');

            string featuresIds = firstLine.Substring(temppos + 1);
            FeaturesIDs = featuresIds.Split(',').Select(int.Parse).ToList();

            lines = lines.Where((val, idx) => idx > 0).ToArray();

            foreach (string line in lines)
            {
                int pos = line.IndexOf(',');
                string className = line.Substring(0, pos);
                int classNamePos = className.IndexOf(' ');
                className = className.Substring(0, classNamePos);

                string features = line.Substring(pos + 1);
                var featuresValues = features.Split(',').Select(y=>float.Parse(y.Replace('.',','))).ToList();

                if (classFeaturesNumber == featuresValues.Count)
                {
                    AddObject(new ObjectModel(className, featuresValues));
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private void Clear()
        {
            Objects.Clear();
            ClassCounters.Clear();
            ClassNames.Clear();
            FeaturesIDs.Clear();
        }
    }
}
