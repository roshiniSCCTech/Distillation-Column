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
        double thicknessofNeckPlate;
        double gasketPlateThickness;
        double widthOfGasketPlate;
        double thicknessofLiningPlate;
        double coverPlateThickness;
        double handleDistancefromplateorigin;
        double handleDistance;
        double HandleRodDiamter;
        PolyBeam flangePlate;
        PolyBeam gasketPlate;

        TSM.ContourPoint origin;
        TSM.ContourPoint plateOrigin;
        TSM.ContourPoint topPoint;
        TSM.ContourPoint bottomPoint;
        TSM.ContourPoint leftPoint;
        TSM.ContourPoint rightPoint;


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
            gasketPlateThickness = 6;
            coverPlateThickness = 6;
            thicknessofNeckPlate = 112;
            widthOfGasketPlate = 60;
            thicknessofLiningPlate = 60;
            handleDistancefromplateorigin = 150;
            handleDistance = 75;
            HandleRodDiamter = 20;
            topPoint = new TSM.ContourPoint();
            bottomPoint = new TSM.ContourPoint();
            leftPoint = new TSM.ContourPoint();
            rightPoint = new TSM.ContourPoint();


            _pointsList = new List<TSM.ContourPoint>();
            CreateCircularAccessDoor();

         
        }
        void CreateCircularAccessDoor()
        {
            stackRadius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList,true);
            origin = new TSM.ContourPoint(_global.Origin, null);
            origin = _tModel.ShiftVertically(origin, elevation);
            plateOrigin = _tModel.ShiftHorizontallyRad(origin, stackRadius, 1, orientationAngle);  
            
            CreateIntialPlate();
            CreateFlangePlate();
            CreateGasketPlate();
            //CreateBolts();
           // CreateCoverPlate();
            //CreateHandle();
        }
        void CreateIntialPlate()
        {
            /*TSM.ContourPoint point2 = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(startPoint, plateRadius, 2), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            TSM.ContourPoint point1 = _tModel.ShiftVertically(startPoint, plateRadius);
            TSM.ContourPoint point3 = _tModel.ShiftVertically(startPoint, -plateRadius);
            TSM.ContourPoint point4 = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(startPoint, plateRadius, 4), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
*/

            topPoint = _tModel.ShiftVertically(plateOrigin, plateRadius);
            bottomPoint = _tModel.ShiftVertically(plateOrigin, -plateRadius);
            leftPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, plateRadius, 4,orientationAngle), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            rightPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, plateRadius, 2,orientationAngle), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));


            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;
            _global.Position.Plane = TSM.Position.PlaneEnum.LEFT;
            _global.Position.Rotation = TSM.Position.RotationEnum.BELOW;


            _pointsList.Add(topPoint);
            _pointsList.Add(leftPoint);
            _pointsList.Add(bottomPoint);
            _pointsList.Add(rightPoint);
            _pointsList.Add(topPoint);

            _tModel.CreatePolyBeam(_pointsList, "PL" + widthOfGasketPlate + "*" + plateThickness, Globals.MaterialStr, "5", _global.Position, "");
            _pointsList.Clear();
        }
        
        void CreateFlangePlate()
        {
            
            plateOrigin = _tModel.ShiftHorizontallyRad(origin, stackRadius+thicknessofNeckPlate, 1, orientationAngle);
            topPoint = _tModel.ShiftVertically(plateOrigin, plateRadius);
            bottomPoint = _tModel.ShiftVertically(plateOrigin, -plateRadius);
            leftPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, plateRadius, 4), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            rightPoint = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(plateOrigin, plateRadius, 2), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));

            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;
            _global.Position.Plane = TSM.Position.PlaneEnum.LEFT;
            _global.Position.Rotation = TSM.Position.RotationEnum.BELOW;


            _pointsList.Add(topPoint);
            _pointsList.Add(leftPoint);
            _pointsList.Add(bottomPoint);
            _pointsList.Add(rightPoint);
            _pointsList.Add(topPoint);

           flangePlate=_tModel.CreatePolyBeam(_pointsList, "PL" + widthOfGasketPlate + "*" + gasketPlateThickness, Globals.MaterialStr, "2", _global.Position, "");
            _pointsList.Clear();
        }
        void CreateGasketPlate()
        {
            double x = plateOrigin.X + plateRadius * Math.Cos(Math.PI / 4);
            double z = plateOrigin.Z + plateRadius* Math.Sin(Math.PI / 4);
            //TSM.ContourPoint x1= new TSM.ContourPoint(_tModel.ShiftHorizontallyRad( (_tModel.ShiftVertically(leftPoint,plateRadius+widthOfGasketPlate*Math.Sin(Math.PI/4))),((plateRadius+widthOfGasketPlate)- plateRadius * Math.Cos(Math.PI / 4)),3),new Chamfer(0 ,0,TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));

            //double y = topPoint.Y + plateRadius * Math.Tan(Math.PI / 4);
            
            //TSM.ContourPoint midPoint1 = new TSM.ContourPoint(_tModel.ShiftAlongCircumferenceRad(plateOrigin,Math.PI/4,1), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            //TSM.ContourPoint midPoint1 = new TSM.ContourPoint(new T3D.Point(x,plateOrigin.Y,z), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            //TSM.ContourPoint midPoint1 = _tModel.ShiftHorizontallyRad(topPoint, plateRadius * Math.Cos(45 * Math.PI / 180), 2,orientationAngle);
            TSM.ContourPoint midPoint1 = _tModel.ShiftVertically(topPoint,-(plateRadius/2));
            midPoint1 = ShiftTangentiallyInRad(plateOrigin, plateRadius*(45*Math.PI/180),orientationAngle,1);
            TSM.ContourPoint point1 = topPoint;
            //TSM.ContourPoint midPoint1 = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(x1, plateRadius, 4), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            TSM.ContourPoint point2= rightPoint; ;
            TSM.ContourPoint point3= bottomPoint;
            TSM.ContourPoint point4= leftPoint;
            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;
            _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT; 
            _global.Position.Rotation = TSM.Position.RotationEnum.BELOW;


            _pointsList.Add(point1);
            _pointsList.Add(midPoint1);
            _pointsList.Add(point2);


            gasketPlate=_tModel.CreatePolyBeam(_pointsList, "PL" + widthOfGasketPlate + "*" + gasketPlateThickness, Globals.MaterialStr, "6", _global.Position, "");
            _pointsList.Clear();
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
            B.Diameter = plateRadius * 1.5;
            B.Insert();
            _tModel.Model.CommitChanges();
        }
        void CreateCoverPlate()
        {
            plateOrigin = _tModel.ShiftHorizontallyRad(origin, stackRadius+thicknessofNeckPlate+2*gasketPlateThickness+thicknessofLiningPlate, 1, orientationAngle);
            TSM.ContourPoint point2 = _tModel.ShiftHorizontallyRad(plateOrigin, plateRadius, 2);
            TSM.ContourPoint point1 = new TSM.ContourPoint(_tModel.ShiftVertically(plateOrigin, plateRadius), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            TSM.ContourPoint point3 = new TSM.ContourPoint(_tModel.ShiftVertically(plateOrigin, -plateRadius), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            TSM.ContourPoint point4 = _tModel.ShiftHorizontallyRad(plateOrigin, plateRadius, 4);

            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;

            _pointsList.Add(point1);
            _pointsList.Add(point2);
            _pointsList.Add(point3);
            _pointsList.Add(point4);

            _tModel.CreateContourPlate(_pointsList, "PLT" + coverPlateThickness, Globals.MaterialStr, "5", _global.Position, "");
            _pointsList.Clear();

        }

        void CreateHandle()
        {
            plateOrigin = _tModel.ShiftHorizontallyRad(plateOrigin,handleDistancefromplateorigin, 1,orientationAngle);
            
            TSM.ContourPoint point1 =(_tModel.ShiftVertically(plateOrigin,handleDistance ));
            TSM.ContourPoint point2 = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(point1, handleDistance, 1), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
            TSM.ContourPoint point3 =(_tModel.ShiftVertically(plateOrigin, -handleDistance));
            TSM.ContourPoint point4 =new TSM.ContourPoint( _tModel.ShiftHorizontallyRad(plateOrigin, handleDistance, 3), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
           // point2.Chamfer

            _pointsList.Add(point1);
            _pointsList.Add(point2);
            _pointsList.Add(point3);
            _pointsList.Add(point4);

            _tModel.CreatePolyBeam(_pointsList, "ROD" + HandleRodDiamter , Globals.MaterialStr, "2", _global.Position, "");
            _pointsList.Clear();
        }

 protected ContourPoint ShiftTangentiallyInRad(ContourPoint pt, double dist, double angle, int side, int plane = 2)
        {
            ContourPoint shiftedPt;
            switch (plane)
            {
                case 0:
                    shiftedPt = new ContourPoint(new T3D.Point(pt.X + dist * Math.Cos(angle * Math.PI / 180), pt.Y + dist * Math.Sin(angle * Math.PI / 180), pt.Z), null);
                    //throw new Exception("Not implemented");
                    break;
                case 1:
                    shiftedPt = new ContourPoint(new T3D.Point(pt.X + dist * Math.Cos(angle * Math.PI / 180), pt.Y + dist * Math.Sin(angle * Math.PI / 180), pt.Z), null);
                    //throw new Exception("Not implemented");
                    break;
                default:
                    switch (side)
                    {
                        case 1:
                            shiftedPt = new ContourPoint(new T3D.Point(pt.X, pt.Y, pt.Z + dist), null);
                            break;
                        case 2:
                            shiftedPt = new ContourPoint(new T3D.Point(pt.X - dist * Math.Cos(Math.PI / 2 - angle), pt.Y + dist * Math.Sin(Math.PI / 2 - angle), pt.Z), null);
                            break;
                        case 3:
                            shiftedPt = new ContourPoint(new T3D.Point(pt.X, pt.Y, pt.Z - dist), null);
                            break;
                        default:
                            shiftedPt = new ContourPoint(new T3D.Point(pt.X + dist * Math.Cos(Math.PI / 2 - angle), pt.Y - dist * Math.Sin(Math.PI / 2 - angle), pt.Z), null);
                            break;
                    }
                    break;
            }
            return shiftedPt;
        }



    }
}
