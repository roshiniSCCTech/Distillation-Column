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
        public double topRingThickness ;
        public double bottomRingThickness;
        public double ringWidth ;
        public double ringRadius;    
        public double insideDistance ;
        public double elevation;
        public double numberOfBolts;
        public double shellThickness;
        List<Part> _ringList;
        public Globals _global;
        public TeklaModelling _tModel;
        List<List<double>> _flangelist;

        public Flange(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;
            _ringList = new List<Part>();
            _flangelist = new List<List<double>>();
            SetFlangeData();
            BuildFlange();
            
        }

        void SetFlangeData()
        {
            List<JToken> flangelist = _global.JData["Flange"].ToList();
            foreach (JToken flange in flangelist)
            {
                elevation = (double)flange["elevation"];
                topRingThickness= (double)flange["top_ring_thickness"];
                 bottomRingThickness = (double)flange["bottom_ring_thickness"];
                 ringWidth=(double)flange["ring_width"]; 
                 insideDistance = (double)flange["inside_distance"] ;
                 numberOfBolts = (double)flange["number_of_bolts"];


               _flangelist.Add(new List<double> { elevation,topRingThickness,bottomRingThickness,ringWidth,insideDistance,numberOfBolts });
            }
        }

        void BuildFlange()
        {
            foreach (List<double> flange in _flangelist)
            {
                elevation = flange[0];
                topRingThickness = flange[1];
                bottomRingThickness = flange[2];
                ringWidth = flange[3];
                insideDistance = flange[4];
                numberOfBolts = flange[5];

                CreateFlange();
            }
        }

     

        public void CreateFlange()
        {

            int n = _tModel.GetSegmentAtElevation(elevation, _global.StackSegList);
            shellThickness = _global.StackSegList[n][2];
            ContourPoint sPoint = new ContourPoint(_tModel.ShiftVertically(_global.Origin, elevation), null);
            ringRadius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList);
            ringRadius += shellThickness;

            for (int i = 0; i < 4; i++)
            {

                ContourPoint ePoint = new ContourPoint(_tModel.ShiftHorizontallyRad(sPoint, ringRadius, i + 1), null);
                CustomPart CPart = new CustomPart();
                CPart.Name = "custom_flange";
                CPart.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;
                CPart.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.MIDDLE;
                CPart.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.MIDDLE;
                CPart.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.TOP;



                CPart.SetInputPositions(sPoint, ePoint);


                CPart.SetAttribute("ringWidth", ringWidth);
                CPart.SetAttribute("Radius", ringRadius);
                CPart.SetAttribute("topRingThickness", topRingThickness);
                CPart.SetAttribute("bottomRingThickness", bottomRingThickness);
                CPart.SetAttribute("insideDistance", insideDistance);
                CPart.SetAttribute("shellThickness", shellThickness);
                CPart.SetAttribute("numberOfBolts", numberOfBolts);
                CPart.SetAttribute("bolt_standard_screwdin", "UNDEINED_BOLT");
                CPart.SetAttribute("Cut_Length", -420);
                CPart.SetAttribute("bolt_diameter", 20);



                CPart.Insert();
                _tModel.Model.CommitChanges();

            }

            //CreateRing("Bottom-Ring");
            //CreateRing("Top-Ring");
            //CreateBolt();


        }


        //public void CreateRing(string ringType)
        //{
           
        //    ringRadius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList);
           
        //    int n = _tModel.GetSegmentAtElevation(elevation, _global.StackSegList);
        //    double ringThickness = 0;
        //    ContourPoint sPoint = new ContourPoint();
        //    if (ringType == "Bottom-Ring")
        //    {
        //        ContourPoint origin = new ContourPoint(_tModel.ShiftVertically(_global.Origin, elevation), null);
        //        // double bottomRingRadius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList);
        //        sPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(origin, (ringRadius) - insideDistance - _global.StackSegList[n][2], 1), null);
        //        ringThickness = bottomRingThickness;
               
                
        //        _global.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.BEHIND;

        //    }

        //    if (ringType == "Top-Ring")
        //    {

        //        // double topRingRadius = _tModel.GetRadiusAtElevation(elevation , _global.StackSegList) ;
        //        ContourPoint origin = new ContourPoint(_tModel.ShiftVertically(_global.Origin, elevation), null);
        //        ringThickness = topRingThickness;
        //        sPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(origin, ringRadius - insideDistance - _global.StackSegList[n][2], 1), null);

        //        _global.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.FRONT;
        //    }



        //    for (int i = 1; i <= 4; i++)
        //    {
        //        List<ContourPoint> pointList = new List<ContourPoint>();

        //        ContourPoint mPoint = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(sPoint, Math.PI / 4, 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
        //        ContourPoint ePoint = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(mPoint, Math.PI / 4, 1), null);


        //        pointList.Add(sPoint);
        //        pointList.Add(mPoint);
        //        pointList.Add(ePoint);

        //        _global.ProfileStr = "PL" + (ringWidth + insideDistance + _global.StackSegList[n][2]) + "*" + ringThickness;
        //        _global.ClassStr = "3";
        //        _global.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
        //        _global.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.FRONT;


        //        _ringList.Add(_tModel.CreatePolyBeam(pointList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "b" + i));

        //        sPoint = ePoint;

        //    }
        //}

        //public void CreateBolt()
        //{


        //    int n1 = _tModel.GetSegmentAtElevation(elevation, _global.StackSegList);
        //    BoltCircle B = new BoltCircle();

        //    B.PartToBeBolted = _ringList[0];
        //    B.PartToBoltTo = _ringList[4];

        //    ContourPoint sPoint = new ContourPoint(_tModel.ShiftVertically(_global.Origin, elevation), null);
        //    ContourPoint ePoint = new ContourPoint(_tModel.ShiftHorizontallyRad(sPoint, ringRadius + _global.StackSegList[n1][2] + (ringWidth), 1), null);
        //    ePoint = _tModel.ShiftAlongCircumferenceRad(ePoint, 100 / (ringRadius + _global.StackSegList[n1][2] + ringWidth), 1);
        //    //sPoint = _tModel.ShiftHorizontallyRad(sPoint, topRingRadius + _global.StackSegList[n1][2] + (currentRingWidth / 2), 1);
        //    //sPoint = _tModel.ShiftAlongCircumferenceRad(sPoint, 100 / (topRingRadius + _global.StackSegList[n1][2] + (currentRingWidth / 2)), 1);




        //    B.FirstPosition = sPoint;
        //    B.SecondPosition = ePoint;

        //    B.BoltSize = 20;
        //    B.Tolerance = 3.00;
        //    B.BoltStandard = "UNDEFINED_BOLT";
        //    B.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
        //    B.CutLength = -300;

        //    B.Length = 100;
        //    B.ExtraLength = 15;
        //    B.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;

        //    B.Position.Depth = Position.DepthEnum.MIDDLE;
        //    B.Position.Plane = Position.PlaneEnum.MIDDLE;
        //    B.Position.Rotation = Position.RotationEnum.FRONT;

        //    B.Bolt = true;
        //    B.Washer1 = true;
        //    B.Washer2 = true;
        //    B.Washer3 = true;
        //    B.Nut1 = true;
        //    B.Nut2 = true;

        //    B.Hole1 = true;
        //    B.Hole2 = true;
        //    B.Hole3 = true;
        //    B.Hole4 = true;
        //    B.Hole5 = true;

        //    //B.AddBoltDistX(0);


        //    //B.AddBoltDistY(0);


        //    B.NumberOfBolts = 10;
        //    B.Diameter = (ringRadius + ringWidth + _global.StackSegList[n1][2]) * 1.5;

        //    if (!B.Insert())
        //        Console.WriteLine("BoltCircle Insert failed!");
        //    _tModel.Model.CommitChanges();


        //}
    }
}
