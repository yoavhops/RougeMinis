using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TBTK{
	
	[System.Serializable]
	public class UIObject{
		public GameObject rootObj;
		[HideInInspector] public Transform rootT;
		[HideInInspector] public RectTransform rectT;
		
		[HideInInspector] public CanvasGroup canvasG;
		
		[HideInInspector] public Image image;
		[HideInInspector] public Text label;
		
		[HideInInspector] public UIItemCallback itemCallback;
		
		public UIObject(){}
		public UIObject(GameObject obj){ rootObj=obj; Init(); }
		
		public virtual void Init(){
			if(rootObj==null){ Debug.LogWarning("Unassgined rootObj"); return; }
			
			rootT=rootObj.transform;
			rectT=rootObj.GetComponent<RectTransform>();
			
			//imgBase=rootObj.GetComponent<Image>();
			
			foreach(Transform child in rectT){
				if(child.name=="Image") image=child.GetComponent<Image>();
				else if(child.name=="Text") label=child.GetComponent<Text>();
			}
		}
		
		public static UIObject Clone(GameObject srcObj, string name="", Vector3 posOffset=default(Vector3)){
			GameObject newObj=UI.Clone(srcObj, name, posOffset);
			return new UIObject(newObj);
		}
		
		public virtual void SetCallback(Callback enter=null, Callback exit=null){
			itemCallback=rootObj.GetComponent<UIItemCallback>();
			if(itemCallback==null) itemCallback=rootObj.AddComponent<UIItemCallback>();
			itemCallback.SetEnterCallback(enter);
			itemCallback.SetExitCallback(exit);
		}
		
		public virtual void SetActive(bool flag){ rootObj.SetActive(flag); }
		
		//public void DisableSound(bool disableHover, bool disablePress){ itemCallback.DisableSound(disableHover, disablePress); }
	}
	
	
	
	#region UIButton
	[System.Serializable]
	public class UIButton : UIObject{
		
		[HideInInspector] public Text labelAlt;
		//[HideInInspector] public Text labelAlt2;
		
		[HideInInspector] public Image imageAlt;
		
		[HideInInspector] public Image imgHovered;
		[HideInInspector] public Image imgDisabled;
		[HideInInspector] public Image imgHighlight;
		
		[HideInInspector] public Button button;
		
		public UIButton(){}
		public UIButton(GameObject obj){ rootObj=obj; Init(); }
		
		public override void Init(){
			base.Init();
			
			button=rootObj.GetComponent<Button>();
			canvasG=rootObj.GetComponent<CanvasGroup>();
			
			foreach(Transform child in rectT){
				if(child.name=="TextAlt")				labelAlt=child.GetComponent<Text>();
				else if(child.name=="ImageAlt")	imageAlt=child.GetComponent<Image>();
				else if(child.name=="Hovered") 	imgHovered=child.GetComponent<Image>();
				else if(child.name=="Disabled") 	imgDisabled=child.GetComponent<Image>();
				else if(child.name=="Highlight") 	imgHighlight=child.GetComponent<Image>();
			}
		}
		
		public static new UIButton Clone(GameObject srcObj, string name="", Vector3 posOffset=default(Vector3)){
			GameObject newObj=UI.Clone(srcObj, name, posOffset);
			return new UIButton(newObj);
		}
		
		public override void SetCallback(Callback enter=null, Callback exit=null){
			base.SetCallback(enter, exit);
			//itemCallback.SetButton(button);
		}
		
		public override void SetActive(bool flag){
			//~ if(flag && imgHovered!=null) imgHovered.enabled=false;
			//~ if(flag && imgDisabled!=null) imgDisabled.enabled=false;
			base.SetActive(flag);
		}
		
	}
	#endregion
	
	
	
	#region callback
	public delegate void Callback(GameObject uiObj);	
	public class UIItemCallback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{
		//private bool useCustomClip=false;
		//public AudioClip enterClip;
		//public AudioClip downClip;
		
		//private Button button;
		
		private Callback enterCB;
		private Callback exitCB;
		
		public void SetButton(Button but){}// button=but; }
		public void SetEnterCallback(Callback callback){ enterCB=callback; }
		public void SetExitCallback(Callback callback){ exitCB=callback; }
		
		public void OnPointerEnter(PointerEventData eventData){ 
			//if(enterClip!=null && button!=null && button.interactable) AudioManager.PlayUISound(enterClip);
			if(enterCB!=null) enterCB(thisObj);
		}
		public void OnPointerExit(PointerEventData eventData){ 
			if(exitCB!=null) exitCB(thisObj);
		}
		
		private GameObject thisObj;
		void Awake(){
			thisObj=gameObject;
			//SetupAudioClip();
		}
		
		//~ void SetupAudioClip(){
			//~ if(useCustomClip) return;
			//~ //enterClip=AudioManager.GetHoverButtonSound();
			//~ //downClip=AudioManager.GetPressButtonSound();
		//~ }
		
		//public void SetSound(AudioClip eClip, AudioClip dClip){
		//	useCustomClip=true;	enterClip=eClip;	downClip=dClip;
		//}
		
		//public void DisableSound(bool disableHover, bool disablePress){
		//	if(disableHover) enterClip=null;
		//	if(disablePress) downClip=null;
		//}
	}
	#endregion

}