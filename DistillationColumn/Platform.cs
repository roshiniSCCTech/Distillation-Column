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

namespace DistillationColumn
{
    class Platform
    {
        Globals _global;
        TeklaModelling _tModel;

        List<TSM.ContourPoint> _pointsList;

        List<List<double>> _platformList;

        TSM.ContourPoint origin;
        double ladderWidth = 790; // 470 (ladderWidth rung to rung) + 2 * 100 (stringer width) + 2 * 50 (handrail diameter) + 2 * 10 (weld plate thickness)
        double radius; // stackRadius + distance from stack
        double theta;
        double phi;
        double innerPlateWidth; // inner arc length of a grating plate

        double elevation;
        double orientationAngle;
        double plateWidth;
        double platformLength;
        double platformStartAngle;
        double platformEndAngle;
        double distanceFromStack;
        double gapBetweenGratingPlate;
        double gratingThickness;
        double extensionLength;
        double extensionStartAngle;
        double extensionEndAngle;

        public Platform(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;

            _pointsList = new List<TSM.ContourPoint>();
            _platformList = new List<List<double>>();

            SetAccessDoorData();
            Build();
        }

        void SetAccessDoorData()
        {
            List<JToken> accessDoorList = _global.JData["Ladder"].ToList();

            foreach (JToken accessDoor in accessDoorList)
            {
                double elevation = (float)accessDoor["Elevation"];
                double orientationAngle = (float)accessDoor["Orientation_Angle"];
                double platfomWidth = (float)accessDoor["Platform_Width"];
                double platformLength = (float)accessDoor["Platform_Length"];
                double platformStartAngle = (float)accessDoor["Platform_Start_Angle"];
                double platformEndAngle = (float)accessDoor["Platfrom_End_Angle"];
                double distanceFromStack = (float)accessDoor["Distance_From_Stack"];
                double gapBetweenGratingPlate = (float)accessDoor["Gap_Between_Grating_Plate"];
                double gratingThickness = (float)accessDoor["Grating_Thickness"];
                double extensionLength = (float)accessDoor["Extended_Length"];
                double extensionStartAngle = (float)accessDoor["Extended_Start_Angle"];
                double extensionEndAngle = (float)accessDoor["Extended_End_Angle"];


                _platformList.Add(new List<double> { elevation, orientationAngle, platfomWidth, platformLength, platformStartAngle, platformEndAngle, distanceFromStack, gapBetweenGratingPlate, gratingThickness, extensionLength, extensionStartAngle, extensionEndAngle });
            }

        }

        void Build()
        {
            foreach (List<double> platform in _platformList)
            {
                elevation = platform[0];
                orientationAngle = platform[1] * Math.PI / 180;
                plateWidth = platform[2];
                platformLength = platform[3];
                platformStartAngle = platform[4] * Math.PI / 180;
                platformEndAngle = platform[5] * Math.PI / 180;
                distanceFromStack = platform[6];
                gapBetweenGratingPlate = platform[7];
                gratingThickness = platform[8];
                extensionLength = platform[9];
                extensionStartAngle = platform[10] * Math.PI / 180;
                extensionEndAngle = platform[11] * Math.PI / 180;

                radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList, true) + distanceFromStack;
                theta = Math.Asin((ladderWidth / 2) / radius);
                origin = _tModel.ShiftVertically(_global.Origin, elevation);
                innerPlateWidth = radius / (radius + platformLength) * plateWidth;

                CreatePlatform();
            }
        }

        void CreatePlatform()
        {

            TSM.ContourPoint startPt;
            TSM.ContourPoint endPt;

            // platform module before ladder
            startPt = _tModel.ShiftHorizontallyRad(origin, radius, 1, platformStartAngle);
            endPt = _tModel.ShiftHorizontallyRad(origin, radius, 1, orientationAngle - theta);
            if (platformStartAngle < orientationAngle - theta)
            {
                CreatePlatformModule(startPt, endPt, false, true);
            }


            // platform module after ladder
            startPt = _tModel.ShiftHorizontallyRad(origin, radius, 1, orientationAngle + theta);
            endPt = _tModel.ShiftHorizontallyRad(origin, radius, 1, platformEndAngle);
            if (platformEndAngle > orientationAngle + theta)
            {
                CreatePlatformModule(startPt, endPt, true, false);
            }
            
        }

        void CreatePlatformModule(TSM.ContourPoint startPoint, TSM.ContourPoint endPoint, bool parallelAtStart, bool parallelAtEnd)
        {

            CreateGrating(startPoint, endPoint, parallelAtStart, parallelAtEnd);
        }

        void CreateGrating(TSM.ContourPoint startPoint, TSM.ContourPoint endPoint, bool parallelAtStart, bool parallelAtEnd)
        {
            double length;

            TSM.ContourPoint point1 = startPoint;
            TSM.ContourPoint point2;
            TSM.ContourPoint point3;
            TSM.ContourPoint point4;
            TSM.ContourPoint point5;
            TSM.ContourPoint point6;

            _global.ProfileStr = "PL25";
            _global.ClassStr = "10";
            _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
            _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;
            _global.Position.Depth = TSM.Position.DepthEnum.FRONT;

            while (_tModel.AngleAtCenter(point1) < _tModel.AngleAtCenter(endPoint))
            {
                length = platformLength;

                point2 = new TSM.ContourPoint(_tModel.ShiftAlongCircumferenceRad(point1, innerPlateWidth / 2, 2), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                point3 = _tModel.ShiftAlongCircumferenceRad(point2, innerPlateWidth / 2, 2);

                if (_tModel.AngleAtCenter(point1) >= extensionStartAngle && _tModel.AngleAtCenter(point1) <= extensionEndAngle)
                {
                    length += extensionLength;
                }

                // if extension starts at the middle of current plate
                if (_tModel.AngleAtCenter(point1) < extensionStartAngle && _tModel.AngleAtCenter(point3) >= extensionStartAngle)
                {
                    point3 = _tModel.ShiftHorizontallyRad(origin, radius, 1, extensionStartAngle);
                    point2 = _tModel.ShiftAlongCircumferenceRad(point1, _tModel.ArcLengthBetweenPointsXY(point1, point3) / 2, 2);
                }

                // if extension ends at the middle of current plate
                if (_tModel.AngleAtCenter(point1) < extensionEndAngle && _tModel.AngleAtCenter(point3) > extensionEndAngle)
                {
                    point3 = _tModel.ShiftHorizontallyRad(origin, radius, 1, extensionEndAngle);
                    point2 = _tModel.ShiftAlongCircumferenceRad(point1, _tModel.ArcLengthBetweenPointsXY(point1, point3) / 2, 2);
                }

                if (_tModel.AngleAtCenter(point3) > _tModel.AngleAtCenter(endPoint))
                {
                    point3 = endPoint;
                    point2 = _tModel.ShiftAlongCircumferenceRad(point1, _tModel.ArcLengthBetweenPointsXY(point1, point3)/2, 2);
                }

                point4 = _tModel.ShiftHorizontallyRad(point1, length, 1);
                if ( point1 == startPoint && parallelAtStart)
                {
                    phi = Math.Asin((ladderWidth / 2) / (radius + length));
                    point4 = _tModel.ShiftHorizontallyRad(origin, radius + length, 1, orientationAngle + phi);

                }
                point5 = _tModel.ShiftHorizontallyRad(point2, length, 1);
                point5.Chamfer = new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT);

                point6 = _tModel.ShiftHorizontallyRad(point3, length, 1);
                if (point3 == endPoint && parallelAtEnd)
                {
                    phi = Math.Asin((ladderWidth / 2) / (radius + length));
                    point6 = _tModel.ShiftHorizontallyRad(origin, radius + length, 1, orientationAngle - phi);

                }
                
                

                _pointsList.Add(point1);
                _pointsList.Add(point2);
                _pointsList.Add(point3);
                _pointsList.Add(point6);
                _pointsList.Add(point5);
                _pointsList.Add(point4);

                _tModel.CreateContourPlate(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Platform");

                _pointsList.Clear();

                point1 = _tModel.ShiftAlongCircumferenceRad(point3, gapBetweenGratingPlate, 2);

            }

        }
    }
}
