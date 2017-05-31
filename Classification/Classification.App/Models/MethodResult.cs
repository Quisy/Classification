using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classification.App.Models
{
    public class MethodResult
    {
        public string MethodName { get; set; }

        public IList<ClassificationResult> ClassificationResults { get; set; }

        public float Effectiveness
        {
            get
            {
                return ClassificationResults.Count(x => x.Object.ClassName.Equals(x.AssignedClassName)) * 100f / ClassificationResults.Count;
            }
        }

        public MethodResult()
        {
            ClassificationResults = new List<ClassificationResult>();
        }
    }
}
