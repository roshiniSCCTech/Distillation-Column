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
using Newtonsoft.Json.Linq;

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
        double radius;
        double width;
        double number_of_plates;
        double height;


        List<List<double>> chairlist;

        public Chair(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;

            chairlist = new List<List<double>>();

            SetChairData();
            CreateChair();
        }
        public void SetChairData()
        {
            List<JToken> _chairlist = _global.jData["chair"].ToList();
            foreach (JToken chair in _chairlist)
            {
                //radius = (float)chair["radius"];
                width = (float)chair["width"];
                number_of_plates = (float)chair["number_of_plates"];
                height = (float)chair["height"];

                chairlist.Add(new List<double> { radius, width, number_of_plates, height });
            }
        }
        public void CreateChair()
        {

            //CreateRing("Top-Ring");
            //CreateRing("Bottom-Ring");
            //CreateStiffnerPlates();
            foreach (List<double> chair in chairlist)
            {
                for (int i = 0; i < 4; i++)
                {
                    height = chair[3];
                    width = chair[1];
                    number_of_plates = chair[2];
                    double elevation = 0;
                    radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList, true);
                    CustomPart CPart = new CustomPart();
                    CPart.Name = "FinalChair";
                    CPart.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;
                    CPart.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.LEFT;
                    CPart.Position.PlaneOffset = 0;
                    CPart.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.BEHIND;
                    CPart.Position.DepthOffset = 0;
                    CPart.Position.RotationOffset = i * 90;
                    CPart.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.TOP;
                    CPart.SetInputPositions(new Point(0, 0, 0), new Point(0, 0, 6000));
                    CPart.Insert();
                    CPart.SetAttribute("P5", height);
                    CPart.SetAttribute("P3", number_of_plates);
                    CPart.SetAttribute("P10", width);
                    CPart.SetAttribute("P1", radius);
                    CPart.Modify();

                    _tModel.Model.CommitChanges();
                }
            }
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
            double distance1 =(( 2 * Math.PI * _global.StackSegList[0][0]/2)-(stiffnerCount*distBetStiff)+(2*stffThick))/stiffnerCount;
            double radius = _tModel.GetRadiusAtElevation(stiffLength, _global.StackSegList);
            double distance2 = ((2 * Math.PI * radius) - (stiffnerCount * distBetStiff) + (2 * stffThick)) / stiffnerCount;

            ContourPoint sPoint1 = new ContourPoint(_tModel.ShiftHorizontallyRad(_global.Origin, _global.StackSegList[0][1] / 2, 1), null);
            
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

                sPoint1 = _tModel.ShiftAlongCircumferenceRad(sPoint1, distance1, 2);
                sPoint2 = _tModel.ShiftAlongCircumferenceRad(sPoint2, distance2, 2);

            }

        }

       




    }
}
