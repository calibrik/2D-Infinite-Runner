#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEditorWindow : EditorWindow
{
    private GameObject _endTriggerSectionPreFab;
    private GameObject _spikePreFab;
    private GameObject _coinPreFab;
    private string _fileNameToSave="";
    private TextAsset _fileToLoad;
    //private SectionParams _sectionToLoad;
    void OnGUI()
    {
        if (SceneManager.GetActiveScene().name!="LevelEditor")
        {
            GUILayout.Label("Wrong scene opened, should be LevelEditor");
            return;
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("End of section trigger preFab: ", EditorStyles.boldLabel);
        _endTriggerSectionPreFab = EditorGUILayout.ObjectField(_endTriggerSectionPreFab, typeof(GameObject), false, GUILayout.ExpandWidth(true)) as GameObject;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Spike preFab: ", EditorStyles.boldLabel);
        _spikePreFab = EditorGUILayout.ObjectField(_spikePreFab, typeof(GameObject), false, GUILayout.ExpandWidth(true)) as GameObject;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Coin preFab: ", EditorStyles.boldLabel);
        _coinPreFab = EditorGUILayout.ObjectField(_coinPreFab, typeof(GameObject), false, GUILayout.ExpandWidth(true)) as GameObject;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Drag and drop XML File:", EditorStyles.boldLabel);
        _fileToLoad=EditorGUILayout.ObjectField(_fileToLoad, typeof(TextAsset),false,GUILayout.ExpandWidth(true)) as TextAsset;
        Rect area=GUILayoutUtility.GetLastRect();
        GUILayout.EndHorizontal();
        if (area.Contains(Event.current.mousePosition))
        {
            switch (Event.current.type)
            {
                case EventType.DragPerform:
                    HandleDragPerform();
                    break;
                case EventType.Repaint:
                    HandleDragUpdated();
                    break;
            }
        }
        if (GUILayout.Button("Load section from XML", GUILayout.ExpandWidth(false)))
            LoadXMLtoScene();

        /*GUILayout.BeginHorizontal();
        GUILayout.Label("Drag and drop Asset File:", EditorStyles.boldLabel);
        _sectionToLoad = EditorGUILayout.ObjectField(_sectionToLoad, typeof(SectionParams), false, GUILayout.ExpandWidth(true)) as SectionParams;
        Rect area = GUILayoutUtility.GetLastRect();
        GUILayout.EndHorizontal();
        if (area.Contains(Event.current.mousePosition))
        {
            switch (Event.current.type)
            {
                case EventType.DragPerform:
                    HandleDragPerform();
                    break;
                case EventType.Repaint:
                    HandleDragUpdated();
                    break;
            }
        }
        if (GUILayout.Button("Load section from Asset", GUILayout.ExpandWidth(false)))
            LoadSectionFromAsset();*/
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Enter file name:", EditorStyles.boldLabel);
        _fileNameToSave = EditorGUILayout.TextField(_fileNameToSave, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Save section to XML", GUILayout.ExpandWidth(false)))
            SaveXMLFromScene();
        
        GUILayout.Space(20);

        if (GUILayout.Button("Clear scene",GUILayout.ExpandWidth(false)))
            ClearScene();
        /*if (GUILayout.Button("XML to Obj"))
            XMLtoObj();*/
        }
    /*void XMLtoObj()
    {
        if (!_fileToLoad)
        {
            Debug.LogError("No file has been chosen.");
            return;
        }
        SectionParams section=CreateInstance<SectionParams>();
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(_fileToLoad.text);
        XmlNode item = xmlDoc.SelectSingleNode("section");
        section.endSectionTriggerPosX = float.Parse(item.SelectSingleNode("endSectionTriggerPosX").InnerText);
        XmlNodeList spikesXML = item.SelectNodes("spikes/spike");
        section.spikes=new List<SpikeParams>();
        foreach (XmlNode spikeXML in spikesXML)
        {
            SpikeParams spikeParam = new SpikeParams();
            XmlNode pos = spikeXML.SelectSingleNode("pos");
            spikeParam.pos = new Vector2(float.Parse(pos.Attributes["x"].Value), float.Parse(pos.Attributes["y"].Value));
            spikeParam.rotZ = float.Parse(spikeXML.SelectSingleNode("rotZ").InnerText);
            spikeParam.isRotating = bool.Parse(spikeXML.SelectSingleNode("isRotating").InnerText);
            spikeParam.rotSpeed = float.Parse(spikeXML.SelectSingleNode("rotSpeed").InnerText);
            section.spikes.Add(spikeParam);
        }
        XmlNodeList coinsXML = item.SelectNodes("coins/coin");
        section.coins = new List<CoinParams>();
        foreach (XmlNode coinXML in coinsXML)
        {
            CoinParams coinParam = new CoinParams();
            XmlNode pos = coinXML.SelectSingleNode("pos");
            coinParam.pos = new Vector2(float.Parse(pos.Attributes["x"].Value), float.Parse(pos.Attributes["y"].Value));
            section.coins.Add(coinParam);
        }
        section.length = float.Parse(item.SelectSingleNode("length").InnerText);
        Debug.Log($"Transfered {_fileToLoad.name}");
        AssetDatabase.CreateAsset(section, $"Assets/Resources/XML/SectionsAssets/{_fileToLoad.name}.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        _fileToLoad = null;
    }*/
    void ClearScene()
    {
        GameObject endSectionTrigger = GameObject.FindWithTag("EndSectionTrigger");
        if (endSectionTrigger) DestroyImmediate(endSectionTrigger);

        GameObject[] spikes = GameObject.FindGameObjectsWithTag("Spike");
        if (spikes.Length > 0)
            foreach (GameObject spike in spikes) DestroyImmediate(spike); 
        
        GameObject[] coinBlocks = GameObject.FindGameObjectsWithTag("CoinBlock");
        if (coinBlocks.Length > 0)
            foreach (GameObject coinBlock in coinBlocks) DestroyImmediate(coinBlock);

        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        if (coins.Length > 0)
            foreach (GameObject coin in coins) DestroyImmediate(coin);

        Debug.Log("Scene cleared.");
    }
    void LoadXMLtoScene()
    {
        if (!_coinPreFab||!_spikePreFab||!_coinPreFab)
        {
            Debug.LogError("Set all preFabs.");
            return;
        }
        if (!_fileToLoad)
        {
            Debug.LogError("No file has been chosen.");
            return;
        }
        ClearScene();
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(_fileToLoad.text);
        XmlNode item = xmlDoc.SelectSingleNode("section");
        float endSectionTriggerPosX = float.Parse(item.SelectSingleNode("endSectionTriggerPosX").InnerText);
        GameObject endSectionTrigger = PrefabUtility.InstantiatePrefab(_endTriggerSectionPreFab) as GameObject;
        endSectionTrigger.transform.position = new Vector2(endSectionTriggerPosX, 5.53f);
        XmlNodeList spikesXML = item.SelectNodes("spikes/spike");
        foreach (XmlNode spikeXML in spikesXML)
        {
            GameObject spike = PrefabUtility.InstantiatePrefab(_spikePreFab) as GameObject;
            XmlNode pos = spikeXML.SelectSingleNode("pos");
            spike.transform.position = new Vector2(float.Parse(pos.Attributes["x"].Value), float.Parse(pos.Attributes["y"].Value));
            spike.transform.localEulerAngles = new Vector3(0, 0, float.Parse(spikeXML.SelectSingleNode("rotZ").InnerText));
            Spike spikeComp = spike.GetComponent<Spike>();
            spikeComp.isRotating = bool.Parse(spikeXML.SelectSingleNode("isRotating").InnerText);
            spikeComp.rotSpeed = float.Parse(spikeXML.SelectSingleNode("rotSpeed").InnerText);
        }
        XmlNodeList coinsXML = item.SelectNodes("coins/coin");
        foreach (XmlNode coinXML in coinsXML)
        {
            GameObject coin = PrefabUtility.InstantiatePrefab(_coinPreFab) as GameObject;
            XmlNode pos = coinXML.SelectSingleNode("pos");
            coin.transform.position = new Vector2(float.Parse(pos.Attributes["x"].Value), float.Parse(pos.Attributes["y"].Value));
        }
        Debug.Log($"Loaded {_fileToLoad.name}");
        _fileToLoad = null;
    }
    /*void LoadSectionFromAsset()
    {
        if (!_coinPreFab || !_spikePreFab || !_coinPreFab)
        {
            Debug.LogError("Set all preFabs.");
            return;
        }
        if (!_sectionToLoad)
        {
            Debug.LogError("No file has been chosen.");
            return;
        }
        ClearScene();
        GameObject endSectionTrigger = PrefabUtility.InstantiatePrefab(_endTriggerSectionPreFab) as GameObject;
        endSectionTrigger.transform.position = new Vector2(_sectionToLoad.endSectionTriggerPosX, 5.53f);
        foreach (SpikeParams spikeParam in _sectionToLoad.spikes)
        {
            GameObject spike = PrefabUtility.InstantiatePrefab(_spikePreFab) as GameObject;
            spike.transform.position = spikeParam.pos;
            spike.transform.localEulerAngles =new Vector3(0,0,spikeParam.rotZ);
            Spike spikeComp = spike.GetComponent<Spike>();
            spikeComp.isRotating = spikeParam.isRotating;
            spikeComp.rotSpeed = spikeParam.rotSpeed;
        }
        foreach (CoinParams coinParam in _sectionToLoad.coins)
        {
            GameObject coin = PrefabUtility.InstantiatePrefab(_coinPreFab) as GameObject;
            coin.transform.position = coinParam.pos;
        }
        Debug.Log($"Loaded {_sectionToLoad.name}");
        _sectionToLoad = null;
    }*/
    void SaveXMLFromScene()
    {
        if (_fileNameToSave=="")
        {
            Debug.LogError("Enter file name.");
            return;
        }
        
        GameObject[] spikes = GameObject.FindGameObjectsWithTag("Spike");
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        if (spikes.Length == 0 && coins.Length == 0)
        {
            Debug.LogError("No objects in section");
            return;
        }

        GameObject endSectionTrigger = GameObject.FindWithTag("EndSectionTrigger");
        if (!endSectionTrigger)
        {
            Debug.LogError("No end of section trigger was found.");
            return;
        }
        XmlDocument xmlDocument = new XmlDocument();
        XmlElement section = xmlDocument.CreateElement("section");
        xmlDocument.AppendChild(section);
        float endSectionTriggerPosX = endSectionTrigger.transform.position.x;
        XmlElement endSectionTriggerPos = xmlDocument.CreateElement("endSectionTriggerPosX");
        endSectionTriggerPos.InnerText = endSectionTriggerPosX.ToString();
        section.AppendChild(endSectionTriggerPos);

        XmlElement spikesXML = xmlDocument.CreateElement("spikes");
        float minX = float.MaxValue;
        foreach (GameObject spike in spikes)
        {
            XmlElement spikeXML = xmlDocument.CreateElement("spike");
            XmlElement posXML = xmlDocument.CreateElement("pos");
            if (spike.transform.position.x < minX) minX = spike.transform.position.x;
            posXML.SetAttribute("x", spike.transform.position.x.ToString());
            posXML.SetAttribute("y", spike.transform.position.y.ToString());
            XmlElement rotZXML = xmlDocument.CreateElement("rotZ");
            rotZXML.InnerText = spike.transform.rotation.eulerAngles.z.ToString();
            Spike spikeComp = spike.GetComponent<Spike>();
            XmlElement isRotatingXML = xmlDocument.CreateElement("isRotating");
            isRotatingXML.InnerText = spikeComp.isRotating.ToString();
            XmlElement rotSpeedXML = xmlDocument.CreateElement("rotSpeed");
            rotSpeedXML.InnerText = spikeComp.rotSpeed.ToString();
            spikeXML.AppendChild(posXML);
            spikeXML.AppendChild(rotZXML);
            spikeXML.AppendChild(isRotatingXML);
            spikeXML.AppendChild(rotSpeedXML);
            spikesXML.AppendChild(spikeXML);

        }
        section.AppendChild(spikesXML);

        XmlElement coinsXML = xmlDocument.CreateElement("coins");
        foreach (GameObject coin in coins)
        {
            XmlElement coinXML = xmlDocument.CreateElement("coin");
            XmlElement posXML = xmlDocument.CreateElement("pos");
            if (coin.transform.position.x < minX) minX = coin.transform.position.x;
            posXML.SetAttribute("x", coin.transform.position.x.ToString());
            posXML.SetAttribute("y", coin.transform.position.y.ToString());
            coinXML.AppendChild(posXML);
            coinsXML.AppendChild(coinXML);
        }
        section.AppendChild(coinsXML);

        XmlElement lengthXML = xmlDocument.CreateElement("length");
        lengthXML.InnerText = (endSectionTriggerPosX - minX).ToString();
        section.AppendChild(lengthXML);
        if (File.Exists($"Assets/Resources/XML/XMLSections/{_fileNameToSave}.xml"))
            File.Delete($"Assets/Resources/XML/XMLSections/{_fileNameToSave}.xml");
        xmlDocument.Save($"Assets/Resources/XML/XMLSections/{_fileNameToSave}.xml");
        Debug.Log($"Saved to Assets/Resources/XML/XMLSections/{_fileNameToSave}.xml");
        _fileNameToSave = "";
    }
    void HandleDragPerform()
    {
        if (DragAndDrop.objectReferences.Length != 1) return;
        string path = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0]);
        if (string.IsNullOrEmpty(path) || (!path.EndsWith(".xml") && !path.EndsWith(".asset"))) 
            return;
        DragAndDrop.AcceptDrag();
    }
    void HandleDragUpdated()
    {
        if (DragAndDrop.objectReferences.Length != 1) return;
        string path = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0]);
        if (string.IsNullOrEmpty(path) || (!path.EndsWith(".xml") && !path.EndsWith(".asset")))
            DragAndDrop.visualMode = DragAndDropVisualMode.None;
        else
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
    }

    [MenuItem("Custom Tools/Level Editor Window")]
    public static void ShowWindow()
    {
        LevelEditorWindow window = GetWindow<LevelEditorWindow>();
        window.titleContent = new GUIContent("Level Editor Window");
        window.Show();
    }
}
#endif