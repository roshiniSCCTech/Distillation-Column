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
        double arcDistance;
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

        public CircularAccessDoor(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;

            orientationAngle = 40 * Math.PI / 180;
            elevation = 20000;
            plateDiameter = 700;
            plateRadius = plateDiameter / 2;
            plateThickness=20;
            padPlateThickness = 20;
            gasketPlateThickness = 6;
            coverPlateThickness = 6;
            widthofNeckPlate = 112;
            neckPlateThickness = 12;
            widthOfGasketPlate = 60;
            widthofLiningPlate = 60;
            thicknessofLiningPlate = 6;
            arcDistance = 200;
            handleDistancefromplateorigin = 150;
            handleDistance = 75;
            HandleRodDiamter = 20;
            hingeddistancefromcoverPlate = 150;
            horizontalHingedDistance = 474;
            hingedDiameter = 60;
            plateTopPoint = new TSM.ContourPoint();
            plateBottomPoint = new TSM.ContourPoint();
            plateLeftPoint = new TSM.ContourPoint();
            plateRightPoint = new TSM.ContourPoint();

            padTopPoint= new TSM.ContourPoint();
            padRightPoint= new TSM.ContourPoint();
            padBottomPoint= new TSM.ContourPoint();
            padLeftPoint= new TSM.ContourPoint();


            _pointsList = new List<TSM.ContourPoint>();
            CreateCircularAccessDoor();

         
        }
        void CreateCircularAccessDoor()
        {
            stackRadius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList,true);
            stackOrigin = new TSM.ContourPoint(_global.Origin, null);
            stackOrigin = _tModel.ShiftVertically(stackOrigin, elevation);
            plateOrigin = _tModel.ShiftHorizontallyRad(stackOrigin, stackRadius, 1, orientationAngle);  
            
            CreatePadPlate();
            RefernceCutPlate();
            ReferenceneckPlate();
            //CreateNeckPlate();
            //CreateLiningPlate();
            //CreateFlangePlate();
            //CreateGasketPlate();
            //CreateBolts();
            //CreateCoverPlate();
            //CreateHandle();

            //HorizontalHinged();
        }
        void CreatePadPlate()
        { 

            double halfPadPlateAngle = Math.Asin((plateRadius+neckPlateThickness)/stackRadius);
            /* padTopPoint = new TSM.ContourPoint(_tModel.ShiftVertically(plateOrigin, plateRadius), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
             padBottomPoint = new TSM.ContourPoint(_tModel.ShiftVertically(plateOrigin, -plateRadius), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
             padLeftPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(stackOrigin, stackRadius, 1, orientationAngle + halfPadPlateAngle), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
             padRightPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(stackOrigin, stackRadius, 1, orientationAngle - halfPadPlateAngle), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));


             _global.Position.Depth = TSM.Position.DepthEnum.MIDDLE;
             _global.Position.Plane = TSM.Position.PlaneEnum.MIDDLE;
             _global.Position.Rotation = TSM.Position.RotationEnum.BELOW;

             _pointsList.Add(padTopPoint);
             _pointsList.Add(padRightPoint);
             _pointsList.Add(padBottomPoint);
             _pointsList.Add(padLeftPoint);
             _pointsList.Add(padTopPoint);
 */
            //_tModel.CreatePolyBeam(_pointsList, "PL" + widthOfGasketPlate + "*" + padPlateThickness, Globals.MaterialStr, "5", _global.Position, "");
            //_pointsList.Clear();


            

            TSM.ContourPoint startPoint = new TSM.ContourPoint(_tModel.ShiftAlongCircumferenceRad(plateOrigin, halfPadPlateAngle, 1), null);
            startPoint = _tModel.ShiftAlongCircumferenceRad(startPoint, arcDistance, 2);
            TSM.ContourPoint midPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(stackOrigin, stackRadius, 1, orientationAngle), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            TSM.ContourPoint endPoint = new TSM.ContourPoint(_tModel.ShiftAlongCircumferenceRad(plateOrigin, -(halfPadPlateAngle), 1), null);
            endPoint = _tModel.ShiftAlongCircumferenceRad(endPoint, -arcDistance, 2);

            _global.Position.Depth = TSM.Position.DepthEnum.MIDDLE;
            _global.Position.Plane = TSM.Position.PlaneEnum.MIDDLE;
            _global.Position.Rotation = TSM.Position.RotationEnum.TOP;

            _pointsList.Add(startPoint);
            _pointsList.Add(midPoint);
            _pointsList.Add(endPoint);
            

            //padPlate = _tModel.CreateBeam(startPoint, endPoint, "PL" + "500" + "*" + 2*(plateRadius + arcDistance + neckPlateThickness), "IS2062", "11", _global.Position, "");

            padPlate = _tModel.CreatePolyBeam(_pointsList, "PL" + "50" + "*" + 2 * (plateRadius + arcDistance + neckPlateThickness), "IS2062", "11", _global.Position, "");
        }
        void RefernceCutPlate()
        {
            plateOrigin = _tModel.ShiftHorizontallyRad(stackOrigin, stackRadius, 1, orientationAngle);
            plateTopPoint = _tModel.ShiftVertically(plateOrigin, plateRadius+arcDistance+neckPlateThickness);
            plateBottomPoint = _tModel.ShiftVertically(plateOrigin, -(plateRadius+arcDistance+neckPlateThickness));
            plateLeftPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, (plateRadius+arcDistance+neckPlateThickness), 4), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            plateRightPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, (plateRadius+arcDistance+neckPlateThickness), 2), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));

            _global.Position.Depth = TSM.Position.DepthEnum.FRONT;
            _global.Position.Plane = TSM.Position.PlaneEnum.MIDDLE;
            _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;
            _global.ProfileStr = "PL50*1000";//"PL" + (plateRadius + arcDistance + neckPlateThickness) + "*" + (plateRadius + arcDistance + neckPlateThickness);



            _pointsList.Add(plateTopPoint);
            _pointsList.Add(plateRightPoint);
            _pointsList.Add(plateBottomPoint);
            _pointsList.Add(plateLeftPoint);
            _pointsList.Add(plateTopPoint);

           PolyBeam cutPlate= _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, Globals.MaterialStr, BooleanPart.BooleanOperativeClassName, _global.Position, "");
            _pointsList.Clear();

            _tModel.cutPart(cutPlate, padPlate);

            

        }
        void ReferenceneckPlate()
        {
            plateOrigin = _tModel.ShiftHorizontallyRad(stackOrigin, stackRadius, 1, orientationAngle);

            _global.Position.Depth = TSM.Position.DepthEnum.MIDDLE;
            _global.Position.Plane = TSM.Position.PlaneEnum.MIDDLE;
            _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;
            _global.ProfileStr = "ROD"+plateDiameter;


            //plateOrigin = _tModel.ShiftHorizontallyRad(stackOrigin, stackRadius, 1, orientationAngle);
            plateOrigin = _tModel.ShiftHorizontallyRad(stackOrigin, stackRadius-500, 1, orientationAngle);
            TSM.ContourPoint point1= _tModel.ShiftHorizontallyRad(plateOrigin,1000, 1);
            Beam neckPlate1 = _tModel.CreateBeam(plateOrigin,point1,_global.ProfileStr, Globals.MaterialStr, BooleanPart.BooleanOperativeClassName, _global.Position, "");


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

           _tModel.CreatePolyBeam(_pointsList, "PL" +widthofNeckPlate + "*" + neckPlateThickness, Globals.MaterialStr, "12", _global.Position, "");
            
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
            
            plateOrigin = _tModel.ShiftHorizontallyRad(stackOrigin, stackRadius+widthofNeckPlate, 1, orientationAngle);
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
            double angle=0;
            TSM.ContourPoint startPoint = plateTopPoint;
            ContourPoint endPoint=plateRightPoint;
           
            _global.Position.Rotation = TSM.Position.RotationEnum.BELOW;

            for (int i = 0; i < 4; i++)
            {
                if(i == 0)
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

            B.NumberOfBolts = 16;
            B.Diameter = plateRadius * 2.15;
            B.Insert();
            _tModel.Model.CommitChanges();
        }
        void CreateCoverPlate()
        {
            plateOrigin = _tModel.ShiftHorizontallyRad(plateOrigin,widthofLiningPlate, 1, orientationAngle);
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
            plateOrigin = _tModel.ShiftHorizontallyRad(plateOrigin,handleDistancefromplateorigin, 4,orientationAngle);
            
            TSM.ContourPoint point1 =(_tModel.ShiftVertically(plateOrigin,handleDistance ));
            TSM.ContourPoint point2 = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(point1, handleDistance, 1), new TSM.Chamfer(10, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
            TSM.ContourPoint point3 =(_tModel.ShiftVertically(point2, -handleDistancefromplateorigin));
            TSM.ContourPoint point4 =new TSM.ContourPoint( _tModel.ShiftHorizontallyRad(point3, handleDistance, 3), new TSM.Chamfer(10, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
            TSM.ContourPoint point5= (_tModel.ShiftVertically(point4, handleDistance));

            _pointsList.Add(point1);
            _pointsList.Add(point2);
            _pointsList.Add(point3);
            _pointsList.Add(point4);

            _tModel.CreatePolyBeam(_pointsList, "ROD" + HandleRodDiamter , Globals.MaterialStr, "9", _global.Position, "");
            _pointsList.Clear();
        }

        void HorizontalHinged()
        {
            double hingedAngle = Math.Asin(hingeddistancefromcoverPlate / plateRadius);
            //double upHingedAngle= Math.Asin(hingeddistancefromcoverPlate+(hingedDiameter/2) / plateRadius);
            //double downHingedAngle = Math.Asin(hingeddistancefromcoverPlate - (hingedDiameter / 2) / plateRadius);
            plateOrigin = _tModel.ShiftHorizontallyRad(plateOrigin, widthofLiningPlate, 1, orientationAngle);
            TSM.ContourPoint hingedOrigin = _tModel.ShiftAlongCircumferenceRad(plateOrigin, hingedAngle, 1);
            TSM.ContourPoint upPoint = _tModel.ShiftHorizontallyRad(hingedOrigin, hingedDiameter / 2, 3);
            TSM.ContourPoint downPoint= _tModel.ShiftHorizontallyRad(hingedOrigin, hingedDiameter / 2, 1);
            TSM.ContourPoint downRight= _tModel.ShiftHorizontallyRad(plateOrigin, horizontalHingedDistance, 2);
            downRight = _tModel.ShiftVertically(downRight, hingeddistancefromcoverPlate);
            TSM.ContourPoint UpRight = _tModel.ShiftHorizontallyRad(downRight, hingedDiameter, 3);

            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;

            _pointsList.Add(upPoint);
            _pointsList.Add(UpRight);
            _pointsList.Add(downRight);
            _pointsList.Add(downPoint);

            _tModel.CreateContourPlate(_pointsList, "PLT" + coverPlateThickness, Globals.MaterialStr, "99", _global.Position, "");
            _pointsList.Clear();





        }
    }
}
