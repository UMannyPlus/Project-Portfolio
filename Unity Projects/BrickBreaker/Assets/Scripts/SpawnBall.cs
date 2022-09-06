using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBall : MonoBehaviour
{
    [SerializeField] private GameObject _ball;
    [SerializeField] private GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        InstantiateBall();
    }
    public void InstantiateBall()
    {
        Instantiate(_ball, this.GetComponent<Transform>().position, Quaternion.identity);
    }
}
