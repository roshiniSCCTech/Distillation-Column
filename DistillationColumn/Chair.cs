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
        public double topRingThickness = 100;
        public double topRingRadius = 5000;
        public double bottomRingRadius = 7000;
        public double bottomRingThickness = 100;
        public double insideDistance = 500;
        public double ringWidth = 2500;
        public double stiffnerLength = 2000;
        public double stiffnerThickness = 50;
        public double distBetweenStiffner = 500;
        public int stiffnerCount = 4;
        public Globals _global;
        public TeklaModelling _tModel;
        double width;
        double number_of_plates;
        double height;
        List<Part> _rings;


        List<List<double>> chairlist;


        public Chair(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;

            chairlist = new List<List<double>>();
            _rings = new List<Part>();

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

                // chairlist.Add(new List<double> { radius, width, number_of_plates, height });
            }
        }
        public void CreateChair()
        {

            //CreateRing("Top-Ring");
            //CreateRing("Bottom-Ring");
            //CreateStiffnerPlates();
            double topRingWidth=ringWidth;
            double bottomRingWidth=ringWidth;

            for (int i = 0; i < 4; i++)
            {
               
                double elevation = 0;
                //int n = _tModel.GetSegmentAtElevation(stiffnerLength, _global.StackSegList);
                //topRingRadius = _tModel.GetRadiusAtElevation(stiffnerLength, _global.StackSegList);
                //topRingRadius += _global.StackSegList[n][2];
                //bottomRingRadius = (_global.StackSegList[0][0] / 2) + _global.StackSegList[0][2];

                if(topRingRadius>bottomRingRadius)
                {
                    topRingWidth = ringWidth;
                    bottomRingWidth = (topRingRadius - bottomRingRadius) + ringWidth;
                }

                if (bottomRingRadius>topRingRadius )
                {
                    bottomRingWidth = ringWidth;
                    topRingWidth = (bottomRingRadius - topRingRadius) + ringWidth;
                }


                ContourPoint origin = new ContourPoint(_tModel.ShiftVertically(_global.Origin, bottomRingThickness), null);
                ContourPoint ePoint = new ContourPoint(_tModel.ShiftHorizontallyRad(origin, bottomRingRadius, i + 1), null);
                CustomPart CPart = new CustomPart();
                CPart.Name = "Chair_s";
                CPart.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;
                CPart.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.MIDDLE;
                CPart.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.MIDDLE;
                CPart.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.TOP;



                CPart.SetInputPositions(origin, ePoint);
                CPart.SetAttribute("stiffnerLength", stiffnerLength);
                CPart.SetAttribute("topRadius", topRingRadius);
                CPart.SetAttribute("ringWidth", ringWidth);
                CPart.SetAttribute("bottomRadius", bottomRingRadius);
                CPart.SetAttribute("topRingThickness", topRingThickness);
                CPart.SetAttribute("bottomRingThickness", bottomRingThickness);
                CPart.SetAttribute("PlateDistance", distBetweenStiffner);
                CPart.SetAttribute("stiffnerCount", stiffnerCount);
                CPart.SetAttribute("insideDistance", insideDistance);
                CPart.SetAttribute("stiffnerThickness", stiffnerThickness);
                CPart.SetAttribute("topRingWidth", topRingWidth);
                CPart.SetAttribute("bottomRingWidth", bottomRingWidth);

                CPart.Insert();
                _tModel.Model.CommitChanges();

            }
        }
        public void CreateRing(string ringType)
        {
            double _insideDistance = 0;
            ContourPoint sPoint = new ContourPoint();
            if (ringType == "Bottom-Ring")
            {
                _insideDistance = insideDistance;
                sPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(_global.Origin, (_global.StackSegList[0][1] / 2) - insideDistance, 1), null);
            }

            if (ringType == "Top-Ring")
            {
                _insideDistance = 0;
                double radius = _tModel.GetRadiusAtElevation(stiffnerLength + topRingThickness, _global.StackSegList);

                sPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(_tModel.ShiftVertically(_global.Origin, stiffnerLength + topRingThickness), radius, 1), null);
            }



            for (int i = 1; i <= 4; i++)
            {
                List<ContourPoint> pointList = new List<ContourPoint>();

                ContourPoint mPoint = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(sPoint, Math.PI / 4, 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                ContourPoint ePoint = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(mPoint, Math.PI / 4, 1), null);


                pointList.Add(sPoint);
                pointList.Add(mPoint);
                pointList.Add(ePoint);

                _global.ProfileStr = "PL" + (ringWidth + _insideDistance) + "*" + bottomRingThickness;
                _global.ClassStr = "3";
                _global.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
                _global.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.FRONT;
                _global.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.BEHIND;

                _rings.Add(_tModel.CreatePolyBeam(pointList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "b" + i));

                sPoint = ePoint;

            }
        }

        public void CreateStiffnerPlates()
        {
            double distance1 = ((2 * Math.PI * _global.StackSegList[0][0] / 2) - (stiffnerCount * distBetweenStiffner) + (2 * stiffnerThickness)) / stiffnerCount;
            double radius = _tModel.GetRadiusAtElevation(stiffnerLength, _global.StackSegList);
            double distance2 = ((2 * Math.PI * radius) - (stiffnerCount * distBetweenStiffner) + (2 * stiffnerThickness)) / stiffnerCount;

            ContourPoint sPoint1 = new ContourPoint(_tModel.ShiftHorizontallyRad(_global.Origin, _global.StackSegList[0][1] / 2, 1), null);

            ContourPoint sPoint2 = new ContourPoint(_tModel.ShiftHorizontallyRad(_tModel.ShiftVertically(_global.Origin, stiffnerLength), radius, 1), null);

            for (int i = 0; i < stiffnerCount; i++)
            {

                ContourPoint ePoint1 = new ContourPoint(_tModel.ShiftHorizontallyRad(sPoint1, ringWidth, 1), null);
                ContourPoint ePoint2 = new ContourPoint(_tModel.ShiftHorizontallyRad(sPoint2, ringWidth, 1), null);

                _global.ProfileStr = "PL" + stiffnerThickness;
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

                sPoint1 = _tModel.ShiftAlongCircumferenceRad(sPoint1, distBetweenStiffner, 2);
                sPoint2 = _tModel.ShiftAlongCircumferenceRad(sPoint2, distBetweenStiffner, 2);
                ePoint1 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(ePoint1, distBetweenStiffner, 2), null);
                ePoint2 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(ePoint2, distBetweenStiffner, 2), null);
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
