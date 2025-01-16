using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;


public class PlayerController : MonoBehaviour
{
    public static PlayerController S;
    public float jetPower = 5f;
    public bool isImmortal = false;
    public float g = 9;
    public float timeToReverseUp = 0.5f;
    public float gScaleWhenReleaseJet = 1.5f;

    public float minBounceHeight = 0.3f;
    public float surfaceFriction = 10;
    public float vReducementOnBounce = 0.1f;
    public float gameSpeedReducement = 0.02f;
    public float bombGameSpeedReducement = 0.02f;
    public float vToLeave = 10;
    public float bombMinHeight = 5.22f;

    public Vector2 boundsAfterDeath;

    public Animator animator;

    public Vector2 velocity
    {
        get { return _velocity; }
        set
        {
            _velocity = value;
            animator.SetFloat("velocityY", value.y);
        }
    }
    /*public bool isAlive
    {
        get { return _isAlive; }
        set { 
            _isAlive = value;
            animator.SetBool("isAlive", value);
        }
    }*/
    [HideInInspector]
    public bool isInCutscene = true;

    private bool _isReversingUp = false;
    private float _localJetPower;
    private Vector2 _velocity;
    private bool _isAlive = true;

    private Boundaries _bounds;

    void PrepareItems()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "XMLPlayerData");
        if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
        filePath = Path.Combine(filePath, "BoughtItems.xml");
        XmlDocument xmlDoc = new XmlDocument();
        if (!File.Exists(filePath))
        {
            XmlElement root = xmlDoc.CreateElement("items");
            xmlDoc.AppendChild(root);
            xmlDoc.Save(filePath);
            return;
        }
        xmlDoc.Load(filePath);
        XmlNodeList items = xmlDoc.SelectNodes("items/item");
        foreach (XmlNode item in items)
        {
            string itemName = item.Attributes["name"].Value;
            /*switch (itemName)
            {
                case "Magnet":
                    Transform itemComp = transform.Find("Magnet");
                    if (!itemComp) Debug.LogError("No Magnet item attached to Player");
                    else itemComp.gameObject.SetActive(true);
                    break;
                default:
                    Debug.LogError($"Unknown item name {itemName}");
                    break;
            }*/
            Transform itemComp = transform.Find(itemName);
            if (!itemComp) Debug.LogError($"No {itemName} item attached to Player");
            else itemComp.gameObject.SetActive(true);
        }
    }
    void Start()
    {
        if (!S) S = this;
        else
        {
            Destroy(this);
            return;
        }
        velocity = Vector2.zero;
        //_rigid = GetComponent<Rigidbody2D>();
        _bounds=GetComponent<Boundaries>();
        PrepareItems();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Spike"))
            OnSpikeHit();
    }
    public void OnSpikeHit()
    {
        if (isImmortal)
        {
            print($"Hit with Spike on {Main.S.score}m");
            return;
        }
        if (_isAlive)
        {
            _bounds.vertical = boundsAfterDeath;
            //transform.localEulerAngles = new Vector3(0, 0, -90);
            _isAlive = false;
            animator.SetBool("isUsingJetpack", false);
            animator.SetTrigger("isDead");
            Main.S.isGameOver = true;
            StartCoroutine(BounceInAfterlife(Main.S.ShowCasino));
        }
    }
    public void Explode(float initialV, float distance, Action ToDoAfterStop = null)
    {
        IEnumerator Explode(float initialV,float distance, Action ToDoAfterStop)
        {
            animator.SetBool("isSliding", false);
            velocity = Vector2.up*initialV;
            Main.S.gameSpeed = Mathf.Sqrt(2 * bombGameSpeedReducement * distance + Mathf.Pow(vToLeave, 2));
            isInCutscene = true;
            float s0 = Main.S.score;

            while (Main.S.score-s0<distance)
            {

                if (_bounds.isOnBottom)
                {
                    float bounceHeight = Mathf.Pow(velocity.y*(1-vReducementOnBounce), 2) / (2 * g);
                    if (bounceHeight > bombMinHeight)
                        velocity = Vector2.down * velocity.y * (1 - vReducementOnBounce);
                    else
                        velocity = Vector2.down * velocity.y;
                }
                if (_bounds.isOnTop)
                    velocity = Vector2.down * velocity.y * (1 - vReducementOnBounce);

                velocity = Vector2.up * (velocity.y - Time.deltaTime * g);
                Main.S.gameSpeed -= bombGameSpeedReducement * Time.deltaTime;
                //if (Main.S.gameSpeed<vToLeave) Main.S.gameSpeed = vToLeave;
                yield return null;
            }
            //gameSpeedReducement = bombGameSpeedReducement;
            StartCoroutine(BounceInAfterlife(ToDoAfterStop));
        }
        StartCoroutine(Explode(initialV,distance, ToDoAfterStop));
    }
    IEnumerator BounceInAfterlife(Action ToDoAfterStop=null)
    {
        isInCutscene = true;
        float bounceHeight;

        while (true)
        {
            if (_bounds.isOnBottom)
            {
                velocity = Vector2.down*velocity.y*(1-vReducementOnBounce);
                bounceHeight = Mathf.Pow(velocity.y, 2) / (2 * g);
                if (bounceHeight <= minBounceHeight) break;
            }
            if (_bounds.isOnTop)
            {
                velocity = Vector2.down * velocity.y * (1 - vReducementOnBounce);
            }
            Main.S.gameSpeed-=gameSpeedReducement*Time.deltaTime;
            if (Main.S.gameSpeed < 0) Main.S.gameSpeed = 0;
            velocity = Vector2.up * (velocity.y - Time.deltaTime * g);
            yield return null;
        }
        animator.SetBool("isSliding",true);
        velocity = Vector2.zero;
        transform.position = new Vector2(transform.position.x, _bounds.vertical.x);
        while (true)
        {
            Main.S.gameSpeed -= surfaceFriction * Time.deltaTime;
            if (Main.S.gameSpeed <= 0) break;
            yield return null;
        }
        Main.S.gameSpeed = 0;
        ToDoAfterStop();
    }
    
    public void StartCutscene()
    {
        _bounds.isActive = false;
        animator.Play("PlayerStart");
    }
    public void StartGame()
    {
        _bounds.isActive=true;
        transform.position=new Vector2(0,_bounds.vertical.x);
        isInCutscene = false;
        Main.S.gameSpeed = Main.S.initialGameSpeed;
    }
    void Update()
    {
        animator.SetBool("isOnBottom",_bounds.isOnBottom);
        animator.SetBool("isOnTop", _bounds.isOnTop);
        if (isInCutscene)
        {
            transform.Translate(velocity*Time.deltaTime,Space.World);
            return;
        }
        if (_bounds.isOnTop || _bounds.isOnBottom)
            velocity = Vector2.zero;
        if (Input.GetKey(KeyCode.Space))
        {
            animator.SetBool("isUsingJetpack", true);
            if (!_isReversingUp && velocity.y < 0)
            {
                _localJetPower = (-velocity.y) / timeToReverseUp + g;
                _isReversingUp = true;
            }

            if (_isReversingUp)
            {
                velocity = Vector2.up * (velocity.y + Time.deltaTime * _localJetPower - Time.deltaTime * g);
                if (velocity.y >= 0) _isReversingUp = false;
            }
            else
                velocity = Vector2.up * (velocity.y + Time.deltaTime * jetPower - Time.deltaTime * g);
        }
        else
        {
            animator.SetBool("isUsingJetpack", false);
            _isReversingUp = false;
            if (velocity.y > 0) velocity = Vector2.up * (velocity.y - Time.deltaTime * gScaleWhenReleaseJet * g);
            else velocity = Vector2.up * (velocity.y - Time.deltaTime * g);
        }
        transform.Translate(velocity*Time.deltaTime,Space.World);
    }
}
