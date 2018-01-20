using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FPSController : NetworkBehaviour {

	public float walkSpeed = 6.75f, runSpeed = 10f, crouchSpeed = 4f, jumpSpeed = 8f, gravity = 20f;

	public GameObject playerHolder, weaponsHolder;

	public GameObject[] weaponsFPS;

	public FPSMouseLook[] mouseLook;

	public LayerMask groundLayer;

	private Transform firstPersonView, firstPersonCamera;

	private Camera mainCam;

	private Vector3 firstPersonViewRotation = Vector3.zero, moveDirection = Vector3.zero, defaultCamPos;

	private float speed, inputX, inputY, inputXSet, inputYSet, inputModifyFactor, antiBumpFactor = 0.75f, rayDistance, defaultControllerHeight, camHeight, fireRate = 15f, nextTimeToFire = 0f;

	private bool isMoving, isGrounded, isCrouching, limitDiagonalSpeed = true;

	private CharacterController charController;

	private FPSPlayerAnimations playerAnimations;

	[SerializeField]
	private WeaponsManager weaponsManager, handsWeaponsManager;

	private FPSWeapon currentWeapon;

	private FPSHandsWeapon currentHandsWeapon;

	// Use this for initialization
	void Start () {
		firstPersonView = transform.Find ("FPS View").transform;

		charController = GetComponent<CharacterController> ();

		speed = walkSpeed;

		isMoving = false;

		rayDistance = charController.height * 0.5f + charController.radius;

		defaultControllerHeight = charController.height;

		defaultCamPos = firstPersonView.localPosition;

		playerAnimations = GetComponent<FPSPlayerAnimations> ();

		weaponsManager.weapons [0].SetActive (true);

		currentWeapon = weaponsManager.weapons [0].GetComponent<FPSWeapon> ();

		handsWeaponsManager.weapons [0].SetActive (true);

		currentHandsWeapon = handsWeaponsManager.weapons [0].GetComponent<FPSHandsWeapon> ();

		// Change the view based on whether or not the current player
		// is a local player or a remote player
		if (isLocalPlayer) {
			playerHolder.layer = LayerMask.NameToLayer ("Player");

			foreach (Transform child in playerHolder.transform) {
				child.gameObject.layer = LayerMask.NameToLayer ("Player");
			}

			for (int i = 0; i < weaponsFPS.Length; i++) {
				weaponsFPS [i].layer = LayerMask.NameToLayer ("Player");
			}

			weaponsHolder.layer = LayerMask.NameToLayer ("Enemy");

			foreach (Transform child in weaponsHolder.transform) {
				child.gameObject.layer = LayerMask.NameToLayer ("Enemy");
			}
		} else {
			playerHolder.layer = LayerMask.NameToLayer ("Enemy");

			foreach (Transform child in playerHolder.transform) {
				child.gameObject.layer = LayerMask.NameToLayer ("Enemy");
			}

			for (int i = 0; i < weaponsFPS.Length; i++) {
				weaponsFPS [i].layer = LayerMask.NameToLayer ("Enemy");
			}

			weaponsHolder.layer = LayerMask.NameToLayer ("Player");

			foreach (Transform child in weaponsHolder.transform) {
				child.gameObject.layer = LayerMask.NameToLayer ("Player");
			}
		}

		if (!isLocalPlayer) {
			for (int i = 0; i < mouseLook.Length; i++) {
				mouseLook [i].enabled = false;
			}
		}

		mainCam = transform.Find ("FPS View").Find ("FPS Camera").GetComponent<Camera> ();
		mainCam.gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		// Move the local player only and change camera for local playera
		if (!isLocalPlayer) {
			return;
		} else {
			if (!mainCam.gameObject.activeInHierarchy) {
				mainCam.gameObject.SetActive (true);
			}
		}

		PlayerMovement ();

		SelectWeapon ();
	}

	void PlayerMovement () {
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
			// Call crouch and sprint
			CrouchAndSprint ();

			moveDirection = new Vector3 (inputX * inputModifyFactor, -antiBumpFactor, inputY * inputModifyFactor);
			moveDirection = transform.TransformDirection (moveDirection) * speed;

			// Call jump
			Jump ();
		}

		// Apply Gravity
		moveDirection.y -= gravity * Time.deltaTime;

		isGrounded = (charController.Move (moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;

		isMoving = charController.velocity.magnitude > 0.15f;

		HandleAnimations ();
	}

	void CrouchAndSprint () {
		if (Input.GetKeyDown(KeyCode.C)) {
			
			if (!isCrouching) {
				isCrouching = true;
			} else {
				if (CanGetUp ()) {
					isCrouching = false;
				}
			}

			StopCoroutine (MoveCameraWhenCrouching ());
			StartCoroutine (MoveCameraWhenCrouching ());
		}

		if (isCrouching) {
			speed = crouchSpeed;
		} else {
			if (Input.GetKey (KeyCode.LeftShift)) {
				speed = runSpeed;
			} else {
				speed = walkSpeed;
			}
		}

		playerAnimations.PlayerCrouch (isCrouching);
	}

	bool CanGetUp () {
		Ray groundRay = new Ray (transform.position, transform.up);

		RaycastHit groundHit;

		if (Physics.SphereCast (groundRay, charController.radius + 0.05f, out groundHit, rayDistance, groundLayer)) {
		
			if (Vector3.Distance (transform.position, groundHit.point) < 2.3f) {
				return false;
			}
		}
		return true;
	}

	IEnumerator MoveCameraWhenCrouching () {
		charController.height = isCrouching ? defaultControllerHeight / 1.5f : defaultControllerHeight;
		charController.center = new Vector3 (0f, charController.height / 2f, 0f);

		camHeight = isCrouching ? defaultCamPos.y / 1.5f : defaultCamPos.y;

		while (Mathf.Abs(camHeight - firstPersonView.localPosition.y) > 0.01f) {
			firstPersonView.localPosition = Vector3.Lerp (firstPersonView.localPosition, 
				new Vector3(defaultCamPos.x, camHeight, defaultCamPos.z), 
				Time.deltaTime * 11f);

			yield return null;
		}
	}

	void Jump () {
		if (Input.GetKeyDown (KeyCode.Space)) {

			if (isCrouching) {
				// Get up
				if (CanGetUp ()) {
					isCrouching = false;

					playerAnimations.PlayerCrouch (isCrouching);

					StopCoroutine (MoveCameraWhenCrouching ());
					StartCoroutine (MoveCameraWhenCrouching ());
				}
			} else {
				// Jump
				moveDirection.y = jumpSpeed;
			}
		}
	}

	void HandleAnimations () {
		playerAnimations.Movement (charController.velocity.magnitude);
		playerAnimations.PlayerJump (charController.velocity.y);

		if (isCrouching && charController.velocity.magnitude > 0f) {
			playerAnimations.PlayerCrouchWalk (charController.velocity.magnitude);
		}

		// Shooting
		if (Input.GetMouseButtonDown(0) && Time.time > nextTimeToFire) {
			nextTimeToFire = (Time.time + 1f) / fireRate;

			if (isCrouching) {
				playerAnimations.Shoot (false);
			} else {
				playerAnimations.Shoot (true);
			}

			currentWeapon.Shoot ();

			currentHandsWeapon.Shoot ();
		}

		// Reloading
		if (Input.GetKeyDown (KeyCode.R)) {
			playerAnimations.ReloadGun ();

			currentHandsWeapon.Reload ();
		}
	}

	void SelectWeapon() {
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			if (!handsWeaponsManager.weapons [0].activeInHierarchy) {
				for (int i = 0; i < handsWeaponsManager.weapons.Length; i++) {
					handsWeaponsManager.weapons [i].SetActive (false);
				}

				currentHandsWeapon = null;

				handsWeaponsManager.weapons [0].SetActive (true);
				currentHandsWeapon = handsWeaponsManager.weapons [0].GetComponent<FPSHandsWeapon> ();
			}

			if (!weaponsManager.weapons [0].activeInHierarchy) {
				for (int i = 0; i < weaponsManager.weapons.Length; i++) {
					weaponsManager.weapons [i].SetActive (false);
				}

				currentWeapon = null;
				weaponsManager.weapons [0].SetActive (true);
				currentWeapon = weaponsManager.weapons [0].GetComponent<FPSWeapon> ();

				playerAnimations.ChangeController (true);
			}
		} else if (Input.GetKeyDown (KeyCode.Alpha2)) {
			if (!handsWeaponsManager.weapons [1].activeInHierarchy) {
				for (int i = 0; i < handsWeaponsManager.weapons.Length; i++) {
					handsWeaponsManager.weapons [i].SetActive (false);
				}

				currentHandsWeapon = null;

				handsWeaponsManager.weapons [1].SetActive (true);
				currentHandsWeapon = handsWeaponsManager.weapons [1].GetComponent<FPSHandsWeapon> ();
			}
			
			if (!weaponsManager.weapons [1].activeInHierarchy) {
				for (int i = 0; i < weaponsManager.weapons.Length; i++) {
					weaponsManager.weapons [i].SetActive (false);
				}

				currentWeapon = null;
				weaponsManager.weapons [1].SetActive (true);
				currentWeapon = weaponsManager.weapons [1].GetComponent<FPSWeapon> ();

				playerAnimations.ChangeController (false);
			}
		} else if (Input.GetKeyDown (KeyCode.Alpha3)) {
			if (!handsWeaponsManager.weapons [2].activeInHierarchy) {
				for (int i = 0; i < handsWeaponsManager.weapons.Length; i++) {
					handsWeaponsManager.weapons [i].SetActive (false);
				}

				currentHandsWeapon = null;

				handsWeaponsManager.weapons [2].SetActive (true);
				currentHandsWeapon = handsWeaponsManager.weapons [2].GetComponent<FPSHandsWeapon> ();
			}

			if (!weaponsManager.weapons [2].activeInHierarchy) {
				for (int i = 0; i < weaponsManager.weapons.Length; i++) {
					weaponsManager.weapons [i].SetActive (false);
				}

				currentWeapon = null;
				weaponsManager.weapons [2].SetActive (true);
				currentWeapon = weaponsManager.weapons [2].GetComponent<FPSWeapon> ();

				playerAnimations.ChangeController (false);
			}
		}
	}
}
