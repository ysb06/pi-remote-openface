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
using System.Windows.Shapes;
using System.Diagnostics;
using System.Net.Sockets;

namespace OpenFaceOffline.Components
{
    /// <summary>
    /// ScanerLinker.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ScanerLinker : Window
    {
        private static ScanerLinker? CurrentWindow = null;

        private MainWindow parent;
        private UdpClient? RtGateway;

        public ScanerLinker(MainWindow parent)
        {
            if (CurrentWindow != null)
            {
                CurrentWindow.Close();
            }
            InitializeComponent();

            this.parent = parent;
            parent.OnPoseChanged += OnPoseChanged;

            CurrentWindow = this;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (CurrentWindow == this)
            {
                CurrentWindow = null;
            }
            parent.OnPoseChanged -= OnPoseChanged;
        }

        public void OnPoseChanged(MainWindow sender, FacePose pose)
        {
            Dispatcher.BeginInvoke(() =>
            {
                FaceDetectionStateLabel.Content = sender.IsFaceAnalysisActive ? "Running" : "Stopped";
            });
        }

        private void RTGatewayConnectButton_Click(object sender, RoutedEventArgs e)
        {
            RtGateway = new UdpClient();
            // UDP 연결 코드 작성
        }
    }
}
