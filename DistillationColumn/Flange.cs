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
        public double topRingThickness = 100;
        public double bottomRingThickness = 100;
        public double ringWidth = 1500;
        public double topRingRadius = 1750;
        public double bottomRingRadius = 1750;
        public double insideDistance = 200;
        public double elevation = 4000;
        public int numberOfBolts=9;
        public double boltLength=1000;
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
        }
    

        public void CreateRing(string ringType)
        {
            currentRingWidth = ringWidth;
            bottomRingRadius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList);
            topRingRadius = _tModel.GetRadiusAtElevation(elevation+boltLength, _global.StackSegList);
            ContourPoint sPoint = new ContourPoint();
            if (ringType == "Bottom-Ring")
            {    
                ContourPoint origin=new ContourPoint(_tModel.ShiftVertically(_global.Origin,elevation),null);
               // double bottomRingRadius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList);
                sPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(origin, (bottomRingRadius) - insideDistance, 1), null);
              
                if (topRingRadius > bottomRingRadius)
                {
                    currentRingWidth += (bottomRingRadius - bottomRingRadius);
                }

            }

            if (ringType == "Top-Ring")
            {
                insideDistance = 0;
               // double topRingRadius = _tModel.GetRadiusAtElevation(elevation , _global.StackSegList) ;
                ContourPoint origin = new ContourPoint(_tModel.ShiftVertically(_global.Origin, elevation+boltLength), null);

                sPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(origin, topRingRadius, 1), null);

                if (topRingRadius < bottomRingRadius)
                {
                    currentRingWidth += (bottomRingRadius - topRingRadius);
                  
                }
            }



            for (int i = 1; i <= 4; i++)
            {
                List<ContourPoint> pointList = new List<ContourPoint>();

                ContourPoint mPoint = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(sPoint, Math.PI / 4, 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                ContourPoint ePoint = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(mPoint, Math.PI / 4, 1), null);


                pointList.Add(sPoint);
                pointList.Add(mPoint);
                pointList.Add(ePoint);

                _global.ProfileStr = "PL" + (currentRingWidth + insideDistance) + "*" + bottomRingThickness;
                _global.ClassStr = "3";
                _global.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
                _global.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.FRONT;
                _global.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.BEHIND;

                _ringList.Add(_tModel.CreatePolyBeam(pointList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "b" + i));

                sPoint = ePoint;

            }
        }

        public void CreateBolt()
        {
           

            int n1 = _tModel.GetSegmentAtElevation(elevation+boltLength, _global.StackSegList);
            BoltCircle B = new BoltCircle();

            B.PartToBeBolted = _ringList[0];
            B.PartToBoltTo = _ringList[4];

            ContourPoint sPoint= new ContourPoint(_tModel.ShiftVertically(_global.Origin, elevation + boltLength),null);
            ContourPoint ePoint = new ContourPoint(_tModel.ShiftHorizontallyRad(sPoint, topRingRadius + _global.StackSegList[n1][2] + (currentRingWidth), 1),null);

            B.FirstPosition = sPoint;
            B.SecondPosition = ePoint;

            B.BoltSize =20;
            B.Tolerance = 3.00;
            B.BoltStandard = "UNDEFINED_BOLT";
            B.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
            B.CutLength = 105;

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

            B.NumberOfBolts = 10;
            B.Diameter = (topRingRadius+ currentRingWidth + _global.StackSegList[n1][2]) * 1.5 ;

            if (!B.Insert())
                Console.WriteLine("BoltCircle Insert failed!");
            _tModel.Model.CommitChanges();


        }
    }
}
