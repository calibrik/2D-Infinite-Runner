using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RecordLabel : MonoBehaviour
{
    private TextMeshProUGUI _label;
    // Start is called before the first frame update
    private float _maxScore = 0;
    private void Start()
    {
        _maxScore = PlayerPrefs.GetFloat("maxScore", 0);
        _label=GetComponent<TextMeshProUGUI>();
        _label.text = $"Best record: {(int)_maxScore}m";
    }
    public void UpdateScore(float score)
    {
        if (score > _maxScore)
        {
            _maxScore = score;
            PlayerPrefs.SetFloat("maxScore", _maxScore);
        }
        _label.text = $"Best record: {(int)_maxScore}m";
    }
}
