using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Model;
using System.Threading.Tasks;
using TSM = Tekla.Structures.Model;
using T3D = Tekla.Structures.Geometry3d;
using Newtonsoft.Json.Linq;
using HelperLibrary;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model.Operations;

namespace DistillationColumn
{
    class InstrumentNozzle
    {
        Globals _global;
        TeklaModelling _tModel;

        double orientationAngle;
        double elevation;
        double pipeDiameter;
        double plateRadius;
        double pipeLength;
        double pipeThickness;
        double plateThickness;
        double numberOfBolts;
        TSM.ContourPoint pipeStartPoint;
        TSM.ContourPoint pipeEndPoint;
        double radius;
        ContourPlate coverPlate;
        PolyBeam gasketPlate;


        List<TSM.ContourPoint> _pointsList;

        List<List<double>> _instrumentNozzleList;


        public InstrumentNozzle(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;

            //orientationAngle=40 * Math.PI / 180;
            //elevation=60000;
            pipeDiameter = 50;
            plateRadius = 50;
            pipeLength = 100;
            pipeThickness = 5;
            plateThickness = 10;          
            numberOfBolts = 6;
            pipeStartPoint = new TSM.ContourPoint();
            pipeEndPoint = new TSM.ContourPoint();
            _pointsList= new List<TSM.ContourPoint>();
            _instrumentNozzleList= new List<List<double>>();

            Build();

        }
        void Build()
        {
            List<JToken> instrumentNozzleList = _global.JData["instrumental_nozzle"].ToList();
            foreach (JToken instrumentnozzle in instrumentNozzleList)
            {
                double elevation = (float)instrumentnozzle["elevation"];
                double orientationAngle = (float)instrumentnozzle["orientation_angle"];

                _instrumentNozzleList.Add(new List<double> { elevation, orientationAngle });

            }

            foreach (List<double> instrumentNozzle in _instrumentNozzleList)
            {
                elevation = instrumentNozzle[0];
                orientationAngle = instrumentNozzle[1];
                CreateNozzle();
            }
        }

        void CreateNozzle()
        {
            TSM.ContourPoint origin = new TSM.ContourPoint(_global.Origin, null);
            origin = _tModel.ShiftVertically(origin, elevation);
            radius = _tModel.GetRadiusAtElevation(elevation, _global.StackSegList);
            pipeStartPoint = _tModel.ShiftHorizontallyRad(origin, radius-100, 1, orientationAngle);
            pipeEndPoint = _tModel.ShiftHorizontallyRad(pipeStartPoint,pipeLength+100, 1);

            CreatePipe();
            CreateCoverPlate();
            CreateGasketPlate();
            CreateBolts();
        }
        void CreatePipe()
        {
            _global.Position.Depth = TSM.Position.DepthEnum.MIDDLE;
            _global.Position.Plane = TSM.Position.PlaneEnum.MIDDLE;
            _global.Position.Rotation = TSM.Position.RotationEnum.BELOW;
            Beam pipe=_tModel.CreateBeam(pipeStartPoint, pipeEndPoint,"PIPE"+pipeDiameter+"*"+pipeThickness, "IS2062", "6", _global.Position, "");
            Beam beam=_global.SegmentPartList[_tModel.GetSegmentAtElevation(elevation,_global.StackSegList)];
            Beam cutBeam =Operation.CopyObject(beam, new Vector(0,0,0)) as Beam;
            cutBeam.Class = BooleanPart.BooleanOperativeClassName;
            cutBeam.Modify();
            _tModel.cutPart(cutBeam,pipe);


            Beam cutPipe = _tModel.CreateBeam(pipeStartPoint, pipeEndPoint, "PIPE" + pipeDiameter + "*" + pipeThickness, "IS2062", BooleanPart.BooleanOperativeClassName, _global.Position, "");
            _tModel.cutPart(cutPipe, _global.SegmentPartList[_tModel.GetSegmentAtElevation(elevation, _global.StackSegList)]);


        }
        void CreateCoverPlate()
        {
            TSM.ContourPoint point2= _tModel.ShiftHorizontallyRad(pipeEndPoint, plateRadius, 2);
            TSM.ContourPoint point1 = new TSM.ContourPoint(_tModel.ShiftVertically(pipeEndPoint,plateRadius),new TSM.Chamfer(0,0,TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            TSM.ContourPoint point3 = new TSM.ContourPoint(_tModel.ShiftVertically(pipeEndPoint, -plateRadius), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            TSM.ContourPoint point4 = _tModel.ShiftHorizontallyRad(pipeEndPoint, plateRadius, 4);

            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;

            _pointsList.Add(point1);
            _pointsList.Add(point2);
            _pointsList.Add(point3);
            _pointsList.Add(point4);

            coverPlate=_tModel.CreateContourPlate(_pointsList, "PLT" + plateThickness, Globals.MaterialStr, "5", _global.Position, "");
            _pointsList.Clear();

        }
        void CreateGasketPlate()
        {
            TSM.ContourPoint point2 = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(pipeEndPoint, plateRadius, 2), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
            TSM.ContourPoint point1 = _tModel.ShiftVertically(pipeEndPoint, plateRadius);
            TSM.ContourPoint point3 = _tModel.ShiftVertically(pipeEndPoint, -plateRadius);
            TSM.ContourPoint point4 = new TSM.ContourPoint(_tModel.ShiftHorizontallyRad(pipeEndPoint, plateRadius, 4), new TSM.Chamfer(0, 0, TSM.Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));


            _global.Position.Depth = TSM.Position.DepthEnum.BEHIND;
            _global.Position.Plane = TSM.Position.PlaneEnum.LEFT;
            _global.Position.Rotation = TSM.Position.RotationEnum.BELOW;


            _pointsList.Add(point1);
            _pointsList.Add(point2);
            _pointsList.Add(point3);
            _pointsList.Add(point4);
            _pointsList.Add(point1);

            gasketPlate = _tModel.CreatePolyBeam(_pointsList, "PL" +(plateRadius-(pipeDiameter/2))+"*" + plateThickness, Globals.MaterialStr, "5", _global.Position, "");
            _pointsList.Clear();
        }
        void CreateBolts()
        {
            BoltCircle B = new BoltCircle();

            B.PartToBeBolted = coverPlate;
            B.PartToBoltTo = gasketPlate;

            B.FirstPosition = pipeEndPoint ;
            B.SecondPosition = _tModel.ShiftHorizontallyRad(pipeEndPoint, plateRadius, 4); ;

            B.BoltSize = 10;
            B.Tolerance = 3.00;
            B.BoltStandard = "8.8XOX";
            B.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
            B.CutLength = 105;

            B.Length = 100;
            B.ExtraLength = 15;
            B.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;

           
            B.Position.Rotation = Position.RotationEnum.BELOW;

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

            B.NumberOfBolts =numberOfBolts;
            B.Diameter = plateRadius*1.5;
            B.Insert();
            _tModel.Model.CommitChanges();
        }
    }
}
