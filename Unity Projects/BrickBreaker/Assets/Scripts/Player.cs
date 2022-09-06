using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class Player : MonoBehaviour {
    private Rigidbody _rb;
    private Ball _ball;
    private bool _hitWall;
    private Vector3 input;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private GameManager gameManager;

    // Start is called before the first frame update
    void Awake () {
        _rb = this.GetComponent<Rigidbody> ();
        gameManager.InitPosition = this.transform.position; //Stores the platform in GM
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (gameManager.Lives > 0 && gameManager.GameRunning) {
            //Store user input as movement vector
            input = new Vector3 (Input.GetAxis ("Horizontal"), 0, 0);
        }else{
            input = Vector3.zero;
            this.transform.position = gameManager.InitPosition;
        }

        //Stops the player from moving through the wall
        if (_hitWall) {
            _rb.velocity = Vector3.zero;
        } else {
            _rb.velocity = input * _speed;
        }

    }

    void OnCollisionEnter (Collision other) {
        if (other.gameObject.tag == "Player") {
            _ball = other.gameObject.GetComponent<Ball> ();
        } else if (other.gameObject.tag == "Wall") {
            _ball.HitWall = true; //Stops the ball from moving if the player isn't
        }
    }
    void OnCollisionExit (Collision other) {
        if (other.gameObject.tag == "Wall") {
            _ball.HitWall = false; //restarts movement
        }
    }
}