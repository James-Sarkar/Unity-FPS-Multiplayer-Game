using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour {

	public float walkSpeed = 6.75f, runSpeed = 10f, crouchSpeed = 4f, jumpSpeed = 8f, gravity = 20f;

	private Transform firstPersonView, firstPersonCamera;

	private Vector3 firstPersonViewRotation = Vector3.zero, moveDirection = Vector3.zero;

	private float speed, inputX, inputY, inputXSet, inputYSet, inputModifyFactor, antiBumpFactor = 0.75f;

	private bool isMoving, isGrounded, isCrouching, limitDiagonalSpeed = true;

	private CharacterController charController;

	// Use this for initialization
	void Start () {
		firstPersonView = transform.Find ("FPS View").transform;
		charController = GetComponent<CharacterController> ();
		speed = walkSpeed;
		isMoving = false;
	}
	
	// Update is called once per frame
	void Update () {
		PlayerMovement ();
	}

	void PlayerMovement() {
		// Move forwards or backwards
		if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.S)) {
			if (Input.GetKey (KeyCode.W)) {
				inputYSet = 1f;
			} else {
				inputYSet = -1f;
			}
		} else {
			inputYSet = 0f;
		}

		// Move left or right
		if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.D)) {
			if (Input.GetKey (KeyCode.A)) {
				inputXSet = -1f;
			} else {
				inputXSet = 1f;
			}
		} else {
			inputXSet = 0f;
		}

		inputY = Mathf.Lerp (inputY, inputYSet, Time.deltaTime * 19f);
		inputX = Mathf.Lerp (inputX, inputXSet, Time.deltaTime * 19f);

		inputModifyFactor = Mathf.Lerp (inputModifyFactor,
			(inputYSet != 0 && inputXSet != 0 && limitDiagonalSpeed) ? 0.75f : 1.0f,
			Time.deltaTime * 19f);

		firstPersonViewRotation = Vector3.Lerp (firstPersonViewRotation,
			Vector3.zero, Time.deltaTime * 5f);
		firstPersonView.localEulerAngles = firstPersonViewRotation;

		if (isGrounded) {
			moveDirection = new Vector3 (inputX * inputModifyFactor, -antiBumpFactor, inputY * inputModifyFactor);
			moveDirection = transform.TransformDirection (moveDirection) * speed;
		}

		// Apply Gravity
		moveDirection.y -= gravity * Time.deltaTime;

		isGrounded = (charController.Move (moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;

		isMoving = charController.velocity.magnitude > 0.15f;
	}
}
