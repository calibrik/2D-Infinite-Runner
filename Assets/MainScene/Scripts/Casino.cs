using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using TMPro;
using UnityEngine;
using UnityEngine.Apple;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class Casino : MonoBehaviour
{
    public ButtonFunctions canvas;
    //public RectTransform UI;
    public TextMeshProUGUI resultLabel;
    //public float resultLabelOffset = 0.85f;
    //public Vector2 casinoStartFinish;
    //public Vector2 casinoButtonStartFinish;
    public Button casinoButton;
    //public float UIRollTime=0.3f;

    public float[] cordsX;
    public GameObject[] slotPreFabs;
    public float[] chances;
    public string[] results;
    public float falseHopeChance = 0.4f;
    public float maxSpinTime;
    public float maxSpinSpeed;
    /*public float chanceOfNuke = 0.1f;
    public float chanceOfTripleBomb = 0.2f;
    public float chanceOfBomb = 0.3f;
    */

    public float firstPhaseTime = 3f;
    public float firstPhaseReducement = 0.9f;
    public float secondPhaseTime = 1.5f;
    public float extendedSecondPhaseTime = 4;

    public GameObject nukePreFab;
    public GameObject bombPreFab;
    public GameObject tripleBombPreFab;
    public float spawnResultPosY = 7f;

    public bool isMoving
    {
        get { return _isMoving; }
    }
    public RectTransform rect
    {
        get { return _rect; }
    }
    private RectTransform _rect;
    private int _columnsStopped = 0;
    private Column[] _columns;
    private int _result;
    private bool _isMoving = false;
    private GameObject _resultPreFab;

    public class Column
    {     
        public int finalSlot;
        public float currSpeed;
        public int slotsAmount=-1;
        public Casino casino;
        public float spawnOnPastY = 69.7f;
        public float delOnY = -259.4f;
        public float cordX;
        public float friction;
        public float timeToStop;
        public float slotHeight;
        public bool isMoving
        {
            get { return casino.isMoving; }
        }

        private bool _isStopping = false;
        //private Transform lastSpawnedSlot;
        private List<RectTransform> _slots;
        private RectTransform _finalSlot;
        private float _finalTargetPosY;
        
        public Column(Casino casino,float cordX)
        {
            _slots= new List<RectTransform>();
            this.cordX = cordX;
            this.casino= casino;
            RectTransform slot = casino.slotPreFabs[0].GetComponent<RectTransform>();
            slotHeight=slot.localScale.y* slot.rect.height;
            SpawnNewSlot(0, true);
            SpawnNewSlot(-slotHeight, true);
            SpawnNewSlot(slotHeight);
        }
        public void DeleteSlot()
        {
            _slots.RemoveAt(0);
        }
        public IEnumerator ApplyFriction()
        {
            friction = casino.firstPhaseReducement * currSpeed / casino.firstPhaseTime;
            float initialSpeed = currSpeed;
            while (currSpeed>(1-casino.firstPhaseReducement)*initialSpeed)
            {
                currSpeed -= Time.deltaTime * friction;
                yield return null;
            }

            RectTransform lastSpawnedSlot=_slots[^1];
            currSpeed = (1 - casino.firstPhaseReducement) * initialSpeed;
            float s = 0.5f * currSpeed * timeToStop;
            float preciseSlotsAmount = (s - lastSpawnedSlot.localPosition.y) / slotHeight;
            slotsAmount =Mathf.FloorToInt(preciseSlotsAmount);
            float randomDecimals = Random.Range(-0.49f, 0.49f);
            //float randomDecimals = 0;
            preciseSlotsAmount = slotsAmount+randomDecimals;
            _finalTargetPosY=-slotHeight * randomDecimals;
            s=preciseSlotsAmount*slotHeight +lastSpawnedSlot.localPosition.y;
            friction = (currSpeed * timeToStop - s) * 2 / Mathf.Pow(timeToStop, 2);

            _isStopping = true;
            while (currSpeed>0)
            {
                currSpeed -= Time.deltaTime * friction;
                yield return null;
            }
            currSpeed = 0;
            //Debug.Log($"left amount {slotsAmount}");
            //Debug.Log($"Supposed final slot {_finalSlot}, left amount {slotsAmount}, is Final slot crossing zero line {Mathf.Abs(_finalSlot.localPosition.y)<slotHeight/2}\n" +
            //$"final slot posY - target posY {_finalSlot.localPosition.y} {_finalTargetPosY} {_finalSlot.localPosition.y-_finalTargetPosY}");

            float offset = _finalTargetPosY - _finalSlot.localPosition.y;
            foreach (RectTransform slot in _slots)
                slot.Translate(0, offset, 0);

            _isStopping =false;
            casino.ColumnStopped();
        }
        public void SpawnNewSlot(float y,bool hasSpawned=false)
        {
            if (_isStopping) slotsAmount--;
            GameObject slot;
            Slot slotComp;
            RectTransform slotRect;
            if (slotsAmount == 0)
            {
                slot = Instantiate(casino.slotPreFabs[finalSlot], casino.rect);
                slotRect = slot.GetComponent<RectTransform>();
                _finalSlot = slotRect;
                friction = 0.5f * Mathf.Pow(currSpeed, 2) /( y - _finalTargetPosY);
            }
            else
            {
                slot = Instantiate(casino.slotPreFabs[Random.Range(0, casino.chances.Length)], casino.rect);
                slotRect = slot.GetComponent<RectTransform>();
            }
            slotComp = slot.GetComponent<Slot>();
            slotRect.localPosition = new Vector2(cordX, y);
            slotComp.column = this;
            slotComp.hasSpawned = hasSpawned;
            //lastSpawnedSlot = slot.transform;
            _slots.Add(slotRect);
        }
    }


    private void Start()
    {
        _rect = GetComponent<RectTransform>();
        _columns =new Column[3] {new Column(this, cordsX[0]),new Column(this, cordsX[1]),new Column(this, cordsX[2])};
        /*for (int i = 0; i < 3; i++)
        {
            _columns[i].cordX = cordsX[i];
            _columns[i].SpawnNewSlot(0, true);
            _columns[i].SpawnNewSlot(-0.5f, true);
            _columns[i].SpawnNewSlot(0.5f);
        }*/
        resultLabel.text = "";
    }
    public void ColumnStopped()
    {
        if (++_columnsStopped==3)
        {
            _isMoving= false;
            _columnsStopped = 0;
            StartCoroutine(ProcessResult());
        }
    }
    IEnumerator ProcessResult()
    {
        if (_result==5) resultLabel.text = $"Result: Nothing!";
        else resultLabel.text = $"Result: {results[_result]}!";

        switch (_result)
        {
            case 0:
                yield return new WaitForSeconds(3);
                _resultPreFab = nukePreFab;
                canvas.CasinoUItoGameUI();
                break;
            case 1:
                yield return new WaitForSeconds(3);
                _resultPreFab = tripleBombPreFab;
                canvas.CasinoUItoGameUI();
                break;
            case 2:
                yield return new WaitForSeconds(3);
                _resultPreFab = bombPreFab;
                canvas.CasinoUItoGameUI();
                break;
            case 3:
                casinoButton.interactable = true;
                break;
            case 4:
                Main.S.coins += 2000;
                yield return new WaitForSeconds(3);
                Main.S.GameOver();
                break;
            case 5:
                yield return new WaitForSeconds(3);
                Main.S.GameOver();
                break;
        }
    }
    public void SpawnResult()
    {
        /*if (!_resultPreFab)
        {
            Main.S.GameOver();
            return;
        }*/
        GameObject result = Instantiate(_resultPreFab);
        result.transform.position = new Vector3(0, spawnResultPosY, -1);
        //_resultPreFab = null;
    }
    void DetermineFinalRow()
    {
        int[] finalRow=null;
        float chance = Random.Range(0f, 1f);
        for (int i=0;i<chances.Length;i++)
        {
            if (chance <= chances[i])
            {
                finalRow = new int[3] { i, i, i };
                _result = i;
                break;
            }
            else chance -= chances[i];
        }
        if (finalRow == null) 
        {
            _result = chances.Length;
            chance = Random.Range(0f, 1f);
            if (chance <= falseHopeChance)
            {
                int slot = Random.Range(0, chances.Length);
                finalRow = new int[3] { slot, slot, Random.Range(0, chances.Length) };
                if (finalRow[2] == slot)
                {
                    if (--finalRow[2] < 0) finalRow[2] = chances.Length - 1;
                }
            }
            else
            {
                finalRow = new int[3] { Random.Range(0, chances.Length), Random.Range(0, chances.Length), Random.Range(0, chances.Length) };
                if (finalRow[0] == finalRow[1])
                {
                    if (--finalRow[1] < 0) finalRow[1] = chances.Length - 1;
                }
            }
            
        }
        for (int i = 0; i < 3; i++)
        {
            _columns[i].finalSlot = finalRow[i];
        }
    }
    void SetTimings()
    {
        for (int i = 0; i < 3; i++)
        {
            if (i == 2 && _columns[0].finalSlot == _columns[1].finalSlot)
                _columns[i].timeToStop = extendedSecondPhaseTime;
            else
                _columns[i].timeToStop = secondPhaseTime;
            _columns[i].currSpeed = maxSpinSpeed;
        }
    }
    IEnumerator Spin()
    {
        yield return new WaitForSeconds(maxSpinTime);
        foreach (Column column in _columns)
        {
            StartCoroutine(column.ApplyFriction());
            yield return new WaitForSeconds(firstPhaseTime+secondPhaseTime);
        }
    }
    public void OnPush()
    {
        resultLabel.text = "";
        casinoButton.interactable = false;
        DetermineFinalRow();
        SetTimings();
        _isMoving = true;
        StartCoroutine(Spin());
    }
}
