using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Updater : MonoBehaviour {
    [SerializeField] private GameManager _gameManager;
    private GameObject _startPanel;
    private GameObject _endPanel;
    private GameObject _uiPanel;
    private TextMeshProUGUI _scoreText;
    private TextMeshProUGUI _liveText;
    private TextMeshProUGUI _endScore;
    private int _state = 0;

    public int State {
        get {
            return _state;
        }
        set {
            _state = value;
        }
    }

    // Start is called before the first frame update
    void Awake () {
        _startPanel = this.transform.GetChild (0).gameObject;
        _endPanel = this.transform.GetChild (1).gameObject;
        _uiPanel = this.transform.GetChild (2).gameObject;

        _scoreText = _uiPanel.transform.GetChild (0).GetComponent<TextMeshProUGUI> ();
        _liveText = _uiPanel.transform.GetChild (1).GetComponent<TextMeshProUGUI> ();
        _endScore = _endPanel.transform.GetChild (2).GetComponent<TextMeshProUGUI> ();

        SetText ();

        StartCoroutine (UpdateUI ());
    }

    private IEnumerator UpdateUI () {
        while (true) {
            switch (_state) {
                case 0:
                    if (!_startPanel.activeInHierarchy)
                        _startPanel.SetActive (true);

                    break;
                case 1:
                    if (!_uiPanel.activeInHierarchy)
                        _uiPanel.SetActive (true);

                    SetText ();
                    if (_gameManager.Lives <= 0) {
                        _uiPanel.SetActive (false);
                        _state = 2;
                    }
                    break;
                case 2:
                    if (!_endPanel.activeInHierarchy)
                        _endPanel.SetActive (true);

                    _endScore.text = _scoreText.text;
                    break;

                default:
                    break;

            }
            yield return new WaitForSeconds (.1f);
        }
    }

    public void StartGame () {
        _startPanel.SetActive (false);
        _gameManager.GameRunning = true;
        _state = 1;
    }

    private void SetText () {
        _scoreText.text = "Score: " + _gameManager.Score.ToString ();
        _liveText.text = "Lives: " + _gameManager.Lives.ToString ();
    }

    public void RestartExtended () {
        _gameManager.RestartGame ();
        _endPanel.SetActive (false);
    }

    public void EndGame () {
        Application.Quit ();
    }
}