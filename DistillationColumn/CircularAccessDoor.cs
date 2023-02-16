using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSM = Tekla.Structures.Model;
using T3D = Tekla.Structures.Geometry3d;
using Newtonsoft.Json.Linq;
using System.Drawing.Imaging;
using System.Net;
using Tekla.Structures.Filtering;
using Tekla.Structures.Datatype;
using Tekla.Structures.Model;
using Tekla.Structures.ModelInternal;

namespace DistillationColumn
{
    internal class CircularAccessDoor
    {
        Globals _global;
        TeklaModelling _tModel;

        double orientationAngle;
        double elevation;
        double plateDiameter;
        double plateThickness;
        double plateRadius;
        double stackRadius;
        double widthofNeckPlate;
        double neckPlateThickness;
        double gasketPlateThickness;
        double widthOfGasketPlate;
        double padPlateThickness;
        double widthofLiningPlate;
        double coverPlateThickness;
        double hingeddistancefromcoverPlate;
        double hingedDiameter;
        double horizontalHingedDistance;
        double handleDistancefromplateorigin;
        double handleDistance;
        double HandleRodDiamter;
        double thicknessofLiningPlate;
        double numberOfBolts;
        double arcDistance;
        double hingedPlateThickness;
        PolyBeam flangePlate;
        PolyBeam gasketPlate;
        PolyBeam padPlate;

        TSM.ContourPoint stackOrigin;
        TSM.ContourPoint plateOrigin;
        TSM.ContourPoint plateTopPoint;
        TSM.ContourPoint plateBottomPoint;
        TSM.ContourPoint plateLeftPoint;
        TSM.ContourPoint plateRightPoint;

        TSM.ContourPoint padTopPoint;
        TSM.ContourPoint padRightPoint;
        TSM.ContourPoint padBottomPoint;
        TSM.ContourPoint padLeftPoint;


        List<TSM.ContourPoint> _pointsList;

        List<List<double>> _accessDoorList;
        //List<List<double>> _instrumentNozzleList;

        public CircularAccessDoor(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;

            //orientationAngle = 40 * Math.PI / 180;
            //elevation = 20000;
            //plateDiameter = 700;
           
            plateThickness = 20;
            padPlateThickness = 20;
            gasketPlateThickness = 6;
            coverPlateThickness = 6;
            //widthofNeckPlate = 112;
            //neckPlateThickness = 12;
            widthOfGasketPlate = 60;
            //widthofLiningPlate = 60;
            thicknessofLiningPlate = 6;
            arcDistance = 200;
            handleDistancefromplateorigin = 150;
            handleDistance = 75;
            HandleRodDiamter = 20;
            hingeddistancefromcoverPlate = 150;
            horizontalHingedDistance = 474;
            hingedDiameter = 60;
            hingedPlateThickness = 10;
            plateTopPoint = new TSM.ContourPoint();
            plateBottomPoint = new TSM.ContourPoint();
            plateLeftPoint = new TSM.ContourPoint();
            plateRightPoint = new TSM.ContourPoint();

            padTopPoint = new TSM.ContourPoint();
            padRightPoint = new TSM.ContourPoint();
            padBottomPoint = new TSM.ContourPoint();
            padLeftPoint = new TSM.ContourPoint();


            _pointsList = new List<TSM.ContourPoint>();
            _accessDoorList = new List<List<double>>();
            Build();


        }
        void Build()
        {
            List<JToken> circularAccessDoorList = _global.JData["CircularAccessDoor"].ToList();
            foreach (JToken cicularAccessDoor in circularAccessDoorList)
            {
                elevation = (float)cicularAccessDoor["elevation"];
                orientationAngle = (float)cicularAccessDoor["orientation_angle"];
                neckPlateThickness = (float)cicularAccessDoor["neck_plate_Thickness"];
                plateDiameter = (float)cicularAccessDoor["plate_Diameter"];              
                widthofNeckPlate = (float)cicularAccessDoor["neck_plate_width"];
                widthofLiningPlate = (float)cicularAccessDoor["lining_plate_width"];
                numberOfBolts = (float)cicularAccessDoor["number_of_bolts"];

                _accessDoorList.Add(new List<double> { elevation, orientationAngle,neckPlateThickness,plateDiameter,widthofNeckPlate,widthofLiningPlate,numberOfBolts});

            }

            foreach (List<double> circularAccessDoor in _accessDoorList)
            {
                elevation = circularAccessDoor[0];
                orientationAngle = circularAccessDoor[1];
                orientationAngle = orientationAngle * Math.PI / 180;
                neckPlateThickness = circularAccessDoor[2];
                plateDiameter = circularAccessDoor[3];
                plateRadius = plateDiameter / 2;
                widthofNeckPlate = circularAccessDoor[4];
                widthofLiningPlate = circularAccessDoor[5];
                numberOfBolts = circularAccessDoor[6];


                CreateCircularAccessDoor();
            }
        }

        void CreateCircularAccessDoor()
        {
            stackRadius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList, true);
            stackOrigin = new TSM.ContourPoint(_global.Origin, null);
            stackOrigin = _tModel.ShiftVertically(stackOrigin, elevation);
            plateOrigin = _tModel.ShiftHorizontallyRad(stackOrigin, stackRadius, 1, orientationAngle);

            CreatePadPlate();
            RefernceCutPlate();
            ReferenceneckPlate();
            CreateNeckPlate();
            CreateLiningPlate();
            CreateFlangePlate();
            CreateGasketPlate();
            CreateBolts();
            CreateCoverPlate();
            CreateHandle();
            CreateHinge();
        }
        void CreatePadPlate()
        {

            double halfPadPlateAngle = Math.Asin((plateRadius + neckPlateThickness) / stackRadius);
            TSM.ContourPoint startPoint = new TSM.ContourPoint(_tModel.ShiftAlongCircumferenceRad(plateOrigin, halfPadPlateAngle, 1), null);
            startPoint = _tModel.ShiftAlongCircumferenceRad(startPoint, arcDistance, 2);
            TSM.ContourPoint midPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(stackOrigin, stackRadius, 1, orientationAngle), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            TSM.ContourPoint endPoint = new TSM.ContourPoint(_tModel.ShiftAlongCircumferenceRad(plateOrigin, -(halfPadPlateAngle), 1), null);
            endPoint = _tModel.ShiftAlongCircumferenceRad(endPoint, -arcDistance, 2);

            _global.Position.Depth = TSM.Position.DepthEnum.MIDDLE;
            _global.Position.Plane = TSM.Position.PlaneEnum.LEFT;
            _global.Position.Rotation = TSM.Position.RotationEnum.TOP;
            _global.ProfileStr = "PL" + "50" + "*" + 2 * (plateRadius + arcDistance + neckPlateThickness);
            _pointsList.Add(startPoint);
            _pointsList.Add(midPoint);
            _pointsList.Add(endPoint);


            //padPlate = _tModel.CreateBeam(startPoint, endPoint, "PL" + "500" + "*" + 2*(plateRadius + arcDistance + neckPlateThickness), "IS2062", "11", _global.Position, "");

            padPlate = _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, "IS2062", "11", _global.Position, "");
            _pointsList.Clear();

        }
        void RefernceCutPlate()
        {
            //neckPlateThickness = 0;
            plateOrigin = _tModel.ShiftHorizontallyRad(stackOrigin, stackRadius, 1, orientationAngle);
            plateTopPoint = _tModel.ShiftVertically(plateOrigin, plateRadius + neckPlateThickness + arcDistance);
            plateBottomPoint = _tModel.ShiftVertically(plateOrigin, -(plateRadius + neckPlateThickness + arcDistance));
            plateLeftPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, (plateRadius + neckPlateThickness + arcDistance), 4), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            plateRightPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, (plateRadius + neckPlateThickness + arcDistance), 2), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));

            _global.Position.Depth = TSM.Position.DepthEnum.FRONT;
            _global.Position.Plane = TSM.Position.PlaneEnum.MIDDLE;
            _global.Position.Rotation = TSM.Position.RotationEnum.BACK;
            _global.ProfileStr = "PL" + (plateRadius + neckPlateThickness + arcDistance) + "*" + (plateRadius);


            _pointsList.Add(plateTopPoint);
            _pointsList.Add(plateRightPoint);
            _pointsList.Add(plateBottomPoint);
            _pointsList.Add(plateLeftPoint);
            _pointsList.Add(plateTopPoint);

            PolyBeam cutPlate = _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, Globals.MaterialStr, BooleanPart.BooleanOperativeClassName, _global.Position, "");
            _pointsList.Clear();

            _tModel.cutPart(cutPlate, padPlate);

        }
        void ReferenceneckPlate()
        {
            plateOrigin = _tModel.ShiftHorizontallyRad(stackOrigin, stackRadius, 1, orientationAngle);

            _global.Position.Depth = TSM.Position.DepthEnum.MIDDLE;
            _global.Position.Plane = TSM.Position.PlaneEnum.MIDDLE;
            _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;
            _global.ProfileStr = "ROD" + plateDiameter;


            //plateOrigin = _tModel.ShiftHorizontallyRad(stackOrigin, stackRadius, 1, orientationAngle);
            plateOrigin = _tModel.ShiftHorizontallyRad(stackOrigin, stackRadius - 500, 1, orientationAngle);
            TSM.ContourPoint point1 = _tModel.ShiftHorizontallyRad(plateOrigin, 1000, 1);
            Beam neckPlate1 = _tModel.CreateBeam(plateOrigin, point1, _global.ProfileStr, Globals.MaterialStr, BooleanPart.BooleanOperativeClassName, _global.Position, "");


            _tModel.cutPart(neckPlate1, padPlate);

        }

        void CreateNeckPlate()
        {
            plateOrigin = _tModel.ShiftHorizontallyRad(stackOrigin, stackRadius, 1, orientationAngle);
            plateTopPoint = _tModel.ShiftVertically(plateOrigin, plateRadius);
            plateBottomPoint = _tModel.ShiftVertically(plateOrigin, -plateRadius);
            plateLeftPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, plateRadius, 4), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            plateRightPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, plateRadius, 2), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));

            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;
            _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
            _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;


            _pointsList.Add(plateTopPoint);
            _pointsList.Add(plateRightPoint);
            _pointsList.Add(plateBottomPoint);
            _pointsList.Add(plateLeftPoint);
            _pointsList.Add(plateTopPoint);

            _tModel.CreatePolyBeam(_pointsList, "PL" + widthofNeckPlate + "*" + neckPlateThickness, Globals.MaterialStr, "12", _global.Position, "");

            _pointsList.Clear();

        }
        void CreateLiningPlate()
        {
            plateOrigin = _tModel.ShiftHorizontallyRad(stackOrigin, stackRadius + widthofNeckPlate, 1, orientationAngle);
            plateTopPoint = _tModel.ShiftVertically(plateOrigin, plateRadius);
            plateBottomPoint = _tModel.ShiftVertically(plateOrigin, -plateRadius);
            plateLeftPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, plateRadius, 4), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            plateRightPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, plateRadius, 2), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));

            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;
            _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
            _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;


            _pointsList.Add(plateTopPoint);
            _pointsList.Add(plateRightPoint);
            _pointsList.Add(plateBottomPoint);
            _pointsList.Add(plateLeftPoint);
            _pointsList.Add(plateTopPoint);

            _tModel.CreatePolyBeam(_pointsList, "PL" + widthofLiningPlate + "*" + thicknessofLiningPlate, Globals.MaterialStr, "12", _global.Position, "");
            _pointsList.Clear();
        }

        void CreateFlangePlate()
        {

            plateOrigin = _tModel.ShiftHorizontallyRad(stackOrigin, stackRadius + widthofNeckPlate, 1, orientationAngle);
            plateTopPoint = _tModel.ShiftVertically(plateOrigin, plateRadius);
            plateBottomPoint = _tModel.ShiftVertically(plateOrigin, -plateRadius);
            plateLeftPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, plateRadius, 4), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            plateRightPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, plateRadius, 2), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));

            double angle = 0;
            TSM.ContourPoint startPoint = plateTopPoint;
            ContourPoint endPoint = plateRightPoint;

            _global.Position.Rotation = TSM.Position.RotationEnum.BELOW;

            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    angle = (45 * Math.PI / 180);
                    endPoint = plateRightPoint;
                    _global.Position.Depth = TSM.Position.DepthEnum.FRONT;
                    _global.Position.Plane = TSM.Position.PlaneEnum.LEFT;
                }
                if (i == 1)
                {
                    angle = (315 * Math.PI / 180);
                    endPoint = plateBottomPoint;
                    _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;
                    _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
                }
                if (i == 2)
                {
                    angle = (215 * Math.PI / 180);
                    endPoint = plateLeftPoint;
                    _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;
                    _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
                }
                if (i == 3)
                {
                    angle = (135 * Math.PI / 180);
                    endPoint = plateTopPoint;
                    _global.Position.Depth = TSM.Position.DepthEnum.FRONT;
                    _global.Position.Plane = TSM.Position.PlaneEnum.LEFT;
                }


                TSM.ContourPoint midPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, plateRadius * Math.Cos(angle), 2), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                midPoint = _tModel.ShiftVertically(midPoint, plateRadius * Math.Sin(angle));

                _pointsList.Add(startPoint);
                _pointsList.Add(midPoint);
                _pointsList.Add(endPoint);


                flangePlate = _tModel.CreatePolyBeam(_pointsList, "PL" + widthOfGasketPlate + "*" + gasketPlateThickness, Globals.MaterialStr, "2", _global.Position, "");
                _pointsList.Clear();

                startPoint = endPoint;
            }
        }
        void CreateGasketPlate()
        {
            double angle = 0;
            TSM.ContourPoint startPoint = plateTopPoint;
            ContourPoint endPoint = plateRightPoint;

            _global.Position.Rotation = TSM.Position.RotationEnum.BELOW;

            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    angle = (45 * Math.PI / 180);
                    endPoint = plateRightPoint;
                    _global.Position.Depth = TSM.Position.DepthEnum.FRONT;
                    _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
                }
                if (i == 1)
                {
                    angle = (315 * Math.PI / 180);
                    endPoint = plateBottomPoint;
                    _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;
                    _global.Position.Plane = TSM.Position.PlaneEnum.LEFT;
                }
                if (i == 2)
                {
                    angle = (215 * Math.PI / 180);
                    endPoint = plateLeftPoint;
                    _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;
                    _global.Position.Plane = TSM.Position.PlaneEnum.LEFT;
                }
                if (i == 3)
                {
                    angle = (135 * Math.PI / 180);
                    endPoint = plateTopPoint;
                    _global.Position.Depth = TSM.Position.DepthEnum.FRONT;
                    _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
                }

                TSM.ContourPoint midPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, plateRadius * Math.Cos(angle), 2), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                midPoint = _tModel.ShiftVertically(midPoint, plateRadius * Math.Sin(angle));

                _pointsList.Add(startPoint);
                _pointsList.Add(midPoint);
                _pointsList.Add(endPoint);


                gasketPlate = _tModel.CreatePolyBeam(_pointsList, "PL" + widthOfGasketPlate + "*" + gasketPlateThickness, Globals.MaterialStr, "2", _global.Position, "");
                _pointsList.Clear();

                startPoint = endPoint;
            }
        }
        void CreateBolts()
        {
            BoltCircle B = new BoltCircle();

            B.PartToBeBolted = gasketPlate;
            B.PartToBoltTo = flangePlate;

            B.FirstPosition = plateOrigin;
            B.SecondPosition = _tModel.ShiftHorizontallyRad(plateOrigin, plateRadius, 4); ;

            B.BoltSize = 10;
            B.Tolerance = 3.00;
            B.BoltStandard = "8.8XOX";
            B.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
            B.CutLength = 105;

            B.Length = 100;
            B.ExtraLength = 15;
            B.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;


            B.Position.Rotation = Position.RotationEnum.BELOW;

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

            B.NumberOfBolts = numberOfBolts;
            B.Diameter = plateRadius * 2.15;
            B.Insert();
            _tModel.Model.CommitChanges();
        }
        void CreateCoverPlate()
        {
            plateOrigin = _tModel.ShiftHorizontallyRad(plateOrigin, widthofLiningPlate, 1, orientationAngle);
            plateTopPoint = _tModel.ShiftVertically(plateOrigin, plateRadius);
            plateBottomPoint = _tModel.ShiftVertically(plateOrigin, -plateRadius);
            plateLeftPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, plateRadius, 4), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            plateRightPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, plateRadius, 2), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));

            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;

            _pointsList.Add(plateTopPoint);
            _pointsList.Add(plateRightPoint);
            _pointsList.Add(plateBottomPoint);
            _pointsList.Add(plateLeftPoint);

            _tModel.CreateContourPlate(_pointsList, "PLT" + coverPlateThickness, Globals.MaterialStr, "99", _global.Position, "");
            _pointsList.Clear();

        }

        void CreateHandle()
        {
            plateOrigin = _tModel.ShiftHorizontallyRad(plateOrigin, handleDistancefromplateorigin, 4, orientationAngle);

            TSM.ContourPoint point1 = (_tModel.ShiftVertically(plateOrigin, handleDistance));
            TSM.ContourPoint point2 = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(point1, handleDistance, 1), new TSM.Chamfer(10, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
            TSM.ContourPoint point3 = (_tModel.ShiftVertically(point2, -handleDistancefromplateorigin));
            TSM.ContourPoint point4 = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(point3, handleDistance, 3), new TSM.Chamfer(10, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
            TSM.ContourPoint point5 = (_tModel.ShiftVertically(point4, handleDistance));

            _pointsList.Add(point1);
            _pointsList.Add(point2);
            _pointsList.Add(point3);
            _pointsList.Add(point4);

            _tModel.CreatePolyBeam(_pointsList, "ROD" + HandleRodDiamter, Globals.MaterialStr, "9", _global.Position, "");
            _pointsList.Clear();
        }

        void CreateHinge()
        {

            // hinge rod

            TSM.ContourPoint plateOrigin1 = _tModel.ShiftHorizontallyRad(stackOrigin, stackRadius, 1, orientationAngle);
            TSM.ContourPoint hingeRodMidPoint = _tModel.ShiftHorizontallyRad(plateOrigin1, plateRadius+170, 2, orientationAngle);
            hingeRodMidPoint = _tModel.ShiftHorizontallyRad(hingeRodMidPoint, widthofNeckPlate + widthofLiningPlate, 1, orientationAngle);

            TSM.ContourPoint hingeRodPoint1 = _tModel.ShiftVertically(hingeRodMidPoint, hingeddistancefromcoverPlate);
            TSM.ContourPoint hingeRodPoint11 = _tModel.ShiftVertically(hingeRodPoint1, 50);
            TSM.ContourPoint hingeRodPoint2 = _tModel.ShiftVertically(hingeRodMidPoint, -(hingeddistancefromcoverPlate));
            TSM.ContourPoint hingeRodPoint21 = _tModel.ShiftVertically(hingeRodPoint2, -50);

            _global.Position.Depth = TSM.Position.DepthEnum.MIDDLE;
            _global.Position.Plane = TSM.Position.PlaneEnum.MIDDLE;
            _global.Position.Rotation = TSM.Position.RotationEnum.BELOW;


            _tModel.CreateBeam(hingeRodPoint11, hingeRodPoint21, "NUT_M20", Globals.MaterialStr, _global.ClassStr, _global.Position, "HingeRod");



            // top horizontal hinged plate

            plateOrigin = _tModel.ShiftHorizontallyRad(plateOrigin, widthofLiningPlate, 1, orientationAngle);

            
            TSM.ContourPoint hingedOrigin = _tModel.ShiftHorizontallyRad(hingeRodPoint1, ((plateRadius+170)-plateRadius) + coverPlateThickness+plateThickness, 4,orientationAngle);
            TSM.ContourPoint upPoint = _tModel.ShiftHorizontallyRad(hingedOrigin, (hingedDiameter / 2), 3,orientationAngle);
            TSM.ContourPoint downPoint = _tModel.ShiftHorizontallyRad(hingedOrigin, (hingedDiameter / 2), 1,orientationAngle);
            TSM.ContourPoint downRight = _tModel.ShiftHorizontallyRad(hingeRodPoint1, (hingedDiameter / 2), 1, orientationAngle);
            downRight = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(downRight, (hingedDiameter / 2), 2, orientationAngle), new TSM.Chamfer(30, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
            TSM.ContourPoint UpRight = _tModel.ShiftHorizontallyRad(hingeRodPoint1, (hingedDiameter / 2), 3, orientationAngle);
            UpRight = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(UpRight, (hingedDiameter) / 2, 2, orientationAngle), new TSM.Chamfer(30, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));


            _global.Position.Depth = TSM.Position.DepthEnum.FRONT;

            _pointsList.Add(upPoint);
            _pointsList.Add(downPoint);
            _pointsList.Add(downRight);
            _pointsList.Add(UpRight);


            _tModel.CreateContourPlate(_pointsList, "PLT" + hingedPlateThickness, Globals.MaterialStr, "99", _global.Position, "");
            _pointsList.Clear();



            //bottom horizontal hinge plate

            
            hingedOrigin = _tModel.ShiftHorizontallyRad(hingeRodPoint2, ((plateRadius + 170) - plateRadius) +plateThickness + coverPlateThickness, 4,orientationAngle);

            upPoint = _tModel.ShiftHorizontallyRad(hingedOrigin, (hingedDiameter / 2), 3,orientationAngle);
            downPoint = _tModel.ShiftHorizontallyRad(hingedOrigin, (hingedDiameter / 2), 1,orientationAngle);
            downRight = _tModel.ShiftHorizontallyRad(hingeRodPoint2, (hingedDiameter / 2), 1, orientationAngle);
            downRight = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(downRight, (hingedDiameter / 2), 2, orientationAngle), new TSM.Chamfer(30, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
            UpRight = _tModel.ShiftHorizontallyRad(hingeRodPoint2, (hingedDiameter / 2), 3, orientationAngle);
            UpRight = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(UpRight, (hingedDiameter / 2), 2, orientationAngle), new TSM.Chamfer(30, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));


            _global.Position.Depth = TSM.Position.DepthEnum.FRONT;

            _pointsList.Add(upPoint);
            _pointsList.Add(downPoint);
            _pointsList.Add(downRight);
            _pointsList.Add(UpRight);


            _tModel.CreateContourPlate(_pointsList, "PLT" + hingedPlateThickness, Globals.MaterialStr, "99", _global.Position, "");
            _pointsList.Clear();



            //top vertical hinge plate

            double angle1 = ((plateRadius+170) / stackRadius);
            TSM.ContourPoint padMidPoint = _tModel.ShiftAlongCircumferenceRad(plateOrigin1, angle1, 1);
            padMidPoint = _tModel.ShiftVertically(padMidPoint, hingeddistancefromcoverPlate);
            TSM.ContourPoint plateLeftPoint = _tModel.ShiftHorizontallyRad(padMidPoint, hingedDiameter / 2, 4);
            TSM.ContourPoint plateRightPoint = _tModel.ShiftHorizontallyRad(padMidPoint, hingedDiameter / 2, 2);
            TSM.ContourPoint frontLeftPoint = _tModel.ShiftHorizontallyRad(hingeRodPoint1, (hingedDiameter / 2), 4, orientationAngle);
            frontLeftPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(frontLeftPoint, (hingedDiameter / 2), 1, orientationAngle), new TSM.Chamfer(30, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
            TSM.ContourPoint frontRightPoint = _tModel.ShiftHorizontallyRad(frontLeftPoint, (hingedDiameter / 2), 2, orientationAngle);
            frontRightPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(frontRightPoint, (hingedDiameter / 2), 2, orientationAngle), new TSM.Chamfer(30, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));

            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;

            _pointsList.Add(plateLeftPoint);
            _pointsList.Add(plateRightPoint);
            _pointsList.Add(frontRightPoint);
            _pointsList.Add(frontLeftPoint);


            _tModel.CreateContourPlate(_pointsList, "PLT" + hingedPlateThickness, Globals.MaterialStr, "99", _global.Position, "");
            _pointsList.Clear();



            //bottom vertical hinge plate

            padMidPoint = _tModel.ShiftVertically(padMidPoint, -(2 * hingeddistancefromcoverPlate));
            plateLeftPoint = _tModel.ShiftHorizontallyRad(padMidPoint, hingedDiameter / 2, 4);
            plateRightPoint = _tModel.ShiftHorizontallyRad(padMidPoint, hingedDiameter / 2, 2);
            frontLeftPoint = _tModel.ShiftHorizontallyRad(hingeRodPoint2, (hingedDiameter / 2), 4, orientationAngle);
            frontLeftPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(frontLeftPoint, (hingedDiameter / 2), 1, orientationAngle), new TSM.Chamfer(30, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
            frontRightPoint = _tModel.ShiftHorizontallyRad(hingeRodPoint2, (hingedDiameter / 2), 2, orientationAngle);
            frontRightPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(frontRightPoint, (hingedDiameter / 2), 1, orientationAngle), new TSM.Chamfer(30, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));

            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;

            _pointsList.Add(plateLeftPoint);
            _pointsList.Add(plateRightPoint);
            _pointsList.Add(frontRightPoint);
            _pointsList.Add(frontLeftPoint);


            _tModel.CreateContourPlate(_pointsList, "PLT" + hingedPlateThickness, Globals.MaterialStr, "99", _global.Position, "");
            _pointsList.Clear();
        }
    }
}
