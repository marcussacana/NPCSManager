using NPCSManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NPCSGui {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void openScriptToolStripMenuItem_Click(object sender, EventArgs e) {
            openFileDialog1.ShowDialog();
        }

        NPCSManager.NPCSManager Editor;
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e) {
            byte[] Script = System.IO.File.ReadAllBytes(openFileDialog1.FileName);
            Editor = new NPCSManager.NPCSManager(Script);
            listBox1.Items.Clear();
            string[] strs = Editor.Import();
            foreach (string str in strs)
                listBox1.Items.Add(str);
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e) {
            string[] Arr = new string[listBox1.Items.Count];
            listBox1.Items.CopyTo(Arr, 0);
            System.IO.File.WriteAllBytes(saveFileDialog1.FileName, Editor.Export(Arr));
        }

        private void saveScriptToolStripMenuItem_Click(object sender, EventArgs e) {
            saveFileDialog1.ShowDialog();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                int i = listBox1.SelectedIndex;
                Text = string.Format("ID: {0}/{1}", listBox1.Items.Count, listBox1.Items.Count);
                textBox1.Text = listBox1.Items[i].ToString(); 
            }
            catch { }
        }
    }
}
