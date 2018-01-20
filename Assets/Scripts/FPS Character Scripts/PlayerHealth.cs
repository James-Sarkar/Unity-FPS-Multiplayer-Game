using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerHealth : NetworkBehaviour {

	[SyncVar]
	public float health = 100f;

	public void TakeDamage (float damage) {
		if (!isServer) {
			return;
		}

		health -= damage;

		if (health <= 0f) {
			// End game
		}
	}
}