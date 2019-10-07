using PostNummerKlmExport.Model;
using PostNummerKlmExport.Model.Klm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PostNummerKlmExport
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var api = new RestAPI();

            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            var result = await api.Get<DawaZip[]>("http://dawa.aws.dk/postnumre");

            await SaveZipAsKlm("All", result);
        
        }

        private static async Task SaveZipAsKlm(string zip, DawaZip[] result)
        {
            Kml klm = new Kml();

            klm.document = new Document() {
                Name = "Danske postnumre"
            };
            foreach (var area in result)
            {
                Console.Out.WriteLine($"Fecthing zip {area.nr}");
                var polygon = await GetBingArea(area.Center[1], area.Center[0]);
                if (polygon != null)
                {
                    var placemark = new Placemark()
                    {
                        Name = area.nr,
                        Polygon = new Polygon()
                        {
                            OuterBoundary = new OuterBoundary()
                            {
                                LinearRing = new LinearRing()
                                {
                                    Coordinates = LocationsToString(polygon),
                                    Tessellate = "1"
                                }
                            }
                        }
                    };
                    klm.document.Placemarks.Add(placemark);
                }
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Kml));
            FileStream stream = new FileStream(zip+".kml", FileMode.OpenOrCreate);
            StreamWriter writer = new StreamWriter(stream);
            serializer.Serialize(writer, klm);
            writer.Close();
        }


        private static float[] BoundingBoxToCoordinates(float[] boundingBox)
        {
            var lat1 = boundingBox[0];
            var long1 = boundingBox[1];
            var lat2 = boundingBox[2];
            var long2 = boundingBox[3];

            var coords = new List<float>();
            coords.Add(lat1);
            coords.Add(long1);
            coords.Add(lat1);
            coords.Add(long2);
            coords.Add(lat2);
            coords.Add(long2);
            coords.Add(lat2);
            coords.Add(long1);
            return coords.ToArray();
        }

        private static string CoordinatesToString(float[] boundingBox)
        {
            string result = "";
            for (var i = 0; i < boundingBox.Length; i+=2)
            {
                result += LocationToString(boundingBox[i], boundingBox[i+1]);
            }
            return result;
        }

        private static string LocationToString(double longitude, double latitude)
        {
            return $"{longitude}, {latitude}, 0\n";
        }

        private static string LocationsToString(List<Location> polygon)
        {
            string result = "";
            polygon.ForEach(l => result += LocationToString(l.Longitude, l.Latitude));
            return result;
        }

        private static async Task<List<Location>> GetBingArea(double latitude, double longitude)
        {


            var key = Environment.GetEnvironmentVariable("BINGKEY");
            var url = $"http://platform.bing.com/geo/spatial/v1/public/Geodata?SpatialFilter=GetBoundary({latitude},{longitude},0,'Postcode1',0,0)&PreferCuratedPolygons=1&$format=json&key={key}";

            var api = new RestAPI();

            var bingResult = await api.Get(url);

            if (bingResult.d.results.Count > 0) { 
                string coords = bingResult.d.results[0].Primitives[0].Shape;
                List<Location> coordinates = new List<Location>();
                TryParseEncodedValue(coords.Substring(2), out coordinates);
                return coordinates;
            }
            return null;
        }

        public const string safeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-";

        private static bool TryParseEncodedValue(string value, out List<Location> parsedValue)
        {
            parsedValue = null;
            var list = new List<Location>();
            int index = 0;
            int xsum = 0, ysum = 0;

            while (index < value.Length)        // While we have more data,  
            {
                long n = 0;                     // initialize the accumulator  
                int k = 0;                      // initialize the count of bits  

                while (true)
                {
                    if (index >= value.Length)  // If we ran out of data mid-number  
                        return false;           // indicate failure.  

                    int b = safeCharacters.IndexOf(value[index++]);

                    if (b == -1)                // If the character wasn't on the valid list,  
                        return false;           // indicate failure.  

                    n |= ((long)b & 31) << k;   // mask off the top bit and append the rest to the accumulator  
                    k += 5;                     // move to the next position  
                    if (b < 32) break;          // If the top bit was not set, we're done with this number.  
                }

                // The resulting number encodes an x, y pair in the following way:  
                //  
                //  ^ Y  
                //  |  
                //  14  
                //  9 13  
                //  5 8 12  
                //  2 4 7 11  
                //  0 1 3 6 10 ---> X  

                // determine which diagonal it's on  
                int diagonal = (int)((Math.Sqrt(8 * n + 5) - 1) / 2);

                // subtract the total number of points from lower diagonals  
                n -= diagonal * (diagonal + 1L) / 2;

                // get the X and Y from what's left over  
                int ny = (int)n;
                int nx = diagonal - ny;

                // undo the sign encoding  
                nx = (nx >> 1) ^ -(nx & 1);
                ny = (ny >> 1) ^ -(ny & 1);

                // undo the delta encoding  
                xsum += nx;
                ysum += ny;

                // position the decimal point  
                list.Add(new Location(ysum * 0.00001, xsum * 0.00001));
            }

            parsedValue = list;
            return true;
        }
    }

}
