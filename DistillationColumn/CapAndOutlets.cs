using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;
using TSM = Tekla.Structures.Model;
using T3D = Tekla.Structures.Geometry3d;
using Tekla.Structures.Geometry3d;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DistillationColumn
{
    internal class CapAndOutlets
    {
        Globals _global;
        TeklaModelling _tModel;

        double elevation=68000;
        double platformElevation;
        double height1;
        List<List<double>> _platformList;
        List<ContourPoint> _pointList;


        public CapAndOutlets(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;
            _platformList = new List<List<double>>();
            _pointList = new List<ContourPoint>();

            createCapAndOutlets();
            capBrackets();
        }
        public void createCapAndOutlets()
        {
            double heightOfOutletAboveCap = 2000;
            double heightOfOutletBelowCap = 300;
            double middleOutletRadius = 300;
            double sideOutletRadius = 150;
            double radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList, true);
            double diameter = 2 * radius;
            

            TSM.ContourPoint origin = new TSM.ContourPoint(_global.Origin, null);
            TSM.ContourPoint point1 = _tModel.ShiftVertically(origin, elevation);
            TSM.ContourPoint capTop = _tModel.ShiftVertically(point1, radius);
            TSM.ContourPoint capBottom = _tModel.ShiftVertically(point1, -radius);
            _global.Position.Plane = TSM.Position.PlaneEnum.MIDDLE;
            _global.Position.Depth = TSM.Position.DepthEnum.MIDDLE;
            string profile = "CAP" + diameter;
            Beam cap = _tModel.CreateBeam(capTop, capBottom, profile, "IS2062", "8", _global.Position, "cap");


            TSM.ContourPoint middleOutletTop = _tModel.ShiftVertically(capTop, heightOfOutletAboveCap);
            _tModel.CreateBeam(new T3D.Point(capTop.X, capTop.Y, capTop.Z - heightOfOutletBelowCap), middleOutletTop, "PIPE" + middleOutletRadius + "*10", "IS2062", "5", _global.Position, "");
            TSM.ContourPoint outletCap = _tModel.ShiftVertically(middleOutletTop, 20);
            _tModel.CreateBeam(middleOutletTop, outletCap, "ROD" + (middleOutletRadius + 50), "IS2062", "6", _global.Position, "");
            Beam cut = _tModel.CreateBeam(new T3D.Point(capTop.X, capTop.Y, capTop.Z - heightOfOutletBelowCap), middleOutletTop, "ROD" + middleOutletRadius, "IS2062", BooleanPart.BooleanOperativeClassName, _global.Position, "");
            _tModel.cutPart(cut, cap);
            Cuts(new T3D.Point(capTop.X, capTop.Y, capTop.Z - heightOfOutletBelowCap), middleOutletTop, middleOutletRadius);

            TSM.ContourPoint point5 = _tModel.ShiftHorizontallyRad(capTop, radius/2, 1, -45 * (Math.PI / 180));
            for (int i = 1; i <= 4; i++)
            {
                if ((i * 90) <= 360)
                {
                    TSM.ContourPoint sideOutletBottom = _tModel.ShiftAlongCircumferenceRad(point5, i * (90 * (Math.PI / 180)), 1);
                    TSM.ContourPoint sideOutletTop = _tModel.ShiftVertically(sideOutletBottom, heightOfOutletAboveCap);
                    _tModel.CreateBeam(new T3D.Point(sideOutletBottom.X, sideOutletBottom.Y, sideOutletBottom.Z - heightOfOutletBelowCap), sideOutletTop, "PIPE" + sideOutletRadius + "*10", "IS2062", "5", _global.Position, "");
                    Beam cut1 = _tModel.CreateBeam(new T3D.Point(sideOutletBottom.X, sideOutletBottom.Y, sideOutletBottom.Z - heightOfOutletBelowCap), sideOutletTop, "ROD" + sideOutletRadius, "IS2062", BooleanPart.BooleanOperativeClassName, _global.Position, "");
                    _tModel.cutPart(cut1, cap);
                    Cuts(new T3D.Point(sideOutletBottom.X, sideOutletBottom.Y, sideOutletBottom.Z - heightOfOutletBelowCap), sideOutletTop, sideOutletRadius);
                    TSM.ContourPoint point8 = _tModel.ShiftVertically(sideOutletTop, 20);
                    _tModel.CreateBeam(sideOutletTop, point8, "ROD" + (sideOutletRadius + 50), "IS2062", "6", _global.Position, "");

                }
            }

        }

       

        public void Cuts(T3D.Point p1,T3D.Point p2,double radius)
        {
            foreach(var parts in _global.platformParts)
            {
                Beam cut2 = _tModel.CreateBeam(p1, p2, "ROD"+radius, "IS2062", BooleanPart.BooleanOperativeClassName, _global.Position, "");
                _tModel.cutPart(cut2, parts);

            }
        }

        public void capBrackets()
        {
            double radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList, true);
            TSM.ContourPoint origin = new TSM.ContourPoint(_global.Origin, null);
            TSM.ContourPoint point1 = _tModel.ShiftVertically(origin, elevation);
            TSM.ContourPoint capTop = _tModel.ShiftVertically(point1, radius);
            height1 = RectangularPlatform.elevation - capTop.Z;
            TSM.ContourPoint point5 = _tModel.ShiftHorizontallyRad(capTop, (radius)/2, 1, 0 * (Math.PI / 180));
            for (int i = 1; i <= 4; i++)
            {
                if ((i * 90) <= 360)
                {
                    double ang = i * (90 * (Math.PI / 180));
                    if (ang >= 90 * (Math.PI / 180))
                    {
                        _global.Position.Rotation = TSM.Position.RotationEnum.TOP;
                    }
                    if (ang >= 180 * (Math.PI / 180))
                    {
                        _global.Position.Rotation = TSM.Position.RotationEnum.BACK;
                    }
                    if (ang >= 270 * (Math.PI / 180))
                    {
                        _global.Position.Rotation = TSM.Position.RotationEnum.BELOW;
                    }
                    if (ang >= 360 * (Math.PI / 180))
                    {
                        _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;
                    }
                    TSM.ContourPoint capBracketBottom = _tModel.ShiftAlongCircumferenceRad(point5, ang, 1);
                    TSM.ContourPoint capBracketTop = _tModel.ShiftVertically(capBracketBottom, height1);
                    _tModel.CreateBeam(new T3D.Point(capBracketBottom.X, capBracketBottom.Y, capBracketBottom.Z - 300), capBracketTop, "ISMC100", "IS2062", "2", _global.Position, "");


                }
            }

            TSM.ContourPoint point6 = _tModel.ShiftHorizontallyRad(capTop, radius-100, 1, 45 * (Math.PI / 180));
            for (int i = 0; i < 4; i++)
            {
                if ((i * 90) <= 360)
                {
                    double ang = i * (-90 * (Math.PI / 180));
                    _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;
                    TSM.ContourPoint capPlatformBracketBottom = _tModel.ShiftAlongCircumferenceRad(point6, ang, 1);
                    TSM.ContourPoint capPlatformBracketTop = RectangularPlatform._platformPointList[i];
                    _tModel.CreateBeam(new T3D.Point(capPlatformBracketBottom.X, capPlatformBracketBottom.Y, capPlatformBracketBottom.Z - radius), capPlatformBracketTop, "ISMC100", "IS2062", "2", _global.Position, "");
                }
            }

        }

        
        


    }
}
