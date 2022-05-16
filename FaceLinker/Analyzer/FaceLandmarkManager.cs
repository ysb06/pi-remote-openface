using System;
using System.Collections.Generic;
using System.Windows;
using CppInterop.LandmarkDetector;
using UtilitiesOF;
using OpenCVWrappers;
using FaceAnalyser_Interop;

namespace FaceLinker.Analyzer
{
    public enum LandmarkModelType
    {
        CLM, CECLM, CLNF
    }

    public enum FaceDetectorType
    {
        Haar, HOG_SVM, MTCNN
    }

    public enum SourceType
    {
        Image, Video
    }

    public class FaceLandmarkManager
    {
        private CLNF model;
        private FaceModelParameters faceModelParams;
        private Visualizer visualizer;
        private FaceAnalyserManaged face_analyser;

        public bool IsModelLoaded { get { return model.isLoaded(); } }

        // Visualiser와 FaceAnalyser는 임시로 만듦. 추후에 LandmarkDetectorGenerator와 유사하게 생성할지 결정
        // 모든 값은 Offline 데모 초기 설정 값
        public FaceLandmarkManager(
            LandmarkModelType landmarkModel = LandmarkModelType.CLNF,
            FaceDetectorType faceModel = FaceDetectorType.MTCNN,
            SourceType sourceType = SourceType.Video
        )
        {
            (model, faceModelParams) = GenerateModel(landmarkModel, faceModel, sourceType);
            // Setup the visualization
            visualizer = new Visualizer(true, true, true, false);
            // Initialize the face analyser
            face_analyser = new FaceAnalyserManaged(AppDomain.CurrentDomain.BaseDirectory, true, 128, true);
        }

        public RawImage Analyse(RawImage frame, RawImage grayFrame, SequenceReader reader)
        {
            double progress = reader.GetProgress();
            // The face analysis step (for AUs and eye gaze)
            bool detection_succeeding = model.DetectLandmarksInVideo(frame, faceModelParams, grayFrame);
            
            var landmarks = model.CalculateAllLandmarks();
            face_analyser.AddNextFrame(frame, landmarks, detection_succeeding, false);
            // gaze_analyser.AddNextFrame(landmarkDetector, detection_succeeding, reader.GetFx(), reader.GetFy(), reader.GetCx(), reader.GetCy());
            return VisualizeFeatures(frame, detection_succeeding, true, false, reader, progress);
        }

        private static (CLNF, FaceModelParameters) GenerateModel(
            LandmarkModelType ldType,
            FaceDetectorType fdType,
            SourceType sType
        )
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            FaceModelParameters modelParam = new FaceModelParameters(
                root,
                ldType == LandmarkModelType.CECLM,
                ldType == LandmarkModelType.CLNF,
                ldType == LandmarkModelType.CLM
            );
            modelParam.SetFaceDetector(
                fdType == FaceDetectorType.Haar,
                fdType == FaceDetectorType.HOG_SVM,
                fdType == FaceDetectorType.MTCNN
            );
            switch (sType)
            {
                case SourceType.Image:
                    modelParam.optimiseForImages();
                    break;
                case SourceType.Video:
                    modelParam.optimiseForVideo();
                    break;
            }
            CLNF model = new CLNF(modelParam);
            model.Reset();

            return (model, modelParam);
        }

// VisualizeFeatures(frame, visualizer_of, landmark_detector.CalculateAllLandmarks(), landmark_detector.GetVisibilities(), detection_succeeding, 
// true, false, reader.GetFx(), reader.GetFy(), reader.GetCx(), reader.GetCy(), progress);
        public RawImage VisualizeFeatures(
            RawImage frame, 
            bool detection_succeeding, bool new_image, bool multi_face, 
            SequenceReader reader, 
            double progress,
            List<Tuple<float, float>>? landmarks = null
        ){
            var fx = reader.GetFx(); 
            var fy = reader.GetFy();
            var cx = reader.GetCx();
            var cy = reader.GetCy();
            List<bool> visibilities = model.GetVisibilities();
            if (landmarks == null) {
                landmarks = model.CalculateAllLandmarks();
            }

            List<Tuple<Point, Point>> lines = null;
            List<Tuple<float, float>> eye_landmarks = null;
            List<Tuple<Point, Point>> gaze_lines = null;
            Tuple<float, float> gaze_angle = new Tuple<float, float>(0, 0);

            List<float> pose = new List<float>();
            model.GetPose(pose, fx, fy, cx, cy);
            List<float> non_rigid_params = model.GetNonRigidParams();

            double confidence = model.GetConfidence();

            if (confidence < 0)
                confidence = 0;
            else if (confidence > 1)
                confidence = 1;

            double scale = model.GetRigidParams()[0];

            // Helps with recording and showing the visualizations
            if (new_image)
            {
                visualizer.SetImage(frame, fx, fy, cx, cy);
            }
            visualizer.SetObservationHOG(face_analyser.GetLatestHOGFeature(), face_analyser.GetHOGRows(), face_analyser.GetHOGCols());
            visualizer.SetObservationLandmarks(landmarks, confidence, visibilities);
            visualizer.SetObservationPose(pose, confidence);
            // visualizer.SetObservationGaze(gaze_analyser.GetGazeCamera().Item1, gaze_analyser.GetGazeCamera().Item2, landmark_detector.CalculateAllEyeLandmarks(), landmark_detector.CalculateAllEyeLandmarks3D(fx, fy, cx, cy), confidence);

            eye_landmarks = model.CalculateVisibleEyeLandmarks();
            lines = model.CalculateBox(fx, fy, cx, cy);

            // gaze_lines = gaze_analyser.CalculateGazeLines(fx, fy, cx, cy);
            // gaze_angle = gaze_analyser.GetGazeAngle();

            return frame;
        }
    }
}