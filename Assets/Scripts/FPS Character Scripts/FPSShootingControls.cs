using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FPSShootingControls : NetworkBehaviour {

	private Camera mainCam;

	private float fireRate = 15f, nextTimeToFire = 0f;

	[SerializeField]
	private GameObject concreteImpact;

	// Use this for initialization
	void Start () {
		mainCam = Camera.main;
	}

	// Update is called once per frame
	void Update () {
		Shoot ();
	}

	void Shoot() {
		if (Input.GetMouseButtonDown (0) && Time.time > nextTimeToFire) {
			nextTimeToFire = (Time.time + 1f) / fireRate;

			RaycastHit hit;

			if (Physics.Raycast (mainCam.transform.position, mainCam.transform.forward, out hit)) {
				Instantiate (concreteImpact, hit.point, Quaternion.LookRotation (hit.normal));
			}
		}
	}
}