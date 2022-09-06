using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Create_Level : MonoBehaviour {
    [SerializeField] private int _xLength; //length of array
    [SerializeField] private int _yLength; //height of array
    [SerializeField] private GameObject _brick; //brick prefab
    [SerializeField] private GameManager _gameManager;  //The GM
    private int[,] brickArray;  //2D brick array

    // Start is called before the first frame update
    void Start () {
        brickArray = new int[_xLength, _yLength];
        InitializeBricks ();
    }

    //Randomize the array to get the future BrickCounters
    private void RandomizeArray () {
        for (int i = 0; i < brickArray.GetLength (0); i++) {
            for (int j = 0; j < brickArray.GetLength (1); j++) {
                //Random value based on current level with Min = 0 Max = 8
                brickArray[i, j] = Mathf.Clamp (Random.Range (0, 3 + _gameManager.Level), 0, 8); //Can't go higher than 8
            }
        }
    }

    //Creates the bricks in the scene
    public void InitializeBricks () {
        //Randomizes the Array
        RandomizeArray ();

        //Creates the Bricks
        for (int i = 0; i < brickArray.GetLength (0); i++) {
            for (int j = brickArray.GetLength (1) - 1; j >= 0; j--) {
                if (brickArray[i, j] == 0) {    //If the counter is 0 leave an empty space
                    continue;
                }

                GameObject brick = Instantiate (_brick, new Vector3 ((i * 5) - 13f, (j * 3) + 6.5f, 0f), Quaternion.identity);
                _gameManager.AddBrick(brick); //add brick to list in GM

                brick.GetComponent<Brick> ().BrickCounter = brickArray[i, j]; //set the counter
                brick.GetComponent<Brick> ().SetBrickAttribute ();  //Instantiate brick attrib
            }

        }
    }

}