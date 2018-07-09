using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using VideoOS.Platform;
using VideoOS.Platform.Client;
using VideoOS.Platform.UI;
using AVIExporter = VideoOS.Platform.Data.AVIExporter;

namespace SimpleVideoViewer
{
    public partial class MainForm : Form
    {
        private ImageViewerControl imageViewerControl;
        private Item selectedItem;

        private Item _item = null;
        private string _path = null;
        private Timer _timer = new Timer() { Interval = 100 };
        VideoOS.Platform.Data.IExporter _exporter;


        public MainForm()
        {
            InitializeComponent();
        }

        private void OnClose(object sender, EventArgs e)
        {
            if (_exporter != null)
            {
                _exporter.Cancel();
                _exporter.EndExport();
                _exporter.Close();
            }

            VideoOS.Platform.SDK.Environment.RemoveAllServers();
            Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (imageViewerControl != null)
            {
                imageViewerControl.Disconnect();
                imageViewerControl.Close();
                imageViewerControl.Dispose();
                imageViewerControl = null;
            }

            ItemPickerForm picker = new ItemPickerForm();
            picker.KindFilter = Kind.Camera;
            picker.Init(Configuration.Instance.GetItems());

            if (picker.ShowDialog() == DialogResult.OK)
            {
                selectedItem = picker.SelectedItem;
                _item = picker.SelectedItem;
                label2.Text = selectedItem.Name;

                if (_item != null && _path != null)
                {
                    buttonExport.Enabled = true;
                }
                    
                try
                {
                    imageViewerControl = ClientControl.Instance.GenerateImageViewerControl();
                    imageViewerControl.Dock = DockStyle.Fill;
                    panel1.Controls.Clear();
                    panel1.Controls.Add(imageViewerControl);
                    imageViewerControl.CameraFQID = selectedItem.FQID;
                    imageViewerControl.EnableDigitalZoom = true;

                    imageViewerControl.EnableVisibleHeader = false;

                    imageViewerControl.Initialize();
                    imageViewerControl.Connect();

                    //add overlay text on top of video
                    var fontFamily = new System.Windows.Media.FontFamily("Open Sans Semibold");
                    var typeface = new System.Windows.Media.Typeface(fontFamily, System.Windows.FontStyles.Normal, System.Windows.FontWeights.Bold, new System.Windows.FontStretch());
                    var fText = new System.Windows.Media.FormattedText(selectedItem.Name, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, typeface, 25, System.Windows.Media.Brushes.White);
                    var path = new System.Windows.Shapes.Path() { Data = fText.BuildGeometry(new System.Windows.Point(20, 160)), Fill = System.Windows.Media.Brushes.White };
                    var id = imageViewerControl.ShapesOverlayAdd(new List<System.Windows.Shapes.Shape> { path }, new ShapesOverlayRenderParameters() { ZOrder = 100 });

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception " + ex);
                }
            }
        }

        private void exportIntervalStart_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void Export_Button_Click(object sender, EventArgs e)
        {
            try
            {
                List<Item> audioSources = new List<Item>();
                String destPath = _path;

                if (dateTimePickerStart.Value > dateTimePickerEnd.Value)
                {
                    MessageBox.Show("Start time need to be lower than end time");
                    return;
                }

                if (textBoxAVIfilename.Text == "")
                {
                    MessageBox.Show("Please enter a filename for the AVI file.", "Enter Filename");
                    return;
                }
                _exporter = new VideoOS.Platform.Data.AVIExporter()
                {
                    Filename = textBoxAVIfilename.Text,
                };

                destPath = Path.Combine(_path, "Exported Images\\" + _item.Name);

                _exporter.Init();
                _exporter.Path = destPath;
                _exporter.CameraList = new List<Item>() { _item };
                _exporter.AudioList = audioSources;

                bool startresult = _exporter.StartExport(dateTimePickerStart.Value.ToUniversalTime(), dateTimePickerEnd.Value.ToUniversalTime());

                if (startresult)
                {
                    _timer.Tick += ShowProgress;
                    _timer.Start();

                    buttonExport.Enabled = false;
                    buttonCancel.Enabled = true;
                }
                else
                {
                    int lastError = _exporter.LastError;
                    string lastErrorString = _exporter.LastErrorString;
                    //labelError.Text = lastErrorString + "  ( " + lastError + " )";
                    _exporter.EndExport();
                }
            }
            catch (Exception ex)
            {
                EnvironmentManager.Instance.ExceptionDialog("Start Export", ex);
            }
        }

        private void buttonDestination_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _path = dialog.SelectedPath;
                buttonDestination.Text = _path;

                if (_item != null && _path != null)
                    buttonExport.Enabled = true;
            }
        }


        private void ShowProgress(object sender, EventArgs e)
        {
            if (_exporter != null)
            {
                int progress = _exporter.Progress;
                int lastError = _exporter.LastError;
                string lastErrorString = _exporter.LastErrorString;
                if (progress >= 0)
                {
                    progressBar.Value = progress;
                    if (progress == 100)
                    {
                        _timer.Stop();
                        //labelError.Text = "Done";
                        _exporter.EndExport();
                        _exporter = null;
                        buttonCancel.Enabled = false;
                    }
                }
                if (lastError > 0)
                {
                    progressBar.Value = 0;
                    //labelError.Text = lastErrorString + "  ( " + lastError + " )";
                    if (_exporter != null)
                    {
                        _exporter.EndExport();
                        _exporter = null;
                        buttonCancel.Enabled = false;
                    }
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (_exporter != null)
                _exporter.Cancel();
        }

        private void dateTimePickerEnd_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
