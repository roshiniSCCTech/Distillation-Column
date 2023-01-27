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
        public double topRingThickness;
        public double topRingRadius ;
        public double bottomRingRadius ;
        public double bottomRingThickness ;
        public double insideDistance ;
        public double ringWidth;
        public double stiffnerLength ;
        public double stiffnerThickness;
        public double distanceBetweenStiffner;
        public int stiffnerCount;
        public Globals _global;
        public TeklaModelling _tModel;
        List<Part> _rings;


       


        public Chair(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;
           
            _rings = new List<Part>();

            SetChairData();
            CreateChair();
        }
        public void SetChairData()
        {
            List<JToken> _chairlist = _global.JData["chair"].ToList();
            foreach (JToken chair in _chairlist)
            {
                ringWidth = (float)chair["width"];
                stiffnerCount = (int)chair["number_of_plates"];
                stiffnerLength = (float)chair["height"];
                topRingThickness = (float)chair["top_ring_thickness"];
                bottomRingThickness= (float)chair["bottom_ring_thickness"];
                insideDistance = (float)chair["inside_distance"];
                stiffnerThickness = (float)chair["stiffner_plate_thickness"];
                distanceBetweenStiffner = (float)chair["distance_between_stiffner_plates"];

            }
        }
        public void CreateChair()
        {

           
            double topRingWidth=ringWidth;
            double bottomRingWidth=ringWidth;

            for (int i = 0; i < 4; i++)
            {
                int n = _tModel.GetSegmentAtElevation(stiffnerLength+bottomRingThickness, _global.StackSegList);
                topRingRadius = _tModel.GetRadiusAtElevation(stiffnerLength+bottomRingThickness, _global.StackSegList)+ _global.StackSegList[n][2];
                topRingRadius += _global.StackSegList[n][2];
                bottomRingRadius = _tModel.GetRadiusAtElevation(bottomRingThickness, _global.StackSegList) + _global.StackSegList[0][2];

                if (topRingRadius>bottomRingRadius)
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
                CPart.SetAttribute("PlateDistance", distanceBetweenStiffner);
                CPart.SetAttribute("stiffnerCount", stiffnerCount);
                CPart.SetAttribute("insideDistance", insideDistance);
                CPart.SetAttribute("stiffnerThickness", stiffnerThickness);
                CPart.SetAttribute("topRingWidth", topRingWidth);
                CPart.SetAttribute("bottomRingWidth", bottomRingWidth);

                CPart.Insert();
                _tModel.Model.CommitChanges();

            }

            //CreateRing("Top-Ring");
            //CreateRing("Bottom-Ring");
            //CreateStiffnerPlates();
        }
        //public void CreateRing(string ringType)
        //{
        //    double _insideDistance = 0;
        //    ContourPoint sPoint = new ContourPoint();
        //    if (ringType == "Bottom-Ring")
        //    {
        //        _insideDistance = insideDistance;
        //        sPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(_global.Origin, (_global.StackSegList[0][1] / 2) - insideDistance, 1), null);
        //    }

        //    if (ringType == "Top-Ring")
        //    {
        //        _insideDistance = 0;
        //        double radius = _tModel.GetRadiusAtElevation(stiffnerLength + topRingThickness, _global.StackSegList);

        //        sPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(_tModel.ShiftVertically(_global.Origin, stiffnerLength + topRingThickness), radius, 1), null);
        //    }



        //    for (int i = 1; i <= 4; i++)
        //    {
        //        List<ContourPoint> pointList = new List<ContourPoint>();

        //        ContourPoint mPoint = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(sPoint, Math.PI / 4, 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
        //        ContourPoint ePoint = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(mPoint, Math.PI / 4, 1), null);


        //        pointList.Add(sPoint);
        //        pointList.Add(mPoint);
        //        pointList.Add(ePoint);

        //        _global.ProfileStr = "PL" + (ringWidth + _insideDistance) + "*" + bottomRingThickness;
        //        _global.ClassStr = "3";
        //        _global.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
        //        _global.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.FRONT;
        //        _global.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.BEHIND;

        //        _rings.Add(_tModel.CreatePolyBeam(pointList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "b" + i));

        //        sPoint = ePoint;

        //    }
        //}

        //public void CreateStiffnerPlates()
        //{
        //    double distance1 = ((2 * Math.PI * _global.StackSegList[0][0] / 2) - (stiffnerCount * distBetweenStiffner) + (2 * stiffnerThickness)) / stiffnerCount;
        //    double radius = _tModel.GetRadiusAtElevation(stiffnerLength, _global.StackSegList);
        //    double distance2 = ((2 * Math.PI * radius) - (stiffnerCount * distBetweenStiffner) + (2 * stiffnerThickness)) / stiffnerCount;

        //    ContourPoint sPoint1 = new ContourPoint(_tModel.ShiftHorizontallyRad(_global.Origin, _global.StackSegList[0][1] / 2, 1), null);

        //    ContourPoint sPoint2 = new ContourPoint(_tModel.ShiftHorizontallyRad(_tModel.ShiftVertically(_global.Origin, stiffnerLength), radius, 1), null);

        //    for (int i = 0; i < stiffnerCount; i++)
        //    {

        //        ContourPoint ePoint1 = new ContourPoint(_tModel.ShiftHorizontallyRad(sPoint1, ringWidth, 1), null);
        //        ContourPoint ePoint2 = new ContourPoint(_tModel.ShiftHorizontallyRad(sPoint2, ringWidth, 1), null);

        //        _global.ProfileStr = "PL" + stiffnerThickness;
        //        _global.ClassStr = "1";
        //        _global.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
        //        _global.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.TOP;
        //        _global.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.BEHIND;

        //        List<ContourPoint> platePoints = new List<ContourPoint>()
        //        {
        //            sPoint1,ePoint1,ePoint2,sPoint2
        //        };

        //        _tModel.CreateContourPlate(platePoints, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "plate");
        //        platePoints.Clear();

        //        sPoint1 = _tModel.ShiftAlongCircumferenceRad(sPoint1, distBetweenStiffner, 2);
        //        sPoint2 = _tModel.ShiftAlongCircumferenceRad(sPoint2, distBetweenStiffner, 2);
        //        ePoint1 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(ePoint1, distBetweenStiffner, 2), null);
        //        ePoint2 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(ePoint2, distBetweenStiffner, 2), null);
        //        platePoints.Add(sPoint1);
        //        platePoints.Add(ePoint1);
        //        platePoints.Add(ePoint2);
        //        platePoints.Add(sPoint2);


        //        _tModel.CreateContourPlate(platePoints, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "plate");
        //        platePoints.Clear();

        //        sPoint1 = _tModel.ShiftAlongCircumferenceRad(sPoint1, distance1, 2);
        //        sPoint2 = _tModel.ShiftAlongCircumferenceRad(sPoint2, distance2, 2);

        //    }

        //}




    }


    
}
