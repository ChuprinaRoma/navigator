using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using Xamarin.Essentials;
using System;

namespace App1
{
    public static class Mapmanager
    {
        public static List<Location> GetPoint(double lat, double lng, double lat1 = 0, double lng1 = 0, string streat = null)
        {
            string respSt = null;
            List<Location> locations = null;
            if (streat != null)
            {
                respSt = ReqvestMapsApi(streat, lat, lng);
            }
            else
            {

                respSt = ReqvestMapsApi(lat, lng, lat1, lng1);
            }
            List<Step> steps = GetSteps(respSt);
            locations = new List<Location>();
            foreach (var step in steps)
            {
                locations.AddRange(DecodePolylinePoints(step.polyline.points));
            }
            return locations;
        }

        private static List<Step> GetSteps(string respSt)
        {
            List<Step> steps = null;
            var responseAppS = JObject.Parse(respSt);
            var status = responseAppS.Value<string>("status");
            if(status == "OK")
            {
                var stepJson = responseAppS.GetValue("routes").First.Value<JArray>("legs").First.Value<JArray>("steps").ToString();
                steps = JsonConvert.DeserializeObject<List<Step>>(stepJson);
            }
            return steps;
        }

        private static string ReqvestMapsApi(string streat, double lat, double lng)
        {
            IRestResponse response = null;
            string respStr = null;
            RestClient client = new RestClient("https://maps.googleapis.com");
            RestRequest request = new RestRequest($"maps/api/directions/json?origin={lat.ToString().Replace(",", ".")},{lng.ToString().Replace(",", ".")}&destination={streat}&key=AIzaSyC7O3O37rv4xDY27oT34peX1z_l9HihH0w", Method.GET);
            response = client.Execute(request);
            respStr = response.Content;
            return respStr;
        }

        private static string ReqvestMapsApi(double lat, double lng, double lat1, double lng1)
        {
            IRestResponse response = null;
            string respStr = null;
            RestClient client = new RestClient("https://maps.googleapis.com");
            RestRequest request = new RestRequest($"maps/api/directions/json?origin={lat.ToString().Replace(",", ".")},{lng.ToString().Replace(",", ".")}&destination={lat1.ToString().Replace(",", ".")},{lng1.ToString().Replace(",", ".")}&key=AIzaSyC7O3O37rv4xDY27oT34peX1z_l9HihH0w", Method.GET);
            response = client.Execute(request);
            respStr = response.Content;
            return respStr;
        }

        private static List<Location> DecodePolylinePoints(string encodedPoints)
        {
            if (encodedPoints == null || encodedPoints == "") return null;
            List<Location> poly = new List<Location>();
            char[] polylinechars = encodedPoints.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            try
            {
                while (index < polylinechars.Length)
                {
                    // calculate next latitude
                    sum = 0;
                    shifter = 0;
                    do
                    {
                        next5bits = (int)polylinechars[index++] - 63;
                        sum |= (next5bits & 31) << shifter;
                        shifter += 5;
                    } while (next5bits >= 32 && index < polylinechars.Length);

                    if (index >= polylinechars.Length)
                        break;

                    currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                    //calculate next longitude
                    sum = 0;
                    shifter = 0;
                    do
                    {
                        next5bits = (int)polylinechars[index++] - 63;
                        sum |= (next5bits & 31) << shifter;
                        shifter += 5;
                    } while (next5bits >= 32 && index < polylinechars.Length);

                    if (index >= polylinechars.Length && next5bits >= 32)
                        break;

                    currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);
                    Location p = new Location();
                    p.Latitude = Convert.ToDouble(currentLat) / 100000.0;
                    p.Longitude = Convert.ToDouble(currentLng) / 100000.0;
                    poly.Add(p);
                }
            }
            catch (Exception ex)
            {
                // logo it
            }
            return poly;
        }
    }
}
