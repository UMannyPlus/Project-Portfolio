using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Brick : MonoBehaviour {
    //Text for counter
    [SerializeField] private TextMeshProUGUI _text;
    private GameManager gameManager;
    //counter to check until when brick breaks
    public int BrickCounter { get; set; }
    //checks if brick will drop a power-up
    public bool IsSpecial { get; set; }

    private void Awake () {
        _text.text = BrickCounter.ToString ();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }

    //Sets the text and Color for bricks
    public void SetBrickAttribute () {
            SetText ();
            SetBrickColor ();
    }

    //Lowers the counter on each hit
    public void DecrementBrick()
    {
        BrickCounter--;
        if (BrickCounter <= 0)
        {
            gameManager.Score += 100;
            DestroyBrick();
        } else {
            gameManager.Score += 10;
            SetBrickAttribute();
        }
    }

    //Destroy the brick once counter = 0
    private void DestroyBrick()
    {
        gameManager.RemoveBrick(this.gameObject);
        Destroy(this.gameObject);
    }

    private void SetText () {
        _text.text = BrickCounter.ToString ();
    }

    //Sets the color based on the Brick Counter
    private void SetBrickColor () {
        var color = this.GetComponent<Renderer> ();
        switch (BrickCounter) {
            case 1:
                color.material.SetColor ("_BaseColor", Color.white);
                break;
            case 2:
                color.material.SetColor ("_BaseColor", Color.blue);
                break;
            case 3:
                color.material.SetColor ("_BaseColor", Color.yellow);
                break;
            case 4:
                color.material.SetColor ("_BaseColor", Color.green);
                break;
            case 5:
                color.material.SetColor ("_BaseColor", Color.cyan);
                break;
            case 6:
                color.material.SetColor ("_BaseColor", Color.magenta);
                break;
            case 7:
                color.material.SetColor ("_BaseColor", Color.red);
                break;
            case 8:
                color.material.SetColor ("_BaseColor", Color.black);
                break;

            default:
                break;
        }
    }
}