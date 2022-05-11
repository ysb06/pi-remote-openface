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
        public WebcamSequence()
        {
            InitializeComponent();

            ReloadCameras();
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            ReloadCameras();
        }

        public async void ReloadCameras()
        {
            WebcamSelector.Items.Clear();
            WebcamSelector.Items.Add("Loading...");
            WebcamSelector.IsEnabled = false;
            ReloadButton.IsEnabled = false;

            await Task.Run(() =>
            {
                Debug.WriteLine("Loading...");
                string root = AppDomain.CurrentDomain.BaseDirectory;
                var cameras = SequenceReader.GetCameras(root);
                Debug.WriteLine("Loading Complete");

                WebcamSelector.Dispatcher.BeginInvoke(() =>
                {
                    WebcamSelector.Items.Clear();
                    foreach (var camera in cameras)
                    {
                        WebcamSelector.Items.Add(new Webcam(camera));
                    }
                    WebcamSelector.IsEnabled = true;
                });
                ReloadButton.Dispatcher.BeginInvoke(() =>
                {
                    ReloadButton.IsEnabled = true;
                });
                Debug.WriteLine("Task Finished");
            });
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
                foreach (var item in cam_e.Item3)
                {
                    Resolutions.Add(item);
                }
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private void WebcamSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("Changed");
        }
    }
}
