using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TBTK{

	public static class UI {
		
		public static float GetScaleFactor(){ return UIControl.GetScaleReferenceWidth()/Screen.width; }
		
		
		//inputID=-1 - mouse cursor, 	inputID>=0 - touch finger index
		public static bool IsCursorOnUI(int inputID=-1){
			EventSystem eventSystem = EventSystem.current;
			return ( eventSystem.IsPointerOverGameObject( inputID ) );
		}
		
		public static GameObject Clone(GameObject srcObj, string name="", Vector3 posOffset=default(Vector3)) {
			GameObject newObj=(GameObject)MonoBehaviour.Instantiate(srcObj);
			newObj.name=name=="" ? srcObj.name : name ;
			
			newObj.transform.SetParent(srcObj.transform.parent);
			newObj.transform.localPosition=srcObj.transform.localPosition+posOffset;
			newObj.transform.localScale=srcObj.transform.localScale;
			//newObj.transform.localScale=new Vector3(1, 1, 1);
			
			return newObj;
		}
		
		
		//0 - bottom left
		//1 - top left
		//2 - top right
		//3 - bottom right
		public static Vector3 GetCorner(RectTransform rectT, int corner=0){
			Vector3[] fourCornersArray=new Vector3[4];
			rectT.GetWorldCorners(fourCornersArray);
			return fourCornersArray[corner];
		}
		
		
		//public static IEnumerator SwitchScreen(UIScreen outScreen, UIScreen inScreen){
		//	outScreen.Hide(0.2f);
		//	yield return CRoutine.Get().StartCoroutine(CRoutine.WaitForRealSeconds(0.2f));
		//	inScreen.Show(0.2f);
		//}
		
		
		public static int GetIdxFromList(List<UIButton> buttonList, GameObject butObj, int idx=0){
			for(int i=0; i<buttonList.Count; i++){
				if(buttonList[i].rootObj==butObj){ idx=i; break; }
			}
			return idx;
		}
		public static int GetIdxFromList(List<UIObject> buttonList, GameObject butObj, int idx=0){
			for(int i=0; i<buttonList.Count; i++){
				if(buttonList[i].rootObj==butObj){ idx=i; break; }
			}
			return idx;
		}
		
		
		#region canvasgroup fade
		public static void FadeIn(CanvasGroup canvasGroup, float duration=0.25f, GameObject obj=null){
			if(obj!=null) obj.SetActive(true);
			FadeCanvas(canvasGroup, duration, 0f, 1f);
		}
		public static void FadeOut(CanvasGroup canvasGroup, float duration=0.25f, GameObject obj=null){
			FadeCanvas(canvasGroup, duration, 1f, 0f);
			if(obj!=null) CRoutine.Get().StartCoroutine(DeactivateObject(obj, duration));
		}
		
		public static IEnumerator DeactivateObject(GameObject obj, float duration){
			yield return CRoutine.Get().StartCoroutine(CRoutine.WaitForRealSeconds(duration));
			obj.SetActive(false);
		}
		
		public static void FadeCanvas(CanvasGroup canvasGroup, float duration=0.25f, float startValue=0.5f, float endValue=0.5f){ 
			CRoutine.RunRoutine(_FadeCanvas(canvasGroup, 1f/duration, startValue, endValue));
		}
		public static IEnumerator _FadeCanvas(CanvasGroup canvasGroup, float timeMul, float startValue, float endValue){
			float duration=0;
			while(duration<1){
				canvasGroup.alpha=Mathf.Lerp(startValue, endValue, duration);
				duration+=Time.unscaledDeltaTime*timeMul;
				yield return null;
			}
			canvasGroup.alpha=endValue;
		}
		#endregion
		
	}

}
