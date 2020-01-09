using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace CyGateWMS.ViewModels
{
    [DataContract]
    public class DataPoint
    {
        public DataPoint(double y, string label)
        {
            this.Y = y;
            this.Label = label;
        }
        public DataPoint(double y, string label, string legendText)
        {
            this.Y = y;
            this.Label = label;
            this.legendText = legendText;
        }
        //Explicitly setting the name to be used while serializing to JSON. 
        [DataMember(Name = "label")]
        public string Label = null;

        [DataMember(Name = "legendText")]
        public string legendText = null;

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "y")]
        public Nullable<double> Y = null;
    }
}
