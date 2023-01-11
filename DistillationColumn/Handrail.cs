using HelperLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSM = Tekla.Structures.Model;
using Tekla.Structures.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Tekla.Structures.Geometry3d;
using Tekla.Structures;

namespace DistillationColumn
{
    class Handrail
    {

        Globals _global;
        TeklaModelling _tModel;


        double ladderOrientation = 0;
        double platFormStartAngle = 50;
        double platFormEndAngle = 150;
        double elevation = 26000;
        double platformLength = 1000;
        double distanceFromStack = 200;
        double gratingOuterRadius;
        List<double> arcLengthList = new List<double>();

        public Handrail(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;

            createHandrail();
        }

        //public void SetHandrailrData()
        //{
        //  List<JToken> HandrailList = _global.jData["Handrail"].ToList();
        //  foreach (JToken handrail in HandrailList)
        //  {
        //    orientationAngle = (float)ladder["Orientation_Angle"];
        //    elevation = (float)ladder["Elevation"];
        //    width = (float)ladder["Width"];
        //    height = (float)ladder["Height"];
        //    rungSpacing = (float)ladder["Rungs_spacing"];
        //    _ladderList.Add(new List<double> { orientationAngle, elevation, width, height, rungSpacing });
        //  }

        void createHandrail1()
        {
            CustomPart handrail = new CustomPart();
            handrail.Name = "Final_HandRail";
            handrail.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;
            
            TSM.ContourPoint point1 = _tModel.ShiftVertically(_global.Origin, elevation);
          
            
            TSM.ContourPoint point2 = _tModel.ShiftHorizontallyRad(point1, gratingOuterRadius,1,platFormStartAngle*(Math.PI/180));
            point2 = _tModel.ShiftAlongCircumferenceRad(point2, 275, 2);

           
          
            for (int i = 0; i < arcLengthList.Count; i++)
            {
                if (i+1== arcLengthList.Count - 1)
                {
                    if(arcLengthList[i+1]>1100 && arcLengthList[i + 1]<=2600)
                    {
                        arcLengthList.Add(arcLengthList[i + 1] - 600);
                        arcLengthList.RemoveAt(i + 1);
                    }


                    if (arcLengthList[i + 1] >2500)
                    {
                        arcLengthList.Add((arcLengthList[i + 1] - 1100)/2);
                        arcLengthList.Add((arcLengthList[i + 1] - 1100) / 2);
                        arcLengthList.RemoveAt(i + 1);
                    }

                    //if (arcLengthList[i+1] < 600)
                    //{
                    //    double sum = (arcLengthList[i+1] + arcLengthList[i]) / 2;
                    //    arcLengthList.RemoveAt(i);
                    //    arcLengthList.RemoveAt(i + 1);
                    //    arcLengthList.Add(sum);
                    //    arcLengthList.Add(sum);
                    //}

                }
                

              

                if(i>0)
                {
                    point2 = _tModel.ShiftAlongCircumferenceRad(point2, arcLengthList[i-1] + 600, 2);
                }


                handrail.SetInputPositions(point1, point2);
                handrail.SetAttribute("Radius", gratingOuterRadius);
                handrail.SetAttribute("Arc_Length", arcLengthList[i]);
                handrail.SetAttribute("P1", 0);

                handrail.Position.Plane = Position.PlaneEnum.LEFT;
                handrail.Position.PlaneOffset = -300;
                handrail.Position.Rotation = Position.RotationEnum.TOP;
                handrail.Position.Depth = Position.DepthEnum.FRONT;

                bool b=handrail.Insert();
               

                       
               
            }
            _tModel.Model.CommitChanges();
        }

        void createHandrail()
        {
            double elevation = 5500;
            double orientationAngle = 0 * Math.PI / 180;
            double radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList);
            gratingOuterRadius = radius + distanceFromStack + platformLength;
            createGrating();
            double totalArcLength;
            double totalAngle = platFormEndAngle - platFormStartAngle;
            totalArcLength = Math.Abs(2 * Math.PI * gratingOuterRadius * (totalAngle / 360));
            calculateArcLengthOfCircularHandrail(totalArcLength);
            createHandrail1();
            arcLengthList.Clear();

        }
        void calculateArcLengthOfCircularHandrail(double totalArcLength)
        {
            double tempArcLength = 0.0;
            while (totalArcLength > 0)
            {
                tempArcLength = totalArcLength - 2600;
                if (tempArcLength < 600)
                {
                    arcLengthList.Add(totalArcLength);
                    break;
                }
                else
                {
                    arcLengthList.Add(2000);
                    totalArcLength = totalArcLength - 2600;
                }
            }

        }
        void createGrating()
        {
            List<TSM.ContourPoint> _pointsList = new List<ContourPoint>();

            double midAngle = ((platFormEndAngle + platFormStartAngle) / 2) ;

            TSM.ContourPoint origin = new TSM.ContourPoint(_global.Origin, null);
            TSM.ContourPoint point1 = _tModel.ShiftVertically(origin, elevation);
            TSM.ContourPoint startPoint = _tModel.ShiftHorizontallyRad(point1, gratingOuterRadius, 1, platFormStartAngle*Math.PI/180);
            TSM.ContourPoint midPoint = _tModel.ShiftHorizontallyRad(point1, gratingOuterRadius, 1, (midAngle * Math.PI / 180));
            midPoint = new ContourPoint(midPoint, new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            TSM.ContourPoint endPoint = _tModel.ShiftHorizontallyRad(point1, gratingOuterRadius, 1, (platFormEndAngle * Math.PI / 180));


            //_global.ProfileStr = "PL" + "1200" + "*25";
            //_global.ClassStr = "10";
            _global.Position.Plane = TSM.Position.PlaneEnum.LEFT;
            _global.Position.Rotation = TSM.Position.RotationEnum.FRONT;
            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;
            _pointsList.Add(startPoint);
            _pointsList.Add(midPoint);
            _pointsList.Add(endPoint);
            _tModel.CreatePolyBeam(_pointsList, "PL1200*25", "IS2062", "5", _global.Position, "");

        }
    }
}
