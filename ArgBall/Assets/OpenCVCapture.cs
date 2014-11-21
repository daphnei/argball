using UnityEngine;
using System.Collections;
using System;
using System.IO;
using OpenCvSharp;
using System.Runtime.InteropServices;
using OpenCvSharp.CPlusPlus;

public class OpenCVCapture : MonoBehaviour {

	private GameObject planeObject;
	private WebCamTexture webcamTexture;
	private Texture2D outputTexture;
	private string deviceName;
	private int devId = 1;
	private string errorMsg = "No errors found!";
	static IplImage matrix2;
	byte[] data;

	private VideoCapture vcapture;
	private CvCapture capture;
	static IplImage captureImage2;
	static IplImage convertedImage2;
	static IplImage convertedImage3;
	private int IM_WIDTH = 320;
	private int IM_HEIGHT = 240;
	static Mat readCapture;

	// Use this for initialization
	void Start() {
		
		this.capture = CvCapture.FromCamera(0);
		//this.vcapture = VideoCapture.FromCamera(0);
		convertedImage2 = new IplImage(new CvSize(IM_WIDTH, IM_HEIGHT), BitDepth.U8, 3);
		convertedImage3 = new IplImage(new CvSize(IM_WIDTH, IM_HEIGHT), BitDepth.U8, 1);

		outputTexture = new Texture2D(IM_WIDTH, IM_HEIGHT, TextureFormat.RGB24, false);
		planeObject = GameObject.Find("Plane");
		planeObject.renderer.material.mainTexture = outputTexture;
		
		//readCapture = new Mat(
	}

	void Update() {
		//this.vcapture.Read(readCapture);

		if (this.capture.GrabFrame() != 0) {
			captureImage2 = this.capture.RetrieveFrame();
		}
		//captureImage2 = this.capture.QueryFrame();
		captureImage2.Resize(convertedImage2);
		convertedImage2.Canny(convertedImage3, 2, 2);
		this.IplImageToTexture2D(convertedImage3);
	}

	void IplImageToTexture2D(IplImage displayImg) {
		int jBackwards = IM_HEIGHT;

		for (int i = 0; i < displayImg.Height; i++) {
			for (int j = 0; j < displayImg.Width; j++) {
				float b = (float)displayImg[i, j].Val0;
				float g = (float)displayImg[i, j].Val1;
				float r = (float)displayImg[i, j].Val2;
				Color color = new Color(r / 255.0f, g / 255.0f, b / 255.0f);
				outputTexture.SetPixel(j, displayImg.Height - i - 1, color);
			}
		}
		outputTexture.Apply();
		
		//Debug.Log(matrix.ToString());
		//texImage.LoadImage(matrix.(".png"));
		//matrix.ToBytes(".png");
		//Marshal.Copy(, 0, texImage.GetNativeTexturePtr(), 640 * 480 * 4);

		

		//outputTexture.Apply();
		//planeObject.renderer.material.mainTexture = outputTexture;
	}

	Color32[] colors = new Color32[640 * 480];

	void Texture2DtoIplImage() {
		int jBackwards = IM_HEIGHT;

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
		Debug.Log(bytes.Length);
		Marshal.Copy(bytes, 0, matrix2.ImageData, 640 * 480 * 4);

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
