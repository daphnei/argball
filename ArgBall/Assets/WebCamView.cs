using UnityEngine;
using System.Collections;

/// <summary>
/// An attempt at using Unity's default camera API to light
/// image processing in native C#. This technique proved far too slow.
/// Some parts of this code were adapted from various Unity camera tutorials.
/// </summary>
public class WebCamView : MonoBehaviour {

	const int CAMERA_RESOLUTION = 100;
	const int FAIL_HUE = 180;

	WebCamTexture cameraTexture;
	Texture2D outputTexture;

	Color32[] cameraData;	
	IEnumerator cameraProcessor;
	bool cameraDataChanged = false;

	void Start() {
		// Find the webcam device.
		WebCamDevice[] devices = WebCamTexture.devices;
		string webCamName = "";
		for (int i = 0; i < devices.Length; i++) {
			if (!devices[i].isFrontFacing) {
				webCamName = devices[i].name;
			}
		}

		// Set up the webcam device.
		this.cameraTexture = new WebCamTexture(webCamName, CAMERA_RESOLUTION, CAMERA_RESOLUTION * (Screen.height / Screen.width), 60);
		this.cameraTexture.Play();

		// Build buffers for camera data manipulation and texture exporting.
		this.cameraData = new Color32[cameraTexture.width * cameraTexture.height];
		this.outputTexture = new Texture2D(cameraTexture.width, cameraTexture.height);
	}

	void Update() {
		// An optimization attempt; only do processing if camera detects an update.
		if (this.cameraProcessor == null && cameraTexture.didUpdateThisFrame) {
			this.cameraProcessor = UpdateProcessedCamera();
		}

		// Another optimization technique; use timeslicing to improve performance by
		// doing calculations over several frames.
		if (this.cameraProcessor != null) {
			float lastTime = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup - lastTime < 0.016f) {
				if (!this.cameraProcessor.MoveNext()) {
					this.cameraProcessor = null;
					break;
				}
			}
		}
	}

	/// <summary>
	/// A timeslice-able function that process 'redness' estimation.
	/// </summary>
	IEnumerator UpdateProcessedCamera() {
		cameraTexture.GetPixels32(this.cameraData);
		for (int x = 0; x < cameraTexture.width; x += 1) {
			for (int y = 0; y < cameraTexture.height; y += 1) {
				int i = y * this.cameraTexture.height + x;
				float luminance, saturation;
				int hue = this.GetHue(cameraData[i], out saturation, out luminance);
				if (hue < 15 || hue > 345) {
					cameraData[i].r = 255;
					cameraData[i].g = 255;
					cameraData[i].b = 255;
				}
			}
			this.cameraDataChanged = true;
			yield return null;
		}
	}

	
	/// <summary>
	/// Adapted from a StackOverflow post; gets the hue associated with a color.
	/// </summary>
	int GetHue(Color32 color, out float saturation, out float luminance) {
		float r = color.r / 255f;
		float g = color.g / 255f;
		float b = color.b / 255f;

		float max = Mathf.Max(r, g, b);
		float min = Mathf.Min(r, g, b);

		luminance = (max + min) / 2f;
		if (luminance < 0.5) {
			saturation = (max - min) / (max + min);
		} else {
			saturation = (max - min) / (2 - max - min);
		}

		if (luminance < 0.2 || saturation < 0.6f) {
			return FAIL_HUE;
		}

		if (max == min) {
			return FAIL_HUE;
		}

		float value;
		if (r > b && r > g) {
			value = (g - b) / (max - min);
		} else if (b > r && b > g) {
			value = 4 + (r - g) / (max - min);
		} else {
			value = 2 + (b - r) / (max - min);
		}
		if (value < 0) {
			value += 6;
		}
		return (int)(value * 60);
	}

	/// <summary>
	/// Get the difference between color vectors.
	/// </summary>
	float GetColorSquaredDistance(Color a, Color b) {
		return (a.r - b.r) * (a.r - b.r)
			+ (a.g - b.g) * (a.g - b.g)
			+ (a.b - b.b) * (a.b - b.b);
	}

	/// <summary>
	/// Draws debug information onto the viewport.
	/// </summary>
	void OnGUI() {
		GUI.depth = 100;
		float scale = Mathf.Max(1, Mathf.Min(Mathf.Floor(Screen.width / outputTexture.width), Mathf.Floor(Screen.height / outputTexture.height)));

		if (!Input.GetMouseButton(0)) {
			GUI.DrawTexture(new Rect(0, 0, outputTexture.width * scale, outputTexture.height * scale), this.cameraTexture);
		} else {
			if (this.cameraDataChanged) {
				this.outputTexture.SetPixels32(this.cameraData);
				this.outputTexture.Apply();
				this.cameraDataChanged = false;
			}
			GUI.DrawTexture(new Rect(0, 0, outputTexture.width * scale, outputTexture.height * scale), this.outputTexture);
		}

		if (this.cameraProcessor == null) {
			// DING! When a frame is finished processing.
			GUI.Label(new Rect(100, 100, 100, 100), "DING!");
		}
	}
}
