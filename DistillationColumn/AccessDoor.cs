using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSM = Tekla.Structures.Model;
using T3D = Tekla.Structures.Geometry3d;
using Newtonsoft.Json.Linq;
using HelperLibrary;
using System.Security.Cryptography.X509Certificates;
using Tekla.Structures.Model;
using static Tekla.Structures.ModelInternal.Operation;

namespace DistillationColumn
{
    class AccessDoor
    {
        Globals _global;
        TeklaModelling _tModel;

        double orientationAngle;
        double elevation;
        double height;
        double width;
        double breadth;

        TSM.ContourPoint TopRight;
        TSM.ContourPoint TopLeft;
        TSM.ContourPoint BottomRight;
        TSM.ContourPoint BottomLeft;
        TSM.ContourPoint BackTopRight;
        TSM.ContourPoint BackTopLeft;
        TSM.ContourPoint BackBottomRight;
        TSM.ContourPoint BackBottomLeft;

        List<TSM.ContourPoint> _pointsList;

        List<List<double>> _accessDoorList;


        public AccessDoor(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;

            /*orientationAngle = 0 * Math.PI/180;
            elevation = 1000;
            height = 800;
            width = 1000;
            breadth = 500;*/

            TopRight = new TSM.ContourPoint();
            TopLeft = new TSM.ContourPoint();
            BottomRight = new TSM.ContourPoint();
            BottomLeft = new TSM.ContourPoint();

            _pointsList = new List<TSM.ContourPoint>();
            _accessDoorList = new List<List<double>>();

      //   Build();
      SetAccessDoorData();
      CreateAccessDoor();
    }

    public void CreateAccessDoor()
    {
      foreach (List<double> acDoor in _accessDoorList)
      {
         elevation = acDoor[0];
        orientationAngle = acDoor[1]*Math.PI/180;
        double topRadius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList, true);
        double bottomRadius = _tModel.GetRadiusAtElevation(elevation - acDoor[2], _global.StackSegList, true);

        TSM.ContourPoint origin = new TSM.ContourPoint(_global.Origin, null);
        TSM.ContourPoint point1 = _tModel.ShiftVertically(origin, elevation);
        TSM.ContourPoint point2 = _tModel.ShiftHorizontallyRad(point1, topRadius, 1, orientationAngle);


        CustomPart accessDoor = new CustomPart();
        accessDoor.Name = "HingedAccessDoor";
        accessDoor.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;

        accessDoor.SetInputPositions(point1, point2);
        accessDoor.SetAttribute("P1", acDoor[3]); // width
        accessDoor.SetAttribute("P2", topRadius); // top radius
        accessDoor.SetAttribute("P3", acDoor[2]); // height
        accessDoor.SetAttribute("P4", acDoor[4]); // breadth
        accessDoor.SetAttribute("P6", bottomRadius); // bottom radius
        accessDoor.Insert();
        _tModel.Model.CommitChanges();
      }
    }

    public void SetAccessDoorData()
    {
      List<JToken> accessDoorList = _global.JData["access_door"].ToList();
      foreach (JToken accessDoor in accessDoorList)
      {
         elevation = (float)accessDoor["elevation"];
         orientationAngle = (float)accessDoor["orientation_angle"];
         height = (float)accessDoor["height"];
         width = (float)accessDoor["width"];
         breadth = (float)accessDoor["breadth"];

        _accessDoorList.Add(new List<double> { elevation, orientationAngle, height, width, breadth });
      }
    }
    public void Build()
        {
            /*InitialisePlatePoints();

            // top plate
            CreateTopPlate();

            // bottom plate
            CreateBottomPlate();

            // left plate
            CreateLeftPlate();

            // right plate
            CreateRightPlate();

            // cover plate
            CreateCoverPlate();*/
        }

        public void InitialisePlatePoints()
        {
            TSM.ContourPoint origin = new TSM.ContourPoint(_global.Origin, null);

            // front points 
            origin = _tModel.ShiftVertically(origin, elevation + height/2);
            origin = _tModel.ShiftHorizontallyRad(origin, _tModel.GetRadiusAtElevation(elevation + height/2, _global.StackSegList, true) + breadth, 1, orientationAngle);
            TopRight = _tModel.ShiftHorizontallyRad(origin, width/2, 2);
            TopLeft = _tModel.ShiftHorizontallyRad(origin, width/2, 4);
            origin = _tModel.ShiftVertically(origin, -height);
            BottomRight = _tModel.ShiftHorizontallyRad(origin, width / 2, 2);
            BottomLeft = _tModel.ShiftHorizontallyRad(origin, width / 2, 4);

            // back points
            origin = new TSM.ContourPoint(_global.Origin, null);
            origin = _tModel.ShiftVertically(origin, elevation + height / 2);

            double rad = _tModel.GetRadiusAtElevation(elevation + height / 2, _global.StackSegList, true);
            double halfWidthAngle = Math.Asin((width / 2) / rad);

            origin = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(origin, rad, 1, orientationAngle), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));

            BackTopLeft = _tModel.ShiftAlongCircumferenceRad(origin, -halfWidthAngle, 1);
            BackTopRight = _tModel.ShiftAlongCircumferenceRad(origin, halfWidthAngle, 1);

            origin = new TSM.ContourPoint(_global.Origin, null);
            origin = _tModel.ShiftVertically(origin, elevation - height / 2);

            rad = _tModel.GetRadiusAtElevation(elevation - height / 2, _global.StackSegList, true);
            halfWidthAngle = Math.Asin((width / 2) / rad);

            origin = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(origin, rad, 1, orientationAngle), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));

            BackBottomLeft = _tModel.ShiftAlongCircumferenceRad(origin, -halfWidthAngle, 1);
            BackBottomRight = _tModel.ShiftAlongCircumferenceRad(origin, halfWidthAngle, 1);


        }

        public void CreateTopPlate()
        {
            TSM.ContourPoint origin = new TSM.ContourPoint(_global.Origin, null);
            origin = _tModel.ShiftVertically(origin, elevation + height / 2);
            origin = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(origin, _tModel.GetRadiusAtElevation(elevation + height / 2, _global.StackSegList, true), 1, orientationAngle), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));


            _pointsList.Add(origin);
            _pointsList.Add(BackTopLeft);
            _pointsList.Add(TopLeft);
            _pointsList.Add(TopRight);
            _pointsList.Add(BackTopRight);


            _global.ProfileStr = "PL" + 30;
            _global.ClassStr = "12";
            _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
            _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;
            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;

            _tModel.CreateContourPlate(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position);
            _pointsList.Clear();

        }

        public void CreateBottomPlate()
        {
            TSM.ContourPoint origin = new TSM.ContourPoint(_global.Origin, null);
            origin = _tModel.ShiftVertically(origin, elevation - height / 2);
            origin = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(origin, _tModel.GetRadiusAtElevation(elevation - height / 2, _global.StackSegList, true), 1, orientationAngle), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));

            _pointsList.Add(origin);
            _pointsList.Add(BackBottomLeft);
            _pointsList.Add(BottomLeft);
            _pointsList.Add(BottomRight);
            _pointsList.Add(BackBottomRight);


            _global.ProfileStr = "PL" + 30;
            _global.ClassStr = "12";
            _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
            _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;
            _global.Position.Depth = TSM.Position.DepthEnum.FRONT;

            _tModel.CreateContourPlate(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position);
            _pointsList.Clear();

        }

        public void CreateLeftPlate()
        {
            _pointsList.Add(BackTopLeft);
            _pointsList.Add(TopLeft);
            _pointsList.Add(BottomLeft);
            _pointsList.Add(BackBottomLeft);
            
            _global.ProfileStr = "PL" + 30;
            _global.ClassStr = "12";
            _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
            _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;
            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;

            _tModel.CreateContourPlate(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position);
            _pointsList.Clear();

        }

        public void CreateRightPlate()
        {
            _pointsList.Add(BackTopRight);
            _pointsList.Add(TopRight);
            _pointsList.Add(BottomRight);
            _pointsList.Add(BackBottomRight);

            _global.ProfileStr = "PL" + 30;
            _global.ClassStr = "12";
            _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
            _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;
            _global.Position.Depth = TSM.Position.DepthEnum.FRONT;

            _tModel.CreateContourPlate(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position);
            _pointsList.Clear();

        }

        public void CreateCoverPlate()
        {

            _pointsList.Add(_tModel.ShiftHorizontallyRad(TopRight, 30, 2, orientationAngle));
            _pointsList.Add(_tModel.ShiftHorizontallyRad(TopLeft, 30, 4, orientationAngle));
            _pointsList.Add(_tModel.ShiftHorizontallyRad(BottomLeft, 30, 4, orientationAngle));
            _pointsList.Add(_tModel.ShiftHorizontallyRad(BottomRight, 30, 2, orientationAngle));


            _global.ProfileStr = "PL" + 30;
            _global.ClassStr = "1";
            _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
            _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;
            _global.Position.Depth = TSM.Position.DepthEnum.FRONT;

            _tModel.CreateContourPlate(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position);
            _pointsList.Clear();
        }

        public void CreateHandle()
        {
            TSM.ContourPoint handlePoint1 = _tModel.ShiftHorizontallyRad(TopLeft, 30, 1, orientationAngle);
            handlePoint1 = _tModel.ShiftHorizontallyRad(handlePoint1, 200, 2, orientationAngle);
            handlePoint1 = _tModel.ShiftVertically(handlePoint1, -((height/2) - Math.Min(height/8, 100)));
            TSM.ContourPoint handlePoint2 = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(handlePoint1, 80, 1, orientationAngle), new Chamfer(10, 0, Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
            TSM.ContourPoint handlePoint3 = new TSM.ContourPoint(_tModel.ShiftVertically(handlePoint2, -2 * Math.Min(height / 8, 100)), new Chamfer(10, 0, Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
            TSM.ContourPoint handlePoint4 = _tModel.ShiftVertically(handlePoint1, -2 * Math.Min(height / 8, 100));

            _pointsList.Add(handlePoint1);
            _pointsList.Add(handlePoint2);
            _pointsList.Add(handlePoint3);
            _pointsList.Add(handlePoint4);

            _global.ProfileStr = "ROD20";
            _global.ClassStr = "1";
            _global.Position.Plane = TSM.Position.PlaneEnum.MIDDLE;
            _global.Position.Depth = TSM.Position.DepthEnum.MIDDLE;
            _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;

            _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Handle");

            _pointsList.Clear();
        }

        public void CreateHinge()
        {
            // hinge rod

            TSM.ContourPoint hingeOrigin = _tModel.ShiftHorizontallyRad(TopRight, 45, 1, orientationAngle);
            hingeOrigin = _tModel.ShiftVertically(hingeOrigin, -height / 2);
            hingeOrigin = _tModel.ShiftHorizontallyRad(hingeOrigin, 45, 2, orientationAngle);


            TSM.ContourPoint hingeRodPoint1 = _tModel.ShiftVertically(hingeOrigin, height/4);
            TSM.ContourPoint hingeRodPoint2 = _tModel.ShiftVertically(hingeOrigin, -height/4);

            _tModel.CreateBeam(hingeRodPoint1, hingeRodPoint2, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "HingeRod");

            // front top hinge plate

            TSM.ContourPoint hingePlatePoint1 = _tModel.ShiftVertically(hingeRodPoint1, -30);
            hingePlatePoint1 = _tModel.ShiftHorizontallyRad(hingePlatePoint1, 10, 4, orientationAngle);
            hingePlatePoint1 = _tModel.ShiftHorizontallyRad(hingePlatePoint1, 10, 3, orientationAngle);
            TSM.ContourPoint hingePlatePoint2 = _tModel.ShiftHorizontallyRad(hingePlatePoint1, 10, 1, orientationAngle);
            TSM.ContourPoint hingePlatePoint3 = _tModel.ShiftHorizontallyRad(hingePlatePoint2, 10, 1, orientationAngle);
            hingePlatePoint3 = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(hingePlatePoint3, 10, 2, orientationAngle), new Chamfer(10, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            TSM.ContourPoint hingePlatePoint4 = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(hingePlatePoint2, 20, 2, orientationAngle), new Chamfer(10, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            TSM.ContourPoint hingePlatePoint5 = _tModel.ShiftHorizontallyRad(hingePlatePoint3, 20, 3, orientationAngle);
            TSM.ContourPoint hingePlatePoint6 = _tModel.ShiftHorizontallyRad(hingePlatePoint5, 100, 4, orientationAngle);

            _pointsList.Add(hingePlatePoint1);
            _pointsList.Add(hingePlatePoint2);
            _pointsList.Add(hingePlatePoint3);
            _pointsList.Add(hingePlatePoint4);
            _pointsList.Add(hingePlatePoint5);
            _pointsList.Add(hingePlatePoint6);

            _global.ProfileStr = "TANKO5*20";
            _global.ClassStr = "1";
            _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;

            _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Handle");

            _pointsList.Clear();

            // front bottom hinge plate

            hingePlatePoint1 = _tModel.ShiftVertically(hingePlatePoint1, -(height/2 - 60));
            hingePlatePoint2 = _tModel.ShiftVertically(hingePlatePoint2, -(height/2 - 60));
            hingePlatePoint3 = _tModel.ShiftVertically(hingePlatePoint3, -(height/2 - 60));
            hingePlatePoint4 = _tModel.ShiftVertically(hingePlatePoint4, -(height/2 - 60));
            hingePlatePoint5 = _tModel.ShiftVertically(hingePlatePoint5, -(height/2 - 60));
            hingePlatePoint6 = _tModel.ShiftVertically(hingePlatePoint6, -(height/2 - 60));

            _pointsList.Add(hingePlatePoint1);
            _pointsList.Add(hingePlatePoint2);
            _pointsList.Add(hingePlatePoint3);
            _pointsList.Add(hingePlatePoint4);
            _pointsList.Add(hingePlatePoint5);
            _pointsList.Add(hingePlatePoint6);

            _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Handle");

            _pointsList.Clear();

            // side bottom hinge plate

            hingePlatePoint1 = _tModel.ShiftVertically(hingePlatePoint1, 20);
            hingePlatePoint2 = _tModel.ShiftVertically(hingePlatePoint2, 20);
            hingePlatePoint3 = _tModel.ShiftVertically(hingePlatePoint3, 20);
            hingePlatePoint4 = _tModel.ShiftVertically(hingePlatePoint4, 20);
            hingePlatePoint5 = _tModel.ShiftVertically(hingePlatePoint5, 20);
            hingePlatePoint6 = _tModel.ShiftHorizontallyRad(hingePlatePoint2, 100, 3, orientationAngle);

            _pointsList.Add(hingePlatePoint1);
            _pointsList.Add(hingePlatePoint5);
            _pointsList.Add(hingePlatePoint4);
            _pointsList.Add(hingePlatePoint3);
            _pointsList.Add(hingePlatePoint2);
            _pointsList.Add(hingePlatePoint6);

            _global.Position.Plane = TSM.Position.PlaneEnum.LEFT;

            _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Handle");

            _pointsList.Clear();

            // side top hinge plate

            hingePlatePoint1 = _tModel.ShiftVertically(hingePlatePoint1, (height / 2 - 100));
            hingePlatePoint2 = _tModel.ShiftVertically(hingePlatePoint2, (height / 2 - 100));
            hingePlatePoint3 = _tModel.ShiftVertically(hingePlatePoint3, (height / 2 - 100));
            hingePlatePoint4 = _tModel.ShiftVertically(hingePlatePoint4, (height / 2 - 100));
            hingePlatePoint5 = _tModel.ShiftVertically(hingePlatePoint5, (height / 2 - 100));
            hingePlatePoint6 = _tModel.ShiftVertically(hingePlatePoint6, (height / 2 - 100));

            _pointsList.Add(hingePlatePoint1);
            _pointsList.Add(hingePlatePoint5);
            _pointsList.Add(hingePlatePoint4);
            _pointsList.Add(hingePlatePoint3);
            _pointsList.Add(hingePlatePoint2);
            _pointsList.Add(hingePlatePoint6);

            _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Handle");

            _pointsList.Clear();
        }

        public void CreateStackCut()
        {
            TSM.ContourPoint cutPoint1 = _tModel.ShiftHorizontallyRad(BackTopRight, width / 2, 4, orientationAngle);
            cutPoint1 = _tModel.ShiftVertically(cutPoint1, -height/2);
            cutPoint1 = _tModel.ShiftHorizontallyRad(cutPoint1, 200, 3);
            TSM.ContourPoint cutPoint2 = _tModel.ShiftHorizontallyRad(cutPoint1, 400, 1);

            _global.ProfileStr = "RHS" + width + "*" + (height-60) + "*5";
            _global.Position.Plane = TSM.Position.PlaneEnum.MIDDLE;

            TSM.Beam cut = _tModel.CreateBeam(cutPoint1, cutPoint2, _global.ProfileStr, Globals.MaterialStr, BooleanPart.BooleanOperativeClassName, _global.Position, "HingeRod");

            _tModel.cutPart(cut, _global.SegmentPartList[_tModel.GetSegmentAtElevation(elevation, _global.StackSegList)]);
        }
    }
}
 