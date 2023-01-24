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
        double elevation;
        double platformStartAngle;
        double platformEndAngle;
        double platformLength;
        double extensionStartAngle;
        double extensionEndAngle;
        double extensionLength;
        double width;
        double gap;
        double gratingThickness;
        double distanceFromStack;
        double frameStartAngle;
        double frameEndAngle;

        double stackRadius;
        double radius;
        double plateAngle;
        double xcod, ycod, zcod;
        double orientationAngle;
        double theta;
        double ladderWidth = 470;

        double startAngle;
        double endAngle;
        double length;

        bool extensionStartsAtMiddleOfPlatform;
        bool extensionEndsAtMiddleOfPlatform;

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
                platformStartAngle = (float)grating["Platform_Start_Angle"];
                platformEndAngle = (float)grating["Platfrom_End_Angle"];
                elevation = (float)grating["Elevation"];
                width = (float)grating["Platform_Width"];
                platformLength = (float)grating["Platform_Length"];
                gap = (float)grating["Gap_Between_Grating_Plate"];
                gratingThickness = (float)grating["Grating_Thickness"];
                distanceFromStack = (float)grating["Distance_From_Stack"];
                orientationAngle = (float)grating["Orientation_Angle"];
                extensionStartAngle = (float)grating["Extended_Start_Angle"];
                extensionEndAngle = (float)grating["Extended_End_Angle"];
                extensionLength = (float)grating["Extended_Length"];
                gratinglist.Add(new List<double> { platformStartAngle, platformEndAngle, elevation, width, platformLength, gap, gratingThickness, distanceFromStack, orientationAngle, extensionStartAngle, extensionEndAngle, extensionLength });
            }
        }

        public void ShiftAngle()
        {
            if (orientationAngle == startAngle)
            {
                theta = (180 / Math.PI) * (Math.Atan((ladderWidth + 125) / (stackRadius * 2)));
                //theta = ((ladderWidth / 2) / (radius + length)) * 180 / Math.PI;
                startAngle = startAngle + theta;

            }
            if (orientationAngle == endAngle)
            {
                theta = (180 / Math.PI) * (Math.Atan((ladderWidth + 125) / (2 * stackRadius)));
                //theta = ((ladderWidth / 2) / (radius + length)) * 180 / Math.PI;
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
                platformStartAngle = grating[0];
                platformEndAngle = grating[1];
                elevation = grating[2];
                width = grating[3];
                platformLength = grating[4];
                gap = grating[5];
                distanceFromStack = grating[7];
                orientationAngle = grating[8];
                gratingThickness = (float)grating[6];
                extensionStartAngle = grating[9];
                extensionEndAngle = grating[10];
                extensionLength = grating[11];
                theta = theta = (180 / Math.PI) * (Math.Atan(ladderWidth / (radius * 2)));

                stackRadius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList, true);
                radius = stackRadius + distanceFromStack;
                extensionStartsAtMiddleOfPlatform = false;
                extensionEndsAtMiddleOfPlatform = false;

                if (extensionStartAngle > platformStartAngle)
                {
                    extensionStartsAtMiddleOfPlatform = true;
                }

                if (extensionEndAngle < platformEndAngle)
                {
                    extensionEndsAtMiddleOfPlatform = true;
                }

                //ShiftAngle();

                plateAngle = (180 * width) / (Math.PI * (radius + platformLength));

                // first half of platform

                startAngle = platformStartAngle;
                endAngle = extensionStartAngle;
                length = platformLength;
                frameStartAngle = startAngle;
                frameEndAngle = endAngle;


                int count;

                if (startAngle != endAngle)
                {
                    ShiftAngle();

                    count = Convert.ToInt32((endAngle - startAngle) / plateAngle);

                    CreateCircularGrating(count);
                    CreateFrame(1);
                }

                // extension

                startAngle = extensionStartAngle;
                endAngle = extensionEndAngle;
                length = platformLength + extensionLength;
                frameStartAngle = startAngle;
                frameEndAngle = endAngle;

                if (startAngle != endAngle)
                {
                    ShiftAngle();

                    count = Convert.ToInt32((endAngle - startAngle) / plateAngle);

                    CreateCircularGrating(count);
                    CreateFrame(2);
                }


                // second half of platform

                startAngle = extensionEndAngle;
                endAngle = platformEndAngle;
                length = platformLength;
                frameStartAngle = startAngle;
                frameEndAngle = endAngle;

                if (startAngle != endAngle)
                {
                    ShiftAngle();

                    count = Convert.ToInt32((endAngle - startAngle) / plateAngle);

                    CreateCircularGrating(count);
                    CreateFrame(3);
                }

                CreateBrackets2();
            }


        }

        public void CreateCuts(Part poly)
        {
            ContourPoint xaxis = new ContourPoint(new Point(radius, 0, elevation), null);
            o1 = _tModel.ShiftHorizontallyRad(_tModel.ShiftAlongCircumferenceRad(xaxis, (Math.PI / 180) * orientationAngle, 1), length, 1);
            o2 = _tModel.ShiftHorizontallyRad(o1, radius + length, 3);

            Beam cut1 = new Beam();
            cut1.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.MIDDLE;
            cut1.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.MIDDLE;
            cut1 = _tModel.CreateBeam(o1, o2, "PL" + (ladderWidth + 200) + "*" + (ladderWidth + 200), "IS2062", BooleanPart.BooleanOperativeClassName, cut1.Position, "");
            _tModel.cutPart(cut1, poly);
        }

        public void CreateCircularGrating(int count)
        {
            {




                for (int i = 0; i <= count; i++)
                {
                    ContourPoint origin = new ContourPoint(_tModel.ShiftVertically(_global.Origin, elevation+50+gratingThickness),null);
                    radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList, true);
                    radius = radius + distanceFromStack;



                    ContourPlate poly = new ContourPlate();
                    poly.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.BACK;
                    poly.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.LEFT;



                    if ((startAngle + ((i + 1) * plateAngle)) < endAngle)
                    {
                        xcod = (radius) * Math.Cos((Math.PI / 180) * (startAngle + (i * plateAngle)));
                        ycod = (radius) * Math.Sin((Math.PI / 180) * (startAngle + (i * plateAngle)));
                        zcod = elevation + 50 + gratingThickness;



                        p1 = new ContourPoint(new Point(xcod, ycod, zcod), null);
                        p2 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 180) * (plateAngle / 2), 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                        p3 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 180) * (plateAngle), 1), null);
                        p4 = new ContourPoint(_tModel.ShiftHorizontallyRad(p1, length, 1), null);
                        if (orientationAngle + theta == startAngle && i == 0)
                        {
                            double t = Math.Asin(500 / (radius + length)); ;
                            p4 = new ContourPoint(_tModel.ShiftHorizontallyRad(origin, radius + length, 1,(orientationAngle*Math.PI/180)+t), null);
                           
                            //p4 = new ContourPoint(_tModel.ShiftHorizontallyRad(p1, length, 1, orientationAngle * Math.PI / 180), null);
                        }




                        p5 = new ContourPoint(_tModel.ShiftHorizontallyRad(p2, length, 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                        p6 = new ContourPoint(_tModel.ShiftHorizontallyRad(p3, length, 1), null);




                        List<ContourPoint> gratingPointList = new List<ContourPoint> { p1, p2, p3, p6, p5, p4 };



                        poly = _tModel.CreateContourPlate(gratingPointList, "PL" + gratingThickness, "IS2062", "1", poly.Position);



                        if (orientationAngle == startAngle)
                        {
                            CreateCuts(poly);
                        }
                        if (orientationAngle > startAngle && orientationAngle < endAngle && orientationAngle - theta <= startAngle + ((i + 1) * plateAngle) && orientationAngle + theta >= startAngle + ((i) * plateAngle))
                        {
                            CreateCuts(poly);
                        }




                    }
                    else
                    {



                        xcod = (radius) * Math.Cos((Math.PI / 180) * (startAngle + (i * plateAngle)));
                        ycod = (radius) * Math.Sin((Math.PI / 180) * (startAngle + (i * plateAngle)));
                        zcod = elevation + 50 + gratingThickness;
                        double lastPlateAngle = (endAngle - (startAngle + (i * plateAngle)));
                        p1 = new ContourPoint(new Point(xcod, ycod, zcod), null);
                        p2 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 180) * (lastPlateAngle / 2), 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                        p3 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 180) * (lastPlateAngle), 1), null);



                        p4 = new ContourPoint(_tModel.ShiftHorizontallyRad(p1, length, 1), null);
                        p5 = new ContourPoint(_tModel.ShiftHorizontallyRad(p2, length, 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                        p6 = new ContourPoint(_tModel.ShiftHorizontallyRad(p3, length, 1), null);
                        if (orientationAngle - theta == endAngle)
                        {
                            double t = Math.Asin(500 / (radius + length)); ;
                            p6 = new ContourPoint(_tModel.ShiftHorizontallyRad(origin, radius + length, 1, (orientationAngle * Math.PI / 180) - t), null);
                        }



                        List<ContourPoint> gratingPointList = new List<ContourPoint> { p1, p2, p3, p6, p5, p4 };



                        poly = _tModel.CreateContourPlate(gratingPointList, "PL" + gratingThickness, "IS2062", "1", poly.Position);



                        if (orientationAngle == endAngle)
                        {
                            CreateCuts(poly);
                        }
                        if (orientationAngle > startAngle && orientationAngle < endAngle && orientationAngle - theta <= startAngle + ((i + 1) * plateAngle) && orientationAngle + theta >= startAngle + ((i) * plateAngle))
                        {
                            CreateCuts(poly);
                        }
                        break;
                    }
                }



            }



        }

        public void CreateFrame(int platformSection)
        {

            ContourPoint origin = _tModel.ShiftVertically(_global.Origin, elevation);
            p1 = new ContourPoint(new Point(radius * Math.Cos(Math.PI * frameStartAngle / 180), radius * Math.Sin(Math.PI * frameStartAngle / 180), elevation), null);


            double phi =Math.Atan( ((ladderWidth + 125) / 2) / stackRadius);
            double phi2 = Math.Atan(((ladderWidth + 125) / 2) / (stackRadius + length));

            p4 = new ContourPoint(_tModel.ShiftHorizontallyRad(p1, length, 1), null);
            if (orientationAngle == frameStartAngle)
            {
                p1 = _tModel.ShiftAlongCircumferenceRad(p1, phi, 1); ;
                double t = Math.Asin(500 / (radius + length)) ;
                p4 = new ContourPoint(_tModel.ShiftHorizontallyRad(origin, radius + length, 1, (orientationAngle * Math.PI / 180) + t), null);
            }
            /*p2 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 360) * (frameEndAngle - frameStartAngle), 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            p3 = _tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 180) * (frameEndAngle - frameStartAngle), 1);
            p5 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(p4, (Math.PI / 360) * (frameEndAngle - frameStartAngle), 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            p6 = _tModel.ShiftAlongCircumferenceRad(p4, (Math.PI / 180) * (frameEndAngle - frameStartAngle), 1);*/

            //ContourPoint origin = _tModel.ShiftVertically(_global.Origin, elevation);

            p2 = new ContourPoint(_tModel.ShiftHorizontallyRad(origin, radius, 1, (frameEndAngle + frameStartAngle) * Math.PI / 360), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            p3 = _tModel.ShiftHorizontallyRad(origin, radius, 1, frameEndAngle * Math.PI / 180);
            p5 = new ContourPoint(_tModel.ShiftHorizontallyRad(origin, radius + length, 1, (frameEndAngle + frameStartAngle) * Math.PI / 360), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            p6 = _tModel.ShiftHorizontallyRad(origin, radius + length, 1, frameEndAngle * Math.PI / 180);

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

            if (platformSection == 2)
            {
                if (extensionStartsAtMiddleOfPlatform && orientationAngle != frameStartAngle)
                {
                    p1 = _tModel.ShiftHorizontallyRad(p1, platformLength - 100, 1);
                }
                if (extensionEndsAtMiddleOfPlatform && orientationAngle != frameEndAngle)
                {
                    p3 = _tModel.ShiftHorizontallyRad(p3, platformLength - 100, 1);
                }
            }

            if (!(platformSection == 3 && extensionEndsAtMiddleOfPlatform && orientationAngle != frameStartAngle))
            {
                Beam startBeam = _tModel.CreateBeam(p4, p1, "C100*100*10", "IS2062", "3", innerBeam.Position);
            }

            if (!(platformSection == 1 && extensionStartsAtMiddleOfPlatform && orientationAngle != frameEndAngle))
            {
                Beam endBeam = _tModel.CreateBeam(p3, p6, "C100*100*10", "IS2062", "3", innerBeam.Position);
            }

            if (platformSection == 2)
            {
                if (extensionStartsAtMiddleOfPlatform)
                {
                    p1 = _tModel.ShiftHorizontallyRad(p1, platformLength - 100, 3);
                }
                if (extensionEndsAtMiddleOfPlatform)
                {
                    p3 = _tModel.ShiftHorizontallyRad(p3, platformLength - 100, 3);
                }
            }

            if (orientationAngle > startAngle && orientationAngle < endAngle)
            {
                /*double theta2 = (180 / Math.PI) * Math.Atan((ladderWidth + 250) / (2 * (radius + length)));

                theta = (180 / Math.PI) * (Math.Atan((ladderWidth + 200) / (2 * stackRadius)));

                //p1 = new ContourPoint(new Point(radius * Math.Cos((Math.PI / 180) * (orientationAngle - theta)), radius * Math.Sin((Math.PI / 180) * (orientationAngle - theta)), elevation), null);

                ContourPoint x1 = new ContourPoint(new Point(radius * Math.Cos((Math.PI / 180) * (orientationAngle - theta)), radius * Math.Sin((Math.PI / 180) * (orientationAngle - theta)), elevation), null);

                _tModel.CreateBeam(x1, _tModel.ShiftAlongCircumferenceRad(o1, -(Math.PI * theta2 / 180), 1), "C100*100*10", "IS2062", "3", innerBeam.Position);

                _tModel.CreateBeam(_tModel.ShiftAlongCircumferenceRad(o1, (Math.PI * theta2 / 180), 1), _tModel.ShiftAlongCircumferenceRad(x1, (Math.PI * theta / 90), 1), "C100*100*10", "IS2062", "3", innerBeam.Position);
*/

                double angle1 = Math.Asin(((ladderWidth + 200) /2) / radius);
                double angle2 = Math.Asin(((ladderWidth + 200) / 2) / (radius + length));

                ContourPoint p1 = _tModel.ShiftHorizontallyRad(origin, radius, 1, (orientationAngle * Math.PI / 180) + angle1);
                ContourPoint p2 = _tModel.ShiftHorizontallyRad(origin, radius + length, 1, (orientationAngle * Math.PI / 180) + angle2);

                _tModel.CreateBeam(p2, p1, "C100*100*10", "IS2062", "3", innerBeam.Position);

                p1 = _tModel.ShiftHorizontallyRad(origin, radius, 1, (orientationAngle * Math.PI / 180) - angle1);
                p2 = _tModel.ShiftHorizontallyRad(origin, radius + length, 1, (orientationAngle * Math.PI / 180) - angle2);

                _tModel.CreateBeam(p1, p2, "C100*100*10", "IS2062", "3", innerBeam.Position);

                CreateCuts(outerBeam);

                CreateCuts(innerBeam);
            }
        }

        public void CreateBrackets()
        {
            double arcLength = (Math.PI / 180) * (frameEndAngle - frameStartAngle) * (radius + length);
            int count = Convert.ToInt32((arcLength - 200) / 1000);

            double theta2 = Math.Atan(((ladderWidth / 2) / (radius + length)) * (180 / Math.PI));
            double cutArc = (Math.PI / 180) * (orientationAngle - frameStartAngle + theta2) * (radius + length);
            double cutArc2 = (Math.PI / 180) * (orientationAngle - frameStartAngle - theta2) * (radius + length);

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
                CPart.SetAttribute("P2", length);



                ContourPoint b1 = _tModel.ShiftAlongCircumferenceRad(p4, 200, 2);
                ContourPoint b2 = _tModel.ShiftAlongCircumferenceRad(p1, 200, 2);
                b2 = _tModel.ShiftHorizontallyRad(b2, 40, 3);

                if (i == 0)
                {
                    CPart.SetInputPositions(b2, b1);
                }
                else
                {
                    b1 = _tModel.ShiftAlongCircumferenceRad(b1, i * 1000, 2);
                    b2 = _tModel.ShiftHorizontallyRad(b1, length + 40, 3);
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

        public void CreateBrackets2()
        {
            if (orientationAngle == platformStartAngle)
            {
                theta = (180 / Math.PI) * (Math.Atan((ladderWidth + 125) / (stackRadius * 2)));
                //theta = ((ladderWidth / 2) / (radius + length)) * 180 / Math.PI;
                platformStartAngle = platformStartAngle + theta;

            }
            if (orientationAngle == platformEndAngle)
            {
                theta = (180 / Math.PI) * (Math.Atan((ladderWidth + 125) / (2 * stackRadius)));
                //theta = ((ladderWidth / 2) / (radius + length)) * 180 / Math.PI;
                platformEndAngle = platformEndAngle - theta;
            }

            ContourPoint origin = _tModel.ShiftVertically(_global.Origin, elevation - 50);

            double angleBetweenbrackets = 1000 / (radius + length);
            double startOffsetAngle = 200 / (radius);

            int count = Convert.ToInt16((((platformEndAngle - platformStartAngle) * Math.PI / 180) - startOffsetAngle) / angleBetweenbrackets);

            ContourPoint b1 = _tModel.ShiftHorizontallyRad(origin, radius - 40, 1, platformStartAngle * Math.PI / 180);
            ContourPoint b2 = _tModel.ShiftHorizontallyRad(b1, length, 1);

            for ( int i = 0; i < count; i++)
            {
                length = platformLength;
                double bracketAngle = _tModel.AngleAtCenter(b1);
                if (bracketAngle >= (extensionStartAngle * Math.PI / 180) && bracketAngle <= (extensionEndAngle * Math.PI / 180))
                {
                    length = platformLength + extensionLength;
                }

                if (i == 0)
                {
                    b1 = _tModel.ShiftAlongCircumferenceRad(b1, startOffsetAngle, 1);
                    b2 = _tModel.ShiftHorizontallyRad(b1, length, 1, bracketAngle);

                    if (orientationAngle + theta== platformStartAngle)
                    {
                        b2 = _tModel.ShiftHorizontallyRad(b1, length, 1, (orientationAngle) * Math.PI / 180);
                    }

                }
                CustomPart CPart = new CustomPart();
                CPart.Name = "Platform_Bracket";
                CPart.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;
                CPart.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.LEFT;
                CPart.Position.PlaneOffset = -10;
                CPart.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.BEHIND;
                CPart.Position.DepthOffset = 5;
                CPart.Position.RotationOffset = 0;
                CPart.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.TOP;
                CPart.SetAttribute("P1", distanceFromStack);
                CPart.SetAttribute("P2", length);
                CPart.SetInputPositions(b1, b2);
                if (!(bracketAngle > ((orientationAngle - theta) * Math.PI / 180) && bracketAngle < ((orientationAngle + theta) * Math.PI / 180)))
                {
                    CPart.Insert();
                    _tModel.Model.CommitChanges();
                }

                b1 = _tModel.ShiftAlongCircumferenceRad(b1, angleBetweenbrackets, 1);
                b2 = _tModel.ShiftHorizontallyRad(b1, length, 1);

            }

            
        }
    }
}
