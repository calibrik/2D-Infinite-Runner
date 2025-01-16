using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;

public class CoinParams
{
    public Vector2 pos;
}
public class SpikeParams
{
    public Vector2 pos;
    public float rotZ;
    public bool isRotating;
    public float rotSpeed;
}
/*public enum CoinBlockType
{
    cube2x2 = 0,
    rect2x4 = 1,
    rect6x2 = 2,
    rect6x4 = 3,
    cube6x6 = 4,
    rect10x6 = 5,
    rect12x6 = 6
}
public class CoinBlock
{
    public CoinBlockType type;
    public Vector2 pos;
    public float rotZ;
}*/
public class SectionParams
{
    public List<CoinParams> coins;
    public float length;
    public float endSectionTriggerPosX;
    public List<SpikeParams> spikes;
}
public class Main : MonoBehaviour
{
    public GameObject spikePreFab;
    public GameObject coinPreFab;
    public GameObject endSectionTriggerPreFab;
    public static Main S;
    public float initialGameSpeed = 5f;
    public TextMeshProUGUI scoreLabel;
    public TextMeshProUGUI speedLabel;
    public TextMeshProUGUI coinLabel;
    public float gapBetweenSections=20f;
    public float speedCap = 150000f;
    public float initialSpawnOffset = 50;
    public float metresForIncreaseScale = 1.05f;
    public int metresForIncrease = 100;
    public Transform spikesPool;
    public Transform coinsPool;
    public RecordLabel recordLabel;
    public MoneyLabel moneyLabel;
    public ButtonFunctions UI;
    public GameObject sectionPreFab;
    [HideInInspector]
    public bool isGameOver = false;

    //public int coinUpdateFrames = 3;
    //public ShopItems shopItems;
    public float score
    {
        get { return _score; }
        set {
            _score = value;
            scoreLabel.text = $"Metres past:{(int)_score}";
        }
    }
    public float gameSpeed
    {
        get { return _gameSpeed; }
        set {
            _gameSpeed = value;
            speedLabel.text = $"Speed:{Math.Round(_gameSpeed, 2)}";
            if (PlayerController.S) 
                PlayerController.S.animator.SetFloat("gameSpeed",value/speedCap);
        }
    }
    public int coins
    {
        get { return _coins; }
        set {
            coinLabel.text = $"Money collected: {MoneyLabel.ShortForm(value)}";
            _coins = value;
        }
    }
    protected int _amountSpikesInPool=0;
    private Transform _lastEndSectionTrigger=null;
    protected int _amountCoinsInPool=0;
    protected List<SectionParams> _sections=null;
    private int _coins =0;
    protected float _gameSpeed;
    protected int _stage = 1;
    private float _score = 0;
    private float _scoreOnLastIncrease = 0;

    void LoadXMLtoSection(TextAsset file)
    {
        if (_sections==null) _sections = new List<SectionParams>();
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(file.text);
        XmlNode item = xmlDoc.SelectSingleNode("section");
        SectionParams section = new SectionParams();
        section.spikes = new List<SpikeParams>();
        section.length = float.Parse(item.SelectSingleNode("length").InnerText);
        section.endSectionTriggerPosX = float.Parse(item.SelectSingleNode("endSectionTriggerPosX").InnerText);
        XmlNodeList spikesXML = item.SelectNodes("spikes/spike");
        foreach (XmlNode spikeXML in spikesXML)
        {
            SpikeParams spike = new SpikeParams();
            XmlNode pos = spikeXML.SelectSingleNode("pos");
            spike.pos = new Vector2(float.Parse(pos.Attributes["x"].Value), float.Parse(pos.Attributes["y"].Value));
            spike.rotZ = float.Parse(spikeXML.SelectSingleNode("rotZ").InnerText);
            spike.isRotating = bool.Parse(spikeXML.SelectSingleNode("isRotating").InnerText);
            spike.rotSpeed = float.Parse(spikeXML.SelectSingleNode("rotSpeed").InnerText);
            section.spikes.Add(spike);
        }
        section.coins = new List<CoinParams>();
        XmlNodeList coinsXML = item.SelectNodes("coins/coin");
        foreach (XmlNode coinXML in coinsXML)
        {
            CoinParams coin = new CoinParams();
            XmlNode pos = coinXML.SelectSingleNode("pos");
            coin.pos = new Vector2(float.Parse(pos.Attributes["x"].Value), float.Parse(pos.Attributes["y"].Value));
            section.coins.Add(coin);
        }
        if (_amountCoinsInPool < section.coins.Count*3) _amountCoinsInPool = section.coins.Count*3;
        if (_amountSpikesInPool < section.spikes.Count*3) _amountSpikesInPool = section.spikes.Count*3;
        _sections.Add(section);
    }
    void LoadAllXML()
    {
        TextAsset[] files = Resources.LoadAll<TextAsset>("XML/XMLSections");
        foreach (TextAsset file in files)
            LoadXMLtoSection(file);
        Resources.UnloadUnusedAssets();
    }
    protected void LoadSectionToScene(int index)
    {
        if (_sections==null|| _sections.Count == 0)
        {
            Debug.LogError("Nothing in sections parameters.");
            return;
        }
        float localZeroX;
        if (!_lastEndSectionTrigger)
            localZeroX = initialSpawnOffset + _sections[index].length - _sections[index].endSectionTriggerPosX;
        else
            localZeroX = _lastEndSectionTrigger.position.x + _sections[index].length - _sections[index].endSectionTriggerPosX+gapBetweenSections;

        GameObject section = Instantiate(sectionPreFab);
        GameObject endSectionTrigger = Instantiate(endSectionTriggerPreFab);
        _lastEndSectionTrigger = endSectionTrigger.transform;
        endSectionTrigger.transform.position = new Vector3(localZeroX + _sections[index].endSectionTriggerPosX, 5.53f, 0);
        EndSectionTrigger triggerComp= endSectionTrigger.GetComponent<EndSectionTrigger>();
        triggerComp.SetComponents(false);
        endSectionTrigger.transform.parent=section.transform;
        foreach (SpikeParams spike in _sections[index].spikes)
        {
            GameObject newSpike = spikesPool.GetChild(0).gameObject;
            newSpike.transform.parent = section.transform;
            newSpike.SetActive(true);
            newSpike.transform.position=new Vector3(spike.pos.x+localZeroX,spike.pos.y,0);
            newSpike.transform.eulerAngles = new Vector3(0, 0, spike.rotZ);
            Spike spikeComp=newSpike.GetComponent<Spike>();
            spikeComp.isRotating = spike.isRotating;
            spikeComp.rotSpeed = spike.rotSpeed;
            //spikeComp.pool = spikesPool;
            //spikeComp.SetComponents(false);
        }
        //int currFrameCount = 0;
        foreach (CoinParams coin in _sections[index].coins)
        {
            GameObject newCoin= coinsPool.GetChild(0).gameObject;
            newCoin.transform.parent = section.transform;
            newCoin.SetActive(true);
            newCoin.transform.position = new Vector3(coin.pos.x + localZeroX, coin.pos.y, 0);
            Coin coinComp = newCoin.GetComponent<Coin>();
            //coinComp.pool = coinsPool;
            //coinComp.SetComponents(false);
        }
    }
    protected void FillPool()
    {
        for (int i=0;i<_amountCoinsInPool;i++)
        {
            GameObject coin=Instantiate(coinPreFab);
            coin.transform.position = coinsPool.transform.position;
            coin.transform.parent = coinsPool.transform;
            Coin coinComp = coin.GetComponent<Coin>();
            coinComp.pool = coinsPool;
            coinComp.SetComponents(false);
            coin.SetActive(false);
        }
        for (int i=0;i<_amountSpikesInPool;i++)
        {
            GameObject spike =Instantiate(spikePreFab);
            spike.transform.position=spikesPool.transform.position;
            spike.transform.parent = spikesPool.transform;
            Spike spikeComp = spike.GetComponent<Spike>();
            spikeComp.pool = spikesPool;
            spikeComp.SetComponents(false);
            spike.SetActive(false);
        }
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (!S) S = this;
        else
        {
            Destroy(this);
            return;
        }

        gameSpeed = 0;
        LoadAllXML();
        FillPool();
        LoadSectionToScene(UnityEngine.Random.Range(0,_sections.Count));
        LoadSectionToScene(UnityEngine.Random.Range(0, _sections.Count));
        coinLabel.text = $"Money collected: 0";
    }
    
    public void GameStart()
    {
        PlayerController.S.StartCutscene();
        //StartCoroutine(PlayerController.S.StartCutscene());
    }
    public virtual void SectionEnded()
    {
        LoadSectionToScene(UnityEngine.Random.Range(0, _sections.Count));
    }
    public void CollectedCoin()
    {
        coins++;
    }
    public void ShowCasino()
    {
        UI.GameUItoCasinoUI();
    }
    virtual public void GameOver()
    {
        recordLabel.UpdateScore(score);
        moneyLabel.ChangeMoney(coins);
        gameSpeed = 0;
        UI.GameOverUI();
    }
    void IncreaseSpeed()
    {
        if (gameSpeed == speedCap) return;
        //print($"{(score / metresForIncrease + 1)} {(float)Math.Pow(2, 0.25f * (score / metresForIncrease + 1)) + initialGameSpeed - 1}");
        gameSpeed=Mathf.Min(Mathf.Pow(2,0.28f*_stage++)+initialGameSpeed-1,speedCap);
        metresForIncrease =(int)Mathf.Round(metresForIncrease* metresForIncreaseScale);
        _scoreOnLastIncrease = score;
    }
    // Update is called once per frame
    protected virtual void Update()
    {
        score += gameSpeed *Time.deltaTime;
        if (!isGameOver&&score-_scoreOnLastIncrease>= metresForIncrease) IncreaseSpeed();
    }
}
