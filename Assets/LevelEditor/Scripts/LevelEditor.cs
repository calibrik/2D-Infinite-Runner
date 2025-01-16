#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LevelEditor : Main
{
    private int _sectionsOnThatSpeedCompleted = 0;
    //private float _originalGapBetweenSections;
    private int _sectionsSpawnedOnSameSpeed;
    // Start is called before the first frame update
    protected override void Start()
    {
        if (!S) S = this;
        gameSpeed = initialGameSpeed;
        PlayerController.S.isInCutscene = false;
        coinLabel.text = $"Money collected: 0";
        SaveSectionFromScene();
        FillPool();
        LoadSectionToScene(0);
        LoadSectionToScene(0);
        _sectionsSpawnedOnSameSpeed = 2;
    }
    public override void SectionEnded()
    {
        if (gameSpeed != speedCap && _sectionsSpawnedOnSameSpeed == 2)
        {
            float originalGapBetweenSections = gapBetweenSections;
            gapBetweenSections *= 2;
            LoadSectionToScene(0);
            _sectionsSpawnedOnSameSpeed = 1;
            gapBetweenSections = originalGapBetweenSections;
        }
        else
        {
            LoadSectionToScene(0);
            _sectionsSpawnedOnSameSpeed++;
        }
        _sectionsOnThatSpeedCompleted++;
        if (_sectionsOnThatSpeedCompleted == 2)
        {
            _sectionsOnThatSpeedCompleted = 0;
            gameSpeed = initialGameSpeed * ++_stage;
        }
    }
    public override void GameOver()
    {
        gameSpeed = 0;
    }
    void SaveSectionFromScene()
    {
        
        GameObject[] spikes = GameObject.FindGameObjectsWithTag("Spike");
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        if (spikes.Length ==0 && coins.Length==0) 
        {
            Debug.LogError("No objects in section");
            return;
        }

        GameObject endSectionTrigger = GameObject.FindWithTag("EndSectionTrigger");
        if (!endSectionTrigger)
        {
            foreach (GameObject spike in spikes) Destroy(spike);
            foreach (GameObject coin in coins) Destroy(coin);
            Debug.LogError("No end of section trigger was found.");
            return;
        }
        _sections = new List<SectionParams>();
        SectionParams section = new SectionParams();
        section.endSectionTriggerPosX = endSectionTrigger.transform.position.x;
        Destroy(endSectionTrigger);

        section.spikes = new List<SpikeParams>();
        float minX = float.MaxValue;
        foreach (GameObject spike in spikes)
        {
            SpikeParams spikeParam = new SpikeParams();
            spikeParam.pos = spike.transform.position;
            if (spike.transform.position.x < minX)
                minX = spike.transform.position.x;
            spikeParam.rotZ = spike.transform.localEulerAngles.z;
            Spike spikeComp = spike.GetComponent<Spike>();
            spikeParam.isRotating = spikeComp.isRotating;
            spikeParam.rotSpeed = spikeComp.rotSpeed;
            Destroy(spike);
            section.spikes.Add(spikeParam);
        }
        section.coins = new List<CoinParams>();
        foreach (GameObject coin in coins)
        {
            CoinParams coinParam = new CoinParams();
            coinParam.pos = coin.transform.position;
            if (coin.transform.position.x < minX)
                minX = coin.transform.position.x;
            Destroy(coin);
            section.coins.Add(coinParam);
        }
        section.length = section.endSectionTriggerPosX - minX;
        _amountCoinsInPool = section.coins.Count * 3;
        _amountSpikesInPool = section.spikes.Count * 3;
        _sections.Add(section);
        GameObject[] coinBlocks = GameObject.FindGameObjectsWithTag("CoinBlock");
        if (coinBlocks.Length > 0)
            foreach (GameObject coinBlock in coinBlocks) Destroy(coinBlock);
    }
    
    // Update is called once per frame
    protected override void Update()
    {
        score += gameSpeed * Time.deltaTime;
    }
}
#endif