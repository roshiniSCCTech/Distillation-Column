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

            orientationAngle = 0 * Math.PI/180;
            elevation = 10000;
            height = 1000;
            width = 1500;
            breadth = 500;

            TopRight = new TSM.ContourPoint();
            TopLeft = new TSM.ContourPoint();
            BottomRight = new TSM.ContourPoint();
            BottomLeft = new TSM.ContourPoint();

            _pointsList = new List<TSM.ContourPoint>();
            _accessDoorList = new List<List<double>>();

            Build();
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

            SetAccessDoorData();
            CreateAccessDoor();
        }


        public void CreateAccessDoor()
        {
            foreach (List<double> acDoor in _accessDoorList)
            {
                double elevation = acDoor[0];
                double orientationAngle = acDoor[1];
                double radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList, true);

                TSM.ContourPoint origin = new TSM.ContourPoint(_global.Origin, null);
                TSM.ContourPoint point1 = _tModel.ShiftVertically(origin, elevation);
                TSM.ContourPoint point2 = _tModel.ShiftHorizontallyRad(point1, radius, 1, orientationAngle);


                CustomPart accessDoor = new CustomPart();
                accessDoor.Name = "AccessDoor";
                accessDoor.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;

                accessDoor.SetInputPositions(point1, point2);
                accessDoor.SetAttribute("P1", acDoor[3]); // width
                accessDoor.SetAttribute("P2", acDoor[4]); // breadth
                accessDoor.SetAttribute("P3", radius); // radius
                accessDoor.SetAttribute("P5", acDoor[2]); // height
                accessDoor.Insert();
                _tModel.Model.CommitChanges();
            }
        }

        public void SetAccessDoorData()
        {
            List<JToken> accessDoorList = _global.JData["access_door"].ToList();
            foreach (JToken accessDoor in accessDoorList)
            {
                double elevation = (float)accessDoor["elevation"];
                double orientationAngle = (float)accessDoor["orientation_angle"];
                double height = (float)accessDoor["height"];
                double width = (float)accessDoor["width"];
                double breadth = (float)accessDoor["breadth"];

                _accessDoorList.Add(new List<double> { elevation, orientationAngle, height, width, breadth});
            }
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
    }
}
 