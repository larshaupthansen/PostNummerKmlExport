using System;
using System.Collections.Generic;
using System.Text;

namespace PostNummerKlmExport.Model
{
   
    public class Location
    {
         private double _latitude;
        private double _longitude;
        public Location(double latitude, double longitude) {
            _latitude = latitude;
            _longitude = longitude;

        }

        public double Latitude {  get { return _latitude; } }
        public double Longitude { get { return _longitude; } }
    }
}
