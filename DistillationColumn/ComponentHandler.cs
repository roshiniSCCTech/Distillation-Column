using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSM = Tekla.Structures.Model;
using T3D = Tekla.Structures.Geometry3d;
using HelperLibrary;

namespace DistillationColumn
{
    class ComponentHandler
    {
        Globals _global;
        TeklaModelling _tModel;
        public ComponentHandler(Globals global, TeklaModelling teklaModel)
        {
            _global = global;
            _tModel = teklaModel;

            new ImportCustomComponent(_global, _tModel);

            new Stack(_global, _tModel);

            //new Chair(_global, _tModel);
            //new AccessDoor(_global, _tModel);
            //new Flange(_global, _tModel);
            //new StiffnerRings(_global, _tModel);
            //new CircularGrating(_global, _tModel);
            //new Platform(_global, _tModel);
            //new Handrail(_global, _tModel);

            //new CircularAccessDoor(_global, _tModel);
            new RectangularPlatform(_global, _tModel);

            new CapAndOutlets(_global, _tModel);
            new Ladder(_global, _tModel);
            //new InstrumentNozzle(_global, _tModel);

        }
    }
}
