using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSMouseLook : MonoBehaviour {

	public enum RotationAxes {
		MouseX,
		MouseY
	}

	public RotationAxes axes = RotationAxes.MouseY;

	private float currentSensivityX = 1.5f, currentSensivityY = 1.5f, sensivityX = 1.5f, sensivityY = 1.5f, rotationX, rotationY,
	minimumX = -360f, maximumX = 360f, minimumY = -60f, maximumY = 60f, mouseSensivity = 1.7f;

	private Quaternion originalRotation;

	// Use this for initialization
	void Start () {
		originalRotation = transform.rotation;
	}

	void LateUpdate () {
		HandleRotation ();
	}

	float ClampAngle(float angle, float min, float max) {
		if (angle < -360f) {
			angle += 360f;
		} else if (angle > 360f) {
			angle -= 360f;
		}

		return Mathf.Clamp (angle, min, max);
	}

	void HandleRotation() {
		if (currentSensivityX != mouseSensivity || currentSensivityY != mouseSensivity) {
			currentSensivityX = currentSensivityY = mouseSensivity;
		}

		sensivityX = currentSensivityX;

		sensivityY = currentSensivityY;

		if (axes == RotationAxes.MouseX) {
			rotationX += Input.GetAxis ("Mouse X") * sensivityX;
			rotationX = ClampAngle (rotationX, minimumX, maximumX);

			Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);

			transform.localRotation = originalRotation * xQuaternion;
		} else if (axes == RotationAxes.MouseY) {
			rotationY += Input.GetAxis ("Mouse Y") * sensivityY;
			rotationY = ClampAngle (rotationY, minimumY, maximumY);

			Quaternion yQuaternion = Quaternion.AngleAxis (-rotationY, Vector3.right);

			transform.localRotation = originalRotation * yQuaternion;
		}
	}
}