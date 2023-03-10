using HelperLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DistillationColumn
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btn_createModel_Click(object sender, EventArgs e)
        {
            Globals global = new Globals();

            TeklaModelling teklaModel = new TeklaModelling(global.Origin.X, global.Origin.Y, global.Origin.Z);

            new ComponentHandler(global, teklaModel);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
