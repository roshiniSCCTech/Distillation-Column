using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;
using static Tekla.Structures.Filtering.Categories.PartFilterExpressions;
using static Tekla.Structures.Filtering.Categories.ReinforcingBarFilterExpressions;
using Tekla.Structures.ModelInternal;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Datatype;

namespace DistillationColumn
{
    class Chair
    {
        public double topRingThick = 100;
        public double bottomRingThick = 100;
        public double insideDist = 100;
        public double ringWidth = 1500;
        public double stiffLength = 2000;
        public double stffThick = 50;
        public double distBetStiff = 500;
        public int stiffnerCount = 9;
        public Globals _global;
        public TeklaModelling _tModel;

        public Chair(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;

            CreateChair();
        }

        public void CreateChair()
        {
            
            CreateRing("Top-Ring");
            CreateRing("Bottom-Ring");
            CreateStiffnerPlates();
         
        }


        public void CreateRing(string ringType)
        {
            double insideDistance = 0;
            ContourPoint sPoint=new ContourPoint();
            if(ringType=="Bottom-Ring")
            {
                insideDistance= insideDist;
                sPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(_global.Origin, (_global.StackSegList[0][1] / 2)-insideDistance, 1), null);
            }

            if(ringType=="Top-Ring")
            {
                insideDistance = 0;
                double radius = _tModel.GetRadiusAtElevation(stiffLength+topRingThick, _global.StackSegList);

                sPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(_tModel.ShiftVertically(_global.Origin, stiffLength+topRingThick), radius, 1), null);
            }

            

            for (int i = 1; i <= 4; i++)
            {
                List<ContourPoint> pointList = new List<ContourPoint>();

                ContourPoint mPoint = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(sPoint, Math.PI/4, 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                ContourPoint ePoint = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(mPoint, Math.PI / 4, 1), null);


                pointList.Add(sPoint);
                pointList.Add(mPoint);
                pointList.Add(ePoint);

                _global.ProfileStr = "PL" + (ringWidth+insideDistance) + "*" + bottomRingThick;
                _global.ClassStr = "3";
                _global.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
                _global.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.FRONT;
                _global.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.BEHIND;

                _tModel.CreatePolyBeam(pointList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "b" + i);

                sPoint = ePoint;

            }
        }

        public void CreateStiffnerPlates()
        {
            double distance =(( 2 * Math.PI * _global.StackSegList[0][0]/2)-(stiffnerCount*distBetStiff))/stiffnerCount;

            ContourPoint sPoint1 = new ContourPoint(_tModel.ShiftHorizontallyRad(_global.Origin, _global.StackSegList[0][1] / 2, 1), null);
            double radius = _tModel.GetRadiusAtElevation(stiffLength, _global.StackSegList);
            ContourPoint sPoint2 = new ContourPoint(_tModel.ShiftHorizontallyRad(_tModel.ShiftVertically(_global.Origin, stiffLength), radius, 1), null);

            for (int i = 0; i < stiffnerCount; i++)
            {

                 ContourPoint ePoint1=new ContourPoint(_tModel.ShiftHorizontallyRad(sPoint1,ringWidth,1), null);
                 ContourPoint ePoint2 = new ContourPoint(_tModel.ShiftHorizontallyRad(sPoint2, ringWidth, 1), null);
                _global.ProfileStr = "PL" +stffThick;
                _global.ClassStr = "1";
                _global.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
                _global.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.TOP;
                _global.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.BEHIND;

                List<ContourPoint> platePoints = new List<ContourPoint>()
                {
                    sPoint1,ePoint1,ePoint2,sPoint2
                };
                
                _tModel.CreateContourPlate(platePoints, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "plate");
                platePoints.Clear();

                sPoint1 = _tModel.ShiftAlongCircumferenceRad(sPoint1, distBetStiff, 2);
                sPoint2 = _tModel.ShiftAlongCircumferenceRad(sPoint2, distBetStiff, 2);
                ePoint1 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(ePoint1, distBetStiff, 2), null);
                ePoint2 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(ePoint2, distBetStiff, 2), null);
                platePoints.Add(sPoint1);
                platePoints.Add(ePoint1);
                platePoints.Add(ePoint2);
                platePoints.Add(sPoint2);

                _tModel.CreateContourPlate(platePoints, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "plate");
                platePoints.Clear();

                sPoint1 = _tModel.ShiftAlongCircumferenceRad(sPoint1, distance, 2);
                sPoint2 = _tModel.ShiftAlongCircumferenceRad(sPoint2, distance, 2);

            }

        }

       




    }
}
