using System;
using System.Collections.Generic;
using Tekla.Structures.Datatype;
using Tekla.Structures.Model;
using static Tekla.Structures.Filtering.Categories.ReinforcingBarFilterExpressions;
using static Tekla.Structures.Filtering.Categories.TaskFilterExpressions;
using T3D = Tekla.Structures.Geometry3d;
using TSM = Tekla.Structures.Model;

namespace HelperLibrary
{
    public class GeometricalHelperClass
    {
        readonly T3D.Point _origin;

        protected GeometricalHelperClass(double originX, double originY, double originZ)
        {
            _origin = new T3D.Point(originX, originY, originZ);
        }


        public double AngleAtCenter(T3D.Point point)
        {
            double angle = Math.Atan((point.Y - _origin.Y) / (point.X - _origin.X)); // angle of point from x-axis
            if (point.X == _origin.X)
            {
                angle = Math.PI / 2;

                if (point.Y == _origin.Y)
                {
                    angle = 0;
                }

                else if (point.Y < _origin.Y)
                {
                    angle = 3 * Math.PI / 2;
                }
            }
            else if (point.X < _origin.X)
            {
                angle += Math.PI;
            }

            if(angle < 0)
            {
                angle += 2 * Math.PI;
            }

            return angle;
        }

        // new point gets shifted along circumference at same elevation of given point
        // it gets shifted anti-clockwise if offset is positive, it gets shifted clockwise if offset is negative
        public TSM.ContourPoint ShiftAlongCircumferenceRad(TSM.ContourPoint point, double offset, short option) // 1. offset = angle in radians / arcLen / chordLen, 2. option = 1(angle), 2(arcLen), 3(chordLen)
        {
            TSM.ContourPoint shiftedPt;
            double ptAngle = AngleAtCenter(point);
            
            double rad = Math.Sqrt(Math.Pow((point.Y - _origin.Y), 2) + Math.Pow((point.X - _origin.X), 2));
            switch (option)
            {
                case 1:  // shift point by offset = angle (in radians)
                    shiftedPt = new TSM.ContourPoint(new T3D.Point(
                    _origin.X + (rad * Math.Cos(ptAngle + offset)),
                    _origin.Y + (rad * Math.Sin(ptAngle + offset)),
                    point.Z), null);
                    break;

                case 2:  // shift point by offset = arc length
                    shiftedPt = new TSM.ContourPoint(new T3D.Point(
                    _origin.X + (rad * Math.Cos(ptAngle + (offset / rad))),
                    _origin.Y + (rad * Math.Sin(ptAngle + (offset / rad))), 
                    point.Z), null);
                    break;

                case 3: // shift point by offset = chord length
                    double theta = Math.Asin(offset / (2 * rad)) * 2;
                    shiftedPt = new TSM.ContourPoint(new T3D.Point(
                    _origin.X + (rad * Math.Cos(ptAngle + theta)),
                    _origin.Y + (rad * Math.Sin(ptAngle + theta)),
                    point.Z), null);
                    break;
                default:
                    shiftedPt = point;
                    break;
            }
            return shiftedPt;
        }

        // new point gets shifted along the 4 axis. 
        // when angle is not given as parametrer, the angle formed by point (first parameter) at the origin from x - axis is taken as angle.
        // when angle is given, the 4 axis gets rotated by that angle.
        public TSM.ContourPoint ShiftHorizontallyRad(TSM.ContourPoint point, double dist, int side, double angle = double.NaN)
        {
            TSM.ContourPoint shiftedPt;
            if (double.IsNaN(angle))
            {
                angle = AngleAtCenter(point);
            }

            switch(side)
            {
                case 1:
                    shiftedPt = new TSM.ContourPoint( new T3D.Point(
                    point.X + (dist * Math.Cos(angle)),
                    point.Y + (dist * Math.Sin(angle)),
                    point.Z), null);
                    break;
                case 2:
                    shiftedPt = new TSM.ContourPoint(new T3D.Point(
                    point.X - (dist * Math.Sin(angle)),
                    point.Y + (dist * Math.Cos(angle)),
                    point.Z), null);
                    break;
                case 3:
                    shiftedPt = new TSM.ContourPoint(new T3D.Point(
                    point.X - (dist * Math.Cos(angle)),
                    point.Y - (dist * Math.Sin(angle)),
                    point.Z), null);
                    break;
                case 4:
                    shiftedPt = new TSM.ContourPoint(new T3D.Point(
                    point.X + (dist * Math.Sin(angle)),
                    point.Y - (dist * Math.Cos(angle)),
                    point.Z), null);
                    break;
                default:
                    shiftedPt = point;
                    break;
            }


            return shiftedPt;
        }

        // new point gets shifted along z-axis by dist
        // it gets shifted above if dist is positive, gets shifted below if dist is negative
        public TSM.ContourPoint ShiftVertically(TSM.ContourPoint point, double dist)
        {
            TSM.ContourPoint shiftedPt = new TSM.ContourPoint(point, point.Chamfer);
            shiftedPt.Z += dist;
            return shiftedPt;
        }

        public double DistanceBetweenPoints( T3D.Point point1, T3D.Point point2)
        {
            double distance = Math.Sqrt((Math.Pow((point1.Y - point2.Y), 2) + Math.Pow((point1.X - point2.X), 2)) + Math.Pow((point1.Z - point2.Z), 2));
            return distance;
        }

        public double DistanceBetweenPointsXY(T3D.Point point1, T3D.Point point2)
        {
            double distance = Math.Sqrt((Math.Pow((point1.Y - point2.Y), 2) + Math.Pow((point1.X - point2.X), 2)));
            return distance;
        }

        public TSM.ContourPoint MidPoint( T3D.Point point1, T3D.Point point2)
        {
            TSM.ContourPoint mid = new TSM.ContourPoint(new T3D.Point((point1.X + point2.X)/2, (point1.Y + point2.Y)/2, (point1.Z + point2.Z)/2), null);

            return mid;
        }

        public double AngleBetweenPointsXY(T3D.Point point1, T3D.Point point2)
        {
            double angle1 = AngleAtCenter(point1);
            double angle2 = AngleAtCenter(point2);
            if(angle2 < angle1)
            {
                angle2 += 2 * Math.PI;
            }

            double angle = angle2 - angle1;

            return angle;
        }

        public double ArcLengthBetweenPointsXY(T3D.Point point1, T3D.Point point2)
        {
            double rad = DistanceBetweenPointsXY(_origin, point1);
            double angle = AngleBetweenPointsXY(point1, point2);

            double arcLength = rad * angle;
            return arcLength;
        }

        // returns index of segment at elevation FROM STACK BASE
        public int GetSegmentAtElevation(double elevation, List<List<double>> stackSegList)
        {
            int index = 0;

            for (int seg = 0; seg < stackSegList.Count; seg++)
            {
                if (elevation < stackSegList[seg][4] + stackSegList[seg][3])
                {
                    index = seg;
                    break;
                }
            }

            return index;
        }

        // returns inner radius of segment at elevation FROM STACK BASE
        public double GetRadiusAtElevation(double elevation, List<List<double>> stackSeglist, bool thickness = false)
        {
            int seg = GetSegmentAtElevation(elevation, stackSeglist);
            
            double height1 = stackSeglist[seg][3];
            double base1 = (stackSeglist[seg][0] - stackSeglist[seg][1]) / 2;
            double height2 = (stackSeglist[seg][4] + stackSeglist[seg][3]) - elevation;
            double base2 = base1 * height2 / height1;

            double radius = (stackSeglist[seg][1] / 2) + base2;
            if (thickness)
            {
                radius += stackSeglist[seg][2];
            }

            return radius;
        }

        public double SlopeOfLine(double[] P, double[] Q)
        {
            double slope = (Q[1] - P[1]) / (Q[0] - P[0]) ;
            return slope;
        }

        // returns f(X) given the X for the equation of a point-slope form of a line
        public double PointSlopeForm(double[] P, double slope, double X)
        {
            double Fx = (X * slope) - (P[0] * slope) + P[1];
            return Fx;
        }

        public TSM.ContourPoint IntersectionOfLineXY(TSM.ContourPoint P1, TSM.ContourPoint P2, TSM.ContourPoint Q1, TSM.ContourPoint Q2)
        {
            double slope1 = SlopeOfLine(new[] { P1.X, P1.Y }, new[] { P2.X, P2.Y });
            double slope2 = SlopeOfLine(new[] { Q1.X, Q1.Y }, new[] { Q2.X, Q2.Y });

            double X = ((P1.X * slope1) - P1.Y - (Q1.X * slope2) + Q1.Y) / (slope1 - slope2);
            double Y = PointSlopeForm(new[] { P1.X, P1.Y }, slope1, X);

            return new TSM.ContourPoint(new T3D.Point(X, Y, P1.Z), null);
        }

        public TSM.ContourPoint IntersectionOfLineXZ(TSM.ContourPoint P1, TSM.ContourPoint P2, TSM.ContourPoint Q1, TSM.ContourPoint Q2)
        {
            double slope1 = SlopeOfLine(new[] { P1.X, P1.Z }, new[] { P2.X, P2.Z });
            double slope2 = SlopeOfLine(new[] { Q1.X, Q1.Z }, new[] { Q2.X, Q2.Z });

            double X = ((P1.X * slope1) - P1.Z - (Q1.X * slope2) + Q1.Z) / (slope1 - slope2);
            double Z = PointSlopeForm(new[] { P1.X, P1.Z }, slope1, X);

            return new TSM.ContourPoint(new T3D.Point(X, P1.Y, Z), null);
        }
    }
}
