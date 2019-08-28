using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace TBTK{

	public class UIAbilityUnit : UIScreen {
		
		private RectTransform buttonParentRectT;
		public static void SetStartingOffset(float value){
			instance.buttonParentRectT.localPosition=new Vector2(value, instance.buttonParentRectT.localPosition.y);
		}
		
		public int buttonLimit=8;
		public List<UIButton> buttonList=new List<UIButton>();
		
		private static UIAbilityUnit instance;
		
		public override void Awake(){
			base.Awake();
			
			instance=this;
		}
		
		public override void Start(){
			for(int i=0; i<buttonLimit; i++){
				if(i>0) buttonList.Add(UIButton.Clone(buttonList[0].rootObj, "UAbilityButton"+(i)));
				buttonList[i].Init();
				
				int idx=i;	buttonList[i].button.onClick.AddListener(delegate { OnButton(idx); });
				buttonList[i].SetCallback(this.OnHoverButton, this.OnExitButton);
				
				//~ cooldownSlider.Add(abilityButtons[idx].rootT.GetChild(0).gameObject.GetComponent<Slider>());
				
				buttonList[i].SetActive(false);
			}
			
			buttonParentRectT=buttonList[0].rootT.parent.GetComponent<RectTransform>();
			buttonParentRectT.gameObject.SetActive(false);
			
			canvasGroup.alpha=1;
		}
		
		
		void OnEnable(){
			TBTK.onSelectUnitE += OnSelectUnit ;
			TBTK.onActionInProgressE += OnActionInProgressE ;
			TBTK.onAbilityTargetingE += OnAbilityTargeting ;
		}
		void OnDisable(){
			TBTK.onSelectUnitE -= OnSelectUnit ;
			TBTK.onActionInProgressE -= OnActionInProgressE ;
			TBTK.onAbilityTargetingE -= OnAbilityTargeting ;
		}
		
		void OnSelectUnit(Unit unit){ UpdateDisplay(unit); }
		
		void OnActionInProgressE(bool flag){
			for(int i=0; i<buttonLimit; i++){
				if(!buttonList[i].rootObj.activeInHierarchy) continue;
				buttonList[i].button.interactable=!flag;
			}
		}
		
		
		public void OnButton(int idx){
			if(idx>=buttonList.Count) return;
			if(!buttonList[idx].button.interactable || !buttonList[idx].rootObj.activeInHierarchy) return;
			
			if(AbilityManager.IsWaitingForTargetU() && AbilityManager.GetSelectedIdx()==idx){
				//buttonList[AbilityManager.GetSelectedIdx()].imgHighlight.gameObject.SetActive(false);
				AbilityManager.ExitAbilityTargetMode();
				ClearSelection();
				return;
			}
			
			//ClearSelection();
			buttonList[idx].imgHighlight.gameObject.SetActive(true);
			UnitManager.GetSelectedUnit().SelectAbility(idx);
		}
		
		public void OnHoverButton(GameObject butObj){
			int idx=0;
			for(int i=0; i<buttonList.Count; i++){
				if(buttonList[i].rootObj==butObj){ idx=i; break; }
			}
			
			Vector3 sPos=UI.GetCorner(buttonList[idx].rectT, 1)+new Vector3(0, 10*buttonList[idx].rectT.lossyScale.y, 0);
			UITooltip.Show(UnitManager.GetSelectedUnit().GetAbility(idx), sPos, new Vector2(0, 20));
		}
		public void OnExitButton(GameObject butObj){
			UITooltip.HideTooltip();
		}
		
		
		void Update(){
			if(Input.GetKeyDown(KeyCode.Alpha1)) OnButton(0);
			if(Input.GetKeyDown(KeyCode.Alpha2)) OnButton(1);
			if(Input.GetKeyDown(KeyCode.Alpha3)) OnButton(2);
			if(Input.GetKeyDown(KeyCode.Alpha4)) OnButton(3);
			if(Input.GetKeyDown(KeyCode.Alpha5)) OnButton(4);
			if(Input.GetKeyDown(KeyCode.Alpha6)) OnButton(5);
			if(Input.GetKeyDown(KeyCode.Alpha7)) OnButton(6);
			if(Input.GetKeyDown(KeyCode.Alpha8)) OnButton(7);
			if(Input.GetKeyDown(KeyCode.Alpha9)) OnButton(8);
			if(Input.GetKeyDown(KeyCode.Alpha0)) OnButton(9);
			
			if(Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)){
				if(AbilityManager.IsWaitingForTargetU()){
					UIInput.ClearTouchModeCursor();
					AbilityManager.ExitAbilityTargetMode();
					ClearSelection();
				}
			}
		}
		
		
		private int curHighlightIdx=-1;
		public static void OnAbilityTargeting(Ability ab){ instance._OnAbilityTargeting(ab); }
		public void _OnAbilityTargeting(Ability ab){
			if(curHighlightIdx>=0){
				if(ab==null || ab.isFacAbility || ab.index!=curHighlightIdx)
					buttonList[curHighlightIdx].imgHighlight.gameObject.SetActive(false);
			}
			
			if(ab!=null && ab.isUnitAbility){
				curHighlightIdx=ab.index;
				buttonList[curHighlightIdx].imgHighlight.gameObject.SetActive(true);
			}
		}
		
		
		public static void UpdateDisplay(Unit unit){ instance._UpdateDisplay(unit); }
		public void _UpdateDisplay(Unit unit){
			if(unit==null || !unit.playableUnit){
				buttonParentRectT.gameObject.SetActive(false);
				//for(int i=0; i<buttonLimit; i++) buttonList[i].SetActive(false);
				return;
			}
			
			int activeCount=0;
			
			for(int i=0; i<buttonLimit; i++){
				if(i<unit.abilityList.Count){
					Ability ab=unit.abilityList[i];
					
					buttonList[i].image.sprite=ab.icon;
					buttonList[i].imageAlt.sprite=ab.icon;
					
					int isAvailable=ab.IsAvailable();
					buttonList[i].imageAlt.enabled=false;//isAvailable!=0;
					buttonList[i].label.text=ab.HasUseLimit() ? ab.GetUseRemain().ToString() : "" ;
					//buttonList[i].label.text=ab.currentCD>0 ? ab.currentCD.ToString() : "" ;
					
					buttonList[i].imgHighlight.gameObject.SetActive(false);
					
					buttonList[i].button.interactable=(isAvailable==0);
					
					activeCount+=1;
				}
				
				buttonList[i].SetActive(i<unit.abilityList.Count);
			}
			
			buttonParentRectT.gameObject.SetActive(activeCount>0);
		
		}
		
		
		public static void ClearSelection(){
			for(int i=0; i<instance.buttonLimit; i++) instance.buttonList[i].imgHighlight.gameObject.SetActive(false);
		}
		
	}

}