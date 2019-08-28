using System.Collections;
using System.Collections.Generic;
using Devdog.InventoryPro;
using UnityEngine;

namespace TBTK{

	public class MinisItemManager : MonoBehaviour
	{
	    public static MinisItemManager Singleton;
        public List<MinisItem> MinisItems;
        

	    void Awake()
	    {
	        Singleton = this;
	        gameObject.AddComponent<DontDestroyOnLoad>();
        }

	    public static void ConnectUnitToMinisItem(Unit unit, int minisItemId)
	    {
	        PlayerPrefs.SetInt("UnitToMinisItem#" + unit.prefabID, minisItemId);
	    }

	    public int ItemToAbility(string itemName)
	    {
	        foreach (var minisItem in MinisItems)
	        {
	            if (minisItem.MimisItemName == itemName)
	            {
	                return minisItem.AbilityId;
	            }
	        }
	        return -1;
	    }
        
	}




}