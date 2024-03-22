using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using TopazioRevitPluginShared;

namespace TopazioRevitPlugin.TopazioPanel.Documentação
{
    public partial class ConfigLevelForm : System.Windows.Forms.Form
    {
        ConfigLevelButton button;
        public ExternalEvent aplicarEvent;
        public ExternalEvent resetarEvent;

        public List<System.Windows.Forms.TextBox> textBoxList = new List<System.Windows.Forms.TextBox>() {};

        public ConfigLevelForm(ConfigLevelButton but)
        {
            this.button = but;
            InitializeComponent();

            textBoxList.Add(textBox1);
            textBoxList.Add(textBox2);
            textBoxList.Add(textBox3);
            textBoxList.Add(textBox4);
            textBoxList.Add(textBox5);
            textBoxList.Add(textBox6);
            textBoxList.Add(textBox7);
            textBoxList.Add(textBox8);
            textBoxList.Add(textBox9);
            textBoxList.Add(textBox10);
            textBoxList.Add(textBox11);
            textBoxList.Add(textBox12);
            textBoxList.Add(textBox13);
            textBoxList.Add(textBox14);
            textBoxList.Add(textBox15);
            textBoxList.Add(textBox16);
            textBoxList.Add(textBox17);
            textBoxList.Add(textBox18);
            textBoxList.Add(textBox19);
            textBoxList.Add(textBox20);
        }

        private void ConfigModelForm_Load(object sender, EventArgs e)
        {

        }

        //APLICAR
        private void button21_Click(object sender, EventArgs e)
        {
            aplicarEvent.Raise();
        }

        //RESETAR
        private void button1_Click(object sender, EventArgs e)
        {
            resetarEvent.Raise();
        }

    }
}
