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
    internal class StiffnerRings
    {
        public double _startHeight=45000;
        public double _endHeight=50000;
        public int _stiffnerRingCount=10;
        List<ContourPoint> _pointList;
        Globals _global;
        TeklaModelling _tModel;

        public StiffnerRings(Globals global, TeklaModelling tModel) 
        {
            _global = global;
            _tModel = tModel;
            _pointList= new List<ContourPoint>();
            CreateStiffnerRings();
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
                _tModel.CreatePolyBeam(_pointList,"L100*100*10", Globals.MaterialStr, _global.ClassStr, _global.Position, "stiffnerRing"+i);
                _pointList.Clear();

                _pointList.Add(ePoint);
                _pointList.Add(new ContourPoint(_tModel.ShiftHorizontallyRad(origin, radius, 4), new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT)));
                _pointList.Add(sPoint);
                _tModel.CreatePolyBeam(_pointList, "L100*100*10", Globals.MaterialStr, _global.ClassStr, _global.Position, "stiffnerRing" + i);
                _pointList.Clear();

                elevation = elevation + spacing;
                origin= _tModel.ShiftVertically(_global.Origin, elevation);



            }
        }
    }
}
