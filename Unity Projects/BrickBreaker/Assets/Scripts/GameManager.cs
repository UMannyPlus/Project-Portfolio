using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private int _level; //The current level
    [SerializeField] private int _lives; //Current amount of lives
    [SerializeField] private int _score; //Player Score
    [SerializeField] private SpawnBall _ballSpawner;
    [SerializeField] private Create_Level _levelCreator;
    [SerializeField] private UI_Updater _ui;
    //List that stores the bricks on the level
    private List<GameObject> _brickList;

    //Boolean to keep track if ball is connected to the platform
    public bool IsBallConnected { get; set; }

    //Platform initial position
    public Vector3 InitPosition { get; set; }

    public bool GameRunning { get; set; }

    //Properties for _level to be called out of script
    public int Level {
        get {
            return _level;
        }
        set {
            _level = value;
        }
    }

    //properties for _lives to be called out of script
    public int Lives {
        get {
            return _lives;
        }
        set {
            _lives = value;
        }

    }

    //Properties for the _score to be called out of script
    public int Score {
        get {
            return _score;
        }
        set {
            _score = value;
        }
    }

    void Awake () {
        IsBallConnected = true;
        _brickList = new List<GameObject> (); //Instantiate the list of Bricks
    }

    private void Start () {
        StartCoroutine (TransitionLevel ());
    }

    private IEnumerator TransitionLevel () {
        while (true) {
            if (_brickList.Count == 0 && _level < 5) {

                yield return new WaitForSeconds (2.0f);
                GoToNextLevel ();
            }
            yield return new WaitForSeconds (.1f);

        }
    }

    //Simple add bricks to list
    public void AddBrick (GameObject gameObject) {
        _brickList.Add (gameObject);
    }

    //Simple remove bricks from list
    public void RemoveBrick (GameObject gameObject) {
        _brickList.Remove (gameObject);
    }

    public void DestroyAllBricks () {
        foreach (GameObject brick in _brickList) {
            Destroy (brick);
        }
        _brickList.Clear ();
    }

    //If the ball goes out-of-bounds the player loses a life
    public void DecrementLife () {
        //Lose a life
        _lives--;
        if (_lives <= 0) {
            //end the game
            //show score
        } else {
            //Make sure the ball is connected to platform
            IsBallConnected = true;
            _ballSpawner.InstantiateBall (); //spawn the ball
        }
    }

    //Once all the bricks in the current level are destroyed 
    //transition to the next level (Max 5)
    public void GoToNextLevel () {

        //Increment level
        Level++;
        //destory the ball in the scene
        GameObject[] ballList = GameObject.FindGameObjectsWithTag ("Player");
        foreach (GameObject ball in ballList) {
            Destroy (ball);
        }
        //Create level and instantiated ball
        CreateLevel();

    }

    public void RestartGame () {
        //Destroy all the bricks in current scene
        DestroyAllBricks ();
        //reset lives, level, and score
        _lives = 3;
        _level = 1;
        _score = 0;
        //rebuild first level
        CreateLevel();
        //set the gamestate and GameRunning back to working
        _ui.State = 1;
        GameRunning = true;
    }

    //Helps recreate levels and instantiates ball to platform
    private void CreateLevel () {
        //Constrain ball to platform
        IsBallConnected = true;
        //reset platform position
        GameObject platform = GameObject.FindGameObjectWithTag ("Platform");
        platform.transform.position = InitPosition;
        //spawn in new ball
        _ballSpawner.InstantiateBall ();
        //Create bricks
        _levelCreator.InitializeBricks ();
    }
}