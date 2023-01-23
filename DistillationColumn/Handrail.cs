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
using Render;

namespace DistillationColumn
{
  class Handrail
  {
    Globals _global;
    TeklaModelling _tModel;


    double ladderOrientation ;
    double platformStartAngle;
    double platformEndAngle ;
    double elevation ;
    double platformLength ;
    double distanceFromStack;
    double gratingOuterRadius;
    double gratingThickness;
    double extensionStartAngle;
    double extensionEndAngle;
    double extensionLength;
    double startAngle;
    double endAngle;
    double radius;
    double length;
    double ladderWidth = 800.0;
    double theta;
    List<double> arcLengthList = new List<double>();
    List<List<double>> handRailData;

    public Handrail(Globals global, TeklaModelling tModel)
    {
        _global = global;
        _tModel = tModel;
        handRailData= new List<List<double>>();
        SetHandrailrData();



        foreach (List<double> grating in handRailData)
        {
            platformStartAngle = grating[0];
            platformEndAngle = grating[1];
            elevation = grating[2];
            platformLength = grating[3];
            distanceFromStack = grating[5];
            gratingThickness = (float)grating[4];
            ladderOrientation = grating[6];
            extensionStartAngle = grating[7];
            extensionEndAngle = grating[8];
            extensionLength = grating[9];
                
                        

                        // first half of platform

   

            startAngle = platformStartAngle;
            endAngle = extensionStartAngle;
            length = platformLength;
            radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList,true);
            gratingOuterRadius =( radius+25+10) + distanceFromStack + length; //25 Pipe radius and 10-plate

                        

            if (startAngle != endAngle)
            {
                    ShiftAngle();
                    createHandrail();
            }

            // extension

            startAngle = extensionStartAngle;
            endAngle = extensionEndAngle;
            length = platformLength + extensionLength;
          
            if (startAngle != endAngle)
            {
                ShiftAngle();
                createHandrail();

            }


            // second half of platform

            startAngle = extensionEndAngle;
            endAngle = platformEndAngle;
            length = platformLength;
                       
            if (startAngle != endAngle)
            {
                ShiftAngle();
                createHandrail();

            }

            //CreateBrackets2();
        }
    }

    public void ShiftAngle()
    {
        if (ladderOrientation == startAngle)
        {
            theta =  180/Math.PI*(Math.Atan((ladderWidth + 125) / (gratingOuterRadius * 2)));
            //theta = ((ladderWidth / 2) / (radius + length)) * 180 / Math.PI;
            startAngle = startAngle + theta;

        }
        if (ladderOrientation == endAngle)
        {
            theta = 180 / Math.PI*(Math.Atan((ladderWidth + 125) / (2 * gratingOuterRadius)));
            //theta = ((ladderWidth / 2) / (radius + length)) * 180 / Math.PI;
            endAngle = endAngle - theta;
        }
    }




        public void SetHandrailrData()
    {
          
            List<JToken> _gratinglist = _global.JData["Ladder"].ToList();
            foreach (JToken grating in _gratinglist)
            {
                platformStartAngle = (float)grating["Platform_Start_Angle"];
                platformEndAngle = (float)grating["Platfrom_End_Angle"];
                elevation = (float)grating["Elevation"];             
                platformLength = (float)grating["Platform_Length"];          
                gratingThickness = (float)grating["Grating_Thickness"];
                distanceFromStack = (float)grating["Distance_From_Stack"];
                ladderOrientation = (float)grating["Orientation_Angle"];
                extensionStartAngle = (float)grating["Extended_Start_Angle"];
                extensionEndAngle = (float)grating["Extended_End_Angle"];
                extensionLength = (float)grating["Extended_Length"];
                handRailData.Add(new List<double> { platformStartAngle, platformEndAngle, elevation,  platformLength, gratingThickness, distanceFromStack, ladderOrientation, extensionStartAngle, extensionEndAngle, extensionLength });
            }
            

    }

    void createHandrail1()
    {
        CustomPart handrail = new CustomPart();
        handrail.Name = "Final_HandRail";
        handrail.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;
        
        //origin for handrail
        TSM.ContourPoint point1 = _tModel.ShiftVertically(_global.Origin, elevation-50);
        TSM.ContourPoint point2 = _tModel.ShiftHorizontallyRad(point1, gratingOuterRadius, 1,startAngle * (Math.PI / 180));
        //second point for handrail
        point2 = _tModel.ShiftAlongCircumferenceRad(point2, 275, 2);



        for (int i = 0; i < arcLengthList.Count; i++)
        {
        //if only one distance available

            if (i > 0)
            {
                point2 = _tModel.ShiftAlongCircumferenceRad(point2, arcLengthList[i - 1] + 600, 2);
            }
             CreateWeld(point2, arcLengthList[i]);


            handrail.SetInputPositions(point1, point2);
            handrail.SetAttribute("Radius", gratingOuterRadius);
            handrail.SetAttribute("Arc_Length", arcLengthList[i]);
            handrail.SetAttribute("P1", 0);

            handrail.Position.Plane = Position.PlaneEnum.LEFT;
            handrail.Position.PlaneOffset = -300;
            handrail.Position.Rotation = Position.RotationEnum.TOP;
            handrail.Position.Depth = Position.DepthEnum.FRONT;

            handrail.Insert();




        }
        _tModel.Model.CommitChanges();
    }

    void createHandrail()
    {
      //double elevation = 5500;
    
      //double radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList,true);
      gratingOuterRadius = radius+25+10 + distanceFromStack + length;//25 Pipe radius and 10-plate
                                                                       
      double totalArcLength;
      double totalAngle;
      if ((startAngle-theta)<ladderOrientation && (endAngle+theta)>ladderOrientation)
       {
            //theta = 180 / Math.PI * (Math.Atan((ladderWidth + 125) / (gratingOuterRadius * 2)));
            endAngle = ladderOrientation- theta;               
            totalAngle = endAngle - startAngle;
            totalArcLength = Math.Abs(2 * Math.PI * gratingOuterRadius * (totalAngle / 360));
            calculateArcLengthOfCircularHandrail(totalArcLength);
            ManageLastDistance();
            createHandrail1();
            arcLengthList.Clear();
            startAngle = ladderOrientation + theta;
            endAngle = extensionEndAngle;
       }
     
      totalAngle = endAngle - startAngle;
      totalArcLength = Math.Abs(2 * Math.PI * gratingOuterRadius * (totalAngle / 360));
      calculateArcLengthOfCircularHandrail(totalArcLength);
      ManageLastDistance();
      createHandrail1();
      arcLengthList.Clear();

    }
    void calculateArcLengthOfCircularHandrail(double totalArcLength)
    {
      double tempArcLength = 0.0;
      while (totalArcLength > 0)
      {
        tempArcLength = totalArcLength - 2600; //2500+50+50  2500-module length 50-diameter of pipe
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

    public void ManageLastDistance()
    {
        int n = arcLengthList.Count - 1;
        if (n == 0)
        {
            n = 1;
        }
        for (int i = 0; i < n; i++)
        {
            //if only one distance available
            if (arcLengthList.Count == 1)
            {
                if (arcLengthList[i] >= 1200 && arcLengthList[i] <= 2600)
                {
                    arcLengthList.Add(arcLengthList[i] - 600);
                    arcLengthList.RemoveAt(i);
                }

                else if (arcLengthList[i] > 2600)
                {
                    arcLengthList.Add((arcLengthList[i] - 1200) / 2);
                    arcLengthList.Add((arcLengthList[i] - 1200) / 2);
                    arcLengthList.RemoveAt(i);
                }

                //To create only single handrail minimum 1200 distance is required 
                else if (arcLengthList[0] < 1200)
                {
                    break;
                }
            }

            //check the distance of last handrail and according to that either modify second last or keep as it is
            if (i + 1 == arcLengthList.Count - 1)
            {
                if (arcLengthList[i + 1] >= 1200 && arcLengthList[i + 1] <= 2600)
                {
                    arcLengthList.Add(arcLengthList[i + 1] - 600);
                    arcLengthList.RemoveAt(i + 1);
                }


                else if (arcLengthList[i + 1] > 2600)
                {
                    double sum = (arcLengthList[i + 1]) / 2;
                    arcLengthList.RemoveAt(i + 1);
                    arcLengthList.Add(sum - 600);
                    arcLengthList.Add(sum - 600);

                }

                else if (arcLengthList[i + 1] >= 600 && arcLengthList[i + 1] < 1200)
                {
                    double sum = (arcLengthList[i + 1] + arcLengthList[i]) / 2;
                    arcLengthList.RemoveAt(i);
                    arcLengthList.RemoveAt(i);
                    arcLengthList.Add(sum);
                    arcLengthList.Add(sum - 600);
                }

            }
        }

    }

    public void CreateWeld(ContourPoint pt,double arclength)
    {
            CreatePlate(pt);
            if(arclength>600)
            {
                CreatePlate(_tModel.ShiftAlongCircumferenceRad(pt, arclength / 2, 2));
            }          
            CreatePlate(_tModel.ShiftAlongCircumferenceRad(pt, arclength, 2));
    }

    public void CreatePlate(ContourPoint point)
    {
            point = _tModel.ShiftHorizontallyRad(point, 25, 3);
            ContourPoint pt = new ContourPoint(_tModel.ShiftVertically(point, 130), null);
            ContourPoint pt1 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(pt, -85, 2), null);
            ContourPoint pt2 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(pt, 85, 2), null);
            ContourPoint pt3 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(point, -85, 2), null);
            ContourPoint pt4 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(point, 85, 2), null);
            List<ContourPoint> pts = new List<ContourPoint>() { pt1,pt2,pt4,pt3};


            ContourPlate cp=_tModel.CreateContourPlate(pts, "PL10", "IS2062", "3", _global.Position);
            pt1 = _tModel.ShiftVertically(_tModel.ShiftAlongCircumferenceRad(pt1, 30, 2),-65);
            pt2 = _tModel.ShiftAlongCircumferenceRad(pt1, 110, 2);

            BoltArray B = new BoltArray();
            B.PartToBeBolted =cp;
            B.PartToBoltTo = cp;

            B.FirstPosition = pt1;
            B.SecondPosition = pt2;

            B.BoltSize = 20;
            B.Tolerance = 3.00;
            B.BoltStandard = "UNDEFINED_BOLT";
            B.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
            B.CutLength = 100;

            B.Length = 100;
            B.ExtraLength = 15;
            B.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;

            B.Position.Depth = Position.DepthEnum.MIDDLE;
            B.Position.Plane = Position.PlaneEnum.MIDDLE;
            B.Position.Rotation = Position.RotationEnum.TOP;

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

            B.AddBoltDistX(110);


            B.AddBoltDistY(50);


            if (!B.Insert())
                Console.WriteLine("BoltCircle Insert failed!");
            _tModel.Model.CommitChanges();



        }


    }
}
