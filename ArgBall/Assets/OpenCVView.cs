using UnityEngine;
using System.Collections;
using System;
using System.IO;
using OpenCvSharp;
using System.Runtime.InteropServices;
using OpenCvSharp.CPlusPlus;

public class OpenCVView : MonoBehaviour {

	public GameObject planeObj;
	public WebCamTexture webcamTexture;
	public Texture2D texImage;
	public string deviceName;
	private int devId = 1;
	private int imWidth = 640;
	private int imHeight = 480;
	private string errorMsg = "No errors found!";

	static IplImage matrix;
	byte[] data;

	// Use this for initialization
	void Start() {
		WebCamDevice[] devices = WebCamTexture.devices;
		Debug.Log("num:" + devices.Length);

		for (int i = 0; i < devices.Length; i++) {
			print(devices[i].name);
			if (devices[i].name.CompareTo(deviceName) == 1) {
				devId = i;
			}
		}

		if (devId >= 0) {
			planeObj = GameObject.Find("Plane");
			texImage = new Texture2D(imWidth, imHeight, TextureFormat.RGB24, false);
			webcamTexture = new WebCamTexture(devices[devId].name, imWidth, imHeight, 60);
			webcamTexture.Play();
			data = new byte[imWidth * imHeight * 3];
			matrix = new IplImage(imWidth, imHeight, BitDepth.U8, 4);
		}
	}

	void Update() {
		if (devId >= 0) {

			Texture2DtoIplImage();

			CvFont font = new CvFont(FontFace.Vector0, 1.0, 1.0);
			CvColor rcolor = CvColor.Random();
			Cv.PutText(matrix, "Snapshot taken!", new CvPoint(15, 30), font, rcolor);

			IplImage cny = new IplImage(imWidth, imHeight, BitDepth.U8, 1);
			matrix.CvtColor(cny, ColorConversion.RgbToGray);

			Cv.Canny(cny, cny, 50, 50, ApertureSize.Size3);
			Cv.CvtColor(cny, matrix, ColorConversion.GrayToBgr);

			if (webcamTexture.didUpdateThisFrame) {
				//IplImageToTexture2D();
			}

			//SIFT sift = new SIFT();
			//KeyPoint[] keypoints1, keypoints2;
			//MatOfFloat descriptors1 = new MatOfFloat();
			//sift.Run(matrix, null, out keypoints1, descriptors1);

			//VideoCapture vc = new VideoCapture()

		} else {
			Debug.Log("Can't find camera!");
		}
	}

	void OnGUI() {
		GUI.Label(new UnityEngine.Rect(200, 200, 100, 90), errorMsg);
	}


	void IplImageToTexture2D() {
		int jBackwards = imHeight;

		for (int i = 0; i < imHeight; i++) {
			for (int j = 0; j < imWidth; j++) {
				float r = (float)matrix[i, j].Val0;
				float g = (float)matrix[i, j].Val1;
				float b = (float)matrix[i, j].Val2;
				Color color = new Color(r / 255.0f, g / 255.0f, b / 255.0f);


				jBackwards = imHeight - i - 1; // notice it is jBackward and i
				texImage.SetPixel(j, jBackwards, color);
			}
		}
		
		//Debug.Log(matrix.ToString());
		//texImage.LoadImage(matrix.(".png"));
		//matrix.ToBytes(".png");
		//Marshal.Copy(, 0, texImage.GetNativeTexturePtr(), 640 * 480 * 4);

		

		texImage.Apply();
		planeObj.renderer.material.mainTexture = texImage;

	}

	Color32[] colors = new Color32[640 * 480];

	void Texture2DtoIplImage() {
		int jBackwards = imHeight;

		/*for (int v = 0; v < imHeight; ++v) {
			for (int u = 0; u < imWidth; ++u) {

				CvScalar col = new CvScalar();
				col.Val0 = (double)webcamTexture.GetPixel(u, v).b * 255;
				col.Val1 = (double)webcamTexture.GetPixel(u, v).g * 255;
				col.Val2 = (double)webcamTexture.GetPixel(u, v).r * 255;

				jBackwards = imHeight - v - 1;

				matrix.Set2D(jBackwards, u, col);
				matrix.se
				//matrix [jBackwards, u] = col;
			}
		}*/

		this.webcamTexture.GetPixels32(colors);
		byte[] bytes = Color32ArrayToByteArray(colors); // OPTIMIZE
		Marshal.Copy(bytes, 0, matrix.ImageData, 640 * 480 * 4);

		/*IntPtr ptr = matrix.ImageData;
		for (int x = 0; x < imWidth; x++) {
			for (int y = 0; y < imHeight; y++) {
				int offset = (image.WidthStep * y) + (x * 3);
				byte b = Marshal.ReadByte(ptr, offset + 0);    // B
				byte g = Marshal.ReadByte(ptr, offset + 1);    // G
				byte r = Marshal.ReadByte(ptr, offset + 2);    // R
				Marshal.WriteByte(ptr, offset, r);
				Marshal.WriteByte(ptr, offset, g);
				Marshal.WriteByte(ptr, offset, b);
			}
		}


		Marshal.Copy(this.webcamTexture.GetNativeTexturePtr, matrix., 0, 640 * 480 * 3);*/
		//Cv.SaveImage ("C:\\Hasan.jpg", matrix);
	}

	private static byte[] Color32ArrayToByteArray(Color32[] colors) {
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
