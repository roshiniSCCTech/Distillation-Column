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
//using Render;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.IO.Ports;
using Tekla.Structures.Geometry3d;
using System.Collections;
using Tekla.Structures.Model.Operations;

namespace DistillationColumn
{
    internal class RectangularPlatform
    {
        Globals _global;
        TeklaModelling _tModel;

        static public double elevation;
        double height;
        double width;
        double plateWidth;
        double plateThickness = 40;
        string profile1;
        List<List<double>> _platformList;
        List<double> _distanceLengthList;
        static public List<TSM.ContourPoint> _platformPointList;
        List<Point> Intersect;
        ArrayList myList = new ArrayList();
        List<Part> _platformBracketParts;
        //List<Point> my = new List<Point>();
        public RectangularPlatform(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;
            _platformList = new List<List<double>>();
            _distanceLengthList = new List<double>();
            _platformPointList = new List<TSM.ContourPoint>();
            Intersect = new List<Point>();
            _platformBracketParts = new List<Part>();


            SetPlatformData();
            createPlatform();
            CreateHandRailParallelToXAxis();
            CreateHandRailParallelToYAxis();
            CreateSupportPlate();
            //createSupportBrackets();
            //createSupportBrackets1();
        }

        public void SetPlatformData()
        {
            List<JToken> platformlist = _global.JData["RectangularPlatform"].ToList();
            foreach (JToken platform in platformlist)
            {
                elevation = (float)platform["elevation"];
                height = (float)platform["height"];
                width = (float)platform["width"];
                plateWidth = (float)platform["plateWidth"];
                _platformList.Add(new List<double> { elevation, height, width, plateWidth });
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
                        profile1 = "PL" + plateWidth + "*"+plateThickness;
                    }
                    else
                    {
                        double plateWidth1 = width - (i * plateWidth);
                        profile1 = "PL" + plateWidth1 + "*"+plateThickness;

                    }
                    T3D.Point start = new T3D.Point(point2.X - i * plateWidth, point2.Y, point2.Z);
                    T3D.Point end = new T3D.Point(point2.X - i * plateWidth, point2.Y + height / 2, point2.Z);
                    T3D.Point end2 = new T3D.Point(point2.X - i * plateWidth, -(point2.Y + height / 2), point2.Z);
                    _global.Position.Depth = TSM.Position.DepthEnum.FRONT;
                    _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;
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
                ManageLastDistance();
                ContourPoint sPoint = new ContourPoint();
                ContourPoint ePoint = new ContourPoint();


                if (j == 0)
                {
                    sPoint = new ContourPoint(new T3D.Point((origin.X + (width / 2)), origin.Y, origin.Z), null);
                    sPoint = new ContourPoint(new T3D.Point((sPoint.X - 250 - (_distanceLengthList[0] / 2) - 50), sPoint.Y, sPoint.Z), null);
                    ePoint = new ContourPoint(new T3D.Point(sPoint.X, sPoint.Y + (height / 2), sPoint.Z), null);
                }

                else
                {

                    sPoint = new ContourPoint(new T3D.Point((origin.X - (width / 2)), origin.Y, origin.Z), null);
                    sPoint = new ContourPoint(new T3D.Point((sPoint.X + 250 + (_distanceLengthList[0] / 2) + 50), sPoint.Y, sPoint.Z), null);
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


                    CPart.SetInputPositions(sPoint, ePoint);

                    CPart.SetAttribute("width", height / 2);
                    CPart.SetAttribute("distance", _distanceLengthList[i]);
                    CPart.SetAttribute("P2", 0);

                    bool b = CPart.Insert();

                    if ((i + 1) <= _distanceLengthList.Count - 1)
                    {
                        if (j == 0)
                        {
                            sPoint = new ContourPoint(new T3D.Point((sPoint.X - (250 + _distanceLengthList[i] / 2) - (250 + _distanceLengthList[i + 1] / 2) - 100), sPoint.Y, sPoint.Z), null);
                            ePoint = new ContourPoint(new T3D.Point(sPoint.X, sPoint.Y + (height / 2), sPoint.Z), null);
                        }
                        else
                        {
                            sPoint = new ContourPoint(new T3D.Point((sPoint.X + (250 + _distanceLengthList[i] / 2) + (250 + _distanceLengthList[i + 1] / 2) + 100), sPoint.Y, sPoint.Z), null);
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
                ManageLastDistance();
                ContourPoint sPoint = new ContourPoint();
                ContourPoint ePoint = new ContourPoint();

                if (j == 0)
                {
                    sPoint = new ContourPoint(new T3D.Point((origin.X), origin.Y + (height / 2)-50, origin.Z), null);
                    sPoint = new ContourPoint(new T3D.Point((sPoint.X), sPoint.Y - 250 - (_distanceLengthList[0] / 2), sPoint.Z), null);
                    ePoint = new ContourPoint(new T3D.Point(sPoint.X + (width / 2), sPoint.Y, sPoint.Z), null);
                }

                else
                {

                    sPoint = new ContourPoint(new T3D.Point((origin.X), origin.Y - (height / 2)+50, origin.Z), null);
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

        public void ManageLastDistance()
        {
            int n = _distanceLengthList.Count - 1;
            if (n == 0)
            {
                n = 1;
            }
            for(int i=0;i< n;i++)
            {
                //if only one distance available
                if (_distanceLengthList.Count == 1)
                {
                    if (_distanceLengthList[i] >= 1200 && _distanceLengthList[i] <= 2600)
                    {
                        _distanceLengthList.Add(_distanceLengthList[i] - 600);
                        _distanceLengthList.RemoveAt(i);
                    }

                    if (_distanceLengthList[i] > 2500)
                    {
                        _distanceLengthList.Add((_distanceLengthList[i] - 1200) / 2);
                        _distanceLengthList.Add((_distanceLengthList[i] - 1200) / 2);
                        _distanceLengthList.RemoveAt(i);
                    }

                    //To create only single handrail minimum 1200 distance is required 
                    if (_distanceLengthList[0] < 1200)
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


                    else if (_distanceLengthList[i + 1] > 2600)
                    {
                        double sum = (_distanceLengthList[i + 1]) / 2;
                        _distanceLengthList.RemoveAt(i + 1);
                        _distanceLengthList.Add(sum-600);
                        _distanceLengthList.Add(sum-600);

                    }

                    else if (_distanceLengthList[i + 1] >=600 && _distanceLengthList[i + 1] < 1200)
                    {
                        double sum = (_distanceLengthList[i + 1] + _distanceLengthList[i]) / 2;
                        _distanceLengthList.RemoveAt(i);
                        _distanceLengthList.RemoveAt(i);
                        _distanceLengthList.Add(sum);
                        _distanceLengthList.Add(sum - 600);
                    }

                }
            }
           
        }


        public void CreateSupportPlate()
        {
            TSM.ContourPoint origin = _tModel.ShiftVertically(_global.Origin, elevation);
            TSM.ContourPoint point1 = new ContourPoint(new T3D.Point((origin.X + (width / 2)), origin.Y, origin.Z), null);
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
                    _platformPointList[0].X += 25;
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



        public void createSupportBrackets()
        {
            _global.ProfileStr = "ISMC100";
            _global.Position.Plane = Position.PlaneEnum.MIDDLE;
            _global.Position.Depth = Position.DepthEnum.BEHIND;
            _global.Position.Rotation = Position.RotationEnum.TOP;
            _global.ClassStr = "2";

            TSM.ContourPoint origin = _tModel.ShiftVertically(_global.Origin, elevation);
            TSM.ContourPoint point1 = _tModel.ShiftHorizontallyRad(origin, width / 2, 1);
            TSM.ContourPoint point2 = new ContourPoint(new T3D.Point(point1.X - 200, point1.Y + height / 2, point1.Z), null);
            TSM.ContourPoint point3 = new ContourPoint(new T3D.Point(point2.X, (point2.Y - height), point2.Z), null);
            Beam p1 = _tModel.CreateBeam(point2, point3, _global.ProfileStr, "ASTMA106", _global.ClassStr, _global.Position, "beam");
            _platformBracketParts.Add(p1);

            TSM.ContourPoint point21 = new ContourPoint(new T3D.Point(point1.X - width + 200, point1.Y + height / 2, point1.Z), null);
            TSM.ContourPoint point31 = new ContourPoint(new T3D.Point(point21.X, (point21.Y - height), point21.Z), null);
            Beam p2 = _tModel.CreateBeam(point21, point31, _global.ProfileStr, "ASTMA106", _global.ClassStr, _global.Position, "beam");
            _platformBracketParts.Add(p2);

            double no = (width - 400) / 1000;
            var x = (no - Math.Truncate(no)) * 1000;
            if (x > 200)
            {
                double no1 = Math.Truncate(no);

                for (int i = 1; i <= no1; i++)
                {

                    TSM.ContourPoint point4 = new ContourPoint(new T3D.Point(point2.X - i * 1000, point2.Y, point2.Z), null);
                    TSM.ContourPoint point5 = new ContourPoint(new T3D.Point(point3.X - i * 1000, (point3.Y), point3.Z), null);
                    Beam p3 = _tModel.CreateBeam(point4, point5, _global.ProfileStr, "ASTMA106", _global.ClassStr, _global.Position, "beam");
                    _platformBracketParts.Add(p3);
                }
            }
            else
            {
                double no1 = Math.Truncate(no);

                for (int i = 1; i <= no1 - 1; i++)
                {

                    TSM.ContourPoint point4 = new ContourPoint(new T3D.Point(point2.X - i * 1000, point2.Y, point2.Z), null);
                    TSM.ContourPoint point5 = new ContourPoint(new T3D.Point(point3.X - i * 1000, (point3.Y), point3.Z), null);
                    Beam p4 = _tModel.CreateBeam(point4, point5, _global.ProfileStr, "ASTMA106", _global.ClassStr, _global.Position, "beam");
                    _platformBracketParts.Add(p4);
                }
                double dist = x + 1000;
                dist = dist / 2;

                TSM.ContourPoint point41 = new ContourPoint(new T3D.Point(point2.X - (no1 - 1) * 1000 - dist, point2.Y, point2.Z), null);
                TSM.ContourPoint point51 = new ContourPoint(new T3D.Point(point3.X - (no1 - 1) * 1000 - dist, (point3.Y), point3.Z), null);
                Beam p5 = _tModel.CreateBeam(point41, point51, _global.ProfileStr, "ASTMA106", _global.ClassStr, _global.Position, "beam");
                _platformBracketParts.Add(p5);
            }
        }

        public void createSupportBrackets1()
        {
            TSM.ContourPoint origin = _tModel.ShiftVertically(_global.Origin, elevation);
            TSM.ContourPoint point1 = _tModel.ShiftHorizontallyRad(origin, width / 2, 1);
            TSM.ContourPoint point2 = new ContourPoint(new T3D.Point(point1.X, (point1.Y - height / 2) + 200, point1.Z), null);
            TSM.ContourPoint point3 = new ContourPoint(new T3D.Point(point2.X - width, (point2.Y), point2.Z), null);
            Beam p1 = _tModel.CreateBeam(point2, point3, _global.ProfileStr, "ASTMA106", _global.ClassStr, _global.Position, "beam");
            _platformBracketParts.Add(p1);
            Intersection(point2, point3);

            TSM.ContourPoint point21 = new ContourPoint(new T3D.Point(point1.X, (point1.Y + height / 2) - 200, point1.Z), null);
            TSM.ContourPoint point31 = new ContourPoint(new T3D.Point(point21.X - width, (point21.Y), point21.Z), null);
            Beam p2 = _tModel.CreateBeam(point21, point31, _global.ProfileStr, "ASTMA106", _global.ClassStr, _global.Position, "beam");
            _platformBracketParts.Add(p2);

            double no = (height - 400) / 1000;
            var x = (no - Math.Truncate(no)) * 1000;
            if (x > 200)
            {
                double no1 = Math.Truncate(no);

                for (int i = 1; i <= no1; i++)
                {

                    TSM.ContourPoint point4 = new ContourPoint(new T3D.Point(point2.X, point2.Y + i * 1000, point2.Z), null);
                    TSM.ContourPoint point5 = new ContourPoint(new T3D.Point(point3.X, point3.Y + i * 1000, point3.Z), null);
                    Beam p3 = _tModel.CreateBeam(point4, point5, _global.ProfileStr, "ASTMA106", _global.ClassStr, _global.Position, "beam");
                    _platformBracketParts.Add(p3);
                    Intersection(point4, point5);
                }
            }
            else
            {
                double no1 = Math.Truncate(no);

                for (int i = 1; i <= no1 - 1; i++)
                {
                    TSM.ContourPoint point4 = new ContourPoint(new T3D.Point(point2.X, point2.Y + i * 1000, point2.Z), null);
                    TSM.ContourPoint point5 = new ContourPoint(new T3D.Point(point3.X, point3.Y + i * 1000, point3.Z), null);
                    Beam p4 = _tModel.CreateBeam(point4, point5, _global.ProfileStr, "ASTMA106", _global.ClassStr, _global.Position, "beam");
                    _platformBracketParts.Add(p4);
                    Intersection(point4, point5);

                }
                double dist = x + 1000;
                dist = dist / 2;

                TSM.ContourPoint point41 = new ContourPoint(new T3D.Point(point2.X, point2.Y + (no1 - 1) * 1000 + dist, point2.Z), null);
                TSM.ContourPoint point51 = new ContourPoint(new T3D.Point(point3.X, point3.Y + (no1 - 1) * 1000 + dist, point3.Z), null);
                Beam p5 = _tModel.CreateBeam(point41, point51, _global.ProfileStr, "ASTMA106", _global.ClassStr, _global.Position, "beam");
                _platformBracketParts.Add(p5);

                Intersection(point41, point51);
            }
            Intersection(point21, point31);
        }

        public void Intersection(Point p1, Point p2)
        {
            foreach (Beam part in _platformBracketParts)
            {
                Solid Solid = part.GetSolid();
                myList = Solid.Intersect(p1, p2);
                if (myList.Count == 0)
                    break;

                for (int i = 0; i < myList.Count; i++)
                {
                    Point intersect = (Point)myList[i];
                    Intersect.Add(intersect);
                }
                foreach (var p in Intersect)
                {
                    Operation.Split(part, p);
                    _tModel.Model.CommitChanges();
                  
                }


            }


        }

    }





}


