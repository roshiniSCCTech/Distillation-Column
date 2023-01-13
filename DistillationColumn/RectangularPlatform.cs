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

namespace DistillationColumn
{
    internal class RectangularPlatform
    {
        Globals _global;
        TeklaModelling _tModel;

        double elevation;
        double height;
        double width;
        List<List<double>> _platformList;
        List<double> _distanceLengthList;
        public RectangularPlatform(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;
            _platformList = new List<List<double>>();
            _distanceLengthList = new List<double>();

            SetPlatformData();
            create();
            CreateHandRailParallelToXAxis();
            CreateHandRailParallelToYAxis();
        }

        public void SetPlatformData()
        {
            List<JToken> platformlist = _global.jData["RectangularPlatform"].ToList();
            foreach (JToken platform in platformlist)
            {
                elevation = (float)platform["elevation"];
                height = (float)platform["height"];
                width = (float)platform["width"];
                _platformList.Add(new List<double> { elevation,  height , width });
            }
        }
        public void create ()
        {
            foreach (List<double> platform in _platformList)
            {
                double radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList, true);

                TSM.ContourPoint origin = new TSM.ContourPoint(_global.Origin, null);
                TSM.ContourPoint point1 = _tModel.ShiftVertically(origin, elevation);
                TSM.ContourPoint point2 = _tModel.ShiftHorizontallyRad(point1, width / 2, 1);
                double number = width / 400;

                for (int i = 0; i < number; i++)
                {
                    T3D.Point start = new T3D.Point(point2.X - i * 400, point2.Y, point2.Z);
                    T3D.Point end = new T3D.Point(point2.X - i * 400, point2.Y + height / 2, point2.Z);
                    T3D.Point end2 = new T3D.Point(point2.X - i * 400, -(point2.Y + height / 2), point2.Z);
                    _global.Position.Plane = TSM.Position.PlaneEnum.LEFT;

                    _tModel.CreateBeam(start, end, "PL400*20", "IS2062", "5", _global.Position, "");
                    _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
                    _tModel.CreateBeam(start, end2, "PL400*20", "IS2062", "5", _global.Position, "");
                }
            }
        }

        public void CreateHandRailParallelToXAxis()
        {
            
            ContourPoint origin = _tModel.ShiftVertically(_global.Origin, elevation);
           


            for (int j=0;j<2;j++)
            {
                CalculateDistanceBetweenPost(width);
                ContourPoint sPoint = new ContourPoint();
                ContourPoint ePoint = new ContourPoint();


                if (j==0)
                {
                    sPoint = new ContourPoint(new T3D.Point((origin.X + (width / 2)), origin.Y, origin.Z), null);
                    sPoint = new ContourPoint(new T3D.Point((sPoint.X - 250 - (_distanceLengthList[0] / 2)), sPoint.Y, sPoint.Z), null);
                    ePoint = new ContourPoint(new T3D.Point(sPoint.X, sPoint.Y + (height / 2), sPoint.Z), null);
                }

                else
                {

                    sPoint = new ContourPoint(new T3D.Point((origin.X - (width / 2)), origin.Y, origin.Z), null);
                    sPoint = new ContourPoint(new T3D.Point((sPoint.X + 250 + (_distanceLengthList[0] / 2)), sPoint.Y, sPoint.Z), null);
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
                            _distanceLengthList.Add(sum-600);
                            _distanceLengthList.Add(sum-600);

                        }

                        //else if (_distanceLengthList[i + 1] > 600 && _distanceLengthList[i + 1] < 1200)
                        //{
                        //    double sum = (_distanceLengthList[i + 1] + _distanceLengthList[i]) / 2;
                        //    _distanceLengthList.RemoveAt(i);
                        //    _distanceLengthList.RemoveAt(i);
                        //    _distanceLengthList.Add(sum);
                        //    _distanceLengthList.Add(sum - 600);
                        //}

                    }






                    CPart.SetInputPositions(sPoint, ePoint);
                   
                    CPart.SetAttribute("width", height / 2);
                    CPart.SetAttribute("distance", _distanceLengthList[i]);
                    CPart.SetAttribute("P2", 0);

                    bool b = CPart.Insert();

                    if ((i + 1) <= _distanceLengthList.Count - 1)
                    {
                        if(j==0)
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
                ContourPoint sPoint = new ContourPoint();
                ContourPoint ePoint = new ContourPoint();


                if (j == 0)
                {
                    sPoint = new ContourPoint(new T3D.Point((origin.X), origin.Y+(height/2), origin.Z), null);
                    sPoint = new ContourPoint(new T3D.Point((sPoint.X ), sPoint.Y - 250 - (_distanceLengthList[0] / 2), sPoint.Z), null);
                    ePoint = new ContourPoint(new T3D.Point(sPoint.X + (width / 2), sPoint.Y , sPoint.Z), null);
                }

                else
                {

                    sPoint = new ContourPoint(new T3D.Point((origin.X), origin.Y- (height / 2), origin.Z), null);
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
                            sPoint = new ContourPoint(new T3D.Point(sPoint.X , sPoint.Y - (250 + _distanceLengthList[i] / 2) - (250 + _distanceLengthList[i + 1] / 2) - 100, sPoint.Z), null);
                            ePoint = new ContourPoint(new T3D.Point(sPoint.X + (height / 2), sPoint.Y , sPoint.Z), null);
                        }
                        else
                        {
                            sPoint = new ContourPoint(new T3D.Point(sPoint.X , sPoint.Y + (250 + _distanceLengthList[i] / 2) + (250 + _distanceLengthList[i + 1] / 2) + 100, sPoint.Z), null);
                            ePoint = new ContourPoint(new T3D.Point(sPoint.X - (height / 2), sPoint.Y , sPoint.Z), null);

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

       


    }
}
