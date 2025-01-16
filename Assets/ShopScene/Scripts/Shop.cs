using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public string xmlFilePath = "XML/XMLShopList/shopList.xml";
    public MoneyLabel moneyLabel;
    public TextMeshProUGUI poorErrorMessage;
    public GameObject itemNameLabel;
    public GameObject priceLabel;
    public GameObject buyButtton;
    public float offsetY = -64;
    public float initialY = -116;

    private float _currY;
    // Start is called before the first frame update
    void PrepareShop()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "XMLPlayerData/BoughtItems.xml");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File {filePath} does not exist");
            return;
        }
        XmlDocument boughtItems= new XmlDocument();
        boughtItems.Load(filePath);
        XmlNodeList items=boughtItems.SelectNodes("items/item");
        List<string> boughtItemsNames = new List<string>();
        foreach (XmlNode item in items) 
            boughtItemsNames.Add(item.Attributes["name"].Value);

        _currY = initialY;
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(Resources.Load<TextAsset>(xmlFilePath).text);
        XmlNodeList products = xmlDoc.SelectNodes("items/item");
        RectTransform thisTrans=GetComponent<RectTransform>();
        foreach (XmlNode product in products )
        {
            GameObject newProductNameLabel=Instantiate(itemNameLabel);
            RectTransform trans = newProductNameLabel.GetComponent<RectTransform>();
            trans.SetParent(thisTrans,false);
            trans.anchoredPosition = new Vector2(trans.anchoredPosition.x, _currY);
            string itemName = product.Attributes["name"].Value;
            newProductNameLabel.GetComponent<TextMeshProUGUI>().text = itemName;

            GameObject newPriceLabel = Instantiate(priceLabel);
            trans=newPriceLabel.GetComponent<RectTransform>();
            trans.SetParent(thisTrans, false);
            trans.anchoredPosition = new Vector2(trans.anchoredPosition.x,_currY);
            int price = int.Parse(product.Attributes["price"].Value);
            newPriceLabel.GetComponent<TextMeshProUGUI>().text = $"Price: {MoneyLabel.ShortForm(price)}";

            GameObject newBuyButton = Instantiate(buyButtton);
            trans=newBuyButton.GetComponent<RectTransform>();
            trans.SetParent(thisTrans, false);
            trans.anchoredPosition = new Vector2(trans.anchoredPosition.x, _currY);

            if (boughtItemsNames.Contains(itemName))
            {
                newBuyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Owned";
                newBuyButton.GetComponent<Button>().interactable = false;
            }
            else
            {
                BuyButton buyButtonComponent = newBuyButton.GetComponent<BuyButton>();
                buyButtonComponent.price = price;
                buyButtonComponent.itemName = itemName;
                buyButtonComponent.moneyLabel = moneyLabel;
                buyButtonComponent.poorErrorMessage = poorErrorMessage;
            }
            _currY += offsetY;
        }
        Resources.UnloadUnusedAssets();
    }
    void Start()
    {
        PrepareShop();
    }
    public void BackToGame()
    {
        SceneManager.LoadScene("MainScene");
    }
}
