using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSM = Tekla.Structures.Model;
using T3D = Tekla.Structures.Geometry3d;
using Newtonsoft.Json.Linq;

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

        // 0 - bottom inner diameter, 1 - top inner diameter, 2 - thickness, 3 - height, 4 - height from base stack to bottom of segment
        public readonly List<List<double>> StackSegList;
        public JObject jData;

        public Globals()
        {
            Origin = new TSM.ContourPoint(new T3D.Point(0, 0, 0), null);
            ProfileStr = "";
            ClassStr = "";
            NameStr = "";
            Position = new TSM.Position();
            StackSegList = new List<List<double>>();
            string jDataString = File.ReadAllText("Data.json");
            jData = JObject.Parse(jDataString);

            GetData();
            CalculateElevation();
        }

        public void GetData()
        {
            
            List<JToken> stackList = jData["stack"].ToList();
            foreach (JToken stackSeg in stackList)
            {
                double height = (float)stackSeg["seg_height"] * 1000;
                double topDiameter = (float)stackSeg["inside_dia_top"] * 1000; // inside top diameter
                double bottomDiameter = (float)stackSeg["inside_dia_bottom"] * 1000; // inside bottom diamter
                double thickness = (float)stackSeg["shell_thickness"] * 1000;

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
    }
}
