using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameController _gameController = null;
    [SerializeField] private Text _depthText = null;
    [SerializeField] private InputField _nameField = null;

    private void Update()
    {
        _depthText.text = $"DEPTH: {_gameController.Depth}";
    }

    public void SubmitScore()
    {
        FindObjectOfType<LeaderboardHandler>().SetScore(_nameField.text, _gameController.Depth);
        FindObjectOfType<Transitioner>().TransitionTo("Menu");
    }
}
