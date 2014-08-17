using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DXApplication1
{
    

    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        private List<ValidatableFileInfo> _inputFiles;
        private string _outputDirectory;

        public Form1()
        {
            InitializeComponent();

            _inputFiles = new List<ValidatableFileInfo>();

            this.Load+=Form1_Load;
            this.gridControl1.ProcessGridKey += gridControl1_ProcessGridKey;
        }

        void gridControl1_ProcessGridKey(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                this.gridView1.SelectAll();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.gridView1.Columns.AddField("Name").Visible = true;
        }

        private void cb_open_ItemClicked(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            this.gridControl1.DataSource = _inputFiles;

            var path = Directory.GetCurrentDirectory();

            this.openFileDialog1.InitialDirectory = path;
            this.openFileDialog1.Filter = "KML files (*.kml)|*.kml|XML files (*.xml)|*.xml";
            this.openFileDialog1.Multiselect = true;
            var dialogResult = this.openFileDialog1.ShowDialog();
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                _inputFiles.AddRange((from fileName in this.openFileDialog1.FileNames select new ValidatableFileInfo(new FileInfo(fileName))).Where(file => !_inputFiles.Exists(f => f.FileInfo.FullName == file.FileInfo.FullName)));
                this.gridView1.RefreshData();
            }

        }

        private void cb_parse_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _outputDirectory = _inputFiles.Any()
                ? _inputFiles.First().FileInfo.DirectoryName
                : Directory.GetCurrentDirectory();

            this.folderBrowserDialog1.Description = "Choose directory to output processed files.";

            this.folderBrowserDialog1.SelectedPath = _outputDirectory;
            this.folderBrowserDialog1.ShowDialog();

            this.gridView1.ShowLoadingPanel();

            int[] selectedRows = this.gridView1.GetSelectedRows();
            foreach (int selectedRow in selectedRows)
            {
                var file = gridView1.GetRow(selectedRow) as ValidatableFileInfo;
                if (file == null)
                {
                    continue;
                }
                
                try
                {
                    file.ClearErrors();

                    var result = new StringBuilder();

                    var document = XDocument.Load(file.FileInfo.FullName);
                    if (document.Root == null) continue;

                    var elementToParse = document.Root.Elements("MultiGeometry").ToList();
                    if (!elementToParse.Any())
                    {
                        file.SetColumnError("Name",
                            "This file does not contain any xml elements matching 'MultiGeometry'.");

                        continue;
                    }

                    var element = elementToParse.First();
                    var nodes = element.Elements().ToList();

                    foreach (var node in nodes)
                    {
                        result.AppendLine(node.ToString());
                    }

                    string outputFileName = file.FileInfo.Name.Replace(file.FileInfo.Extension, ".txt");

                    File.WriteAllLines(outputFileName, new List<string> { result.ToString() });
                }
                catch (Exception ex)
                {
                    file.SetColumnError("Name", "Unable to parse this file.\nError: " + ex.Message );
                }

            }

            this.gridView1.RefreshData();
            this.gridView1.HideLoadingPanel();

        }
    }
}
