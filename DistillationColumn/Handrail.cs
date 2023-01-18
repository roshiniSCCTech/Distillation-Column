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
    double platFormStartAngle;
    double platFormEndAngle ;
    double elevation ;
    double platformLength ;
    double distanceFromStack;
    double gratingOuterRadius;
    double gratingThickness;
    List<double> arcLengthList = new List<double>();
    List<List<double>> handRailData;

    public Handrail(Globals global, TeklaModelling tModel)
    {
      _global = global;
      _tModel = tModel;
      handRailData= new List<List<double>>();
      SetHandrailrData();

        foreach (List<double> handrail in handRailData)
        {
            platFormStartAngle = handrail[0];
            platFormEndAngle = handrail[1];
            elevation = handrail[2];
            //  width = (float)handrail["Platform_Width"];
            platformLength = handrail[3];
            distanceFromStack = handrail[4];
            ladderOrientation = handrail[5];
            gratingThickness= handrail[6];
             createHandrail();

        }
    }

    public void SetHandrailrData()
    {           
           
        List<JToken> _handrailList = _global.JData["Ladder"].ToList();
        foreach (JToken handrail in _handrailList)
        {
            platFormStartAngle = (float)handrail["Platform_Start_Angle"];
            platFormEndAngle = (float)handrail["Platfrom_End_Angle"];
            elevation = (float)handrail["Elevation"];
            //  width = (float)handrail["Platform_Width"];
            platformLength = (float)handrail["Platform_Length"];
            distanceFromStack = (float)handrail["Distance_From_Stack"];
            ladderOrientation = (float)handrail["Orientation_Angle"];
            gratingThickness = (float)handrail["Grating_Thickness"];
            handRailData.Add(new List<double>()
            { platFormStartAngle, platFormEndAngle, elevation,  platformLength, distanceFromStack, ladderOrientation,gratingThickness });
        }
            
    }

    void createHandrail1()
    {
        CustomPart handrail = new CustomPart();
        handrail.Name = "Final_HandRail";
        handrail.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;
        //origin for handrail
        TSM.ContourPoint point1 = _tModel.ShiftVertically(_global.Origin, elevation);


        TSM.ContourPoint point2 = _tModel.ShiftHorizontallyRad(point1, gratingOuterRadius, 1, platFormStartAngle * (Math.PI / 180));

        //second point for handrail
        point2 = _tModel.ShiftAlongCircumferenceRad(point2, 275, 2);



        for (int i = 0; i < arcLengthList.Count; i++)
        {
        //if only one distance available
        if (arcLengthList.Count == 1)
        {
            if (arcLengthList[i] > 1100 && arcLengthList[i] <= 2600)
            {
            arcLengthList.Add(arcLengthList[i] - 600);
            arcLengthList.RemoveAt(i);
            }

            if (arcLengthList[i] > 2500)
            {
            arcLengthList.Add((arcLengthList[i] - 1100) / 2);
            arcLengthList.Add((arcLengthList[i] - 1100) / 2);
            arcLengthList.RemoveAt(i + 1);
            }

            //To create only single handrail minimum 1100 distance is required 
            if (arcLengthList[i] < 1100)
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


            if (arcLengthList[i + 1] > 2500)
            {
            double sum = (arcLengthList[i + 1] + arcLengthList[i]) / 2;
            arcLengthList.RemoveAt(i + 1);
            arcLengthList.Add(sum);
            arcLengthList.Add(sum);

            }

            if (arcLengthList[i + 1] > 600 && arcLengthList[i + 1] < 1200)
            {
            double sum = (arcLengthList[i + 1] + arcLengthList[i]) / 2;
            arcLengthList.RemoveAt(i);
            arcLengthList.RemoveAt(i);
            arcLengthList.Add(sum);
            arcLengthList.Add(sum - 600);
            }

        }




        if (i > 0)
        {
            point2 = _tModel.ShiftAlongCircumferenceRad(point2, arcLengthList[i - 1] + 600, 2);
        }


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
      double elevation = 5500;
      double orientationAngle = 0 * Math.PI / 180;
      double radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList);
      gratingOuterRadius = radius + distanceFromStack + platformLength;
      //createGrating();
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
   
  }
}
