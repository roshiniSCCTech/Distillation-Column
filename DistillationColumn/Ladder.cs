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
    class Ladder
    {
        Globals _global;
        TeklaModelling _tModel;

        double orientationAngle;
        double elevation;
        double width = 800;
        double rungSpacing;
        double obstructionDist;
        double ladderBase = 0;

        List<List<double>> _ladderList;

        public Ladder(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;

            _ladderList = new List<List<double>>();

            SetLadderData();
            CreateLadder();
        }

        public void SetLadderData()
        {
            List<JToken> ladderList = _global.JData["Ladder"].ToList();
            foreach (JToken ladder in ladderList)
            {
                orientationAngle = (float)ladder["Orientation_Angle"];
                elevation = (float)ladder["Elevation"];
                //width = (float)ladder["Width"];
                //height = (float)ladder["Height"];
                rungSpacing = (float)ladder["Rungs_spacing"];
                obstructionDist = (float)ladder["Obstruction_Distance"];
                _ladderList.Add(new List<double> { orientationAngle, elevation, rungSpacing, obstructionDist});
            }

            List<JToken> ladderBaseList = _global.JData["chair"].ToList();
            foreach (JToken ladder in ladderBaseList)
            {
                ladderBase = (float)ladder["height"];
            }
        }

        public void CreateLadder()
        {
            foreach (List<double> ladder in _ladderList)
            {
                double elevation = ladder[1];
                double orientationAngle = ladder[0] * Math.PI / 180;
                double Height = elevation - ladderBase + (4 * ladder[2]);
                double radius = _tModel.GetRadiusAtElevation(ladderBase, _global.StackSegList, true);
                double count = 0;
                foreach(var seg in _global.StackSegList)
                {
                    if(seg[4] < ladder[1] && (seg[4]+ seg[3]) > elevation - Height)
                    {
                        if (seg[0] != seg[1])
                            count++;
                    }
                }

                TSM.ContourPoint origin = new TSM.ContourPoint(_global.Origin, null);
                TSM.ContourPoint point1 = _tModel.ShiftVertically(origin, ladderBase);
                TSM.ContourPoint point2;
                if (count != 0)
                {
                    point2 = _tModel.ShiftHorizontallyRad(point1, radius + 400 + ladder[3], 1, orientationAngle);
                }
                else
                {
                    point2 =  _tModel.ShiftHorizontallyRad(point1, radius+ 200 + ladder[3], 1, orientationAngle);
                }


                TSM.ContourPoint point11 = _tModel.ShiftVertically(point1, Height);
                double radius1 = _tModel.GetRadiusAtElevation(point11.Z, _global.StackSegList, true);
                TSM.ContourPoint point21 = _tModel.ShiftHorizontallyRad(point11, radius1 + 200 + ladder[3], 1, orientationAngle);


                ladderBase = elevation;

                CustomPart Ladder = new CustomPart();
                Ladder.Name = "Ladder1";
                Ladder.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;

                Ladder.SetInputPositions(point2, point21);
                Ladder.SetAttribute("P1", width);  //Ladder Width
                Ladder.SetAttribute("P2", Height);  // Ladder Height
                Ladder.SetAttribute("P3", ladder[2]);  // Ladder Dist btwn Rungs


                //Ladder.Position.Rotation = Position.RotationEnum.TOP;
                Ladder.Position.Depth = Position.DepthEnum.MIDDLE;
                Ladder.Position.Rotation = Position.RotationEnum.BACK;
                //Ladder.Position.RotationOffset = ladder[0]+ 270 ;
                Ladder.Insert();

                if (Height > 3000)
                {
                    Detail D = new Detail();
                    D.Name = "testDetail";
                    D.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;
                    D.LoadAttributesFromFile("standard");
                    D.UpVector = new Vector(0, 0, 0);
                    D.PositionType = PositionTypeEnum.MIDDLE_PLANE;
                    D.AutoDirectionType = AutoDirectionTypeEnum.AUTODIR_DETAIL;
                    D.DetailType = DetailTypeEnum.END;

                    D.SetPrimaryObject(Ladder);
                    D.SetReferencePoint(point21);
                    D.SetAttribute("P1", 0);           //Ladder top hoop open at both sides
                    D.SetAttribute("P2", 0);           //Ladder top hoop open at right side
                    D.SetAttribute("P3", 1);           //Ladder top hoop open at left side
                    D.SetAttribute("P4", Height);      //Height of ladder
                    D.SetAttribute("P5", 460);         //Width of ladder
                    D.Insert();

                }
                _tModel.Model.CommitChanges();
            }
            
        }
    }
}

