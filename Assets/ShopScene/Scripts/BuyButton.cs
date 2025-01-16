using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyButton : MonoBehaviour
{
    public int price;
    public string itemName;
    public MoneyLabel moneyLabel;
    public TextMeshProUGUI text;
    public TextMeshProUGUI poorErrorMessage;
    public float showPoorErrorMessageTime = 3;
    IEnumerator ShowPoorErrorMessage()
    {
        poorErrorMessage.gameObject.SetActive(true);
        yield return new WaitForSeconds(showPoorErrorMessageTime);
        poorErrorMessage.gameObject.SetActive(false);
    }
    public void BuyItem()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "XMLPlayerData/BoughtItems.xml");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File {filePath} does not exist");
            return;
        }
        if (moneyLabel.money<price)
        {
            StartCoroutine(ShowPoorErrorMessage());
            return;
        }
        moneyLabel.ChangeMoney(-price);
        XmlDocument xmlDoc= new XmlDocument();
        xmlDoc.Load(filePath);
        XmlNode root=xmlDoc.SelectSingleNode("items");
        XmlElement newItem=xmlDoc.CreateElement("item");
        newItem.SetAttribute("name", itemName);
        root.AppendChild(newItem);
        xmlDoc.Save(filePath);
        text.text = "Owned";
        GetComponent<Button>().interactable = false;
    }
}
