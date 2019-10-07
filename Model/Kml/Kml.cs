using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace PostNummerKlmExport.Model.Klm
{
    [DataContract]
    [XmlRootAttribute("kml", Namespace = "http://www.opengis.net/kml/2.2")]

    public class Kml
    {
        [XmlElement(ElementName ="Document")]
        [DataMember(Name ="Document")]
        public Document document { get; set; }
    }

    [DataContract]
    public class Document
    {
        List<Placemark> _placemarks = new List<Placemark>();


        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "Placemark")]
        public List<Placemark> Placemarks  { get { return _placemarks; } }
    }

    [DataContract]
    public class Placemark
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "Polygon")]

        public Polygon Polygon { get; set; }

    }

    public class Polygon
    {

        [XmlElement(ElementName = "outerBoundaryIs")]
        public OuterBoundary OuterBoundary { get; set; }
    }
    public class OuterBoundary
    {
        [XmlElement(ElementName = "LinearRing")]
        public LinearRing LinearRing { get; set; }
    }
    public class LinearRing
    {
        [XmlElement(ElementName = "tessellate")]

        public string Tessellate { get; set; }

        [XmlElement(ElementName = "coordinates")]

        public string Coordinates{ get; set; }
    }


}
