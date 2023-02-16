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
using static Tekla.Structures.ModelInternal.Operation;

namespace DistillationColumn
{
    class StiffnerRings
    {
        public double _startHeight=15000;
        public double _endHeight=60000;
        public int _stiffnerRingCount=30;
        double elevationofDoor;
        double orientationAngleOfDoor;
        double plateDiameterOfDoor;
        double plateRadiusOfDoor;
        double neckPlateThicknessofDoor;

        List<ContourPoint> _pointList;
        List<List<double>> _accessDoorList;
        Globals _global;
        TeklaModelling _tModel;

        public StiffnerRings(Globals global, TeklaModelling tModel) 
        {
            _global = global;
            _tModel = tModel;
            _pointList= new List<ContourPoint>();
            _accessDoorList= new List<List<double>>();

            DataOfCircularAccessDoor();
            CreateStiffnerRings();

        }
        void DataOfCircularAccessDoor()
        {

            List<JToken> circularAccessDoorList = _global.JData["CircularAccessDoor"].ToList();
            foreach (JToken cicularAccessDoor in circularAccessDoorList)
            {
                elevationofDoor = (float)cicularAccessDoor["elevation"];
                orientationAngleOfDoor = (float)cicularAccessDoor["orientation_angle"];
                neckPlateThicknessofDoor = (float)cicularAccessDoor["neck_plate_Thickness"];
                plateDiameterOfDoor = (float)cicularAccessDoor["plate_Diameter"];


                if (elevationofDoor + (plateDiameterOfDoor / 2) + 200 + neckPlateThicknessofDoor > _startHeight && elevationofDoor - (plateDiameterOfDoor / 2) - 200 - neckPlateThicknessofDoor < _endHeight)
                {
                    _accessDoorList.Add(new List<double> { elevationofDoor, orientationAngleOfDoor, neckPlateThicknessofDoor, plateDiameterOfDoor });

                }

            }

        }
        public void CreateStiffnerRings() 
        {
            double spacing=(_endHeight-_startHeight)/_stiffnerRingCount;
            ContourPoint origin = _tModel.ShiftVertically(_global.Origin, _startHeight);
            double elevation = _startHeight;

            for (int i = 0; i < _stiffnerRingCount; i++)
            {
                double radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList);
                radius += _global.StackSegList[_tModel.GetSegmentAtElevation(radius, _global.StackSegList)][2];

                ContourPoint sPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(origin, radius, 1), null);
                ContourPoint mPoint = new ContourPoint(_tModel.ShiftHorizontallyRad(origin, radius, 2), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                ContourPoint ePoint = new ContourPoint(_tModel.ShiftHorizontallyRad(origin, radius, 3), null);
                _pointList.Add(sPoint);
                _pointList.Add(mPoint);
                _pointList.Add(ePoint);

                _global.ClassStr = "9";
                _global.Position.Plane = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
                _global.Position.Rotation = Tekla.Structures.Model.Position.RotationEnum.BELOW;
                _global.Position.Depth = Tekla.Structures.Model.Position.DepthEnum.BEHIND;
                PolyBeam cutPolyBeam1=_tModel.CreatePolyBeam(_pointList,"L100*100*10", Globals.MaterialStr, _global.ClassStr, _global.Position, "stiffnerRing"+i);
                _pointList.Clear();

                _pointList.Add(ePoint);
                _pointList.Add(new ContourPoint(_tModel.ShiftHorizontallyRad(origin, radius, 4), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT)));
                _pointList.Add(sPoint);
                PolyBeam cutPolyBeam=_tModel.CreatePolyBeam(_pointList, "L100*100*10", Globals.MaterialStr, _global.ClassStr, _global.Position, "stiffnerRing" + i);
                _pointList.Clear();

                foreach (List<double> circularAccessDoor in _accessDoorList)
                {
                    elevationofDoor = circularAccessDoor[0];
                    orientationAngleOfDoor = circularAccessDoor[1];
                    orientationAngleOfDoor = orientationAngleOfDoor * Math.PI / 180;
                    neckPlateThicknessofDoor = circularAccessDoor[2];
                    plateDiameterOfDoor = circularAccessDoor[3];
                    plateRadiusOfDoor = plateDiameterOfDoor / 2;

                    if (elevation > (elevationofDoor - (plateRadiusOfDoor + 200 + neckPlateThicknessofDoor)) && elevation < (elevationofDoor + (plateRadiusOfDoor + 200 + neckPlateThicknessofDoor)))
                    {
                        TSM.ContourPoint origin1 = _tModel.ShiftVertically(_global.Origin, elevationofDoor);
                        origin1 = _tModel.ShiftHorizontallyRad(origin1, radius - 500, 1, orientationAngleOfDoor);
                        TSM.ContourPoint point1 = _tModel.ShiftHorizontallyRad(origin1, 1000, 1);
                        _global.Position.Depth = TSM.Position.DepthEnum.MIDDLE;
                        _global.Position.Plane = TSM.Position.PlaneEnum.MIDDLE;
                        _global.ProfileStr = "ROD" + (plateDiameterOfDoor + 400 + 2 * neckPlateThicknessofDoor).ToString();

                        Beam neckPlate1 = _tModel.CreateBeam(origin1, point1, _global.ProfileStr, Globals.MaterialStr, BooleanPart.BooleanOperativeClassName, _global.Position, "");
                        _tModel.cutPart(neckPlate1, cutPolyBeam);

                        Beam neckPlate2 = _tModel.CreateBeam(origin1, point1, _global.ProfileStr, Globals.MaterialStr, BooleanPart.BooleanOperativeClassName, _global.Position, "");
                        _tModel.cutPart(neckPlate2, cutPolyBeam1);
                    }
                    
                }
                elevation = elevation + spacing;
                origin = _tModel.ShiftVertically(_global.Origin, elevation);


            }
        }
    }
}
