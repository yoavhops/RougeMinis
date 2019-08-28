using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Devdog.InventoryPro;

public class InvetoryPlayers : MonoBehaviour
{
    public static InvetoryPlayers Singleton;
    public Dictionary<int, int> ConnectPrefabIdToIndex = new Dictionary<int, int>();
    public List<InventoryPlayer> InventoryPlayerList;

    void Awake()
    {
        Singleton = this;
        gameObject.AddComponent<DontDestroyOnLoad>();
    }

    public void Connect(int prefabID, int index)
    {
        ConnectPrefabIdToIndex[prefabID] = index;
    }

    public InventoryPlayer GetInventoryPlayer(int prefabID)
    {
        return InventoryPlayerList[ConnectPrefabIdToIndex[prefabID]];
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
