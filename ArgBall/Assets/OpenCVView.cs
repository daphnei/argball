using UnityEngine;
using System.Collections;
using System;
using System.IO;
using OpenCvSharp;
using System.Runtime.InteropServices;
using OpenCvSharp.CPlusPlus;

/// <summary>
/// An attempt to get OpenCV working within Unity.
/// It ended up working decently, but performance seemed inconsistent and the
/// framework often caused crashes when it isn't used properly. 
/// </summary>
public class OpenCVView : MonoBehaviour {

	private const int WEB_WIDTH = 320;
	private const int WEB_HEIGHT = 240;

	public GameObject outputPlane;
	public WebCamTexture webcamTexture;
	public Texture2D outputPlaneImage;
	public string deviceName;

	private int cameraId = 1;
	private string errorMessage = "No errors found!";
	private Color32[] colorBuffer = new Color32[WEB_WIDTH * WEB_HEIGHT];

	private static IplImage ImageBuffer;

	// Use this for initialization
	void Start() {

		// Find a webcam device.
		WebCamDevice[] devices = WebCamTexture.devices;
		for (int i = 0; i < devices.Length; i++) {
			if (devices[i].name.CompareTo(deviceName) == 1) {
				this.cameraId = i;
			}
		}

		// Setup the webcam.
		if (this.cameraId >= 0) {
			this.outputPlane = GameObject.Find("Plane");
			this.outputPlaneImage = new Texture2D(WEB_WIDTH, WEB_HEIGHT, TextureFormat.RGB24, false);

			this.webcamTexture = new WebCamTexture(devices[this.cameraId].name, WEB_WIDTH, WEB_HEIGHT, 60);
			this.webcamTexture.Play();
			this.outputPlane.renderer.material.mainTexture = this.webcamTexture;

			ImageBuffer = new IplImage(WEB_WIDTH, WEB_HEIGHT, BitDepth.U8, 4);			
		}
	}
	
	void Update() {
		if (this.cameraId >= 0) {
			CopyWebcamToIplImage(ImageBuffer);

			// Place some text on the image to test OpenCV.
			CvFont font = new CvFont(FontFace.Vector0, 1.0, 1.0);
			CvColor rcolor = CvColor.Random();
			Cv.PutText(ImageBuffer, "Snapshot taken!", new CvPoint(15, 30), font, rcolor);

			// Run canny on the image to test OpenCV.
			IplImage cny = new IplImage(WEB_WIDTH, WEB_HEIGHT, BitDepth.U8, 1);
			ImageBuffer.CvtColor(cny, ColorConversion.RgbToGray);
			Cv.Canny(cny, cny, 50, 50, ApertureSize.Size3);
			Cv.CvtColor(cny, ImageBuffer, ColorConversion.GrayToBgr);

			// Detect circles using OpenCV.
			CvCircleSegment[] circles = Cv2.HoughCircles(InputArray.Create(new Mat(cny)), HoughCirclesMethod.Gradient, 1.0, 1.0);
			this.errorMessage = "circles " + circles.Length;

			// We tried to get SIFT to work, but it required mapping between different
			// native C++ OpenCV objects, which created some difficultly with the
			// OpenCVSharp wrapper.
			 
			// SIFT sift = new SIFT();
			// KeyPoint[] keypoints1;
			// MatOfFloat descriptors1 = new MatOfFloat();
			// sift.Run(new Mat(matrix), null, out keypoints1, descriptors1);

			// For debugging purposes, 
			if (webcamTexture.didUpdateThisFrame) {
				// this.PushIplImageToOutputPlane();
			}
		} else {
			Debug.Log("Can't find camera!");
		}
	}

	/// <summary>
	/// Show the error messages on the screen.
	/// </summary>
	void OnGUI() {
		GUI.Label(new UnityEngine.Rect(200, 200, 100, 90), this.errorMessage);
	}

	/// <summary>
	/// Converts a OpenCV texture to a Unity texture.
	/// Very slow!
	/// </summary>
	void PushIplImageToOutputPlane() {
		int jBackwards = WEB_HEIGHT;

		for (int i = 0; i < WEB_HEIGHT; i++) {
			for (int j = 0; j < WEB_WIDTH; j++) {
				float r = (float)ImageBuffer[i, j].Val0;
				float g = (float)ImageBuffer[i, j].Val1;
				float b = (float)ImageBuffer[i, j].Val2;
				Color color = new Color(r / 255.0f, g / 255.0f, b / 255.0f);


				jBackwards = WEB_HEIGHT - i - 1; // notice it is jBackward and i
				outputPlaneImage.SetPixel(j, jBackwards, color);
			}
		}
		
		// Debug.Log(matrix.ToString());
		// texImage.LoadImage(matrix.(".png"));
		// matrix.ToBytes(".png");
		// Marshal.Copy(, 0, texImage.GetNativeTexturePtr(), 640 * 480 * 4);

		outputPlaneImage.Apply();
		outputPlane.renderer.material.mainTexture = outputPlaneImage;
	}

	/// <summary>
	/// Copies the webcam image to an OpenCV image.
	/// </summary>
	private void CopyWebcamToIplImage(IplImage image) {
		this.webcamTexture.GetPixels32(colorBuffer);
		byte[] bytes = ConvertColor32ArrayToByteArray(colorBuffer);
		Marshal.Copy(bytes, 0, image.ImageData, WEB_WIDTH * WEB_HEIGHT * 4);
	}

	/// <summary>
	/// Converts color arrays between the OpenCV type and the Unity type.
	/// </summary>
	private static byte[] ConvertColor32ArrayToByteArray(Color32[] colors) {
		if (colors == null || colors.Length == 0)
			return null;

		int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
		int length = lengthOfColor32 * colors.Length;
		byte[] bytes = new byte[length];

		GCHandle handle = default(GCHandle);
		try {
			handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
			IntPtr ptr = handle.AddrOfPinnedObject();
			Marshal.Copy(ptr, bytes, 0, length);
		} finally {
			if (handle != default(GCHandle))
				handle.Free();
		}

		return bytes;
	}
}
