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
using System.Runtime.CompilerServices;
using Render;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
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
        List<double> _distanceLengthList;
        List<TSM.ContourPoint> _platformPointList;
        public RectangularPlatform(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;
            _platformList = new List<List<double>>();
            _distanceLengthList = new List<double>();
            _platformPointList = new List<TSM.ContourPoint>();

            SetPlatformData();
            createPlatform();
            CreateHandRailParallelToXAxis();
            CreateHandRailParallelToYAxis();
            CreateSupportPlate();
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

        public void CreateHandRailParallelToXAxis()
        {

            ContourPoint origin = _tModel.ShiftVertically(_global.Origin, elevation);
            for (int j = 0; j < 2; j++)
            {
                CalculateDistanceBetweenPost(width);
                ContourPoint sPoint = new ContourPoint();
                ContourPoint ePoint = new ContourPoint();


                if (j == 0)
                {
                    sPoint = new ContourPoint(new T3D.Point((origin.X + (width / 2)), origin.Y, origin.Z), null);
                    sPoint = new ContourPoint(new T3D.Point((sPoint.X - 250 - (_distanceLengthList[0] / 2)-50), sPoint.Y, sPoint.Z), null);
                    ePoint = new ContourPoint(new T3D.Point(sPoint.X, sPoint.Y + (height / 2), sPoint.Z), null);
                }

                else
                {

                    sPoint = new ContourPoint(new T3D.Point((origin.X - (width / 2)), origin.Y, origin.Z), null);
                    sPoint = new ContourPoint(new T3D.Point((sPoint.X + 250 + (_distanceLengthList[0] / 2)+50), sPoint.Y, sPoint.Z), null);
                    ePoint = new ContourPoint(new T3D.Point(sPoint.X, sPoint.Y - (height / 2), sPoint.Z), null);


                }


                for (int i = 0; i < _distanceLengthList.Count; i++)
                {
                    CustomPart CPart = new CustomPart();
                    CPart.Name = "Rectangular_Handrail";
                    CPart.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;
                    CPart.Position.Plane = Position.PlaneEnum.MIDDLE;
                    CPart.Position.PlaneOffset = 0.0;
                    CPart.Position.Depth = Position.DepthEnum.FRONT;
                    CPart.Position.DepthOffset = 0;
                    CPart.Position.Rotation = Position.RotationEnum.TOP;


                    //if only one distance available
                    if (_distanceLengthList.Count == 1)
                    {
                        if (_distanceLengthList[i] > 1100 && _distanceLengthList[i] <= 2600)
                        {
                            _distanceLengthList.Add(_distanceLengthList[i] - 600);
                            _distanceLengthList.RemoveAt(i);
                        }

                        if (_distanceLengthList[i] > 2500)
                        {
                            _distanceLengthList.Add((_distanceLengthList[i] - 1100) / 2);
                            _distanceLengthList.Add((_distanceLengthList[i] - 1100) / 2);
                            _distanceLengthList.RemoveAt(i + 1);
                        }

                        //To create only single handrail minimum 1100 distance is required 
                        if (_distanceLengthList[i] < 1100)
                        {
                            break;
                        }
                    }

                    //check the distance of last handrail and according to that either modify second last or keep as it is
                    if (i + 1 == _distanceLengthList.Count - 1)
                    {
                        if (_distanceLengthList[i + 1] >= 1200 && _distanceLengthList[i + 1] <= 2600)
                        {
                            _distanceLengthList.Add(_distanceLengthList[i + 1] - 600);
                            _distanceLengthList.RemoveAt(i + 1);
                        }


                        else if (_distanceLengthList[i + 1] > 2500)
                        {
                            double sum = (_distanceLengthList[i + 1]) / 2;
                            _distanceLengthList.RemoveAt(i + 1);
                            _distanceLengthList.Add(sum - 600);
                            _distanceLengthList.Add(sum - 600);

                        }

                        else if (_distanceLengthList[i + 1] > 600 && _distanceLengthList[i + 1] < 1200)
                        {
                            double sum = (_distanceLengthList[i + 1] + _distanceLengthList[i]) / 2;
                            _distanceLengthList.RemoveAt(i);
                            _distanceLengthList.RemoveAt(i);
                            _distanceLengthList.Add(sum);
                            _distanceLengthList.Add(sum - 600);
                        }

                    }


                   

                    CPart.SetInputPositions(sPoint, ePoint);

                    CPart.SetAttribute("width", height / 2);
                    CPart.SetAttribute("distance", _distanceLengthList[i]);
                    CPart.SetAttribute("P2", 0);

                    bool b = CPart.Insert();

                    if ((i + 1) <= _distanceLengthList.Count - 1)
                    {
                        if (j == 0)
                        {
                            sPoint = new ContourPoint(new T3D.Point((sPoint.X - (250 + _distanceLengthList[i] / 2) - (250 + _distanceLengthList[i + 1] / 2) - 150), sPoint.Y, sPoint.Z), null);
                            ePoint = new ContourPoint(new T3D.Point(sPoint.X, sPoint.Y + (height / 2), sPoint.Z), null);
                        }
                        else
                        {
                            sPoint = new ContourPoint(new T3D.Point((sPoint.X + (250 + _distanceLengthList[i] / 2) + (250 + _distanceLengthList[i + 1] / 2) + 150), sPoint.Y, sPoint.Z), null);
                            ePoint = new ContourPoint(new T3D.Point(sPoint.X, sPoint.Y - (height / 2), sPoint.Z), null);

                        }
                    }


                }
                _distanceLengthList.Clear();
            }
        }

        public void CreateHandRailParallelToYAxis()
        {

            ContourPoint origin = _tModel.ShiftVertically(_global.Origin, elevation);
            for (int j = 0; j < 2; j++)
            {
                CalculateDistanceBetweenPost(height);
                ContourPoint sPoint = new ContourPoint();
                ContourPoint ePoint = new ContourPoint();

                if (j == 0)
                {
                    sPoint = new ContourPoint(new T3D.Point((origin.X), origin.Y + (height / 2), origin.Z), null);
                    sPoint = new ContourPoint(new T3D.Point((sPoint.X), sPoint.Y - 250 - (_distanceLengthList[0] / 2), sPoint.Z), null);
                    ePoint = new ContourPoint(new T3D.Point(sPoint.X + (width / 2), sPoint.Y, sPoint.Z), null);
                }

                else
                {

                    sPoint = new ContourPoint(new T3D.Point((origin.X), origin.Y - (height / 2), origin.Z), null);
                    sPoint = new ContourPoint(new T3D.Point((sPoint.X), sPoint.Y + 250 + (_distanceLengthList[0] / 2), sPoint.Z), null);
                    ePoint = new ContourPoint(new T3D.Point(sPoint.X - (width / 2), sPoint.Y, sPoint.Z), null);


                }


                for (int i = 0; i < _distanceLengthList.Count; i++)
                {
                    CustomPart CPart = new CustomPart();
                    CPart.Name = "Rectangular_Handrail";
                    CPart.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;
                    CPart.Position.Plane = Position.PlaneEnum.MIDDLE;
                    CPart.Position.PlaneOffset = 0.0;
                    CPart.Position.Depth = Position.DepthEnum.FRONT;
                    CPart.Position.DepthOffset = 0;
                    CPart.Position.Rotation = Position.RotationEnum.TOP;


                    //if only one distance available
                    if (_distanceLengthList.Count == 1)
                    {
                        if (_distanceLengthList[i] > 1100 && _distanceLengthList[i] <= 2600)
                        {
                            _distanceLengthList.Add(_distanceLengthList[i] - 600);
                            _distanceLengthList.RemoveAt(i);
                        }

                        if (_distanceLengthList[i] > 2500)
                        {
                            _distanceLengthList.Add((_distanceLengthList[i] - 1100) / 2);
                            _distanceLengthList.Add((_distanceLengthList[i] - 1100) / 2);
                            _distanceLengthList.RemoveAt(i + 1);
                        }

                        //To create only single handrail minimum 1100 distance is required 
                        if (_distanceLengthList[i] < 1100)
                        {
                            break;
                        }
                    }

                    //check the distance of last handrail and according to that either modify second last or keep as it is
                    if (i + 1 == _distanceLengthList.Count - 1)
                    {
                        if (_distanceLengthList[i + 1] >= 1200 && _distanceLengthList[i + 1] <= 2600)
                        {
                            _distanceLengthList.Add(_distanceLengthList[i + 1] - 600);
                            _distanceLengthList.RemoveAt(i + 1);
                        }


                        else if (_distanceLengthList[i + 1] > 2500)
                        {
                            double sum = (_distanceLengthList[i + 1] + _distanceLengthList[i]) / 2;
                            _distanceLengthList.RemoveAt(i + 1);
                            _distanceLengthList.Add(sum);
                            _distanceLengthList.Add(sum);

                        }

                        else if (_distanceLengthList[i + 1] > 600 && _distanceLengthList[i + 1] < 1200)
                        {
                            double sum = (_distanceLengthList[i + 1] + _distanceLengthList[i]) / 2;
                            _distanceLengthList.RemoveAt(i);
                            _distanceLengthList.RemoveAt(i);
                            _distanceLengthList.Add(sum);
                            _distanceLengthList.Add(sum - 600);
                        }

                    }
                    CPart.SetInputPositions(sPoint, ePoint);

                    CPart.SetAttribute("width", width / 2);
                    CPart.SetAttribute("distance", _distanceLengthList[i]);
                    CPart.SetAttribute("P2", 0);

                    bool b = CPart.Insert();

                    if ((i + 1) <= _distanceLengthList.Count - 1)
                    {
                        if (j == 0)
                        {
                            sPoint = new ContourPoint(new T3D.Point(sPoint.X, sPoint.Y - (250 + _distanceLengthList[i] / 2) - (250 + _distanceLengthList[i + 1] / 2) - 100, sPoint.Z), null);
                            ePoint = new ContourPoint(new T3D.Point(sPoint.X + (height / 2), sPoint.Y, sPoint.Z), null);
                        }
                        else
                        {
                            sPoint = new ContourPoint(new T3D.Point(sPoint.X, sPoint.Y + (250 + _distanceLengthList[i] / 2) + (250 + _distanceLengthList[i + 1] / 2) + 100, sPoint.Z), null);
                            ePoint = new ContourPoint(new T3D.Point(sPoint.X - (height / 2), sPoint.Y, sPoint.Z), null);

                        }


                    }

                }
                _distanceLengthList.Clear();

            }

        }

        public void CalculateDistanceBetweenPost(double totalDistance)
        {

            double tempArcLength = 0.0;
            while (totalDistance > 0)
            {
                tempArcLength = totalDistance - 2600;
                if (tempArcLength < 600)
                {
                    _distanceLengthList.Add(totalDistance);
                    break;
                }
                else
                {
                    _distanceLengthList.Add(2000);
                    totalDistance = totalDistance - 2600;
                }
            }

        }


        public void CreateSupportPlate()
        {
            TSM.ContourPoint origin = _tModel.ShiftVertically(_global.Origin, elevation);
            TSM.ContourPoint point1 = new ContourPoint(new T3D.Point((origin.X + (width / 2)), origin.Y, origin.Z - 7.7), null);
            point1.Y += (height / 2);
            _platformPointList.Add(point1);
            TSM.ContourPoint point2 = new ContourPoint(new T3D.Point(point1.X, (point1.Y - height), point1.Z), null);
            _platformPointList.Add(point2);
            TSM.ContourPoint point3 = new ContourPoint(new T3D.Point(point2.X - width, (point2.Y), point2.Z), null);
            _platformPointList.Add(point3);
            TSM.ContourPoint point4 = new ContourPoint(new T3D.Point(point3.X, (point3.Y + height), point3.Z), null);
            _platformPointList.Add(point4);

            for (int i = 0; i < 4; i++)
            {
                _global.ProfileStr = "ISMC100";
                _global.Position.Plane = Position.PlaneEnum.MIDDLE;
                _global.Position.Depth = Position.DepthEnum.BEHIND;
                _global.Position.Rotation = Position.RotationEnum.TOP;
                _global.ClassStr = "2";

                if (i == 0)
                {
                    _platformPointList[i].Y += 25;
                    _platformPointList[i + 1].Y -= 25;
                    

                    _tModel.CreateBeam(_platformPointList[i], _platformPointList[i + 1], _global.ProfileStr, "ASTMA106", _global.ClassStr, _global.Position, "beam" + (i + 1));
                    _platformPointList[i].Y -= 25;
                    _platformPointList[i + 1].Y += 25;
                   
                }
                else if (i == 1)
                {
                    _platformPointList[i].X += 25;
                    _platformPointList[i + 1].X -= 25;
                    _tModel.CreateBeam(_platformPointList[i], _platformPointList[i + 1], _global.ProfileStr, "ASTMA106", _global.ClassStr, _global.Position, "beam" + (i + 1));
                    _platformPointList[i].X -= 25;
                    _platformPointList[i].X += 25;
                }
                else if (i == 3)
                {
                    _platformPointList[i].X -= 25;
                    _platformPointList[0].X += 50;
                    _tModel.CreateBeam(_platformPointList[i], _platformPointList[0], _global.ProfileStr, "ASTMA106", _global.ClassStr, _global.Position, "beam" + (i + 1));
                    _platformPointList[i].X += 25;
                    _platformPointList[0].X -= 25;
                }
                else
                {
                    _platformPointList[i].Y -= 25;
                    _platformPointList[i + 1].Y += 25;
                    _platformPointList[i].X += 25;
                    _tModel.CreateBeam(_platformPointList[i], _platformPointList[i + 1], _global.ProfileStr, "ASTMA106", _global.ClassStr, _global.Position, "beam" + (i + 1));
                    _platformPointList[i].Y += 25;
                    _platformPointList[i + 1].Y -= 25;
                    _platformPointList[i].X -= 25;
                }
            }
        }

       

    }


        


}
       

