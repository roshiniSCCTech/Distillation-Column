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
                origin = _tModel.ShiftVertically(_global.Origin, elevation + 100);
                innerPlateWidth = (radius + 25) / (radius + platformLength - 25) * plateWidth;

                if (platformStartAngle < 0)
                {
                    platformStartAngle += Math.PI * 2;
                    platformEndAngle += Math.PI * 2;
                    extensionStartAngle += Math.PI * 2;
                    extensionEndAngle += Math.PI * 2;
                    orientationAngle += Math.PI * 2;
                }

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
        }

        void SetAccessDoorData()
        {
            List<JToken> platformList = _global.JData["Ladder"].ToList();

            foreach (JToken platform in platformList)
            {
                double elevation = (float)platform["Elevation"];
                double orientationAngle = (float)platform["Orientation_Angle"];
                double platfomWidth = (float)platform["Platform_Width"];
                double platformLength = (float)platform["Platform_Length"];
                double platformStartAngle = (float)platform["Platform_Start_Angle"];
                double platformEndAngle = (float)platform["Platfrom_End_Angle"];
                double distanceFromStack = (float)platform["Distance_From_Stack"];
                double gapBetweenGratingPlate = (float)platform["Gap_Between_Grating_Plate"];
                double gratingThickness = (float)platform["Grating_Thickness"];
                double extensionLength = (float)platform["Extended_Length"];
                double extensionStartAngle = (float)platform["Extended_Start_Angle"];
                double extensionEndAngle = (float)platform["Extended_End_Angle"];


                _platformList.Add(new List<double> { elevation, orientationAngle, platfomWidth, platformLength, platformStartAngle, platformEndAngle, distanceFromStack, gapBetweenGratingPlate, gratingThickness, extensionLength, extensionStartAngle, extensionEndAngle });
            }

        }

        void CreatePlatformModule(TSM.ContourPoint startPoint, TSM.ContourPoint endPoint, bool parallelAtStart, bool parallelAtEnd)
        {
            CreateFrame(startPoint, endPoint, parallelAtStart, parallelAtEnd);
            CreateGrating(startPoint, endPoint, parallelAtStart, parallelAtEnd);
        }

        void CreateGrating(TSM.ContourPoint startPoint, TSM.ContourPoint endPoint, bool parallelAtStart, bool parallelAtEnd)
        {
            startPoint = parallelAtStart ? _tModel.ShiftHorizontallyRad(startPoint, 25, 1, orientationAngle) : _tModel.ShiftHorizontallyRad(startPoint, 25, 1);
            endPoint = parallelAtEnd ? _tModel.ShiftHorizontallyRad(endPoint, 25, 1, orientationAngle) : _tModel.ShiftHorizontallyRad(endPoint, 25, 1);

            TSM.ContourPoint point1 = startPoint, point2, point3, point4, point5, point6;

            _global.ProfileStr = "PL25";
            _global.ClassStr = "10";
            _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
            _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;
            _global.Position.Depth = TSM.Position.DepthEnum.FRONT;


            double length;
            double startAngle = _tModel.AngleAtCenter(startPoint);
            double endAngle = _tModel.AngleAtCenter(endPoint);
            endAngle = endAngle < startAngle ? endAngle + (Math.PI * 2) : endAngle;
            if(extensionStartAngle - (Math.PI * 2) < endAngle && extensionEndAngle - (Math.PI * 2) > startAngle)
            {
                extensionStartAngle -= (Math.PI * 2);
                extensionEndAngle -= (Math.PI * 2);
            }

            double point1Angle = _tModel.AngleAtCenter(point1);
            if (point1Angle < startAngle)
            {
                point1Angle += Math.PI * 2;
            }

            double point3Angle;

            while (point1Angle < endAngle)
            {
                length = platformLength - 50;

                point2 = new TSM.ContourPoint(_tModel.ShiftAlongCircumferenceRad(point1, innerPlateWidth / 2, 2), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                point3 = _tModel.ShiftAlongCircumferenceRad(point2, innerPlateWidth / 2, 2);

                point3Angle = _tModel.AngleAtCenter(point3);
                if (point3Angle < startAngle)
                {
                    point3Angle += Math.PI * 2;
                }

                if (point1Angle >= extensionStartAngle && point1Angle < extensionEndAngle)
                {
                    length += extensionLength;
                }

                // if extension starts at the middle of current plate
                if (point1Angle < extensionStartAngle && point3Angle > extensionStartAngle)
                {
                    point3 = _tModel.ShiftHorizontallyRad(origin, radius + 25, 1, extensionStartAngle);
                    point2 = _tModel.ShiftAlongCircumferenceRad(point1, _tModel.ArcLengthBetweenPointsXY(point1, point3) / 2, 2);
                }

                // if extension ends at the middle of current plate
                if (point1Angle < extensionEndAngle && point3Angle > extensionEndAngle)
                {
                    point3 = _tModel.ShiftHorizontallyRad(origin, radius + 25, 1, extensionEndAngle);
                    point2 = _tModel.ShiftAlongCircumferenceRad(point1, _tModel.ArcLengthBetweenPointsXY(point1, point3) / 2, 2);
                }

                if ( point3Angle > endAngle)
                {
                    point3 = new TSM.ContourPoint(endPoint, null);
                    point2 = _tModel.ShiftAlongCircumferenceRad(point1, _tModel.ArcLengthBetweenPointsXY(point1, point3)/2, 2);
                }

                point4 = _tModel.ShiftHorizontallyRad(point1, length, 1);
                if ( point1 == startPoint)
                {
                    if (parallelAtStart)
                    {
                        phi = Math.Asin((ladderWidth / 2) / (radius + length + 25));
                        point4 = _tModel.ShiftHorizontallyRad(origin, radius + length + 25, 1, orientationAngle + phi);
                    }

                    point1 = _tModel.ShiftAlongCircumferenceRad(point1, 25, 3);
                    point4 = _tModel.ShiftAlongCircumferenceRad(point4, 25, 3);
                }
                point5 = _tModel.ShiftHorizontallyRad(point2, length, 1);
                point5.Chamfer = new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT);

                point6 = _tModel.ShiftHorizontallyRad(point3, length, 1);
                if (point3 == endPoint)
                {
                    if (parallelAtEnd) {
                        phi = Math.Asin((ladderWidth / 2) / (radius + length + 25));
                        point6 = _tModel.ShiftHorizontallyRad(origin, radius + length + 25, 1, orientationAngle - phi);
                    }

                    point3 = _tModel.ShiftAlongCircumferenceRad(point3, -25, 3);
                    point6 = _tModel.ShiftAlongCircumferenceRad(point6, -25, 3);

                    endPoint = new TSM.ContourPoint(point3, null);
                    endAngle = _tModel.AngleAtCenter(endPoint);
                    endAngle = endAngle < startAngle ? endAngle + (Math.PI * 2) : endAngle;
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

                point1Angle = _tModel.AngleAtCenter(point1);
                if (point1Angle < startAngle)
                {
                    point1Angle += Math.PI * 2;
                }


            }

        }

        void CreateFrame(TSM.ContourPoint startPoint, TSM.ContourPoint endPoint, bool parallelAtStart, bool parallelAtEnd)
        {
            double length;
            double startAngle = _tModel.AngleAtCenter(startPoint);
            double endAngle = _tModel.AngleAtCenter(endPoint);
            endAngle = endAngle < startAngle ? endAngle + (Math.PI * 2) : endAngle;
            if (extensionStartAngle - (Math.PI * 2) < endAngle && extensionEndAngle - (Math.PI * 2) > startAngle)
            {
                extensionStartAngle -= (Math.PI * 2);
                extensionEndAngle -= (Math.PI * 2);
            }

            // straight start beam
            length = platformLength;

            if (startAngle >= extensionStartAngle && startAngle <= extensionEndAngle)
            {
                length += extensionLength;
            }

            TSM.ContourPoint outerStartPoint = _tModel.ShiftHorizontallyRad(startPoint, length, 1);

            if (parallelAtStart)
            {
                phi = Math.Asin((ladderWidth / 2) / (radius + length + 25));
                outerStartPoint = _tModel.ShiftHorizontallyRad(origin, radius + length, 1, orientationAngle + phi);
            }

            _global.ProfileStr = "C100*100*10";
            _global.ClassStr = "3";
            _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
            _global.Position.Rotation = TSM.Position.RotationEnum.TOP;
            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;

            _tModel.CreateBeam(outerStartPoint, startPoint, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");

            // straight end beam
            length = platformLength;

            if (endAngle >= extensionStartAngle && endAngle <= extensionEndAngle)
            {
                length += extensionLength;
            }

            TSM.ContourPoint outerEndPoint = _tModel.ShiftHorizontallyRad(endPoint, length, 1);

            if (parallelAtEnd)
            {
                phi = Math.Asin((ladderWidth / 2) / (radius + length + 25));
                outerEndPoint = _tModel.ShiftHorizontallyRad(origin, radius + length, 1, orientationAngle - phi);
            }

            _tModel.CreateBeam(endPoint, outerEndPoint, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");

            // inner curved beam
            TSM.ContourPoint midPoint = _tModel.ShiftAlongCircumferenceRad(startPoint, _tModel.ArcLengthBetweenPointsXY(startPoint, endPoint)/2, 2);
            midPoint.Chamfer = new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT);

            _pointsList.Add(startPoint);
            _pointsList.Add(midPoint);
            _pointsList.Add(endPoint);

            _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");

            _pointsList.Clear();

            // outer curved beam made in 3 parts

            TSM.ContourPoint point1;
            TSM.ContourPoint point2;
            TSM.ContourPoint point3;

            // first half of platform 

            if (startAngle < extensionStartAngle)
            {
                point1 = new TSM.ContourPoint(outerStartPoint, null);
                point3 = _tModel.ShiftHorizontallyRad(origin, radius + platformLength, 1, extensionStartAngle);
                if(endAngle < extensionStartAngle)
                {
                    point3 = outerEndPoint;
                }

                point2 = _tModel.ShiftAlongCircumferenceRad(point1, _tModel.ArcLengthBetweenPointsXY(point1, point3) / 2, 2);
                point2.Chamfer = new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT);

                _pointsList.Add(point3);
                _pointsList.Add(point2);
                _pointsList.Add(point1);

                _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");

                _pointsList.Clear();
            }

            // extension 


            if ( extensionStartAngle < endAngle && extensionEndAngle > startAngle)
            {
                point1 = _tModel.ShiftHorizontallyRad(origin, radius + platformLength + extensionLength, 1, extensionStartAngle);
                if (startAngle > extensionStartAngle)
                {
                    point1 = outerStartPoint;
                }
                point3 = _tModel.ShiftHorizontallyRad(origin, radius + platformLength + extensionLength, 1, extensionEndAngle);
                if (endAngle < extensionEndAngle)
                {
                    point3 = outerEndPoint;
                }

                point2 = _tModel.ShiftAlongCircumferenceRad(point1, _tModel.ArcLengthBetweenPointsXY(point1, point3) / 2, 2);
                point2.Chamfer = new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT);

                _pointsList.Add(point3);
                _pointsList.Add(point2);
                _pointsList.Add(point1);

                _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");

                _pointsList.Clear();

                if (extensionStartAngle > startAngle)
                {
                    _tModel.CreateBeam(point1, _tModel.ShiftHorizontallyRad(point1, extensionLength + 100, 3), _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");
                }

                if (extensionEndAngle < endAngle)
                {
                    _tModel.CreateBeam( _tModel.ShiftHorizontallyRad(point3, extensionLength + 100, 3), point3, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");
                }
            }

            // second half of platform 

            if (endAngle > extensionEndAngle)
            {
                point1 = _tModel.ShiftHorizontallyRad(origin, radius + platformLength, 1, extensionEndAngle);
                if (startAngle > extensionEndAngle)
                {
                    point1 = outerStartPoint;
                }
                point3 = new TSM.ContourPoint(outerEndPoint, null);
                point2 = _tModel.ShiftAlongCircumferenceRad(point1, _tModel.ArcLengthBetweenPointsXY(point1, point3) / 2, 2);
                point2.Chamfer = new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT);

                _pointsList.Add(point3);
                _pointsList.Add(point2);
                _pointsList.Add(point1);

                _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");

                _pointsList.Clear();
            }
        }

    }
}
