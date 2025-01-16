using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MoneyLabel : MonoBehaviour
{
    public int money
    {
        get { return _money; }
    }

    private TextMeshProUGUI _label;
    // Start is called before the first frame update
    private int _money = 0;
    private void Start()
    {
        _money = PlayerPrefs.GetInt("moneyAmount", 0);
        _label = GetComponent<TextMeshProUGUI>();
        _label.text = $"Money: {ShortForm(_money)}";
    }
    public void ChangeMoney(int money)
    {
        _money += money;
        PlayerPrefs.SetInt("moneyAmount", _money);
        _label.text=$"Money: {ShortForm(_money)}";
    }
    static public string ShortForm(int money)
    {
        if (money<1000) return money.ToString();
        return $"{(float)(money / 10) / 100}k";
    }
}
