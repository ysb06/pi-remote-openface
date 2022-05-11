using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using OpenCVWrappers;
using UtilitiesOF;


namespace FaceLinker.Components
{
    /// <summary>
    /// WebcamSequence.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WebcamSequence : UserControl
    {
        private List<Webcam> webcams = new List<Webcam>();
        public WebcamSequence()
        {
            InitializeComponent();

            ReloadCameras();
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            ReloadCameras();
        }

        public void ReloadCameras()
        {
            webcams.Clear();

            string root = AppDomain.CurrentDomain.BaseDirectory;
            var cameras = SequenceReader.GetCameras(root);
            
            foreach (var camera in cameras)
            {
                webcams.Add(new Webcam(camera));
            }
        }

        public class Webcam
        {
            public int Id = 0;
            public string Name = string.Empty;
            public List<Tuple<int, int>> Resolutions = new List<Tuple<int, int>>();

            public Webcam(Tuple<int, string, List<Tuple<int, int>>, RawImage> cam_e)
            {
                Id = cam_e.Item1;
                Name = cam_e.Item2;
                foreach(var item in cam_e.Item3) {
                    Resolutions.Add(item);
                }
            }
        }
    }
}
