using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class Ball : MonoBehaviour {
    [SerializeField] private float _upForce = 150f; //force added when collision
    private Rigidbody _rb;
    private bool _IsStart; //A check for when ball is attached to platform
    private Rigidbody _platformRB; //Rigidbody of the platform
    private GameManager gameManager; //Main GameManager

    //Stop the ball from moving when the platform hits a wall
    public bool HitWall { get; set; }

    void Awake () {
        _rb = this.GetComponent<Rigidbody> (); //Set Rigidbody
        gameManager = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager> (); //Set GM
        _IsStart = gameManager.IsBallConnected; //Get bool from GM
    }

    void Update () {
        LaunchBall ();
    }

    void FixedUpdate () {
        CheckHit ();
    }

    void OnCollisionEnter (Collision other) {
        if (other.gameObject.tag == "Platform") 
        {
            //Set the RB
            _platformRB = other.gameObject.GetComponent<Rigidbody> ();

            //Check if platform is no longer connected
            if (!_IsStart) 
            {
                //On collision keep the xVelocity of the ball but reset rest
                _rb.velocity = new Vector3 (_rb.velocity.x + _platformRB.velocity.x, 0, 0);
                _rb.AddForce (Vector3.up * _upForce, ForceMode.Impulse);    //Add upwards force
            }
        } else if (other.gameObject.tag == "Brick") {
            //Decrement the brick counter
            other.gameObject.GetComponent<Brick> ().DecrementBrick ();
        }
    }

    //Launches the ball and separate it fom platform
    private void LaunchBall () {
        if (gameManager.GameRunning && _IsStart && Input.GetKeyDown (KeyCode.Space)) {
            _IsStart = gameManager.IsBallConnected = false;
            _rb.AddForce (Vector3.up * _upForce, ForceMode.Impulse);
        }
    }

    //Check if the platform hits a wall
    private void CheckHit () {
        if (_IsStart && !HitWall) {
            _rb.velocity = _platformRB.velocity;
        } else if (_IsStart && HitWall) {
            _rb.velocity = Vector3.zero;
        }
    }
}