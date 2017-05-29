using System.Collections.Generic;

namespace Classification.App.Models
{
    public class ObjectModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public IList<float> Features { get; set; }

        public int FeaturesNumber => Features.Count;

        public ObjectModel(string className, IList<float> features)
        {
            ClassId = -1;
            ClassName = className;
            Features = features;
        }
    }
}
