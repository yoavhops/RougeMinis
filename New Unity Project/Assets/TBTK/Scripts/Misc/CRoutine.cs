using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TBTK{

	public class CRoutine : MonoBehaviour{
		public static CRoutine instance;
		public static CRoutine Get(){ Init(); return instance; }
		
		public static void Init(){
			if(instance!=null) return;
			
			GameObject obj=new GameObject("_CRoutine");
			instance=obj.AddComponent<CRoutine>();
			
			DontDestroyOnLoad(obj);
		}
		
		
		//Coroutine.Get().StartCoroutine(funcName());
		
		
		public static void RunRoutine(IEnumerator routine){ Init(); instance.StartCoroutine(routine); }
		
		public static void Delay(float delay, Func<int> cb){ Init(); instance.StartCoroutine(instance._Delay(delay, cb)); }
		IEnumerator _Delay(float delay, Func<int> callback){ yield return new WaitForSeconds(delay); callback(); }
		
		
		public static IEnumerator WaitForRealSeconds(float time){ Init();
			float start = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup < start + time) yield return null;
		}
	}
	
}
