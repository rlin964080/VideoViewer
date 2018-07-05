using System;
using System.Collections.Generic;
using System.Windows.Forms;
using VideoOS.Platform;
using VideoOS.Platform.Client;
using VideoOS.Platform.UI;

namespace SimpleVideoViewer
{
    public partial class MainForm : Form
    {
        private ImageViewerControl imageViewerControl;
        private Item selectedItem;

        public MainForm()
        {
            InitializeComponent();
        }

        private void OnClose(object sender, EventArgs e)
        {
            VideoOS.Platform.SDK.Environment.RemoveAllServers();
            Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(imageViewerControl != null)
            {
                imageViewerControl.Disconnect();
                imageViewerControl.Close();
                imageViewerControl.Dispose();
                imageViewerControl = null;
            }

            ItemPickerForm picker = new ItemPickerForm();
            picker.KindFilter = Kind.Camera;
            picker.Init(Configuration.Instance.GetItems());

            if(picker.ShowDialog() == DialogResult.OK)
            {
                selectedItem = picker.SelectedItem;
                label2.Text = selectedItem.Name;
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

                } catch(Exception ex)  {
                    MessageBox.Show("Exception " + ex);
                }
            }
        }

        
    }
}
