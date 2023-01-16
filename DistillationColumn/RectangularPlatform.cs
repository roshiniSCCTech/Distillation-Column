using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSM = Tekla.Structures.Model;
using T3D = Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Newtonsoft.Json.Linq;
using System.IO.Ports;

namespace DistillationColumn
{
    internal class RectangularPlatform
    {
        Globals _global;
        TeklaModelling _tModel;

        double elevation;
        double height;
        double width;
        double plateWidth;
        string profile1;
        List<List<double>> _platformList;
        public RectangularPlatform(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;
            _platformList = new List<List<double>>();

            SetPlatformData();
            createPlatform();
        }

        public void SetPlatformData()
        {
            List<JToken> platformlist = _global.jData["RectangularPlatform"].ToList();
            foreach (JToken platform in platformlist)
            {
                elevation = (float)platform["elevation"];
                height = (float)platform["height"];
                width = (float)platform["width"];
                plateWidth = (float)platform["plateWidth"];
                _platformList.Add(new List<double> { elevation,  height , width,plateWidth });
            }

        }
        public void createPlatform()
        {
            foreach (List<double> platform in _platformList)
            {
                double radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList, true);
                TSM.ContourPoint origin = new TSM.ContourPoint(_global.Origin, null);
                TSM.ContourPoint point1 = _tModel.ShiftVertically(origin, elevation);
                TSM.ContourPoint point2 = _tModel.ShiftHorizontallyRad(point1, width / 2, 1);
                double number = width / plateWidth;

                for (int i = 0; i <= number; i++)
                {
                    if ((i + 1)* plateWidth <= width)
                    { 
                        profile1 = "PL" + plateWidth + "*20";
                    }
                    else
                    {
                        double plateWidth1 = width - (i * plateWidth);
                        profile1 = "PL" + plateWidth1 + "*20";

                    }
                    T3D.Point start = new T3D.Point(point2.X - i * plateWidth, point2.Y, point2.Z);
                    T3D.Point end = new T3D.Point(point2.X - i * plateWidth, point2.Y + height / 2, point2.Z);
                    T3D.Point end2 = new T3D.Point(point2.X - i * plateWidth, -(point2.Y + height / 2), point2.Z);
                    _global.Position.Plane = TSM.Position.PlaneEnum.LEFT;

                    Beam beamRight = _tModel.CreateBeam(start, end, profile1, "IS2062", "5", _global.Position, "");
                    _global.platformParts.Add(beamRight);
                    _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
                    Beam beamLeft = _tModel.CreateBeam(start, end2, profile1, "IS2062", "5", _global.Position, "");
                    _global.platformParts.Add(beamLeft);


                    
                }
            }
        }




    }
}
