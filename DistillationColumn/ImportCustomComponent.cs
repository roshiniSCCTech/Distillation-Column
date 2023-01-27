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
            CatalogHandler c=new CatalogHandler();
            bool d=c.ImportCustomComponentItems(@"D:\\custom_components\\");
           // if (new CatalogHandler().ImportCustomComponentItems("D:\\custom_components"))
                //Console.WriteLine("Custom components imported successfully to catalog.");
        }
    }

   
}
