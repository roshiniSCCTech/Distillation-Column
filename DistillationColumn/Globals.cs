using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSM = Tekla.Structures.Model;
using T3D = Tekla.Structures.Geometry3d;
using Newtonsoft.Json.Linq;
using Tekla.Structures.Model;

namespace DistillationColumn
{
    class Globals
    {
        public string ProfileStr;
        public const string MaterialStr = "IS2062";
        public string ClassStr;
        public string NameStr;
        public TSM.Position Position;

        public readonly TSM.ContourPoint Origin;

        // 0 - bottom inner diameter, 1 - top inner diameter, 2 - thickness, 3 - height, 4 - height from base of stack to bottom of segment
        public readonly List<List<double>> StackSegList;
        public JObject JData;
        public List<Part> platformParts = new List<Part>();
        public List<double> _originPoints = new List<double>();
        // list of stack segment parts
        public readonly List<TSM.Beam> SegmentPartList;


        public Globals()
        {
            //double x=
            //Origin = new TSM.ContourPoint(new T3D.Point(0, 0, 0), null);
            ProfileStr = "";
            ClassStr = "";
            NameStr = "";
            Position = new TSM.Position();
            StackSegList = new List<List<double>>();
            SegmentPartList = new List<TSM.Beam>();

            string jDataString = File.ReadAllText("test2.json");
            JData = JObject.Parse(jDataString);
            SetOriginData();
            Origin = new TSM.ContourPoint(new T3D.Point(_originPoints[0], _originPoints[1], _originPoints[2]), null);
            SetStackData();
            CalculateElevation();
        }

        public void SetStackData()
        {
            List<JToken> stackList = JData["stack"].ToList();
            foreach (JToken stackSeg in stackList)
            {
                double bottomDiameter = (float)stackSeg["inside_dia_bottom"] * 1000; // inside bottom diamter
                double topDiameter = (float)stackSeg["inside_dia_top"] * 1000; // inside top diameter
                double thickness = (float)stackSeg["shell_thickness"] * 1000;
                double height = (float)stackSeg["seg_height"] * 1000;

                StackSegList.Add(new List<double> { bottomDiameter, topDiameter, thickness, height });
            }
            StackSegList.Reverse();
        }

        void CalculateElevation()
        {
            double elevation = 0;

            foreach (List<double> segment in StackSegList)
            {
                segment.Add(elevation);
                elevation += segment[3];
            }
        }

        void SetOriginData()
        {
            List<JToken> orignData = JData["origin"].ToList();
            foreach (JToken _originCordinates in orignData)
            {
                double x = (double)_originCordinates["x"];
                double y = (double)_originCordinates["y"];
                double z = (double)_originCordinates["z"];
                _originPoints.Add(x);
                _originPoints.Add(y);
                _originPoints.Add(z);
            }
        }
    }
}
