using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Newtonsoft.Json.Linq;
using Render;

namespace DistillationColumn
{
    internal class Flange
    {
        public double topRingThickness = 500;
        public double bottomRingThickness = 300;
        public double ringWidth = 1000;
        public double ringRadius;
        public double topRingRadius = 1750;
        public double bottomRingRadius = 1750;
        public double insideDistance = 200;
        public double elevation = 9000;
        public int numberOfBolts=4;
        public double shellThickness;
        public double currentRingWidth;
        List<Part> _ringList;
        public Globals _global;
        public TeklaModelling _tModel;

        public Flange(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;
            _ringList = new List<Part>();
            CreateFlange();
        }

     

        public void CreateFlange()
        {



            CreateRing("Bottom-Ring");
            CreateRing("Top-Ring");
            CreateBolt();
            //int n = _tModel.GetSegmentAtElevation(elevation, _global.StackSegList);
            //shellThickness = _global.StackSegList[n][2];
            //ContourPoint sPoint = new ContourPoint(_tModel.ShiftVertically(_global.Origin, elevation), null);
            //ringRadius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList);
            //ringRadius += shellThickness;

            //for (int i = 0; i < 4; i++)
            //{

            //    ContourPoint ePoint = new ContourPoint(_tModel.ShiftHorizontallyRad(sPoint, ringRadius, i + 1), null);
            //    CustomPart CPart = new CustomPart();
            //    CPart.Name = "custom_flange";
            //    CPart.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;
            //    CPart.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.MIDDLE;
            //    CPart.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.MIDDLE;
            //    CPart.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.TOP;



            //    CPart.SetInputPositions(sPoint, ePoint);


            //    CPart.SetAttribute("ringWidth", ringWidth);
            //    CPart.SetAttribute("Radius", ringRadius);
            //    CPart.SetAttribute("topRingThickness", topRingThickness);
            //    CPart.SetAttribute("bottomRingThickness", bottomRingThickness);
            //    CPart.SetAttribute("insideDistance", insideDistance);
            //    CPart.SetAttribute("shellThickness", shellThickness);
            //    CPart.SetAttribute("numberOfBolts", numberOfBolts);
            //    CPart.SetAttribute("bolt_standard_screwdin", "4.6CSK");
            //    CPart.SetAttribute("bolt_diameter", 24);



            //    CPart.Insert();
            //    _tModel.Model.CommitChanges();

            //}


        }


        public void CreateRing(string ringType)
        {
            currentRingWidth = ringWidth;
            bottomRingRadius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList);
            topRingRadius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList);
            int n = _tModel.GetSegmentAtElevation(elevation, _global.StackSegList);
            double ringThickness = 0;
            ContourPoint sPoint = new ContourPoint();
            if (ringType == "Bottom-Ring")
            {
                ContourPoint origin = new ContourPoint(_tModel.ShiftVertically(_global.Origin, elevation), null);
                // double bottomRingRadius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList);
                sPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(origin, (bottomRingRadius) - insideDistance - _global.StackSegList[n][2], 1), null);
                ringThickness = bottomRingThickness;
                if (topRingRadius > bottomRingRadius)
                {
                    currentRingWidth += (bottomRingRadius - bottomRingRadius);
                }
                _global.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.BEHIND;

            }

            if (ringType == "Top-Ring")
            {

                // double topRingRadius = _tModel.GetRadiusAtElevation(elevation , _global.StackSegList) ;
                ContourPoint origin = new ContourPoint(_tModel.ShiftVertically(_global.Origin, elevation), null);
                ringThickness = topRingThickness;
                sPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(origin, topRingRadius - insideDistance - _global.StackSegList[n][2], 1), null);

                if (topRingRadius < bottomRingRadius)
                {
                    currentRingWidth += (bottomRingRadius - topRingRadius);

                }
                _global.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.FRONT;
            }



            for (int i = 1; i <= 4; i++)
            {
                List<ContourPoint> pointList = new List<ContourPoint>();

                ContourPoint mPoint = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(sPoint, Math.PI / 4, 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                ContourPoint ePoint = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(mPoint, Math.PI / 4, 1), null);


                pointList.Add(sPoint);
                pointList.Add(mPoint);
                pointList.Add(ePoint);

                _global.ProfileStr = "PL" + (currentRingWidth + insideDistance + _global.StackSegList[n][2]) + "*" + ringThickness;
                _global.ClassStr = "3";
                _global.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
                _global.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.FRONT;


                _ringList.Add(_tModel.CreatePolyBeam(pointList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "b" + i));

                sPoint = ePoint;

            }
        }

        public void CreateBolt()
        {


            int n1 = _tModel.GetSegmentAtElevation(elevation, _global.StackSegList);
            BoltCircle B = new BoltCircle();

            B.PartToBeBolted = _ringList[0];
            B.PartToBoltTo = _ringList[4];

            ContourPoint sPoint = new ContourPoint(_tModel.ShiftVertically(_global.Origin, elevation), null);
            ContourPoint ePoint = new ContourPoint(_tModel.ShiftHorizontallyRad(sPoint, topRingRadius + _global.StackSegList[n1][2] + (currentRingWidth), 1), null);
            //ePoint = _tModel.ShiftAlongCircumferenceRad(ePoint, 100 / (topRingRadius + _global.StackSegList[n1][2] + currentRingWidth), 1);
            //sPoint = _tModel.ShiftHorizontallyRad(sPoint, topRingRadius + _global.StackSegList[n1][2] + (currentRingWidth / 2), 1);
            //sPoint = _tModel.ShiftAlongCircumferenceRad(sPoint, 100 / (topRingRadius + _global.StackSegList[n1][2] + (currentRingWidth / 2)), 1);




            B.FirstPosition = sPoint;
            B.SecondPosition = ePoint;

            B.BoltSize = 40;
            B.Tolerance = 3.00;
            B.BoltStandard = "UNDEFINED_BOLT";
            B.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
            B.CutLength = bottomRingThickness + topRingThickness;

            B.Length = 100;
            B.ExtraLength = 15;
            B.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;

            B.Position.Depth = Position.DepthEnum.MIDDLE;
            B.Position.Plane = Position.PlaneEnum.MIDDLE;
            B.Position.Rotation = Position.RotationEnum.FRONT;

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

            //B.AddBoltDistX(0);


            //B.AddBoltDistY(0);


            B.NumberOfBolts = 10;
            B.Diameter = (topRingRadius + currentRingWidth + _global.StackSegList[n1][2]) * 1.5;

            if (!B.Insert())
                Console.WriteLine("BoltCircle Insert failed!");
            _tModel.Model.CommitChanges();


        }
    }
}
