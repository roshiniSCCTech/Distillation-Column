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
using Tekla.Structures;
using System.Windows.Forms;

namespace DistillationColumn
{
    class CircularGrating
    {
        double startAngle;
        double endAngle;
        double elevation;
        double platLength;
        double width;
        double gap;
        double gratingThickness;
        double distanceFromStack;
        double frameStartAngle;
        double frameEndAngle;

        double radius;
        double plateAngle;
        double xcod, ycod, zcod;
        double orientationAngle;
        double theta;
        double ladderWidth = 800;


        ContourPoint p1;
        ContourPoint p2;
        ContourPoint p3;
        ContourPoint p4;
        ContourPoint p5;
        ContourPoint p6;
        ContourPoint o1;
        ContourPoint o2;

        List<List<double>> gratinglist;
        public Globals _global;
        public TeklaModelling _tModel;

        public void SetGratingData()
        {
            List<JToken> _gratinglist = _global.JData["Ladder"].ToList();
            foreach (JToken grating in _gratinglist)
            {
                startAngle = (float)grating["Platform_Start_Angle"];
                endAngle = (float)grating["Platfrom_End_Angle"];
                elevation = (float)grating["Elevation"];
                width = (float)grating["Platform_Width"];
                platLength = (float)grating["Platform_Length"];
                gap = (float)grating["Gap_Between_Grating_Plate"];
                gratingThickness = (float)grating["Grating_Thickness"];
                distanceFromStack = (float)grating["Distance_From_Stack"];
                orientationAngle = (float)grating["Orientation_Angle"];
                gratinglist.Add(new List<double> { startAngle, endAngle, elevation, width, platLength, gap, gratingThickness, distanceFromStack, orientationAngle });
            }
        }

        public void ShiftAngle()
        {
            if (orientationAngle == startAngle)
            {
                //theta = (180 / Math.PI) * (Math.Atan(ladderWidth / (radius * 2)));
                theta = ((ladderWidth / 2) / (radius + platLength)) * 180 / Math.PI;
                startAngle = startAngle + theta;

            }
            if (orientationAngle == endAngle)
            {
                //theta = (180 / Math.PI) * (Math.Atan(ladderWidth / (2 * radius)));
                theta = ((ladderWidth / 2) / (radius + platLength)) * 180 / Math.PI;
                endAngle = endAngle - theta;
            }
        }

        public CircularGrating(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;

            gratinglist = new List<List<double>>();

            SetGratingData();
            foreach (List<double> grating in gratinglist)
            {
                startAngle = grating[0];
                endAngle = grating[1];
                elevation = grating[2];
                width = grating[3];
                platLength = grating[4];
                gap = grating[5];
                distanceFromStack = grating[7];
                orientationAngle = grating[8];
                gratingThickness = (float)grating[6];

                radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList, true);
                radius = radius + distanceFromStack;

                frameStartAngle = startAngle;
                frameEndAngle = endAngle;

                ShiftAngle();

                plateAngle = (180 * width) / (Math.PI * (radius + platLength));

                int count = Convert.ToInt32((endAngle - startAngle) / plateAngle);

                CreateCircularGrating(count);
                CreateFrame();
                CreateBrackets();
                createHandrail();

            }


        }

        public void CreateCuts(Part poly)
        {
            ContourPoint xaxis = new ContourPoint(new Point(radius, 0, elevation), null);
            o1 = _tModel.ShiftHorizontallyRad(_tModel.ShiftAlongCircumferenceRad(xaxis, (Math.PI / 180) * orientationAngle, 1), platLength, 1);
            o2 = _tModel.ShiftHorizontallyRad(o1, radius + platLength, 3);

            Beam cut1 = new Beam();
            cut1.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.MIDDLE;
            cut1.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.MIDDLE;
            cut1 = _tModel.CreateBeam(o1, o2, "PL" + ladderWidth + "*" + ladderWidth, "IS2062", BooleanPart.BooleanOperativeClassName, cut1.Position, "");
            _tModel.cutPart(cut1, poly);
        }

        public void CreateCircularGrating(int count)
        {
            {


                for (int i = 0; i <= count; i++)
                {
                    radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList, true);
                    radius = radius + distanceFromStack;

                    PolyBeam poly = new PolyBeam();
                    poly.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.BACK;
                    poly.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.LEFT;

                    if ((startAngle + ((i + 1) * plateAngle)) < endAngle)
                    {
                        xcod = (radius + platLength) * Math.Cos((Math.PI / 180) * (startAngle + (i * plateAngle)));
                        ycod = (radius + platLength) * Math.Sin((Math.PI / 180) * (startAngle + (i * plateAngle)));
                        zcod = elevation + 50 + gratingThickness;

                        p1 = new ContourPoint(new Point(xcod, ycod, zcod), null);
                        p2 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 180) * (plateAngle / 2), 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                        p3 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 180) * (plateAngle), 1), null);


                        List<ContourPoint> gratingPointList = new List<ContourPoint> { p1, p2, p3 };

                        poly = _tModel.CreatePolyBeam(gratingPointList, "PL" + gratingThickness + "*" + platLength, "IS2062", "1", poly.Position);

                        if (orientationAngle + theta == startAngle && i == 0)
                        {
                            CreateCuts(poly);
                        }
                        if (orientationAngle > startAngle && orientationAngle < endAngle && orientationAngle > startAngle + ((i - 1) * plateAngle) && orientationAngle < startAngle + ((i + 2) * plateAngle))
                        {
                            CreateCuts(poly);
                        }


                    }
                    else
                    {

                        xcod = (radius + platLength) * Math.Cos((Math.PI / 180) * (startAngle + (i * plateAngle)));
                        ycod = (radius + platLength) * Math.Sin((Math.PI / 180) * (startAngle + (i * plateAngle)));
                        zcod = elevation + 50 + gratingThickness;
                        plateAngle = (endAngle - (startAngle + (i * plateAngle)));
                        p1 = new ContourPoint(new Point(xcod, ycod, zcod), null);
                        p2 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 180) * (plateAngle / 2), 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                        p3 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 180) * (plateAngle), 1), null);

                        List<ContourPoint> gratingPointList = new List<ContourPoint> { p1, p2, p3 };

                        poly = _tModel.CreatePolyBeam(gratingPointList, "PL" + gratingThickness + "*" + platLength, "IS2062", "1", poly.Position);

                        if (orientationAngle - theta == endAngle)
                        {
                            CreateCuts(poly);
                        }
                        break;
                    }
                }

            }

        }

        public void CreateFrame()
        {
            p1 = new ContourPoint(new Point(radius * Math.Cos(Math.PI * frameStartAngle / 180), radius * Math.Sin(Math.PI * frameStartAngle / 180), elevation), null);
            p2 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 360) * (frameEndAngle - frameStartAngle), 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            p3 = _tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 180) * (frameEndAngle - frameStartAngle), 1);

            p4 = new ContourPoint(new Point((radius + platLength) * Math.Cos(Math.PI * frameStartAngle / 180), (radius + platLength) * Math.Sin(Math.PI * frameStartAngle / 180), elevation), null);
            p5 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(p4, (Math.PI / 360) * (frameEndAngle - frameStartAngle), 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            p6 = _tModel.ShiftAlongCircumferenceRad(p4, (Math.PI / 180) * (frameEndAngle - frameStartAngle), 1);

            double phi = (ladderWidth / 2) / radius;
            double phi2 = (ladderWidth / 2) / (radius + platLength);

            if (orientationAngle == frameStartAngle)
            {
                p1 = _tModel.ShiftAlongCircumferenceRad(p1, phi, 1);
                p4 = _tModel.ShiftAlongCircumferenceRad(p4, phi2, 1);
            }
            if (orientationAngle == frameEndAngle)
            {
                p3 = _tModel.ShiftAlongCircumferenceRad(p3, -phi, 1);
                p6 = _tModel.ShiftAlongCircumferenceRad(p6, -phi2, 1);
            }

            List<ContourPoint> innerBeamList = new List<ContourPoint> { p1, p2, p3 };

            PolyBeam innerBeam = new PolyBeam();
            innerBeam.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
            innerBeam.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.MIDDLE;

            innerBeam = _tModel.CreatePolyBeam(innerBeamList, "C100*100*10", "IS2062", "3", innerBeam.Position);

            _tModel.Model.CommitChanges();

            List<ContourPoint> outerBeamList = new List<ContourPoint> { p6, p5, p4 };

            PolyBeam outerBeam = _tModel.CreatePolyBeam(outerBeamList, "C100*100*10", "IS2062", "3", innerBeam.Position);

            Beam startBeam = _tModel.CreateBeam(p4, p1, "C100*100*10", "IS2062", "3", innerBeam.Position);

            Beam endBeam = _tModel.CreateBeam(p3, p6, "C100*100*10", "IS2062", "3", innerBeam.Position);

            if (orientationAngle > startAngle && orientationAngle < endAngle)
            {
                double theta2 = (180 / Math.PI) * (Math.Atan(ladderWidth / (2 * (radius + platLength))));

                theta = (180 / Math.PI) * (Math.Atan(ladderWidth / (2 * radius)));

                //p1 = new ContourPoint(new Point(radius * Math.Cos((Math.PI / 180) * (orientationAngle - theta)), radius * Math.Sin((Math.PI / 180) * (orientationAngle - theta)), elevation), null);

                ContourPoint x1 = new ContourPoint(new Point(radius * Math.Cos((Math.PI / 180) * (orientationAngle - theta)), radius * Math.Sin((Math.PI / 180) * (orientationAngle - theta)), elevation), null);

                _tModel.CreateBeam(x1, _tModel.ShiftAlongCircumferenceRad(o1, -(Math.PI * theta2 / 180), 1), "C100*100*10", "IS2062", "3", innerBeam.Position);

                _tModel.CreateBeam(_tModel.ShiftAlongCircumferenceRad(o1, (Math.PI * theta2 / 180), 1), _tModel.ShiftAlongCircumferenceRad(x1, (Math.PI * theta / 90), 1), "C100*100*10", "IS2062", "3", innerBeam.Position);


                CreateCuts(outerBeam);

                CreateCuts(innerBeam);
            }
        }

        public void CreateBrackets()
        {
            double arcLength = (Math.PI / 180) * (frameEndAngle - frameStartAngle) * (radius + platLength);
            int count = Convert.ToInt32((arcLength - 200) / 1000);

            double theta2 = Math.Atan(((ladderWidth / 2) / (radius + platLength)) * (180 / Math.PI));
            double cutArc = (Math.PI / 180) * (orientationAngle - frameStartAngle + theta2) * (radius + platLength);
            double cutArc2 = (Math.PI / 180) * (orientationAngle - frameStartAngle - theta2) * (radius + platLength);

            for (int i = 0; i < count; i++)
            {
                CustomPart CPart = new CustomPart();
                CPart.Name = "Platform_Bracket";
                CPart.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;
                CPart.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.MIDDLE;
                CPart.Position.PlaneOffset = 0;
                CPart.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.BEHIND;
                CPart.Position.DepthOffset = 50;
                CPart.Position.RotationOffset = 0;
                CPart.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.TOP;
                CPart.SetAttribute("P1", distanceFromStack);
                CPart.SetAttribute("P2", platLength);

                ContourPoint b1 = _tModel.ShiftAlongCircumferenceRad(p4, 200, 2);
                ContourPoint b2 = _tModel.ShiftAlongCircumferenceRad(p1, 200, 2);
                if (i == 0)
                {
                    CPart.SetInputPositions(b2, b1);
                }
                else
                {
                    b1 = _tModel.ShiftAlongCircumferenceRad(b1, i * 1000, 2);
                    b2 = _tModel.ShiftHorizontallyRad(b1, platLength, 3);
                    CPart.SetInputPositions(b2, b1);
                }
                //CPart.Insert();
                if (orientationAngle > frameStartAngle && orientationAngle < frameEndAngle)
                {
                    if ((i * 1000) + 200 < cutArc2 || (i * 1000) > cutArc)
                    {
                        CPart.Insert();
                    }

                }
                else CPart.Insert();
            }

            _tModel.Model.CommitChanges();
        }

        public void createHandrail()
        {
            ContourPoint point = new ContourPoint(new Point(_tModel.ShiftVertically(_global.Origin,elevation)),null);
            ContourPoint point1 = new ContourPoint(new Point(_tModel.ShiftHorizontallyRad(point,radius+(platLength/2),1,(frameEndAngle* Math.PI/180))), null);
            ContourPoint point2 = new ContourPoint(new Point(_tModel.ShiftHorizontallyRad(point1,500 , 2)), null);

            CustomPart CPart = new CustomPart();
            CPart.Name = "Rectangular_Handrail";
            CPart.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;
            CPart.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.MIDDLE;
            CPart.Position.PlaneOffset = 0.0;
            CPart.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.FRONT;
            CPart.Position.DepthOffset = 0;
            CPart.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.TOP;
            CPart.SetInputPositions(point1,point2);

            CPart.SetAttribute("width", 5);
            CPart.SetAttribute("distance",(platLength-600));
            CPart.SetAttribute("P2", 0);

            bool b = CPart.Insert();

        }
    }
}