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
    /// Webcam List Loader
    /// </summary>
    public partial class WebcamSequence : UserControl
    {
        public event WebcamSelectedEventHandler WebcamSelected;

        internal Webcam? CurrentSelectedWebcam
        {
            get
            {
                try
                {
                    return (Webcam)WebcamSelector.SelectedItem;
                }
                catch (InvalidCastException)
                {
                    return null;
                }
            }
        }
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
            WebcamSelector.SelectedIndex = 0;
            WebcamSelector.IsEnabled = false;
            ReloadButton.IsEnabled = false;

            await Task.Run(() =>
            {
                Debug.WriteLine("Loading Camera List...");
                string root = AppDomain.CurrentDomain.BaseDirectory;
                var cameras = SequenceReader.GetCameras(root);
                Debug.WriteLine("Loading Complete");

                Dispatcher.BeginInvoke(() =>
                {
                    WebcamSelector.Items.Clear();
                    foreach (var camera in cameras)
                    {
                        WebcamSelector.Items.Add(new Webcam(camera));
                    }
                    WebcamSelector.IsEnabled = true;
                    ReloadButton.IsEnabled = true;
                });
            });
        }

        private void WebcamSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentSelectedWebcam != null)
            {
                WebcamSelected?.Invoke(this, CurrentSelectedWebcam);
            }
        }
    }
}
