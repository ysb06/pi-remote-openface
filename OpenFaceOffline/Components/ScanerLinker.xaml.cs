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
using System.Net;

namespace OpenFaceOffline.Components
{
    /// <summary>
    /// ScanerLinker.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ScanerLinker : Window
    {
        private struct UdpState
        {
            public UdpClient agent;
            public IPEndPoint myIep;
            public async Task SendMessage(IPEndPoint? remoteIep, double value)
            {
                if (remoteIep != null)
                {
                    var data = BitConverter.GetBytes(value);
                    await agent.SendAsync(data, data.Length, remoteIep);
                }
                else
                {
                    Debug.WriteLine("IP endpoint is null");
                }
            }

            public async Task SendMessage(IPEndPoint? remoteIep, byte[] message)
            {
                if (remoteIep != null)
                {
                    // Debug.WriteLine($"Sending[{remoteIep}]...{message.Length}");
                    await agent.SendAsync(message, message.Length, remoteIep);
                }
                else
                {
                    Debug.WriteLine("IP endpoint is null");
                }
            }
        }

        private static ScanerLinker? CurrentWindow = null;

        private MainWindow parent;

        public FacePose CurrentPose { get; private set; } = new FacePose();

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
            CurrentPose = pose;

            Dispatcher.BeginInvoke(() =>
            {
                FaceDetectionStateLabel.Content = sender.IsFaceAnalysisActive ? "Running" : "Stopped";
            });
        }

        private void RTGatewayConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PortTextbox.Text, out int port) && IPAddress.TryParse(IpTextbox.Text, out IPAddress? ip))
            {
                Debug.WriteLine($"Connecting to {ip}:{port}...");
                UdpState state = new UdpState()
                {
                    agent = new UdpClient(),
                    myIep = new IPEndPoint(ip, port)
                };
                state.agent.Client.Bind(state.myIep);
                state.agent.BeginReceive(OnReceiveRtGatewayMessage, state);
                Debug.WriteLine("Receiving...");

                IpTextbox.IsEnabled = false;
                PortTextbox.IsEnabled = false;
                RTGatewayConnectButton.IsEnabled = false;
            }
        }

        private async void OnReceiveRtGatewayMessage(IAsyncResult ar)
        {
            if (ar.AsyncState != null)
            {
                UdpState resultState = (UdpState)ar.AsyncState;
                UdpClient agent = resultState.agent;
                IPEndPoint myIep = resultState.myIep;
                IPEndPoint? remoteEp = null;

                byte[] receivedBytes = agent.EndReceive(ar, ref remoteEp);
                // Debug.WriteLine($"Sending[{remoteEp}]: {data.Length}...");
                await resultState.SendMessage(remoteEp, CurrentPose.ToByteArray());

                await Dispatcher.BeginInvoke(() =>
                {
                    RtGatewayStateLabel.Content = (agent.Client.Connected).ToString();
                });
                agent.BeginReceive(OnReceiveRtGatewayMessage, ar.AsyncState);
            }
            else
            {
                Debug.Assert(false, "Unknown Error");
            }
        }
    }
}
