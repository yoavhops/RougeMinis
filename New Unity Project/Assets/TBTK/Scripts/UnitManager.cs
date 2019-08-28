using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TBTK{

	[ExecuteInEditMode]
	public class UnitManager : MonoBehaviour {
		
		public List<Unit> allUnitList=new List<Unit>();
		public static List<Unit> GetAllUnitList(){ 
			if(TurnControl.IsUnitPerTurn()) return instance.allUnitList;
			else{
				instance.allUnitList=new List<Unit>();
				for(int i=0; i<instance.factionList.Count; i++) instance.allUnitList.AddRange(instance.factionList[i].unitList);
				return instance.allUnitList;
			}
		}
		
		
		public List<Faction> factionList=new List<Faction>();
		public static List<Faction> GetFactionList(){ return instance.factionList; }
		public static Faction GetFaction(int idx){ return instance.factionList[idx]; }
		
		private bool hasAIInGame=false;
		
		public Unit selectedUnit;
		public static Unit GetSelectedUnit(){ return instance.selectedUnit; }
		
		public static Faction GetSelectedFaction(){ 
			if(GetSelectedUnit()==null) return null; 
			for(int i=0; i<instance.factionList.Count; i++){
				if(instance.factionList[i].factionID==GetSelectedUnit().GetFacID()) return instance.factionList[i];
			}
			return null;
		}
		
		
		private static UnitManager instance;
		
		public static void Init(){
			if(instance==null) instance=(UnitManager)FindObjectOfType(typeof(UnitManager));
			
			for(int i=0; i<instance.factionList.Count; i++){
				instance.factionList[i].Init(i);
				instance.hasAIInGame|=instance.factionList[i].playableFaction;
			}
			
			instance.DeployAllUnit();	//only applies for AI faction unless enableManualDeployment is left unchecked
			
			instance.deployingFacIdx=instance._RequireManualDeployment();
			if(instance.deployingFacIdx>=0) instance.NewFactionDeployment();
			
			instance._Init();
		}
		public void _Init(){
			//~ selectIndicator=GridManager.IsHexGrid() ? selectIndicatorHex : selectIndicatorSq ;
			//~ selectIndicator.localScale*=GridManager.GetNodeSize();
			
			//~ if(GridManager.IsHexGrid()) selectIndicatorSq.gameObject.SetActive(false);
			//~ if(GridManager.IsSquareGrid()) selectIndicatorHex.gameObject.SetActive(false);
		}
		
		public void Start(){
			if(!Application.isPlaying && instance==null) instance=this;
		}
		
		
		public void NewFactionDeployment(){
			deployingUnitIdx=0;
			int deployFacID=instance.factionList[deployingFacIdx].factionID;
			
			List<Node> deploymentNodeList=GridManager.GetDeploymentNode(deployFacID);
			GridIndicator.ShowDeployment(deploymentNodeList);
			
			//~ List<Node> addOnList=new List<Node>();
			//~ for(int i=0; i<deploymentNodeList.Count; i++){
				//~ List<Node> neighbours=deploymentNodeList[i].GetNeighbourList(true);
				//~ for(int n=0; n<neighbours.Count; n++){
					//~ if(!deploymentNodeList.Contains(neighbours[n])) addOnList.Add(neighbours[n]);
				//~ }
			//~ }
			//~ deploymentNodeList.AddRange(addOnList);
			GridManager.SetupFogOfWarForDeployment(deploymentNodeList, deployFacID);
		}
		
		public static bool IsDeploymentDone(){
			if(instance.factionList[instance.deployingFacIdx].deployingList.Count==0) return true;
			
			bool hasEmptyNode=false;
			List<Node> deploymentNodeList=GridManager.GetDeploymentNode(instance.factionList[instance.deployingFacIdx].factionID);
			for(int i=0; i<deploymentNodeList.Count; i++){
				if(deploymentNodeList[i].unit==null){ hasEmptyNode|=true; break; }
			}
			
			return !hasEmptyNode;
		}
		
		public static bool CheckDeploymentHasUnitOnGrid(){
			return instance.factionList[instance.deployingFacIdx].unitList.Count>0;
		}
		
		public static bool DeployUnit(Node node, int idx=0){ return instance._DeployUnit(node, idx); }
		public bool _DeployUnit(Node node, int idx=0){
			if(node.deployFacID!=factionList[deployingFacIdx].factionID) return false;
			
			if(node.unit!=null){
				if(node.unit.GetFacID()!=deployingFacIdx) return false;
				factionList[deployingFacIdx].unitList.Remove(node.unit);
				factionList[deployingFacIdx].deployingList.Add(node.unit);
				node.unit.transform.position=new Vector3(0, 99999, 0);
				node.unit=null;
				
				//PrevUnitToDeploy();
			}
			else{
				if(factionList[deployingFacIdx].deployingList.Count==0) return false;
				node.unit=factionList[deployingFacIdx].deployingList[idx];
				node.unit.transform.position=node.GetPos();
				node.unit.transform.rotation=Quaternion.Euler(0, factionList[deployingFacIdx].direction, 0);
				node.unit.node=node;
				factionList[deployingFacIdx].unitList.Add(node.unit);
				factionList[deployingFacIdx].deployingList.RemoveAt(idx);
				
				if(factionList[deployingFacIdx].unitList.Count<=1) deployingUnitIdx=0;
			}
			return true;
		}
		public static int EndDeployment(){
			GridIndicator.HideDeployment();
			instance.deployingFacIdx=instance._RequireManualDeployment(instance.deployingFacIdx+1);
			if(instance.deployingFacIdx>=0) instance.NewFactionDeployment();
			return instance.deployingFacIdx;
		}
		
		
		public static bool RequireManualDeployment(){ return instance._RequireManualDeployment()>=0; }
		public int _RequireManualDeployment(int offset=0){
			if(!GameControl.EnableUnitDeployment()) return -1;
			//if(unitDeployed) return -1;
			
			for(int i=0+offset; i<factionList.Count; i++){
				if(!factionList[i].playableFaction) continue;
				if(factionList[i].deployingList.Count>0) return i;
			}
			
			return -1;
		}
		
		public int deployingFacIdx=-1;
		public int deployingUnitIdx=-1;
		public static bool DeployingUnit(){ return instance.deployingFacIdx>=0; }
		
		public static int GetDeployingFacIdx(){ return instance.deployingFacIdx; }
		public static int GetDeployingUnitIdx(){ return instance.deployingUnitIdx; }
		public static void SetDeployingUnitIdx(int idx){ instance.deployingUnitIdx=idx; }
		//~ public static void NextUnitToDeploy(){
			//~ instance.deployingUnitIdx+=1;
			//~ if(instance.deployingUnitIdx>=instance.factionList[deployingFacIdx].deployingList.Count) instance.deployingUnitIdx=0;
		//~ }
		//~ public static void PrevUnitToDeploy(){
			//~ instance.deployingUnitIdx-=1;
			//~ if(instance.deployingUnitIdx<0) instance.deployingUnitIdx=instance.factionList[deployingFacIdx].deployingList.Count-1;
		//~ }
		
		//private bool unitDeployed=false;
		//public static bool UnitDeployed(){ return instance.unitDeployed; }
		
		public void DeployAllUnit(){
			//unitDeployed=true;
			for(int i=0; i<factionList.Count; i++){
				if(GameControl.EnableUnitDeployment() && factionList[i].playableFaction) continue;
				factionList[i].factionID=i;
				factionList[i].DeployUnit();
			}
		}
		
		
		public void InitAITrigger(){
			if(!hasAIInGame) return;
			for(int i=0; i<factionList.Count; i++){
				if(factionList[i].playableFaction) continue;
				for(int n=0; n<factionList[i].unitList.Count; n++) CheckAITrigger(factionList[i].unitList[n]);
			}
		}
		public static void CheckAITrigger(Unit unit){ instance._CheckAITrigger(unit); }
		public void _CheckAITrigger(Unit unit){
			if(!hasAIInGame) return;
			
			for(int i=0; i<factionList.Count; i++){
				if(unit.triggered && factionList[i].playableFaction) continue;
				if(factionList[i].factionID==unit.GetFacID()) continue;
				
				for(int n=0; n<factionList[i].unitList.Count; n++){
					if(unit.triggered && factionList[i].unitList[n].triggered) continue;
					
					float dist=GridManager.GetDistance(unit.node, factionList[i].unitList[n].node);
					if(dist<=factionList[i].unitList[n].GetSight()) factionList[i].unitList[n].triggered=true;
					
					if(dist<=unit.GetSight()){
						unit.triggered=true;
						if(factionList[i].playableFaction) break;
					}
				}
			}
		}
		
		
		public static void StartGame(){ instance._StartGame(); }
		public void _StartGame(){
			for(int i=0; i<factionList.Count; i++) factionList[i].StartGame(i);
			
			if(TurnControl.IsUnitPerTurn()){
				for(int i=0; i<factionList.Count; i++){
					for(int n=0; n<factionList[i].unitList.Count; n++){
						allUnitList.Add(factionList[i].unitList[n]);
						//factionList[i].unitList[n].instanceID=Random.Range(0, 9999);
					}
				}
				allUnitList=SortUnitListByPriority(allUnitList);
			}
			
			if(TurnControl.IsFactionPerTurn()){
				for(int i=0; i<factionList.Count; i++){
					factionList[i].unitList=SortUnitListByPriority(factionList[i].unitList);
				}
			}
			
			InitAITrigger();
		}
		
		
		
		public static bool CheckIfNewTurnEndRound(int turnIdx){
			if(TurnControl.IsUnitPerTurn()){
				return turnIdx>=instance.allUnitList.Count;
			}
			else if(TurnControl.IsFactionPerTurn()){
				return turnIdx>=instance.factionList.Count;
			}
			return false;
		}
		
		public static void EndTurn_UnitPerTurn(int turnIdx){
			//if(turnIdx>=instance.allUnitList.Count) turnIdx=0;
			if(instance.allUnitList[turnIdx]==null || instance.allUnitList[turnIdx].hp<=0){	//in case the unit is destroy by dot or other effect
				TurnControl.EndTurn();
				return;
			}
			
			instance.allUnitList[turnIdx].NewTurn();
			
			if(instance.allUnitList[turnIdx].playableUnit){
				TBSelectUnit(instance.allUnitList[turnIdx]);
				for(int i=0; i<instance.factionList.Count; i++){
					if(instance.factionList[i].factionID==instance.allUnitList[turnIdx].GetFacID()){ 
						TBTK.OnSelectFaction(instance.factionList[i]); break; 
					}
				}
			}
			else{
				AI.MoveUnit(instance.allUnitList[turnIdx]);
			}
			
			//return turnIdx;
		}
		public static void EndTurn_FactionPerTurn(int turnIdx){
			//if(turnIdx>=instance.factionList.Count) turnIdx=0;
			instance.factionList[turnIdx].NewTurn();
			
			if(instance.factionList[turnIdx].playableFaction){
				TBSelectUnit(instance.factionList[turnIdx].unitList[0]);
				TBTK.OnSelectFaction(instance.factionList[turnIdx]);
			}
			else{
				AI.MoveFaction(instance.factionList[turnIdx]);
			}
			
			//return turnIdx;
		}
		
		//~ private bool aiToMoveNext=false;
		//~ private unit unitToMoveNext=false;
		//~ public static Unit NextUnitToMove;
		//~ public static void MoveAI(int turnIdx){
			//~ aiToMoveNext=false;
			
			//~ if(TurnControl.IsUnitPerTurn()){
				//~ AI.MoveUnit(instance.allUnitList[turnIdx]);
			//~ }
			//~ else if(TurnControl.IsFactionPerTurn()){
				//~ AI.MoveFaction(instance.factionList[turnIdx]);
			//~ }
		//~ }
		
		
		
		//to iterate unit ability and  effect CD
		public static IEnumerator EndTurn_IterateCD(){
			for(int i=0; i<instance.factionList.Count; i++){
				for(int n=0; n<instance.factionList[i].unitList.Count; n++){
					instance.factionList[i].unitList[n].IterateCD();
					
					if(TurnControl.WaitForUnitDestroy() && instance.factionList[i].unitList[n].hp<=0) 
						yield return instance.StartCoroutine(instance.factionList[i].unitList[n].DestroyRoutine());
				}
			}
		}
		
		//~ public static bool CanSwitchUnit(){
			//~ if(!TurnControl.AllowUnitSwitching) return false;
			//~ if(TurnControl.IsUnitPerTurn()) return false;
			//~ return true;
		//~ }
		
		
		[Space(8)] public int currentFacID=-1;
		public static int GetCurrentFactionID(){ return instance.currentFacID; }
		
		public static void SelectNextUnit(){ 
			//if(TurnControl.IsUnitPerTurn()){
			//	GameControl.EndTurn();
			//}
			if(TurnControl.IsFactionPerTurn()){
				if(GetSelectedFaction().GetMoveCount()>=TurnControl.GetUnitLimit()) TBSelectUnit(instance.selectedUnit);
				else TBSelectUnit(instance.factionList[instance.currentFacID].GetNextUnitInTurn());
			}
		}
		
		
		public static void TBSelectUnit(Unit unit){ instance._SelectUnit(unit); }	//by non UI-user event
		public static void SelectUnit(Unit unit){ 
			if(!TurnControl.CanSwitchUnit(unit)) return;
			instance._SelectUnit(unit);
		}
		public void _SelectUnit(Unit unit){
			selectedUnit=unit;
			
			if(unit!=null && unit.playableUnit){

			    if (InvetoryPlayers.Singleton.ConnectPrefabIdToIndex.ContainsKey(unit.prefabID))
			    {
			        var index = InvetoryPlayers.Singleton.ConnectPrefabIdToIndex[unit.prefabID];
			        CharWindows.Singleton.Show(index);
                }
			    else
			    {
			        CharWindows.Singleton.CloseAll();
                }

                //selectIndicator.position=selectedUnit.GetPos();

                currentFacID =instance.selectedUnit.GetFacID();
				factionList[currentFacID].UpdateTurnIdx(selectedUnit);
				
				GridManager.SelectUnit(unit);
				
				GridIndicator.SetSelect(unit.node);
				GridIndicator.ShowMovable(GridManager.GetWalkableList());
				GridIndicator.ShowHostile(GridManager.GetAttackableList());
				
				selectedUnit.AudioPlaySelect();
			}
			else{
			    CharWindows.Singleton.CloseAll();
                GridIndicator.HideAll();
				GridIndicator.SetSelect(null);
				//else selectIndicator.position=new Vector3(0, 9999, 0);
			}
			
			TBTK.OnSelectUnit(unit);
		}
		
		public static List<Unit> SortUnitListByPriority(List<Unit> list){
			List<Unit> newList=new List<Unit>();
			while(list.Count>0){
				int highestIdx=0;
				float highestVal=0;
				for(int i=0; i<list.Count; i++){
					float turnPriority=list[i].GetTurnPriority();
					if(turnPriority>highestVal || turnPriority==highestVal && Random.value<0.5f){
						highestVal=turnPriority;
						highestIdx=i;
					}
				}
				newList.Add(list[highestIdx]);
				list.RemoveAt(highestIdx);
			}
			return newList;
		}
		
		
		private IEnumerator SelectedUnitDestroyed(){
			while(GameControl.ActionInProgress() || AI.ActionInProgress()) yield return null;
			if(TurnControl.IsFactionPerTurn()) SelectNextUnit();
			else if(TurnControl.IsUnitPerTurn()) GameControl.EndTurn();
		}
		
		public static void UnitDestroyed(Unit unit){ instance._UnitDestroyed(unit); }
		public void _UnitDestroyed(Unit unit){
			if(selectedUnit==unit){
				StartCoroutine(SelectedUnitDestroyed());
				//~ Debug.Log("selected unit is destroyed");
				//~ if(TurnControl.IsFactionPerTurn()) SelectNextUnit();
				//~ else if(TurnControl.IsUnitPerTurn()) GameControl.EndTurn();
				//if(TurnControl.IsUnitPerTurn()) 
			}
			
			if(TurnControl.IsUnitPerTurn()){
				if(allUnitList.IndexOf(unit)<=TurnControl.GetTurn()) TurnControl.RevertTurn();
				allUnitList.Remove(unit);
			}
			
			TBTK.OnUnitDestroyed(unit);
			
			bool factionCleared=factionList[unit.GetBaseFacID()].RemoveUnit(unit);
			if(unit.tempFacID>=0) factionCleared|=factionList[unit.tempFacID].RemoveUnit(unit);
			if(!factionCleared) return;
			
			int facWithUnitCount=0;
			int facIdxWithUnit=-1;
			for(int i=0; i<factionList.Count; i++){
				if(factionList[i].unitList.Count>0){
					facIdxWithUnit=i;
					facWithUnitCount+=1;
				}
			}
			
			if(facWithUnitCount<=1) GameControl.GameOver(factionList[facIdxWithUnit]);
		}
		
		public static void GameOver(){
			for(int i=0; i<instance.factionList.Count; i++) instance.factionList[i].CacheUnit();
		}
		
		
		
		public static List<Unit> GetAllHostileUnits(int facID){
			List<Unit> list=new List<Unit>();
			
			for(int i=0; i<instance.factionList.Count; i++){
				if(instance.factionList[i].factionID==facID) continue;
				list.AddRange(instance.factionList[i].unitList);
			}
			
			return list;
		}
		
		public static List<Unit> GetAllUnits(){
			if(TurnControl.IsUnitPerTurn()) return instance.allUnitList;
			
			List<Unit> list=new List<Unit>();
			for(int i=0; i<instance.factionList.Count; i++) list.AddRange(instance.factionList[i].unitList);
			return list;
		}
		
		
		public static void AddUnit(Unit unit, int facID){
			for(int i=0; i<instance.factionList.Count; i++){
				if(instance.factionList[i].factionID!=facID) continue;
				instance.factionList[i].AddUnit(unit);
				break;
			}
		}
		
		
		public static void AddAbilityToPlayerFaction(int abilityPID){
			foreach(Faction fac in instance.factionList){
				if(!fac.playableFaction) continue;
				fac.AddAbility(abilityPID);
			}
		}
		public static void AddAbilityToPlayerUnit(int abilityPID, List<int> unitPIDList){
			foreach(Faction fac in instance.factionList){
				if(!fac.playableFaction) continue;
				foreach(Unit unit in fac.unitList){
					if(unitPIDList!=null && !unitPIDList.Contains(unit.prefabID)) continue;
					unit.AddAbility(abilityPID);
				}
			}
		}
		
		
		public static void AddFacSwitchUnit(Unit unit){
			for(int i=0; i<instance.factionList.Count; i++){
				if(instance.factionList[i].factionID!=unit.tempFacID) continue;
				instance.factionList[i].unitList.Add(unit);
				//factionList.tempUnitList.Add(unit);
				break;
			}
		}
		public static void RemoveFacSwitchUnit(Unit unit){
			for(int i=0; i<instance.factionList.Count; i++){
				if(instance.factionList[i].factionID!=unit.tempFacID) continue;
				instance.factionList[i].unitList.Remove(unit);
				//factionList.tempUnitList.Remove(unit);
				break;
			}
		}
		
		
		//only used in editor 
		public static Unit PlaceUnit(GameObject unitPrefab, Node node, float dir, bool spawn=true){
			//GameObject obj=(GameObject)MonoBehaviour.Instantiate(unitPrefab, nodeList[rand].GetPos(), Quaternion.identity);
			
			GameObject obj;
			Quaternion rot=Quaternion.Euler(0, dir, 0);
			
			if(spawn){
				//~ #if UNITY_EDITOR
					//~ obj=(GameObject)PrefabUtility.InstantiatePrefab(unitPrefab);
					//~ obj.transform.position=node.GetPos();
				//~ #else
					
					obj=(GameObject)MonoBehaviour.Instantiate(unitPrefab, node.GetPos(), rot);
				//~ #endif
			}
			else{
				obj=unitPrefab;
				obj.transform.position=node.GetPos();
				obj.transform.rotation=rot;
			}
			
			Unit unit=obj.GetComponent<Unit>();
			unit.node=node;		node.unit=unit;
			return unit;
		}
		
		
		//for gizmo
		public static Color GetFacColor(int idx){	
			if(instance==null) instance=(UnitManager)FindObjectOfType(typeof(UnitManager));
			return instance.factionList[idx].color;
		}
		
		
		
		public static List<Faction> cachedFactionList=new List<Faction>();
		
		public static void ClearCache(){ cachedFactionList.Clear(); }
		public static void CacheFaction(int factionID, List<Unit> unitList, bool postBattle=false){
			int index=CheckCachedFaction(factionID);
			
			if(postBattle){
				for(int i=0; i<unitList.Count; i++) unitList[i]=UnitDB.GetPrefab(unitList[i].prefabID);
			}
			
			if(index>=0){
				cachedFactionList[index].unitList=unitList;
			}
			else{
				Faction fac=new Faction();
				fac.factionID=factionID;
				fac.unitList=unitList;
				cachedFactionList.Add(fac);
			}
		}
		
		public static int CheckCachedFaction(int factionID){
			for(int i=0; i<cachedFactionList.Count; i++){
				if(cachedFactionList[i].factionID==factionID) return i;
			}
			return -1;
		}
		public static List<Unit> GetCachedUnitList(int index){
			return ((index>=0 && cachedFactionList.Count>index) ? cachedFactionList[index].unitList : new List<Unit>() );
		}
	}
	
	
	//~ public class CacheFaction{
		//~ public int factionID;
		//~ public List<CacheUnit> unitList=new List<CacheUnit>();
	//~ }
	//~ public class CacheUnit{
		//~ public float Unit;
		//~ public float HP;
	//~ }
	

	[System.Serializable]
	public class Faction{
		//[HideInInspector] 
		public int factionID=0;
		[HideInInspector] public int factionIdx=-1;
		
		public string name="Faction";
		public Color color;	//not used in runtime
		public float direction;
		
		public int turnIdx;		//indicate which unit in the faction is being selected right now, used in FactionPerTurn only
		public void UpdateTurnIdx(Unit unit){ turnIdx=unitList.IndexOf(unit); }
		
		[HideInInspector]
		public int movedUnitCount=0;
		
		public bool playableFaction=true;
		
		public List<Unit> unitList=new List<Unit>();
		
		public List<Unit> startingUnitList=new List<Unit>();
		public List<Unit> deployingList=new List<Unit>();
		
		public List<SpawnGroup> spawnGroupList=new List<SpawnGroup>();
		
		
		public bool loadFromData;
		public bool saveToData;
		public bool saveLoadedUnitOnly;
		//public int cachedFacID=0;	//the factionID as assigned in editor
		
		
		[HideInInspector] //for grid regeneration
		public List<Vector3> deploymentPointList=new List<Vector3>();
		
		
		public void CacheUnit(){
			if(!saveToData) return;
			
			if(!saveLoadedUnitOnly){
				UnitManager.CacheFaction(factionID, unitList, false);
				return;
			}
			
			List<Unit> newList=new List<Unit>();
			for(int i=0; i<unitList.Count; i++){
				if(unitList[i].loadedFromCache) newList.Add(unitList[i]);
			}
			UnitManager.CacheFaction(factionID, newList, true);
		}
		
		
		public void Init(int facID){
			factionIdx=facID;
			
			bool loadedFromCache=false;
			if(loadFromData){
				int index=UnitManager.CheckCachedFaction(factionID);
				if(index>=0){
					startingUnitList=UnitManager.GetCachedUnitList(index);
					loadedFromCache=true;
				}
			}
			
			for(int i=0; i<startingUnitList.Count; i++){
				if(startingUnitList[i]==null) continue;
				GameObject unitObj=(GameObject)MonoBehaviour.Instantiate(startingUnitList[i].gameObject, new Vector3(0, 99999, 0), Quaternion.identity);
				deployingList.Add(unitObj.GetComponent<Unit>());
				if(loadedFromCache) deployingList[deployingList.Count-1].loadedFromCache=true;
			}
			
			for(int i=0; i<deployingList.Count; i++){
				deployingList[i].SetFacID(factionID);
				deployingList[i].playableUnit=playableFaction;
			}
			
			for(int i=0; i<unitList.Count; i++){
				unitList[i].SetFacID(factionID);
				unitList[i].playableUnit=playableFaction;
			}
			
			//abilityIDList=new List<int>{ 0, 1 };
			for(int i=0; i<abilityIDList.Count; i++) AddAbility(abilityIDList[i]);
		}
		public void AddAbility(int abPrefabID){
			abilityList.Add(AbilityFDB.GetPrefab(abPrefabID).Clone());
			abilityList[abilityList.Count-1].Init(factionID, abilityList.Count-1);
		}
		
		public void AddUnit(Unit unit){	//add unit during runtime (mid-game)
			unit.SetFacID(factionID);
			unit.playableUnit=playableFaction;
			unit.NewTurn();
			unitList.Add(unit);
		}
		
		public void DeployUnit(){
			List<Node> nodeList=GridManager.GetDeploymentNode(factionID);
			
			int count=deployingList.Count;
			for(int i=0; i<count; i++){
				if(nodeList.Count==0) break;
				
				int rand=Random.Range(0, nodeList.Count);
				
				Unit unit=UnitManager.PlaceUnit(deployingList[0].gameObject, nodeList[rand], direction, false);
				unitList.Add(unit);
				
				nodeList.RemoveAt(rand);		deployingList.RemoveAt(0);
			}
		}
		
		public void StartGame(int facID){
			for(int i=0; i<unitList.Count; i++){
				unitList[i].NewTurn(true);
			}
		}
		
		public bool RemoveUnit(Unit unit){
			unitList.Remove(unit);
			return unitList.Count==0;
		}
		
		public void NewTurn(){
			turnIdx=0;
			for(int i=0; i<unitList.Count; i++) unitList[i].NewTurn();
			
			movedUnitCount=0;
		}
		
		public void SelectUnit(Unit unit){
			UpdateTurnIdx(unit);
		}
		
		public Unit GetNextUnitInTurn(){
			if(unitList.Count==0) return null;
			int newIdx=(turnIdx+1)%unitList.Count;
			return unitList[newIdx];
		}
		
		
		public int GetMoveCount(){	//only used in factionPerTurn
			int count=0;
			for(int i=0; i<unitList.Count; i++) count+=unitList[i].HasMoved() ? 1 : 0 ;
			return count;
		}
		
		
		
		public List<int> abilityIDList=new List<int>();
		public List<Ability> abilityList=new List<Ability>();	//runtime attribute
		public Ability GetAbility(int idx){ return abilityList[idx]; }
		
		public int SelectAbility(int idx){
			int usable=abilityList[idx].IsAvailable();
			if(usable!=0) return usable;
			
			if(!abilityList[idx].requireTarget){
				UseAbility(idx, null);
			}
			else{
				AbilityManager.AbilityTargetModeFac(this, abilityList[idx]);
			}
			
			return 0;
		}
		
		public void UseAbility(int idx, Node target){ 
			GameControl.FactionUseAbility(this, abilityList[idx], target);
			//StartCoroutine(_UseAbility(abilityList[idx], target)); 
		}
		public IEnumerator UseAbilityRoutine(Ability ability, Node target){
			Debug.Log("UseAbilityRoutine   "+target);
			ability.Activate();
			
			//ap-=ability.apCost;
			
			//~ yield return CRoutine.Get().StartCoroutine(AbilityHit(ability, target));
			yield return CRoutine.Get().StartCoroutine(ability.HitTarget(target));
		}
		
		//~ public IEnumerator AbilityHit(Ability ability, Node target){
			//~ ability.HitTarget(target);
		//~ }
	}
	
	
	[System.Serializable]
	public class SpawnGroup{
		public int ID;
		
		public int countMin=5;
		public int countMax=5;
		
		public List<Unit> unitList=new List<Unit>();
		public List<int> unitCountMinList=new List<int>();
		public List<int> unitCountMaxList=new List<int>();
		
		[HideInInspector] //for grid regeneration
		public List<Vector3> spawnPointList=new List<Vector3>();
		
		public void Spawn(Faction fac){
			//use a dummy list so the original list wont get altered if this is run in EditMode
			List<Unit> cloneUnitList=new List<Unit>( unitList );	
			List<int> cloneMinList=new List<int>( unitCountMinList );
			List<int> cloneMaxList=new List<int>( unitCountMaxList );
			List<Unit> spawnedUnitList=new List<Unit>();	
			
			List<Node> nodeList=GridManager.GetSpawnGroup(fac.factionID, ID);
			int limit=(int)Mathf.Min(nodeList.Count, Rand.Range(countMin, countMax));		int currentCount=0;
			
			//loop through the unitlist, place the limited amount of them
			while(nodeList.Count>0 && currentCount<limit){
				bool hasMinimum=false;
				for(int i=0; i<cloneUnitList.Count; i++){
					if(nodeList.Count==0) break;
					if(unitCountMinList[i]<=0) continue;
					
					int nodeIdx=Rand.Range(0, nodeList.Count);
					spawnedUnitList.Add(UnitManager.PlaceUnit(cloneUnitList[i].gameObject, nodeList[nodeIdx], fac.direction));
					nodeList.RemoveAt(nodeIdx);		currentCount+=1;
					
					cloneMinList[i]-=1;	cloneMaxList[i]-=1;
					
					if(cloneMinList[i]<=0 || cloneMaxList[i]<=0){
						cloneUnitList.RemoveAt(i); 	cloneMinList.RemoveAt(i); 	cloneMaxList.RemoveAt(i); 	i-=1;
						continue;
					}
				}
				if(!hasMinimum) break;
			}
			
			while(currentCount<limit && nodeList.Count>0 && cloneUnitList.Count>0){
				int unitIdx=Rand.Range(0, cloneUnitList.Count);
				int nodeIdx=Rand.Range(0, nodeList.Count);
				
				spawnedUnitList.Add(UnitManager.PlaceUnit(cloneUnitList[unitIdx].gameObject, nodeList[nodeIdx], fac.direction));
				nodeList.RemoveAt(nodeIdx);		currentCount+=1;
				
				cloneMaxList[unitIdx]-=1;
				if(cloneMaxList[unitIdx]<=0){
					cloneUnitList.RemoveAt(unitIdx);	cloneMaxList.RemoveAt(unitIdx);
					continue;
				}
			}
			
			for(int i=0; i<spawnedUnitList.Count; i++) fac.unitList.Add(spawnedUnitList[i]);
		}
		
	}
	
	
}