using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Threading;
using System.Diagnostics;

using OpenCVWrappers;
using CppInterop.LandmarkDetector;
using FaceAnalyser_Interop;
using GazeAnalyser_Interop;
using FaceDetectorInterop;
using UtilitiesOF;

using FaceLinker.Analyzer;

namespace FaceLinker.Components
{
    public class Webcam
    {
        public int Id = 0;
        public string Name = string.Empty;
        public List<Tuple<int, int>> Resolutions = new List<Tuple<int, int>>();
        public WriteableBitmap CurrentScreenImage;
        public Image? Target;
        private bool isActivated = false;

        public Webcam(Tuple<int, string, List<Tuple<int, int>>, RawImage> cam_e)
        {
            Id = cam_e.Item1;
            Name = cam_e.Item2;
            Resolutions.AddRange(cam_e.Item3);
            CurrentScreenImage = cam_e.Item4.CreateWriteableBitmap();
            cam_e.Item4.UpdateWriteableBitmap(CurrentScreenImage);
        }

        public Webcam(int id, string name, List<Tuple<int, int>> resolutionList, RawImage rawImage)
        {
            Id = id;
            Name = name;
            Resolutions.AddRange(resolutionList);
            CurrentScreenImage = rawImage.CreateWriteableBitmap();
            rawImage.UpdateWriteableBitmap(CurrentScreenImage);
        }

        public void SetActiveTarget(Image screen)
        {
            Target = screen;
            Target.Source = CurrentScreenImage;
        }

        public void ClearTarget()
        {
            if (Target != null)
            {
                Target.Source = null;
                Target = null;
            }
        }

        public async void ActivateSequence(int x = -1, int y = -1)
        {
            if (isActivated)
            {
                Debug.WriteLine("This webcam is already activated");
            }
            else
            {
                SequenceReader reader = new SequenceReader(Id, x, y);
                // 추후 웹캠이 아닌 이미지 시퀸스의 경우 코드 구조를 어떻게 짤 것인지 고민할 것

                isActivated = true;
                await Task.Run(() =>
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Highest;
                    FaceLandmarkManager fl_manager = new FaceLandmarkManager();

                    // 다른 모델이 생성 가능하다고 확인 되면 다른 모델 대응이 되도록 수정
                    if (fl_manager.IsModelLoaded == false)
                    {
                        isActivated = false;
                        return;
                    }


                    // Setup the visualization
                    

                    // Loading an image file
                    var frame = reader.GetNextImage();
                    var grayFrame = reader.GetCurrentFrameGray();

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    long prevTime = sw.ElapsedMilliseconds;
                    while (!grayFrame.IsEmpty || !isActivated)
                    {
                        long currentTime = sw.ElapsedMilliseconds;
                        if (currentTime - prevTime >= 16)
                        {
                            Debug.WriteLine("Update");
                            RawImage image = fl_manager.Analyse(frame, grayFrame, reader);


                            // Only the final face will contain the details
                            // fl_manager.Visualize(frame, visualizer_of, landmarkDetector.CalculateAllLandmarks(), landmarkDetector.GetVisibilities(), detection_succeeding, true, false, reader.GetFx(), reader.GetFy(), reader.GetCx(), reader.GetCy(), progress);
                            
                            if (Target != null)
                            {
                                Target.Dispatcher.Invoke(() => {
                                    CurrentScreenImage = image.CreateWriteableBitmap();
                                    image.UpdateWriteableBitmap(CurrentScreenImage);
                                    Target.Source = CurrentScreenImage;
                                });
                            }

                            frame = reader.GetNextImage();
                            grayFrame = reader.GetCurrentFrameGray();
                        }
                    }
                });
            }
        }

        public void DeactivateSequece()
        {
            isActivated = false;
        }

        public override string ToString() { return Name; }
    }
}
