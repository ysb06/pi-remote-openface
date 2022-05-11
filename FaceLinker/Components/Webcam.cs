using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

using OpenCVWrappers;
using CppInterop.LandmarkDetector;
using FaceAnalyser_Interop;
using GazeAnalyser_Interop;
using FaceDetectorInterop;
using UtilitiesOF;

namespace FaceLinker.Components
{
    public class Webcam
    {
        public int Id = 0;
        public string Name = string.Empty;
        public List<Tuple<int, int>> Resolutions = new List<Tuple<int, int>>();
        public WriteableBitmap CurrentScreenImage;
        public Image? Target;

        public Webcam(Tuple<int, string, List<Tuple<int, int>>, RawImage> cam_e)
        {
            Id = cam_e.Item1;
            Name = cam_e.Item2;
            Resolutions.AddRange(cam_e.Item3);
            CurrentScreenImage = cam_e.Item4.CreateWriteableBitmap();
        }

        public Webcam(int id, string name, List<Tuple<int, int>> resolutionList, RawImage rawImage)
        {
            Id = id;
            Name = name;
            Resolutions.AddRange(resolutionList);
            CurrentScreenImage = rawImage.CreateWriteableBitmap();
        }

        public void SetActiveTarget(Image screen)
        {
            Target = screen;
            Target.Source = CurrentScreenImage;
        }

        public void GetCameraSequence()
        {
            SequenceReader reader = new SequenceReader(Id, -1, -1);
        }
        

        public override string ToString() { return Name; }
    }
}
