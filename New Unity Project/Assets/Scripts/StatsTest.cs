using System.Collections;
using System.Collections.Generic;
using Devdog.General;
using Devdog.InventoryPro;
using UnityEngine;

public class StatsTest : MonoBehaviour
{

    private IStat _str;

    // Start is called before the first frame update
    void Start()
    {
        _str = PlayerManager.instance.currentPlayer.inventoryPlayer.stats.Get("Default", "Strength");
        _str.SetCurrentValueRaw(33);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
