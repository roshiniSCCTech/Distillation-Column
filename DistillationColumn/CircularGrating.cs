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


    double radius;
    double plateAngle;
    double xcod, ycod, zcod;
    double orientationAngle;
    double theta;
    double ladderWidth = 800;


    ContourPoint p1;
    ContourPoint p2;
    ContourPoint p3;
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
        radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList, true);
        radius = radius + distanceFromStack;

        if (orientationAngle == startAngle)
        {
          theta = ((ladderWidth/2) / (radius+platLength)) * 180 / Math.PI;

          startAngle = startAngle + theta;

        }
        if (orientationAngle == endAngle)
        {
          theta = ((ladderWidth / 2) / (radius + platLength)) * 180 / Math.PI;
          endAngle = endAngle - theta;
        }
        if (orientationAngle > startAngle && orientationAngle < endAngle)
        {

          ContourPoint xaxis = new ContourPoint(new Point(radius, 0, elevation), null);
          o1 = _tModel.ShiftHorizontallyRad(_tModel.ShiftAlongCircumferenceRad(xaxis, (Math.PI / 180) * orientationAngle, 1), platLength, 1);
          o2 = _tModel.ShiftHorizontallyRad(o1, radius + platLength, 3);
          //Beam cut = _tModel.CreateBeam(o1,o2, "PL"+ladderWidth+"*50", "IS2062", "5", _global.Position, "");

        }

        plateAngle =  ((width) / (radius + platLength)) * 180 / Math.PI;


        int count = Convert.ToInt32((endAngle - startAngle) / plateAngle);

        CreateCircularGrating(count);
        CreateFrame();

      }


    }

    public void CreateCircularGrating(int count)
    {
      //foreach (List<double> grating in gratinglist)
      {


        for (int i = 0; i <= count; i++)
        {
          radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList, true);
          radius = radius + distanceFromStack;
          PolyBeam poly = new PolyBeam();
          poly.Profile.ProfileString = "PL20*" + platLength;
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


            poly.AddContourPoint(p1);
            poly.AddContourPoint(p2);
            poly.AddContourPoint(p3);
            poly.Insert();

            _tModel.Model.CommitChanges();
            if (orientationAngle + theta == startAngle && i == 0)
            {
              //Beam cut = _tModel.CreateBeam(_tModel.ShiftAlongCircumferenceRad(p1,(Math.PI/180)*(-theta),1), _tModel.ShiftHorizontallyRad(p1,radius+platLength,3), "PL" + ladderWidth + "*50", "IS2062", "5", _global.Position, "");
              Beam cut1 = new Beam();
              cut1.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.MIDDLE;
              cut1.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.MIDDLE;
              cut1.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.BACK;

              cut1 = _tModel.CreateBeam(_tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 180) * (-theta), 1), _tModel.ShiftHorizontallyRad(p1, radius + platLength, 3), "PL" + ladderWidth + "*250", "IS2062", BooleanPart.BooleanOperativeClassName, cut1.Position, "");

              _tModel.cutPart(cut1, poly);
            }
            if (orientationAngle > startAngle && orientationAngle < endAngle && orientationAngle > startAngle + ((i - 1) * plateAngle) && orientationAngle < startAngle + ((i + 2) * plateAngle))
            {
              Beam cut1 = new Beam();
              cut1.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.MIDDLE;
              cut1.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.MIDDLE;
              cut1.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.BACK;

              cut1 = _tModel.CreateBeam(o1, o2, "PL" + ladderWidth + "*250", "IS2062", BooleanPart.BooleanOperativeClassName, _global.Position, "");

              _tModel.cutPart(cut1, poly);
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

            poly.AddContourPoint(p1);
            poly.AddContourPoint(p2);
            poly.AddContourPoint(p3);


            poly.Insert();
            _tModel.Model.CommitChanges();
            if (orientationAngle - theta == endAngle)
            {
              //Beam cut = _tModel.CreateBeam(_tModel.ShiftAlongCircumferenceRad(p3, (Math.PI / 180) * (theta), 1), _tModel.ShiftHorizontallyRad(p3, radius + platLength, 3), "PL" + ladderWidth + "*50", "IS2062", "5", _global.Position, "");
              Beam cut1 = new Beam();
              cut1.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.MIDDLE;
              cut1.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.MIDDLE;
              cut1.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.BACK;

              cut1 = _tModel.CreateBeam(_tModel.ShiftAlongCircumferenceRad(p3, (Math.PI / 180) * (theta), 1), _tModel.ShiftHorizontallyRad(p3, radius + platLength, 3), "PL" + ladderWidth + "*250", "IS2062", BooleanPart.BooleanOperativeClassName, _global.Position, "");

              _tModel.cutPart(cut1, poly);
            }
            break;
          }
          //p1 = new ContourPoint(new Point(xcod, ycod, zcod), null);
          //p2 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 180) * (plateAngle / 2), 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
          //p3 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 180) * (plateAngle), 1), null);



          //if (orientationAngle < startAngle && i == 0)
          //{
          //    theta = startAngle - orientationAngle;
          //    ContourPoint p0 = _tModel.ShiftHorizontallyRad(p1, 100, 4, theta);
          //    poly.AddContourPoint(p0);
          //}

          //poly.AddContourPoint(p1);
          //poly.AddContourPoint(p2);
          //poly.AddContourPoint(p3);

          //if (orientationAngle > endAngle && i == count)
          //{
          //    theta = orientationAngle-endAngle;
          //    ContourPoint p0 = _tModel.ShiftHorizontallyRad(p1, 100, 3, theta);
          //    poly.AddContourPoint(p0);
          //}

          //poly.Insert();
          //_tModel.Model.CommitChanges();
        }

      }

    }

    public void CreateFrame()
    {
      p1 = new ContourPoint(new Point(radius * Math.Cos(Math.PI * startAngle / 180), radius * Math.Sin(Math.PI * startAngle / 180), elevation), null);
      p2 = new ContourPoint(_tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 360) * (endAngle - startAngle), 1), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
      p3 = _tModel.ShiftAlongCircumferenceRad(p1, (Math.PI / 180) * (endAngle - startAngle), 1);
      PolyBeam innerBeam = new PolyBeam();
      innerBeam.Profile.ProfileString = "C100*100*10";
      innerBeam.AddContourPoint(p1);
      innerBeam.AddContourPoint(p2);
      innerBeam.AddContourPoint(p3);
      innerBeam.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
      innerBeam.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.MIDDLE;
      innerBeam.Insert();
      _tModel.Model.CommitChanges();

      Beam startBeam = new Beam(_tModel.ShiftHorizontallyRad(p1, platLength, 1), p1);
      startBeam.Profile.ProfileString = "C100*100*10";
      startBeam.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.MIDDLE;
      startBeam.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
      startBeam.Insert();

      Beam endBeam = new Beam(p3, _tModel.ShiftHorizontallyRad(p3, platLength, 1));
      endBeam.Profile.ProfileString = "C100*100*10";
      endBeam.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.MIDDLE;
      endBeam.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
      endBeam.Insert();


      PolyBeam outerBeam = new PolyBeam();
      outerBeam.AddContourPoint(_tModel.ShiftHorizontallyRad(p3, platLength, 1));
      outerBeam.AddContourPoint(new ContourPoint(new Point(_tModel.ShiftHorizontallyRad(p2, platLength, 1)), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT)));
      outerBeam.AddContourPoint(_tModel.ShiftHorizontallyRad(p1, platLength, 1));
      outerBeam.Profile.ProfileString = "C100*100*10";
      outerBeam.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.MIDDLE;
      outerBeam.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
      outerBeam.Insert();
      _tModel.Model.CommitChanges();

      if (orientationAngle > startAngle && orientationAngle < endAngle)
      {
        double theta2 = (180 / Math.PI) * (Math.Atan(ladderWidth / (2 * (radius + platLength))));

        theta = (180 / Math.PI) * (Math.Atan(ladderWidth / (2 * radius)));

        p1 = new ContourPoint(new Point(radius * Math.Cos((Math.PI / 180) * (orientationAngle - theta)), radius * Math.Sin((Math.PI / 180) * (orientationAngle - theta)), elevation), null);

        Beam midBeam1 = new Beam(p1, _tModel.ShiftAlongCircumferenceRad(o1, -(Math.PI * theta2 / 180), 1));
        //Beam midBeam1 = new Beam(p1, _tModel.ShiftHorizontallyRad(p1, platLength, 1));
        midBeam1.Profile.ProfileString = "C100*100*10";
        midBeam1.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
        midBeam1.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.MIDDLE;
        midBeam1.Insert();

        Beam midBeam2 = new Beam(_tModel.ShiftAlongCircumferenceRad(o1, (Math.PI * theta2 / 180), 1), _tModel.ShiftAlongCircumferenceRad(p1, (Math.PI * theta / 90), 1));
        midBeam2.Profile.ProfileString = "C100*100*10";
        midBeam2.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
        midBeam2.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.MIDDLE;
        midBeam2.Insert();


        Beam cut2 = _tModel.CreateBeam(o1, o2, "PL" + ladderWidth + "*100", "IS2062", BooleanPart.BooleanOperativeClassName, _global.Position, "");
        _tModel.cutPart(cut2, outerBeam);

        Beam cut1 = _tModel.CreateBeam(o1, o2, "PL" + ladderWidth + "*100", "IS2062", BooleanPart.BooleanOperativeClassName, _global.Position, "");
        _tModel.cutPart(cut1, innerBeam);

        _tModel.Model.CommitChanges();
      }
    }
  }
}