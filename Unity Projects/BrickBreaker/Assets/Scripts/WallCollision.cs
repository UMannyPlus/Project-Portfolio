using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCollision : MonoBehaviour {
    [SerializeField, Range (0, 1)] private int _wallPOS; //Set lft or right wall in Inspector
    [SerializeField] private float _force = 25f; //Force given by wall

    void OnCollisionEnter (Collision other) {
        if (other.gameObject.tag == "Player") {
            Rigidbody otherRb = other.gameObject.GetComponent<Rigidbody> ();
            switch (_wallPOS) {
                case 0:
                    otherRb.AddForce (Vector3.right * _force, ForceMode.Impulse);
                    break;

                case 1:
                    otherRb.AddForce (Vector3.left * _force, ForceMode.Impulse);
                    break;
            }
        }
    }

}