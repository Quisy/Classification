using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classification.App.Models
{
    public class ClassificationResult
    {
        public ObjectModel Object { get; set; }
        public string AssignedClassName { get; set; }
    }
}
