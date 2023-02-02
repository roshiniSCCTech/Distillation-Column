using HelperLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSM = Tekla.Structures.Model;
using Tekla.Structures.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Tekla.Structures.Geometry3d;
using Tekla.Structures;
using Tekla.Structures.Datatype;
using Tekla.Structures.ModelInternal;

namespace DistillationColumn
{
    class Ladder
    {
        Globals _global;
        TeklaModelling _tModel;

        double orientationAngle;
        double startAngle;
        double endAngle;
        double elevation;
        double width = 470;
        double rungSpacing;
        double obstructionDist;
        double ladderBase = 0;
        double radius;
        double platformElevation;

        TSM.ContourPoint point2;
        TSM.ContourPoint point21;

        List<List<double>> _ladderList;
        List<TSM.ContourPoint> _box = new List<TSM.ContourPoint>();
        List<TSM.ContourPoint> _pointsList;

        public Ladder(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;

            _ladderList = new List<List<double>>();
            _pointsList = new List<TSM.ContourPoint>();

            int lastStackCount1 = _global.StackSegList.Count - 1;
            double stackElevation1 = _global.StackSegList[lastStackCount1][4] + _global.StackSegList[lastStackCount1][3];
            double radius2 = (_global.StackSegList[lastStackCount1][1]) / 2;
            platformElevation = stackElevation1 + radius2 + 1000;

            SetLadderData();
            CreateLadder();
        }

        public void SetLadderData()
        {
            List<JToken> ladderList = _global.JData["Ladder"].ToList();
            foreach (JToken ladder in ladderList)
            {
                orientationAngle = (float)ladder["Orientation_Angle"];
                elevation = (float)ladder["Elevation"];
                //width = (float)ladder["Width"];
                //height = (float)ladder["Height"];
                rungSpacing = (float)ladder["Rungs_spacing"];
                obstructionDist = (float)ladder["Obstruction_Distance"];
                startAngle = (float)ladder["Platform_Start_Angle"];
                endAngle = (float)ladder["Platfrom_End_Angle"];
                _ladderList.Add(new List<double> { orientationAngle, elevation, rungSpacing, obstructionDist, startAngle, endAngle });
            }
            List<JToken> ladderList1 = _global.JData["RectangularPlatform"].ToList();
            foreach (JToken ladder in ladderList1)
            {
                orientationAngle = (float)ladder["Orientation_Angle"];
                elevation = platformElevation;
                rungSpacing = (float)ladder["Rungs_spacing"];
                obstructionDist = (float)ladder["Obstruction_Distance"];
                startAngle = (float)ladder["Platform_Start_Angle"];
                endAngle = (float)ladder["Platfrom_End_Angle"];
                _ladderList.Add(new List<double> { orientationAngle, elevation, rungSpacing, obstructionDist, startAngle, endAngle });
            }

            List<JToken> ladderBaseList = _global.JData["chair"].ToList();
            foreach (JToken ladder in ladderBaseList)
            {
                ladderBase = (float)ladder["height"] + (float)ladder["top_ring_thickness"] + (float)ladder["bottom_ring_thickness"];
            }
        }

        public void CreateLadder()
        {
            int count1 = 0;
            foreach (List<double> ladder in _ladderList)
            {

                elevation = ladder[1];
                orientationAngle = ladder[0] * Math.PI / 180;
                startAngle = ladder[4] * Math.PI / 180;
                endAngle = ladder[5] * Math.PI / 180;
                double Height = (elevation) - ladderBase + (4 * ladder[2]);

                radius = _tModel.GetRadiusAtElevation(ladderBase, _global.StackSegList, true);
                double count = 0;               

                if (count1 == _ladderList.Count - 1)
                {
                    Height = elevation - ladderBase + 500;
                }

                TSM.ContourPoint origin = new TSM.ContourPoint(_global.Origin, null);
                TSM.ContourPoint point1 = _tModel.ShiftVertically(origin, ladderBase);
                int no = _tModel.GetSegmentAtElevation(ladderBase, _global.StackSegList);
                int no1 = _tModel.GetSegmentAtElevation(ladderBase + Height, _global.StackSegList);
                for (int i = no; i <= no1; i++)
                {
                    if (_global.StackSegList[i][0] != _global.StackSegList[i][1])
                        count++;
                }

                

                if (count != 0)
                {
                    point2 = _tModel.ShiftHorizontallyRad(point1, radius + Math.Max(400, ladder[3]), 1, orientationAngle);
                }
                else
                {
                    point2 = _tModel.ShiftHorizontallyRad(point1, radius + Math.Max(200, ladder[3]), 1, orientationAngle);
                }


                TSM.ContourPoint point11 = _tModel.ShiftVertically(point1, Height);
                double radius1 = _tModel.GetRadiusAtElevation(point11.Z- _global.Origin.Z, _global.StackSegList, true);
                point21 = _tModel.ShiftHorizontallyRad(point11, radius1 + Math.Max(200, ladder[3]), 1, orientationAngle);
                int lastStackCount = _global.StackSegList.Count - 1;
                double stackElevation = _global.StackSegList[lastStackCount][4] + _global.StackSegList[lastStackCount][3];
                if (elevation >= stackElevation)
                {
                    radius1 = (_global.StackSegList[lastStackCount][1]) / 2;
                    point21 = _tModel.ShiftHorizontallyRad(point11, radius1 + Math.Max(200, ladder[3]), 1, orientationAngle);
                }

                CustomPart Ladder = new CustomPart();
                Ladder.Name = "Ladder1";
                Ladder.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;

                Ladder.SetInputPositions(point2, point21);
                Ladder.SetAttribute("P1", width);  //Ladder Width
                Ladder.SetAttribute("P2", Height);  // Ladder Height
                Ladder.SetAttribute("P3", ladder[2]);  // Ladder Dist btwn Rungs



                Ladder.Position.Depth = Position.DepthEnum.MIDDLE;
                if (count == 0 && count1 != _ladderList.Count - 1)
                {
                    double angle = orientationAngle * (180 / Math.PI);
                    if (angle >= 0 && angle <= 45)
                    {
                        Ladder.Position.Rotation = Position.RotationEnum.FRONT;
                        Ladder.Position.RotationOffset = angle;
                    }
                    if (angle > 45 && angle <= 90)
                    {
                        Ladder.Position.Rotation = Position.RotationEnum.TOP;
                        Ladder.Position.RotationOffset = angle - 90;
                    }
                    if (angle > 90 && angle <= 135)
                    {
                        Ladder.Position.Rotation = Position.RotationEnum.TOP;
                        Ladder.Position.RotationOffset = angle - 90;
                    }
                    if (angle > 135 && angle <= 180)
                    {
                        Ladder.Position.Rotation = Position.RotationEnum.BACK;
                        Ladder.Position.RotationOffset = angle - 180;
                    }
                    if (angle > 180 && angle <= 225)
                    {
                        Ladder.Position.Rotation = Position.RotationEnum.BACK;
                        Ladder.Position.RotationOffset = angle - 180;
                    }
                    if (angle > 225 && angle <= 270)
                    {
                        Ladder.Position.Rotation = Position.RotationEnum.BELOW;
                        Ladder.Position.RotationOffset = angle - 270;
                    }
                    if (angle > 270 && angle < 315)
                    {
                        Ladder.Position.Rotation = Position.RotationEnum.BELOW;
                        Ladder.Position.RotationOffset = 315 - angle;
                    }
                    if (angle >= 315 && angle <= 360)
                    {
                        Ladder.Position.Rotation = Position.RotationEnum.FRONT;
                        Ladder.Position.RotationOffset = angle - 360;
                    }
                }
                else
                {
                    Ladder.Position.Rotation = Position.RotationEnum.BACK;
                    Ladder.Position.RotationOffset = 0;
                }
                

                Ladder.Insert();

                if (Height > 3000)
                {
                    Detail D = new Detail();
                    D.Name = "LadderHoop";
                    D.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;
                    D.LoadAttributesFromFile("standard");
                    D.UpVector = new Vector(0, 0, 0);
                    D.PositionType = PositionTypeEnum.MIDDLE_PLANE;
                    D.AutoDirectionType = AutoDirectionTypeEnum.AUTODIR_DETAIL;
                    D.DetailType = DetailTypeEnum.END;

                    D.SetPrimaryObject(Ladder);
                    D.SetReferencePoint(point21);
                    if (orientationAngle == startAngle)
                    {
                        D.SetAttribute("P1", 0);           //Ladder top hoop open at both sides
                        D.SetAttribute("P2", 1);           //Ladder top hoop open at right side
                        D.SetAttribute("P3", 0);           //Ladder top hoop open at left side

                    }
                    else if (orientationAngle == endAngle)
                    {
                        D.SetAttribute("P1", 0);           //Ladder top hoop open at both sides
                        D.SetAttribute("P2", 0);           //Ladder top hoop open at right side
                        D.SetAttribute("P3", 1);           //Ladder top hoop open at left side

                    }
                    else
                    {
                        D.SetAttribute("P1", 1);           //Ladder top hoop open at both sides
                        D.SetAttribute("P2", 0);           //Ladder top hoop open at right side
                        D.SetAttribute("P3", 0);           //Ladder top hoop open at left side
                    }

                    D.SetAttribute("P4", Height);      //Height of ladder
                    D.SetAttribute("P5", 460);         //Width of ladder
                    D.Insert();

                }

                _tModel.Model.CommitChanges();
                double RungDistance = 0;
                if (count1 == _ladderList.Count - 1)
                {
                    RungDistance = 450 + (2 * rungSpacing);
                    CreatePlate(point2, point21, RungDistance,count);
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {

                        if (i == 0)
                        {
                            RungDistance = 450 + (2 * rungSpacing);
                        }
                        if (i == 1)
                        {
                            RungDistance = elevation - ladderBase;
                        }
                        if (i == 2)
                        {
                            RungDistance = 450 + (((Height / rungSpacing) - 2) * rungSpacing);
                        }
                        CreatePlate(point2, point21, RungDistance,count);


                    }
                }
                ladderBase = elevation;
                count1++;
            }
           
            point21 = new ContourPoint(_tModel.ShiftVertically(point21, -(point21.Z - (platformElevation+ _global.Origin.Z))), null);
            createSquareCut(point21);

        }

        public void createSquareCut(TSM.ContourPoint point11)
        {
            TSM.ContourPoint point1 = _tModel.ShiftHorizontallyRad(point11, 900, 1);
            TSM.ContourPoint point2 = _tModel.ShiftHorizontallyRad(point11, 150, 3);
            TSM.ContourPoint bottomLeft = new ContourPoint(_tModel.ShiftHorizontallyRad(point1, 600, 4), null);
            TSM.ContourPoint topLeft = new ContourPoint(_tModel.ShiftHorizontallyRad(point2, 600, 4), null);
            TSM.ContourPoint bottomRight = new ContourPoint(_tModel.ShiftHorizontallyRad(point1, 600, 2), null);
            TSM.ContourPoint topRight = new ContourPoint(_tModel.ShiftHorizontallyRad(point2, 600, 2), null);

            _box.Add(bottomLeft);
            _box.Add(bottomRight);
            _box.Add(topRight);
            _box.Add(topLeft);
            _global.Position.Depth = Position.DepthEnum.MIDDLE;
            int count = 0;
            foreach (var parts in _global.platformParts)
            {
                ContourPlate cut = _tModel.CreateContourPlate(_box, "PL400", "IS2062", BooleanPart.BooleanOperativeClassName, _global.Position, "");
                _tModel.cutPart(cut, parts);
                count++;

            }
            _tModel.Model.CommitChanges();
        }

        void CreateBolts(ContourPlate p1, ContourPlate p2, ContourPoint p3, ContourPoint p4)
        {
            BoltArray B = new BoltArray();

            B.PartToBeBolted = p1;
            B.PartToBoltTo = p2;


            TSM.ContourPoint point0 = _tModel.ShiftHorizontallyRad(p3, -30, 1, orientationAngle);
            TSM.ContourPoint point1 = _tModel.ShiftHorizontallyRad(p4, -30, 1, orientationAngle);
            B.FirstPosition = point0;
            B.SecondPosition = point1;

            B.BoltSize = 20;
            B.Tolerance = 3.00;
            B.BoltStandard = "UNDEFINED_BOLT";
            B.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
            B.CutLength = 100;

            B.Length = 50;
            B.ExtraLength = 15;
            B.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;


            B.Position.Rotation = _global.Position.Rotation;
            B.Position.RotationOffset= _global.Position.RotationOffset;

            B.StartPointOffset.Dx = 30;

            B.Bolt = true;
            B.Washer1 = true;
            B.Washer2 = true;
            B.Washer3 = true;
            B.Nut1 = true;
            B.Nut2 = true;

            B.Hole1 = true;
            B.Hole2 = true;
            B.Hole3 = true;
            B.Hole4 = true;
            B.Hole5 = true;

            B.AddBoltDistX(60);
            B.AddBoltDistY(0);


            if (!B.Insert())
                Console.WriteLine("BoltXYList Insert failed!");
            _tModel.Model.CommitChanges();
        }

        void CreatePlate(TSM.ContourPoint point2, TSM.ContourPoint point21, double rungDistance, double count)
        {

            double inclinationDistance = _tModel.DistanceBetweenPoints(point21, point2);
            double heightOfLadder = (point21.Z - point2.Z);
            double inclinationAngle = Math.Asin(heightOfLadder / inclinationDistance);

            TSM.ContourPoint origin = new TSM.ContourPoint(_global.Origin, null);
            TSM.ContourPoint rightstartPoint = point2;
            TSM.ContourPoint rungmidpoint = _tModel.ShiftHorizontallyRad(rightstartPoint, rungDistance * Math.Cos(inclinationAngle), 3, orientationAngle);
            rungmidpoint = _tModel.ShiftVertically(rungmidpoint, rungDistance * Math.Sin(inclinationAngle));

            origin = _tModel.ShiftVertically(origin, rungmidpoint.Z - _global.Origin.Z);
            double distancefromorigintorungmid = _tModel.DistanceBetweenPoints(origin, rungmidpoint) - (50 + 75 - 30);
            TSM.ContourPoint rightrungmidpoint = _tModel.ShiftHorizontallyRad(rungmidpoint, ((width / 2) + 50), 2);

            double distance = (60 / Math.Tan(inclinationAngle));

            //right side plate

            TSM.ContourPoint ladderRightTopPoint = _tModel.ShiftVertically(rightrungmidpoint, 60);
            ladderRightTopPoint = _tModel.ShiftHorizontallyRad(ladderRightTopPoint, 50, 3);
            ladderRightTopPoint = _tModel.ShiftHorizontallyRad(ladderRightTopPoint, distance, 3, orientationAngle);
            TSM.ContourPoint ladderRightBottomPoint = _tModel.ShiftVertically(rightrungmidpoint, -60);
            ladderRightBottomPoint = _tModel.ShiftHorizontallyRad(ladderRightBottomPoint, 50, 3);
            ladderRightBottomPoint = _tModel.ShiftHorizontallyRad(ladderRightBottomPoint, distance, 1, orientationAngle);


            TSM.ContourPoint rightbackTopPoint = _tModel.ShiftHorizontallyRad(ladderRightTopPoint, 75 + 30, 3, orientationAngle);
            TSM.ContourPoint rightbackBottomPoint = _tModel.ShiftHorizontallyRad(ladderRightBottomPoint, 75 + 30, 3, orientationAngle);

            _global.Position.Depth = TSM.Position.DepthEnum.FRONT;

            _pointsList.Add(ladderRightTopPoint);
            _pointsList.Add(ladderRightBottomPoint);
            _pointsList.Add(rightbackBottomPoint);
            _pointsList.Add(rightbackTopPoint);

            ContourPlate smallRightPlate = _tModel.CreateContourPlate(_pointsList, "PLT10", Globals.MaterialStr, "9", _global.Position, "");
            _pointsList.Clear();



            //stack  right  plate

            rightbackTopPoint = _tModel.ShiftHorizontallyRad(rightbackTopPoint, -60, 3, orientationAngle);
            double radiusatRightTop = _tModel.GetRadiusAtElevation(rightbackTopPoint.Z - _global.Origin.Z, _global.StackSegList, true);
            rightbackBottomPoint = _tModel.ShiftHorizontallyRad(rightbackBottomPoint, -60, 3, orientationAngle);
            double radiusatRightBottom = _tModel.GetRadiusAtElevation(rightbackBottomPoint.Z - _global.Origin.Z, _global.StackSegList, true);

            TSM.ContourPoint rightstackTopPoint = _tModel.ShiftHorizontallyRad(rightbackTopPoint, distancefromorigintorungmid - (Math.Sqrt(Math.Pow(radiusatRightTop, 2) - Math.Pow((width / 2) + 50, 2))), 3, orientationAngle);
            TSM.ContourPoint rightstackBottomPoint = _tModel.ShiftHorizontallyRad(rightbackBottomPoint, distancefromorigintorungmid - (Math.Sqrt(Math.Pow(radiusatRightBottom, 2) - Math.Pow((width / 2) + 50, 2))), 3, orientationAngle);

            _global.Position.Depth = TSM.Position.DepthEnum.FRONT;
            _pointsList.Add(rightstackTopPoint);
            _pointsList.Add(rightstackBottomPoint);
            _pointsList.Add(rightbackBottomPoint);
            _pointsList.Add(rightbackTopPoint);

            ContourPlate largeRightPlate = _tModel.CreateContourPlate(_pointsList, "PLT10", Globals.MaterialStr, "9", _global.Position, "");
            _pointsList.Clear();




            //left side plate

            TSM.ContourPoint leftrungmidpoint = _tModel.ShiftHorizontallyRad(rungmidpoint, ((width / 2) + 50), 4);

            TSM.ContourPoint ladderLeftTopPoint = _tModel.ShiftVertically(leftrungmidpoint, 60);
            ladderLeftTopPoint = _tModel.ShiftHorizontallyRad(ladderLeftTopPoint, 50, 3);
            ladderLeftTopPoint = _tModel.ShiftHorizontallyRad(ladderLeftTopPoint, distance, 3, orientationAngle);
            TSM.ContourPoint ladderLeftBottomPoint = _tModel.ShiftVertically(leftrungmidpoint, -60);
            ladderLeftBottomPoint = _tModel.ShiftHorizontallyRad(ladderLeftBottomPoint, 50, 3);
            ladderLeftBottomPoint = _tModel.ShiftHorizontallyRad(ladderLeftBottomPoint, distance, 1, orientationAngle);


            TSM.ContourPoint leftbackTopPoint = _tModel.ShiftHorizontallyRad(ladderLeftTopPoint, 75 + 30, 3, orientationAngle);
            TSM.ContourPoint leftbackBottomPoint = _tModel.ShiftHorizontallyRad(ladderLeftBottomPoint, 75 + 30, 3, orientationAngle);

            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;

            _pointsList.Add(ladderLeftTopPoint);
            _pointsList.Add(ladderLeftBottomPoint);
            _pointsList.Add(leftbackBottomPoint);
            _pointsList.Add(leftbackTopPoint);

            ContourPlate smallLeftPlate = _tModel.CreateContourPlate(_pointsList, "PLT10", Globals.MaterialStr, "9", _global.Position, "");
            _pointsList.Clear();



            //stack left plate

            leftbackTopPoint = _tModel.ShiftHorizontallyRad(leftbackTopPoint, -60, 3, orientationAngle);
            double radiusatLefttTop = _tModel.GetRadiusAtElevation(leftbackTopPoint.Z - _global.Origin.Z, _global.StackSegList, true);
            leftbackBottomPoint = _tModel.ShiftHorizontallyRad(leftbackBottomPoint, -60, 3, orientationAngle);
            double radiusatLeftBottom = _tModel.GetRadiusAtElevation(leftbackBottomPoint.Z - _global.Origin.Z, _global.StackSegList, true);

            TSM.ContourPoint leftstackTopPoint = _tModel.ShiftHorizontallyRad(leftbackTopPoint, distancefromorigintorungmid - (Math.Sqrt(Math.Pow(radiusatRightTop, 2) - Math.Pow((width / 2) + 50, 2))), 3, orientationAngle);
            TSM.ContourPoint leftstackBottomPoint = _tModel.ShiftHorizontallyRad(leftbackBottomPoint, distancefromorigintorungmid - (Math.Sqrt(Math.Pow(radiusatRightBottom, 2) - Math.Pow((width / 2) + 50, 2))), 3, orientationAngle);

            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;
            _pointsList.Add(leftstackTopPoint);
            _pointsList.Add(leftstackBottomPoint);
            _pointsList.Add(leftbackBottomPoint);
            _pointsList.Add(leftbackTopPoint);

            ContourPlate largeLeftPlate = _tModel.CreateContourPlate(_pointsList, "PLT10", Globals.MaterialStr, "9", _global.Position, "");
            _pointsList.Clear();

            if (count == 0)
            {
                double angle = orientationAngle * (180 / Math.PI);
                if (angle >= 0 && angle < 45)
                {
                    _global.Position.Rotation = Position.RotationEnum.BELOW;
                    _global.Position.RotationOffset = 0 - angle;
                    CreateBolts(smallRightPlate, largeRightPlate, rightbackTopPoint, rightbackBottomPoint);

                    _global.Position.Rotation = Position.RotationEnum.TOP;
                    _global.Position.RotationOffset = 0 - angle;
                    CreateBolts(smallLeftPlate, largeLeftPlate, leftbackTopPoint, leftbackBottomPoint);
                }
                if (angle >= 45 && angle <= 90)
                {
                    _global.Position.Rotation = Position.RotationEnum.BACK;
                    _global.Position.RotationOffset = 90-angle;
                    CreateBolts(smallRightPlate, largeRightPlate, rightbackTopPoint, rightbackBottomPoint);

                    _global.Position.Rotation = Position.RotationEnum.FRONT;
                    _global.Position.RotationOffset = 90 - angle;
                    CreateBolts(smallLeftPlate, largeLeftPlate, leftbackTopPoint, leftbackBottomPoint);
                   
                }
                if (angle > 90 && angle < 135)
                {
                    _global.Position.Rotation = Position.RotationEnum.BACK;
                    _global.Position.RotationOffset = 90 - angle;
                    CreateBolts(smallRightPlate, largeRightPlate, rightbackTopPoint, rightbackBottomPoint);

                    _global.Position.Rotation = Position.RotationEnum.FRONT;
                    _global.Position.RotationOffset = 90 - angle;
                    CreateBolts(smallLeftPlate, largeLeftPlate, leftbackTopPoint, leftbackBottomPoint);
                    
                }
                if (angle >= 135 && angle < 225)
                {
                    _global.Position.Rotation = Position.RotationEnum.TOP;
                    _global.Position.RotationOffset = 180 - angle;
                    CreateBolts(smallRightPlate, largeRightPlate, rightbackTopPoint, rightbackBottomPoint);

                    _global.Position.Rotation = Position.RotationEnum.BELOW;
                    _global.Position.RotationOffset = 180 - angle;
                    CreateBolts(smallLeftPlate, largeLeftPlate, leftbackTopPoint, leftbackBottomPoint);
                    
                }
               
                if (angle >= 225 && angle < 315)
                {
                    _global.Position.Rotation = Position.RotationEnum.FRONT;
                    _global.Position.RotationOffset = 270 - angle;
                    CreateBolts(smallRightPlate, largeRightPlate, rightbackTopPoint, rightbackBottomPoint);

                    _global.Position.Rotation = Position.RotationEnum.BACK;
                    _global.Position.RotationOffset = 270 - angle;
                    CreateBolts(smallLeftPlate, largeLeftPlate, leftbackTopPoint, leftbackBottomPoint);
                   
                }
                
                if (angle >= 315 && angle <= 360)
                {
                    _global.Position.Rotation = Position.RotationEnum.BELOW;
                    _global.Position.RotationOffset = 360 - angle;
                    CreateBolts(smallRightPlate, largeRightPlate, rightbackTopPoint, rightbackBottomPoint);

                    _global.Position.Rotation = Position.RotationEnum.TOP;
                    _global.Position.RotationOffset = 360 - angle;
                    CreateBolts(smallLeftPlate, largeLeftPlate, leftbackTopPoint, leftbackBottomPoint);
                    
                }
            }
            else
            {
                _global.Position.Rotation = Position.RotationEnum.BELOW;
                _global.Position.RotationOffset = 0;
                CreateBolts(smallRightPlate, largeRightPlate, rightbackTopPoint, rightbackBottomPoint);

                _global.Position.Rotation = Position.RotationEnum.TOP;
                _global.Position.RotationOffset = 0;
                CreateBolts(smallLeftPlate, largeLeftPlate, leftbackTopPoint, leftbackBottomPoint);
            }

           


        }



    }
}

