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
using Tekla.Structures.Model;

namespace DistillationColumn
{
    class Platform
    {
        Globals _global;
        TeklaModelling _tModel;

        List<TSM.ContourPoint> _pointsList;

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

            List<JToken> platformList = _global.JData["Ladder"].ToList();

            foreach (JToken platform in platformList)
            {
                elevation = (float)platform["Elevation"];
                orientationAngle = (float)platform["Orientation_Angle"] * Math.PI / 180;
                plateWidth = (float)platform["Platform_Width"];
                platformLength = (float)platform["Platform_Length"];
                platformStartAngle = (float)platform["Platform_Start_Angle"] * Math.PI / 180;
                platformEndAngle = (float)platform["Platfrom_End_Angle"] * Math.PI / 180;
                distanceFromStack = (float)platform["Distance_From_Stack"];
                gapBetweenGratingPlate = (float)platform["Gap_Between_Grating_Plate"];
                gratingThickness = (float)platform["Grating_Thickness"];
                extensionLength = (float)platform["Extended_Length"];
                extensionStartAngle = (float)platform["Extended_Start_Angle"] * Math.PI / 180;
                extensionEndAngle = (float)platform["Extended_End_Angle"] * Math.PI / 180;

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
                    CreateFrame(startPt, endPt, false, true);
                    CreateGrating(startPt, endPt, false, true);
                    CreateBrackets(startPt, endPt, false);
                }


                // platform module after ladder
                startPt = _tModel.ShiftHorizontallyRad(origin, radius, 1, orientationAngle + theta);
                endPt = _tModel.ShiftHorizontallyRad(origin, radius, 1, platformEndAngle);
                if (platformEndAngle > orientationAngle + theta)
                {

                    if (platformEndAngle - (Math.PI * 2) > orientationAngle - theta)
                    {
                        endPt = _tModel.ShiftHorizontallyRad(origin, radius, 1, orientationAngle - theta);

                        CreateFrame(startPt, endPt, true, true);
                        CreateGrating(startPt, endPt, true, true);
                        CreateBrackets(startPt, endPt, true);
                    }

                    else
                    {
                        CreateFrame(startPt, endPt, true, false);
                        CreateGrating(startPt, endPt, true, false);
                        CreateBrackets(startPt, endPt, true);
                    }
                }

            }
        }

        void CreateGrating(TSM.ContourPoint startPoint, TSM.ContourPoint endPoint, bool parallelAtStart, bool parallelAtEnd)
        {
            startPoint = parallelAtStart ? _tModel.ShiftHorizontallyRad(startPoint, 25, 1, orientationAngle) : _tModel.ShiftHorizontallyRad(startPoint, 25, 1);
            endPoint = parallelAtEnd ? _tModel.ShiftHorizontallyRad(endPoint, 25, 1, orientationAngle) : _tModel.ShiftHorizontallyRad(endPoint, 25, 1);

            TSM.ContourPoint point1 = startPoint, point2, point3, point4, point5, point6;

            _global.ProfileStr = "PL" + gratingThickness;
            _global.ClassStr = "10";
            _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
            _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;
            _global.Position.Depth = TSM.Position.DepthEnum.FRONT;


            double length;
            double startAngle = _tModel.AngleAtCenter(startPoint);
            double endAngle = _tModel.AngleAtCenter(endPoint);
            endAngle = endAngle < startAngle ? endAngle + (Math.PI * 2) : endAngle;
            double point1Angle = _tModel.AngleAtCenter(point1);
            point1Angle = point1Angle < startAngle ? point1Angle + (Math.PI * 2) : point1Angle;
            double point3Angle;

            while (point1Angle < endAngle)
            {
                length = (point1Angle >= extensionStartAngle && point1Angle < extensionEndAngle) ? platformLength + extensionLength - 50 : platformLength - 50;

                point2 = new TSM.ContourPoint(_tModel.ShiftAlongCircumferenceRad(point1, innerPlateWidth / 2, 2), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                point3 = _tModel.ShiftAlongCircumferenceRad(point2, innerPlateWidth / 2, 2);

                point3Angle = _tModel.AngleAtCenter(point3);
                point3Angle = point3Angle < startAngle ? point3Angle + (Math.PI * 2) : point3Angle;

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

                if (point3Angle > endAngle)
                {
                    point3 = new TSM.ContourPoint(endPoint, null);
                    point2 = _tModel.ShiftAlongCircumferenceRad(point1, _tModel.ArcLengthBetweenPointsXY(point1, point3) / 2, 2);
                }

                point4 = _tModel.ShiftHorizontallyRad(point1, length, 1);
                if (point1 == startPoint)
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
                    if (parallelAtEnd)
                    {
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
                point1Angle = point1Angle < startAngle ? point1Angle + (Math.PI * 2) : point1Angle;
            }
        }

        void CreateFrame(TSM.ContourPoint startPoint, TSM.ContourPoint endPoint, bool parallelAtStart, bool parallelAtEnd)
        {
            TSM.Part startStraightBeam, endStraightBeam, innerCurvedBeam, outerCurvedStartBeam = null, outerCurvedEndBeam = new PolyBeam();
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
                phi = Math.Asin((ladderWidth / 2) / (radius + length));
                outerStartPoint = _tModel.ShiftHorizontallyRad(origin, radius + length, 1, orientationAngle + phi);
            }

            _global.ProfileStr = "C100*100*10";
            _global.ClassStr = "3";
            _global.Position.Plane = TSM.Position.PlaneEnum.RIGHT;
            _global.Position.Rotation = TSM.Position.RotationEnum.TOP;
            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;

            startStraightBeam = _tModel.CreateBeam(outerStartPoint, startPoint, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");

            // straight end beam
            length = platformLength;

            if (endAngle >= extensionStartAngle && endAngle <= extensionEndAngle)
            {
                length += extensionLength;
            }

            TSM.ContourPoint outerEndPoint = _tModel.ShiftHorizontallyRad(endPoint, length, 1);

            if (parallelAtEnd)
            {
                phi = Math.Asin((ladderWidth / 2) / (radius + length));
                outerEndPoint = _tModel.ShiftHorizontallyRad(origin, radius + length, 1, orientationAngle - phi);
            }


            endStraightBeam = _tModel.CreateBeam(endPoint, outerEndPoint, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");

            // inner curved beam
            TSM.ContourPoint midPoint = _tModel.ShiftAlongCircumferenceRad(startPoint, _tModel.ArcLengthBetweenPointsXY(startPoint, endPoint) / 2, 2);
            midPoint.Chamfer = new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT);

            _pointsList.Add(startPoint);
            _pointsList.Add(midPoint);
            _pointsList.Add(endPoint);

            innerCurvedBeam = _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");

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
                if (endAngle < extensionStartAngle)
                {
                    point3 = outerEndPoint;
                }

                point2 = _tModel.ShiftAlongCircumferenceRad(point1, _tModel.ArcLengthBetweenPointsXY(point1, point3) / 2, 2);
                point2.Chamfer = new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT);

                _pointsList.Add(point3);
                _pointsList.Add(point2);
                _pointsList.Add(point1);

                Part oBeam = _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");

                _pointsList.Clear();

                outerCurvedEndBeam = oBeam;
                if (outerCurvedStartBeam == null)
                {
                    outerCurvedStartBeam = oBeam;
                }

                // Cut at Start Points
                //Cuts(outerStartPoint, parallelAtStart, 3, 2, startStraightBeam, outerCurvedBeam1);
            }

            // extension


            if (extensionStartAngle < endAngle && extensionEndAngle > startAngle)
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

                Part extensionCurvedBeam = _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");

                _pointsList.Clear();

                outerCurvedEndBeam = extensionCurvedBeam;
                if (outerCurvedStartBeam == null)
                {
                    outerCurvedStartBeam = extensionCurvedBeam;
                }

                if (extensionStartAngle > startAngle)
                {
                    Part extensionStraightBeam = _tModel.CreateBeam(point1, _tModel.ShiftHorizontallyRad(point1, extensionLength + 100, 3), _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");

                    // Cuts for Extended Section's Straight & Curved Beam
                    Cuts(point1, false, 3, 2, extensionStraightBeam, extensionCurvedBeam);
                }

                if (extensionEndAngle < endAngle)
                {
                    Part extensionStraightBeam = _tModel.CreateBeam(_tModel.ShiftHorizontallyRad(point3, extensionLength + 100, 3), point3, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");

                    // Cuts for Extended Section's Straight & Curved Beam
                    Cuts(point3, false, 3, 4, extensionStraightBeam, extensionCurvedBeam);
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

                Part oBeam = _tModel.CreatePolyBeam(_pointsList, _global.ProfileStr, Globals.MaterialStr, _global.ClassStr, _global.Position, "Frame");

                _pointsList.Clear();


                outerCurvedEndBeam = oBeam;
                if (outerCurvedStartBeam == null)
                {
                    outerCurvedStartBeam = oBeam;
                }
            }


            // Cuts at Start Points
            Cuts(startPoint, parallelAtStart, 1, 2, startStraightBeam, innerCurvedBeam);
            Cuts(outerStartPoint, parallelAtStart, 3, 2, startStraightBeam, outerCurvedStartBeam);

            // Cuts at End Points
            Cuts(endPoint, parallelAtEnd, 1, 4, endStraightBeam, innerCurvedBeam);
            Cuts(outerEndPoint, parallelAtEnd, 3, 4, endStraightBeam, outerCurvedEndBeam);

        }

        void Cuts(TSM.ContourPoint Point1, bool parallel, int direction, int side, Part straightBeam, Part curvedBeam)
        {
            // direction = inside curved beam point || outside curved beam point
            // side -> to identify if the point is at the start or at the end 2 = start Beam & 4 = End Beam
            double diagonalAngle;

            TSM.ContourPoint origin = _tModel.ShiftVertically(_global.Origin, elevation + 100);

            double distance = direction == 1 ? _tModel.DistanceBetweenPointsXY(origin, Point1) + 100 : _tModel.DistanceBetweenPointsXY(origin, Point1) - 100;

            //diagonalAngle = !parallel ? Math.Asin(100 / distance) + _tModel.AngleAtCenter(Point1) : Math.Asin(((ladderWidth/2)+100)/distance) + orientationAngle;

            //TSM.ContourPoint Point3 = _tModel.ShiftHorizontallyRad(origin, distance, 1, diagonalAngle);
            //TSM.ContourPoint Point2 = _tModel.ShiftHorizontallyRad(Point1, 100, 2);
            //TSM.ContourPoint Point4 = side==1 ? _tModel.ShiftHorizontallyRad(Point1, 100, 1) : _tModel.ShiftHorizontallyRad(Point1, 100, 3);

            TSM.ContourPoint Point3 = new TSM.ContourPoint();
            TSM.ContourPoint Point2 = new TSM.ContourPoint();
            TSM.ContourPoint Point4 = new TSM.ContourPoint();

            switch (side)
            {
                case 2:
                    diagonalAngle = !parallel ? Math.Asin(105 / distance) + _tModel.AngleAtCenter(Point1) : Math.Asin(((ladderWidth / 2) + 105) / distance) + orientationAngle;

                    if (direction == 3)
                    {
                        Point2 = parallel ? _tModel.ShiftHorizontallyRad(Point1, 105, side, orientationAngle - (Math.PI * 5 / 180)) : _tModel.ShiftHorizontallyRad(Point1, 100, side);

                    }

                    break;
                case 4:
                    diagonalAngle = !parallel ? _tModel.AngleAtCenter(Point1) - Math.Asin(105 / distance) : orientationAngle - Math.Asin(((ladderWidth / 2) + 105) / distance);
                    if (direction == 3)
                    {
                        Point2 = parallel ? _tModel.ShiftHorizontallyRad(Point1, 105, side, orientationAngle + (Math.PI * 5 / 180)) : _tModel.ShiftHorizontallyRad(Point1, 100, side);

                    }
                    break;
                default:
                    diagonalAngle = 0;
                    break;

            }

            if (direction == 1)
            {
                Point2 = parallel ? _tModel.ShiftHorizontallyRad(Point1, 100, side, orientationAngle) : _tModel.ShiftHorizontallyRad(Point1, 100, side);

            }
            Point3 = direction == 3 ? _tModel.ShiftHorizontallyRad(origin, distance - 5, 1, diagonalAngle) : _tModel.ShiftHorizontallyRad(origin, distance + 5, 1, diagonalAngle);
            Point4 = direction == 1 ? _tModel.ShiftHorizontallyRad(Point1, 100, 1) : _tModel.ShiftHorizontallyRad(Point1, 100, 3);

            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;
            //_global.ClassStr = BooleanPart.BooleanOperativeClassName;

            // contourplate 1
            _pointsList.Add(Point1);
            _pointsList.Add(Point3);
            _pointsList.Add(Point2);

            TSM.Part cut = _tModel.CreateContourPlate(_pointsList, "PL100", Globals.MaterialStr, BooleanPart.BooleanOperativeClassName, _global.Position);
            _pointsList.Clear();

            _tModel.cutPart(cut, straightBeam);

            // contourplate 2
            _pointsList.Add(Point1);
            _pointsList.Add(Point3);
            _pointsList.Add(Point4);
            cut = _tModel.CreateContourPlate(_pointsList, "PL100", Globals.MaterialStr, BooleanPart.BooleanOperativeClassName, _global.Position);
            _pointsList.Clear();

            _tModel.cutPart(cut, curvedBeam);
        }

        void CreateBrackets(TSM.ContourPoint startPoint, TSM.ContourPoint endPoint, bool parallelAtStart)
        {
            double innerArcLengthBetBrackets = (radius - 40) / (radius + platformLength) * 1000;

            startPoint = _tModel.ShiftHorizontallyRad(startPoint, 40, 3);
            endPoint = _tModel.ShiftHorizontallyRad(endPoint, 40, 3);


            double startAngle = _tModel.AngleAtCenter(startPoint);

            double length = (startAngle >= extensionStartAngle && startAngle < extensionEndAngle) ? platformLength + extensionLength: platformLength;
            TSM.ContourPoint bracketPoint1 = new ContourPoint(startPoint, null);
            TSM.ContourPoint bracketPoint2 = _tModel.ShiftHorizontallyRad(bracketPoint1, length + 40, 1);
            if (parallelAtStart)
            {
                phi = Math.Asin((ladderWidth / 2) / (radius + length));
                bracketPoint2 = _tModel.ShiftHorizontallyRad(origin, radius + length, 1, orientationAngle + phi);
            }
            bracketPoint1 = _tModel.ShiftAlongCircumferenceRad(bracketPoint1, 200, 2);
            bracketPoint2 = _tModel.ShiftAlongCircumferenceRad(bracketPoint2, 200, 2);

            length = _tModel.DistanceBetweenPoints(bracketPoint1, bracketPoint2) - 40;

            double bracketAngle;

            double remainingArcLength = _tModel.ArcLengthBetweenPointsXY(startPoint, endPoint) - 200;

            while (remainingArcLength > 90)  // width of ISMC300
            {
                CustomPart CPart = new CustomPart();
                CPart.Name = "Platform_Bracket";
                CPart.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;
                CPart.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.LEFT;
                CPart.Position.PlaneOffset = -10;
                CPart.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.BEHIND;
                CPart.Position.DepthOffset = 95;
                CPart.Position.RotationOffset = 0;
                CPart.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.TOP;
                CPart.SetAttribute("P1", distanceFromStack);
                CPart.SetAttribute("P2", length);
                CPart.SetInputPositions(bracketPoint1, bracketPoint2);
                CPart.Insert();
                _tModel.Model.CommitChanges();
                
                bracketPoint1 = _tModel.ShiftAlongCircumferenceRad(bracketPoint1, innerArcLengthBetBrackets, 2);
                bracketPoint2 = _tModel.ShiftHorizontallyRad(bracketPoint1, length + 40, 1);

                bracketAngle = _tModel.AngleAtCenter(bracketPoint1);
                bracketAngle = bracketAngle < startAngle ? bracketAngle + (Math.PI * 2) : bracketAngle;

                length = (bracketAngle >= extensionStartAngle && bracketAngle < extensionEndAngle) ? platformLength + extensionLength : platformLength;


                remainingArcLength -= innerArcLengthBetBrackets;
            }
        }

    }
}
