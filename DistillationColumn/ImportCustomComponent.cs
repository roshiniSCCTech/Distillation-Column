using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;
using Tekla.Structures.Catalogs;

namespace DistillationColumn
{

    internal class ImportCustomComponent
    {
        Globals _global;
        TeklaModelling _tModel;
        public ImportCustomComponent(Globals global, TeklaModelling tModel)
        {
            _global = global;
            _tModel = tModel;
            ImportComponent();

        }

        void ImportComponent()
        {
            CatalogHandler component=new CatalogHandler();
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();          
            component.ImportCustomComponentItems(currentDirectory+"\\CustomComponents");
          
        }
    }

   
}
