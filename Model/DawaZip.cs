using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PostNummerKlmExport.Model
{
    /// <summary>
    /// http://dawa.aws.dk/dok/api/postnummer#databeskrivelse
    /// </summary>
    [DataContract]
    public class DawaZip
    {
        [DataMember]
        public string href { get; set; }
        [DataMember]
        public string nr{ get; set; }

        [DataMember(Name = "bbox")]
        public float[] BoundingBox { get; set; }

        [DataMember(Name = "visueltcenter")]
        public float[] Center { get; set; }
    }
}
