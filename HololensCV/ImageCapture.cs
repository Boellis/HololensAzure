using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR.WSA.WebCam;

public class ImageCapture {
    public static ImageCapture instance;
    public int tapsCount;
    private PhotoCapture photoCaptureObject = null;
    private GestureRecognizer recognizer;
    private bool currentlyCapturing = false;

    private void Awake(){
        instance = this;
    }

    void Start(){
        //Subscribing to the Hololens API Gesture Recognizer to track user gestures
        recognizer = new GestureRecognizer();
        recognizer.SetRecogizableGestures(GestureSettings.Tap);
        recognizer.Tapped += TapHandler;
        recognizer.StartCapturingGestures();
    }
    
    //Respond to Tap Input
    private void TapHandler(TappedEventArgs obj){
        //Only allow capturing, if not currently processing a request
        if(currentlyCapturing == false){
            currentlyCapturing = true;

            //Increase taps count, used to name images when saving
            tapsCount++;

            //Create a label in world space using the ResultsLabel class
            ResultsLabel.instance.CreateLabel();

            //Begins the image capture and analysis procedure
            ExecuteImageCaptureAndAnalysis();
        }
    }

    //Register the full execution fo the PhotoCapure. If successful, it will begin the Image Analysis process
    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result){
        //Call StopPhotoMode once the image has successfully captured
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result){
        //Dispose from the object in memory and request the image analysis to the VisionManager class
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
        StartCoroutine(VisionManager.instance.AnalyseLastImageCaptured());
    }

    //Begin the process of Image Capturing and send to Azure Computer Vision Service
    private void ExecuteImageCaptureAndAnalysis(){
        //Set the camera to the highest possible resolution
        Resolution cameraResolution = PhotoCapture.SupportResolutions.OrderByDescending((res) => res.width * res.height).First();
        Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        //Begin capture process, set the image format.
        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
            photoCaptureObject = captureObject;
            CameraParameters camParameters = new CameraParameters();
            camParameters.hologramOpacity = 0.0f;
            camParameters.cameraResolutionWidth = targetTexture.width;
            camParameters.cameraResolutionHeight = targetTexture.height;
            camParameters.pixeFormat = CapturePixelFormat.BGRA32;

            //Capture the image from the camera and save it in the App internal folder
            captureObject.StartPhotoModeAsync(camParameters, delegate (PhotoCapture.PhotoCaptureResult result)
            {
                string filename = string.Format(@"CapturedImage{0}.jpg", tapsCount);
                string filePath = Path.Combine(Application.persistentDataPath, filename);
                VisionManager.instance.imagePath = filePath;
                photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
            });
        });
    }
}