using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace TBTK {
	
	public class UnitEditorWindow : TBEditorWindow {
		
		[MenuItem ("Tools/TBTK/UnitEditor", false, 10)]
		static void OpenUnitEditor () { Init(); }
		
		private static UnitEditorWindow window;
		
		public static void Init (int prefabID=-1) {
			window = (UnitEditorWindow)EditorWindow.GetWindow(typeof (UnitEditorWindow), false, "UnitEditor");
			window.minSize=new Vector2(420, 300);
			
			TBE.Init();
			
			window.InitLabel();
			
			if(prefabID>=0) window.selectID=UnitDB.GetPrefabIndex(prefabID);
			
			window.SelectItem(window.selectID);
		}
		
		private static string[] aiTypeLabel;
		private static string[] aiTypeTooltip;
		
		public void InitLabel(){
			int enumLength = Enum.GetValues(typeof(AI._AIBehaviour)).Length;
			aiTypeLabel=new string[enumLength];
			aiTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				aiTypeLabel[i]=((AI._AIBehaviour)i).ToString();
				if((AI._AIBehaviour)i==AI._AIBehaviour.passive) 
					aiTypeTooltip[i]="The unit won't actively seek out hostile unless the there are hostile within the faction's sight (using unit sight value when Fog-Of-War is not used)";
				if((AI._AIBehaviour)i==AI._AIBehaviour.aggressive) aiTypeTooltip[i]="The unit will actively seek out hostile to engage";
			}
		}
		
		GameObject rootDBObj;
		
		public void OnGUI(){
			TBE.InitGUIStyle();
			
			if(!CheckIsPlaying()) return;
			if(window==null) Init();
			
			List<Unit> unitList=UnitDB.GetList();
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(UnitDB.GetDB(), "abilityDB");
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) TBE.SetDirty();
			
			//if(GUI.Button(new Rect(Math.Max(260, window.position.width-300), 5, 100, 25), "Set")){
			//	for(int i=0; i<unitList.Count; i++){
			//		unitList[i].radius=0.5f;
			//	}
			//	TBE.SetDirty();
			//}
			
			Unit newUnit=null;
			TBE.Label(5, 7, 150, 17, "Add New Unit:", "Drag unit prefab to this slot to add it to the list");
			newUnit=(Unit)EditorGUI.ObjectField(new Rect(95, 7, width, height), newUnit, typeof(Unit), false);
			if(newUnit!=null) Select(NewItem(newUnit));
			
			float startX=5;	float startY=55;
			
			if(minimiseList){ if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false; }
			else{ if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true; }
			
			Vector2 v2=DrawUnitList(startX, startY, unitList);
			startX=v2.x+25;
			
			if(unitList.Count==0) return;
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX, window.position.height-startY);
			Rect contentRect=new Rect(startX, startY, contentWidth, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				spaceX+=10;	width-=10;
				
				EditorGUI.BeginChangeCheck();
				
				v2=DrawUnitConfigurator(startX, startY, unitList[selectID]);
				contentWidth=v2.x-startX;
				contentHeight=v2.y-55;
				
				if(EditorGUI.EndChangeCheck()){
					#if UNITY_2018_3_OR_NEWER
					GameObject unitObj=PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(unitList[selectID].gameObject));
					Unit selectedUnit=unitObj.GetComponent<Unit>();
					selectedUnit=unitList[selectID];
					GameObject obj=PrefabUtility.SavePrefabAsset(selectedUnit.gameObject);
					#endif
				}
				
				spaceX-=10;	width+=10;
			
			GUI.EndScrollView();
			
			if(GUI.changed) TBE.SetDirty();
		}
		
		
		private bool foldStats=true;
		private bool foldSetting=true;
		private bool foldShootPoint=true;
		private bool foldAbNEff=true;
		private bool foldAnimNAudio=true;
		
		private bool foldAttack=true;
		private bool foldAttackMelee=true;
		
		private bool foldCounter=true;
		private bool foldOverwatch=true;
		
		private int objIdx;
		
		private Vector2 DrawUnitConfigurator(float startX, float startY, Unit unit){
			float maxX=startX;
			
				startY=TBE.DrawBasicInfo(startX, startY, unit);
			
			//startY+=spaceY;
				
				TBE.Label(startX, startY, width, height, "Value:", "Just an arbitary number use for squad selection screen in demo");
				unit.value=EditorGUI.IntField(new Rect(startX+spaceX-10, startY, widthS, height), unit.value);
			
				startY+=10+spaceY;
			
				foldSetting=EditorGUI.Foldout(new Rect(startX, startY, spaceX, height), foldSetting, "Settings", TBE.foldoutS);
				if(foldSetting){
					startX+=10;
					
					int aiType=(int)unit.aiBehaviour;			contL=TBE.SetupContL(aiTypeLabel, aiTypeTooltip);
					TBE.Label(startX, startY+=spaceY, width, height, "AI Behaviour:", "The behaviour to use when deployed as AI unit");
					aiType = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), new GUIContent(""), aiType, contL);
					unit.aiBehaviour=(AI._AIBehaviour)aiType;
					
					TBE.Label(startX, startY+=spaceY, width, height, "Require Trigger:", "If checked, the unit will start in 'Passive' AI behaviour until it spotted a hostile unit");
					if(unit.aiBehaviour==AI._AIBehaviour.passive) TBE.Label(startX+spaceX, startY, widthS, height, "n/a");
					else unit.requireTrigger=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.requireTrigger);
					
					startY+=10;
					
					objIdx=GetIndexFromHierarchy(unit.targetPoint, objHierarchyList);
					TBE.Label(startX, startY+=spaceY, width, height, "Target Point:", "The 'center' point of the unit. Any aiming/attack towards this unit will be aim at this point");
					objIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objIdx, objHierarchylabel);
					unit.targetPoint = objHierarchyList[objIdx];
					
					TBE.Label(startX, startY+=spaceY, width, height, "Hit Radius:", "The 'size' of the unit, any shoot-object aimed towards this unit will be considered hit once it reach this radius value");
					unit.radius=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), unit.radius);
					
					startY+=10;
					
					GUIStyle styleSO=unit.soRange==null ? TBE.conflictS : null;
					TBE.Label(startX, startY+=spaceY, width, height, "Shoot Object Range:", "The shoot-object to fire at each attack. Must be a prefab contain a 'ShootObject' component", styleSO);
					unit.soRange=(ShootObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.soRange, typeof(ShootObject), true);
					
					TBE.Label(startX, startY+=spaceY, width, height, "Shoot Object Melee:", "Optional shoot-object to fire at each melee attack. If unassigned, ShootObjectRange will be used instead. Must be a prefab contain a 'ShootObject' component");
					if(!unit.hasMeleeAttack) TBE.Label(startX+spaceX, startY, width, height, "-");
					else unit.soMelee=(ShootObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.soMelee, typeof(ShootObject), true);
					
					startY+=10;
					
					cont=new GUIContent("ShootPoint:", "OPTIONAL - The transform which indicate the position where the shootObject will be fired from\nEach shootPoint assigned will fire a shootObject instance in each attack\nIf left empty, the unit transform itself will be use as the shootPoint\nThe orientation of the shootPoint matter as they dictate the orientation of the shootObject starting orientation.\n");
					foldShootPoint=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldShootPoint, cont);
					int shootPointCount=unit.shootPointList.Count;
					shootPointCount=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), shootPointCount);
					
					if(shootPointCount!=unit.shootPointList.Count){
						while(unit.shootPointList.Count<shootPointCount) unit.shootPointList.Add(null);
						while(unit.shootPointList.Count>shootPointCount) unit.shootPointList.RemoveAt(unit.shootPointList.Count-1);
					}
						
					if(foldShootPoint){
						for(int i=0; i<unit.shootPointList.Count; i++){
							objIdx=GetIndexFromHierarchy(unit.shootPointList[i], objHierarchyList);
							TBE.Label(startX, startY+=spaceY, width, height, "    - Element "+(i+1));
							objIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objIdx, objHierarchylabel);
							unit.shootPointList[i] = objHierarchyList[objIdx];
						}
					}
				
					TBE.Label(startX, startY+=spaceY, width, height, "Shoot Point Spacing:", "The time delay in second between each shoot-point during an attack");
					if(unit.shootPointList.Count<=1) TBE.Label(startX+spaceX, startY, widthS, height, "-", "");
					else unit.shootPointSpacing=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), unit.shootPointSpacing);
				
					startY+=10;
				
					TBE.Label(startX, startY+=spaceY, width, height, "Turret Pivot:", "OPTIONAL - The object under unit's hierarchy which is used to aim toward target\nWhen left unassigned, no aiming will be done.");
					objIdx = GetIndexFromHierarchy(unit.turretPivot, objHierarchyList);
					objIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objIdx, objHierarchylabel);
					unit.turretPivot = objHierarchyList[objIdx];
					
					TBE.Label(startX, startY+=spaceY, width, height, "Barrel Pivot:", "OPTIONAL - The secondary object under unit's hierarchy which is used to aim toward target in x-axis only\nWhen left unassigned, no aiming will be done.");
					if(unit.turretPivot==null) EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
					else{
						objIdx = GetIndexFromHierarchy(unit.barrelPivot, objHierarchyList);
						objIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objIdx, objHierarchylabel);
						unit.barrelPivot = objHierarchyList[objIdx];
					}
					
					TBE.Label(startX, startY+=spaceY, width, height, "Aim In X-Axis:", "Check to have the turret aim in x-axis");
					if(unit.turretPivot==null) EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
					else unit.aimInXAxis=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.aimInXAxis);
					
					
					TBE.Label(startX, startY+=spaceY, width, height, "Rotate Aiming:", "Check to have the unit rotate itself while aiming");
					unit.rotateWhileAiming=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.rotateWhileAiming);
					
					TBE.Label(startX, startY+=spaceY, width, height, "Snap Aiming:", "Check to have the turret aim instantly");
					unit.snapAiming=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.snapAiming);
					
					startY+=10;
					
					string txt="OPTIONAL: The effect object to spawn when the item is spawned on the grid";
					startY=DrawVisualObject(startX, startY+=spaceY, unit.effectOnDestroyed, "Effect On Destroyed:", txt);
					
					startX-=10;	startY+=10;
				}
				
			startY+=spaceY*0.5f;
				
				TBE.Label(startX+10, startY+=spaceY, width, height, "Need LOS To Attack:", "Check if the unit standard attack require the target to be in direct line-of-sight");
				unit.requireLOSToAttack=EditorGUI.Toggle(new Rect(startX+spaceX+10, startY, widthS, height), unit.requireLOSToAttack);
				
				TBE.Label(startX+10, startY+=spaceY, width, height, "Has Melee Attack:", "Check if the unit has alternate 'melee attack' which will be triggered if the target is within specified melee range");
				unit.hasMeleeAttack=EditorGUI.Toggle(new Rect(startX+spaceX+10, startY, widthS, height), unit.hasMeleeAttack);
			
			startY+=spaceY*0.5f;				
			
				foldStats=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldStats, "Stats", TBE.foldoutS);
				if(foldStats){
					Stats item=unit.stats;		startX+=10;


				    startY += spaceY * 0.5f;

				    TBE.Label(startX, startY += spaceY, width, height, "ResisBlunt:", "");
				    item.YoavResisBlunt = EditorGUI.DelayedIntField(new Rect(startX + spaceX, startY, widthS, height), item.YoavResisBlunt);
                
				    TBE.Label(startX, startY += spaceY, width, height, "ResisSlash:", "");
				    item.YoavResisSlash = EditorGUI.DelayedIntField(new Rect(startX + spaceX, startY, widthS, height), item.YoavResisSlash);

				    TBE.Label(startX, startY += spaceY, width, height, "ResisAcid:", "");
				    item.YoavResisAcid = EditorGUI.DelayedIntField(new Rect(startX + spaceX, startY, widthS, height), item.YoavResisAcid);

				    startY += spaceY * 0.5f;

				    TBE.Label(startX, startY += spaceY, width, height, "SideMul:", "");
				    item.SideMul = EditorGUI.DelayedFloatField(new Rect(startX + spaceX, startY, widthS, height), item.SideMul);

				    startY += spaceY * 0.5f;

				    TBE.Label(startX, startY += spaceY, width, height, "ExtraWalk:", "");
				    item.ExtraWalk = EditorGUI.DelayedIntField(new Rect(startX + spaceX, startY, widthS, height), item.ExtraWalk);

				    startY += spaceY * 0.5f;


                TBE.Label(startX, startY+=spaceY, width, height, "Hit Point (HP):", "The base hit-point of the unit");
					item.hp=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.hp);
					
					TBE.Label(startX, startY+=spaceY, width, height, " - HP Regen:", "hit-point gained by the unit each unit");
					GUI.color=unit.stats.hpRegen!=0 ? Color.white : Color.grey ;
					unit.stats.hpRegen=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), unit.stats.hpRegen);		GUI.color=Color.white;
					
					TBE.Label(startX, startY+=spaceY, width, height, "Action Point (AP):", "The base action-point of the unit");
					item.ap=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.ap);
					
					TBE.Label(startX, startY+=spaceY, width, height, " - AP Regen:", "action-point gained by the unit each unit");
					GUI.color=unit.stats.apRegen!=0 ? Color.white : Color.grey ;
					unit.stats.apRegen=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), unit.stats.apRegen);		GUI.color=Color.white;
					
					startY+=spaceY*0.5f;
					
					
					foldAttack=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldAttack, "Default Attack:", TBE.foldoutLS);
					if(foldAttack){
						TBE.Label(startX+10, startY+=spaceY, width, height, "Damage Type:", "");
						unit.damageType = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), unit.damageType, TBE.GetDamageLabel());
						
						startY=DrawAttackStats(startX, startY, item);
					}
					
					startY+=spaceY*0.5f;
					
					if(unit.hasMeleeAttack){
						foldAttackMelee=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldAttackMelee, "Melee Attack:", TBE.foldoutLS);
						if(foldAttackMelee){
							TBE.Label(startX+10, startY+=spaceY, width, height, "Damage Type:", "");
							unit.damageTypeMelee = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), unit.damageTypeMelee, TBE.GetDamageLabel());
							
							startY=DrawAttackStats(startX, startY, unit.statsMelee);
						}
					}
					else TBE.Label(startX, startY+=spaceY, spaceX, height, "- No Melee Attack");
					
					startY+=spaceY*0.5f;
					
					foldCounter=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldCounter, "Counter Attack:", TBE.foldoutLS);
					if(foldCounter){
						TBE.Label(startX+10, startY+=spaceY, width, height, "Damage Muliplier:", "Additional damage multiplier to be applied to the damage when performing a counter-attack");
						item.cDmgMultip=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.cDmgMultip);
						
						TBE.Label(startX+10, startY+=spaceY, width, height, "Hit Penalty:", "Additional negative hit-chance modifier to be applied when performing a counter-attack"+TBE.ChanceTT());
						item.cHitPenalty=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.cHitPenalty);
						
						TBE.Label(startX+10, startY+=spaceY, width, height, "Crit Penalty:", "Additional negative critical-chance modifier to be applied when performing a counter-attack"+TBE.ChanceTT());
						item.cCritPenalty=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.cCritPenalty);
					}
					
					foldOverwatch=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldOverwatch, "Overwatch:", TBE.foldoutLS);
					if(foldOverwatch){
						TBE.Label(startX+10, startY+=spaceY, width, height, "Damage Muliplier:", "Additional damage multiplier to be applied to the damage when performing an overwatch attack");
						item.oDmgMultip=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.oDmgMultip);
						
						TBE.Label(startX+10, startY+=spaceY, width, height, "Hit Penalty:", "Additional negative hit-chance modifier to be applied when performing an overwatch attack"+TBE.ChanceTT());
						item.oHitPenalty=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.oHitPenalty);
						
						TBE.Label(startX+10, startY+=spaceY, width, height, "Crit Penalty:", "Additional negative critical-chance modifier to be applied when performing an overwatch attack"+TBE.ChanceTT());
						item.oCritPenalty=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.oCritPenalty);
					}
					
					startY+=spaceY*0.5f;
					
					//public float armorType=0;
					TBE.Label(startX, startY+=spaceY, width, height, "Armor Type:", "");
					unit.armorType = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), unit.armorType, TBE.GetArmorLabel());
					
					TBE.Label(startX, startY+=spaceY, width, height, "Defense:", "The defense of the unit\nUsed to negate damage when attacked "+TBE.AttackTT());
					item.defense=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.defense);
					
					TBE.Label(startX, startY+=spaceY, width, height, "Dodge:", "The unit's dodge chance\nUse to negate the attacker hit chance when attacked"+TBE.ChanceTT());
					item.dodge=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.dodge);
					
					TBE.Label(startX, startY+=spaceY, width, height, "Crit Reduc:", "The unit's critical hit reduction chance\nUse to negate the attacker critical chance when attacked"+TBE.ChanceTT());
					item.critReduc=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.critReduc);
					
					startY+=spaceY*0.5f;
					
					TBE.Label(startX, startY+=spaceY, width, height, "Turn Priority:", "Use to determined unit's move order in UnitPerTurn (or FactionPerTurn where unit switching is disabled)");
					item.turnPriority=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.turnPriority);
					
					TBE.Label(startX, startY+=spaceY, width, height, "Move Range:", "");
					item.moveRange=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.moveRange);
					
					TBE.Label(startX, startY+=spaceY, width, height, "AttackRange Min/Max:", "");
					GUI.color=item.attackRangeMin>1 ? Color.white : Color.grey;
					item.attackRangeMin=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.attackRangeMin);	GUI.color=Color.white;
					item.attackRange=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+widthS, startY, widthS, height), item.attackRange);
					
					if(unit.hasMeleeAttack){
						TBE.Label(startX, startY+=spaceY, width, height, "AttackRange Melee:", "");
						unit.statsMelee.attackRange=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), unit.statsMelee.attackRange);
						unit.statsMelee.attackRange=Mathf.Min(unit.statsMelee.attackRange, item.attackRange);
					}
					
					TBE.Label(startX, startY+=spaceY, width, height, "Sight:", "The visibility range of the unit when fog-of-war is enabled\n\nAlso used as the hostile detection range for AI unit in 'Passive' mode");
					item.sight=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.sight);
					
					startY+=spaceY*0.5f;
					
					TBE.Label(startX, startY+=spaceY, width, height, "Move Limit:", "The maximum number of moving the unit can do in a single turn");
					item.moveLimit=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.moveLimit);
					
					TBE.Label(startX, startY+=spaceY, width, height, "Attack Limit:", "The maximum number of attacking the unit can do in a single turn");
					item.attackLimit=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.attackLimit);
					
					TBE.Label(startX, startY+=spaceY, width, height, "Counter Limit:", "The maximum number of counter-attacking the unit can do in a single turn");
					item.counterLimit=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.counterLimit);
					
					TBE.Label(startX, startY+=spaceY, width, height, "Ability Limit:", "The maximum number of ability the unit can use in a single turn");
					item.abilityLimit=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.abilityLimit);
					
					startX-=10;
				}
				
				
				startY+=spaceY*0.5f;
				
				
				foldAbNEff=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldAbNEff, "Abilities & Effects", TBE.foldoutS);
				if(foldAbNEff){
					startX+=10;
					
					TBE.Label(startX, startY+=spaceY, width, height, "Effect On Attack:", "The effect to be applied to the target when the unit attacks");
					for(int i=0; i<unit.attackEffectIDList.Count; i++){
						TBE.Label(startX+spaceX-height, startY+=(i>0 ? spaceY : 0), width, height, "-");
						
						int effIdx=EffectDB.GetPrefabIndex(unit.attackEffectIDList[i]);
						effIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), effIdx, EffectDB.label);
						int prefabID=EffectDB.GetItemID(effIdx);
						if(!unit.attackEffectIDList.Contains(prefabID)) unit.attackEffectIDList[i]=prefabID;
						
						if(GUI.Button(new Rect(startX+spaceX+width+3, startY, height, height), "-")){ unit.attackEffectIDList.RemoveAt(i); }
					}
					
					if(unit.attackEffectIDList.Count<EffectDB.GetCount()){
						int newIdx=-1;		CheckColor(unit.attackEffectIDList.Count, 0);
						startY+=unit.attackEffectIDList.Count>0 ? spaceY : 0 ;
						newIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), newIdx, EffectDB.label);
						if(newIdx>=0){
							int newPID=EffectDB.GetItemID(newIdx);
							if(!unit.attackEffectIDList.Contains(newPID)) unit.attackEffectIDList.Add(newPID);
						}
						ResetColor();
					}
					
					
					startY+=spaceY;
					
					
					TBE.Label(startX, startY+=spaceY, width, height, "Abilities:", "The ability possesed by the unit");
					for(int i=0; i<unit.abilityIDList.Count; i++){
						TBE.Label(startX+spaceX-height, startY+=(i>0 ? spaceY : 0), width, height, "-");
						
						int abIdx=AbilityUDB.GetPrefabIndex(unit.abilityIDList[i]);
						abIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), abIdx, AbilityUDB.label);
						int prefabID=AbilityUDB.GetItemID(abIdx);
						if(!unit.abilityIDList.Contains(prefabID)) unit.abilityIDList[i]=prefabID;
						
						if(GUI.Button(new Rect(startX+spaceX+width+3, startY, height, height), "-")) unit.abilityIDList.RemoveAt(i); 					
					}
					
					if(unit.abilityIDList.Count<AbilityUDB.GetCount()){
						int newIdx=-1;		CheckColor(unit.abilityIDList.Count, 0);
						startY+=unit.abilityIDList.Count>0 ? spaceY : 0 ;
						newIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), newIdx, AbilityUDB.label);
						if(newIdx>=0){
							int newPID=AbilityUDB.GetItemID(newIdx);
							if(!unit.abilityIDList.Contains(newPID)) unit.abilityIDList.Add(newPID);
						}
						ResetColor();
					}
					
					
					startY+=spaceY;
					
					
					TBE.Label(startX, startY+=spaceY, width, height, "Immuned Effects:", "The effects the unit is immuned to");
					for(int i=0; i<unit.immuneEffectList.Count; i++){
						TBE.Label(startX+spaceX-height, startY+=(i>0 ? spaceY : 0), width, height, "-");
						
						int effIdx=EffectDB.GetPrefabIndex(unit.immuneEffectList[i]);
						effIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), effIdx, EffectDB.label);
						int prefabID=EffectDB.GetItemID(effIdx);
						if(!unit.attackEffectIDList.Contains(prefabID)) unit.attackEffectIDList[i]=prefabID;
						
						if(GUI.Button(new Rect(startX+spaceX+width+3, startY, height, height), "-")){ unit.immuneEffectList.RemoveAt(i); }
					}
					
					if(unit.immuneEffectList.Count<EffectDB.GetCount()){
						int newIdx=-1;		CheckColor(unit.immuneEffectList.Count, 0);
						startY+=unit.immuneEffectList.Count>0 ? spaceY : 0 ;
						newIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), newIdx, EffectDB.label);
						if(newIdx>=0){
							int newPID=EffectDB.GetItemID(newIdx);
							if(!unit.immuneEffectList.Contains(newPID)) unit.immuneEffectList.Add(newPID);
						}
						ResetColor();
					}
					
					startX-=10;
				}
				
				
				startY+=spaceY*0.5f;
				
				
				foldAnimNAudio=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldAnimNAudio, "Animation and Audio ", TBE.foldoutS);
				if(foldAnimNAudio){
					startX+=10;
					
					TBE.Label(startX, startY+=spaceY, width, height, "Animation:", "", TBE.headerS);
					TBE.Label(startX, startY+=spaceY, width, height, " - Animator Obj:", "The transform object which contain the Animator component");
					objIdx = GetIndexFromHierarchy(unit.animatorT, objHierarchyList);
					objIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objIdx, objHierarchylabel);
					unit.animatorT = objHierarchyList[objIdx];
					
					TBE.Label(startX, startY+=spaceY, width, height, " - Idle:", "");
					unit.clipIdle=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.clipIdle, typeof(AnimationClip), true);
					
					TBE.Label(startX, startY+=spaceY, width, height, " - Move:", "");
					unit.clipMove=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.clipMove, typeof(AnimationClip), true);
					
					TBE.Label(startX, startY+=spaceY, width, height, " - Hit:", "");
					unit.clipHit=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.clipHit, typeof(AnimationClip), true);
					
					TBE.Label(startX, startY+=spaceY, width, height, " - Destroyed:", "");
					unit.clipDestroyed=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.clipDestroyed, typeof(AnimationClip), true);
					
					TBE.Label(startX, startY+=spaceY, width, height, " - Attack Range:", "");
					unit.clipAttackRange=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.clipAttackRange, typeof(AnimationClip), true);
					
					TBE.Label(startX, startY+=spaceY, width, height, "    - Delay:", "The delay in second after the animation is played before the shoot-object are fired\nThis is for synchronizing the attack sequence to the animation");
					if(unit.clipAttackRange==null) TBE.Label(startX+spaceX, startY, width, height, "-", "");
					else unit.animAttackDelayRange=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), unit.animAttackDelayRange);
					
					TBE.Label(startX, startY+=spaceY, width, height, " - Attack Melee:", "");
					unit.clipAttackMelee=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.clipAttackMelee, typeof(AnimationClip), true);
					
					TBE.Label(startX, startY+=spaceY, width, height, "    - Delay:", "The delay in second after the animation is played before the target is hit\nThis is for synchronizing the attack sequence to the animation");
					if(unit.clipAttackMelee==null) TBE.Label(startX+spaceX, startY, width, height, "-", "");
					else unit.animAttackDelayMelee=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), unit.animAttackDelayMelee);
					
					
					startY+=spaceY*0.5f;
					
					
					TBE.Label(startX, startY+=spaceY, width, height, "Audio:", "", TBE.headerS);
					
					TBE.Label(startX, startY+=spaceY, width, height, " - Select:", "");
					unit.selectSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.selectSound, typeof(AudioClip), true);
					
					TBE.Label(startX, startY+=spaceY, width, height, " - Move:", "");
					unit.moveSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.moveSound, typeof(AudioClip), true);
					
					TBE.Label(startX, startY+=spaceY, width, height, "    - Loop Move:", "Check to keep the move sound playing while the unit is moving\nOtherwise it will only play once when the unit start moving");
					if(unit.moveSound==null) TBE.Label(startX+spaceX, startY, width, height, "-", "");
					else unit.loopMoveSound=EditorGUI.Toggle(new Rect(startX+spaceX, startY, width, height), unit.loopMoveSound);
					
					TBE.Label(startX, startY+=spaceY, width, height, " - Attack (Range):", "");
					unit.attackRangeSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.attackRangeSound, typeof(AudioClip), true);
					
					TBE.Label(startX, startY+=spaceY, width, height, " - Attack (Melee):", "");
					unit.attackMeleeSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.attackMeleeSound, typeof(AudioClip), true);
					
					TBE.Label(startX, startY+=spaceY, width, height, " - Hit:", "");
					unit.hitSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.hitSound, typeof(AudioClip), true);
					
					TBE.Label(startX, startY+=spaceY, width, height, " - Destroyed:", "");
					unit.destroySound=(AudioClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.destroySound, typeof(AudioClip), true);
					
					startX-=10;
				}
				
				
			startY+=spaceY*2;
			
				GUIStyle style=new GUIStyle("TextArea");	style.wordWrap=true;
				cont=new GUIContent("Unit description (for runtime and editor): ", "");
				EditorGUI.LabelField(new Rect(startX, startY, 400, height), cont);
				unit.desp=EditorGUI.DelayedTextField(new Rect(startX, startY+spaceY-3, 270, 150), unit.desp, style);
			
			return new Vector2(maxX, startY+170);
		}
		
		
		
		
		private float DrawAttackStats(float startX, float startY, Stats item){
			TBE.Label(startX+10, startY+=spaceY, width, height, "HP Dmg. Min/Max:", "The unit attack's damage potential to target's hit-point");
			GUI.color=(item.dmgHPMin!=0 || item.dmgHPMax!=0) ? Color.white : Color.grey ;
			item.dmgHPMin=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.dmgHPMin);
			item.dmgHPMax=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), item.dmgHPMax);	GUI.color=Color.white;
		
			TBE.Label(startX+10, startY+=spaceY, width, height, "AP Dmg. Min/Max:", "The unit attack's damage potential to target's action-point");
			GUI.color=(item.dmgAPMin!=0 || item.dmgAPMax!=0) ? Color.white : Color.grey ;
			item.dmgAPMin=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.dmgAPMin);
			item.dmgAPMax=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), item.dmgAPMax);	GUI.color=Color.white;
			
			TBE.Label(startX+10, startY+=spaceY, width, height, "Attack:", "The attack value\nUsed along with the target's defense to modify the effective damage"+TBE.AttackTT());
			item.attack=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.attack);
			
			TBE.Label(startX+10, startY+=spaceY, width, height, "Hit:", "The chance for the unit's attack to hit\nThis will be negate by the target's dodge chance"+TBE.ChanceTT());
			item.hit=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.hit);
			
			TBE.Label(startX+10, startY+=spaceY, width, height, "Crit Chance:", "The chance for the unit's attack to critically hit\nThis will be negate by the target's critical reduction chance"+TBE.ChanceTT());
			item.critChance=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.critChance);
			
			TBE.Label(startX+10, startY+=spaceY, width, height, "Crit Muliplier:", "The damage multiplier to use when the unit score a critical hit"+TBE.ChanceTT());
			item.critMultiplier=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.critMultiplier);
			
			//TBE.Label(startX+10, startY+=spaceY, width, height, "Counter Muliplier:", "");
			//item.cMultiplier=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.cMultiplier);
			
			return startY;
		}
			
			
		protected Vector2 DrawUnitList(float startX, float startY, List<Unit> unitList){
			List<EItem> list=new List<EItem>();
			for(int i=0; i<unitList.Count; i++){
				EItem item=new EItem(unitList[i].prefabID, unitList[i].itemName, unitList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		public static int NewItem(Unit item){ return window._NewItem(item); }
		private int _NewItem(Unit item){
			if(UnitDB.GetList().Contains(item)) return selectID;
			
			item.prefabID=TBE.GenerateNewID(UnitDB.GetPrefabIDList());
			
			#if UNITY_2018_3_OR_NEWER
			GameObject obj=PrefabUtility.SavePrefabAsset(item.gameObject);
			item=obj.GetComponent<Unit>();
			#endif
			
			UnitDB.GetList().Add(item);
			UnitDB.UpdateLabel();
			
			return UnitDB.GetList().Count-1;
		}
		
		protected override void DeleteItem(){
			UnitDB.ResetItemPID(deleteID);
			UnitDB.GetList().RemoveAt(deleteID);
			UnitDB.UpdateLabel();
		}
		
		protected override void SelectItem(){ SelectItem(selectID); }
		private void SelectItem(int newID){ 
			selectID=newID;
			if(UnitDB.GetList().Count<=0) return;
			
			selectID=Mathf.Clamp(selectID, 0, UnitDB.GetList().Count-1);
			UpdateObjHierarchyList(UnitDB.GetList()[selectID].transform);
		}
		
		protected override void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		protected override void ShiftItemDown(){ if(selectID<UnitDB.GetList().Count-1) ShiftItem(1); }
		private void ShiftItem(int dir){
			Unit item=UnitDB.GetList()[selectID];
			UnitDB.GetList()[selectID]=UnitDB.GetList()[selectID+dir];
			UnitDB.GetList()[selectID+dir]=item;
			selectID+=dir;
		}
		
		
	}
	
}
