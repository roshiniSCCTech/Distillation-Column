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
                _ladderList.Add(new List<double> { orientationAngle, elevation, rungSpacing });
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

                TSM.ContourPoint origin = new TSM.ContourPoint(_global.Origin, null);
                TSM.ContourPoint point1 = _tModel.ShiftVertically(origin, ladderBase);
                TSM.ContourPoint point2 = _tModel.ShiftHorizontallyRad(point1, radius + 10, 1, orientationAngle);
                TSM.ContourPoint point3 = _tModel.ShiftVertically(point2, Height);

                ladderBase = elevation;

                CustomPart Ladder = new CustomPart();
                Ladder.Name = "Ladder1";
                Ladder.Number = BaseComponent.CUSTOM_OBJECT_NUMBER;

                Ladder.SetInputPositions(point2, point3);
                Ladder.SetAttribute("P1", width);  //Ladder Width
                Ladder.SetAttribute("P2", Height);  // Ladder Height
                Ladder.SetAttribute("P3", ladder[2]);  // Ladder Dist btwn Rungs


                Ladder.Position.Rotation = Position.RotationEnum.TOP;
                Ladder.Position.Depth = Position.DepthEnum.MIDDLE;
                //Ladder.Position.Rotation = Position.RotationEnum.FRONT;
                Ladder.Position.RotationOffset = ladder[0] + 270;
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
                    D.SetReferencePoint(point3);
                    D.SetAttribute("P1", Height);
                    //D.SetAttribute("P3", );
                    D.Insert();

                }
                _tModel.Model.CommitChanges();
            }
        }
    }
}

