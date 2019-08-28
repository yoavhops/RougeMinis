﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TBTK{
	
	
	public enum _GridType{ SquareGrid, HexGrid, }
	public enum _GridColliderType{ Master, Individual, }
	
	
	[RequireComponent(typeof(GridGenerator))] [ExecuteInEditMode]
	public class GridManager : MonoBehaviour {
		
		#if UNITY_EDITOR
		public static bool inspector=false;
		#endif
		
		public bool generateGridOnStart=false;
		public bool generateUnitOnStart=false;
		public bool generateCollectibleOnStart=false;
		
		
		[Space(8)]
		public _GridType gridType;
		public static _GridType GetGridType(){ return instance.cacheGridType; }
		
		public static bool IsSquareGrid(){ return GetGridType()==_GridType.SquareGrid; }
		public static bool IsHexGrid(){ return GetGridType()==_GridType.HexGrid; }
		
		public _GridColliderType colliderType;
		public static bool UseMasterCollider(){ return instance.cacheColliderType==_GridColliderType.Master; }
		public static bool UseIndividualCollider(){ return instance.cacheColliderType==_GridColliderType.Individual; }
		
		
		public enum _RangeCalculation{ ByNode, ByDistance }
		public _RangeCalculation rangeCalculation;
		
		
		public bool enableDiagonalNeighbour=false;
		public static bool EnableDiagonalNeighbour(){ return instance.cacheDiagonalNeighbour; }
		
		
		public float nodeSize=1;
		//public static float GetBaseNodeSize(){ return instance.cacheNodeSize; }
		
		public float nodeSpacing=0.1f;
		public static float GetNodeSpacing(){ return instance.cacheNodeSpacing; }
		
		public static float GetNodeSize(bool addSpacing=true){ return instance._GetNodeSize(addSpacing); }
		public float _GetNodeSize(bool addSpacing=true){ 
			return cacheNodeSize * (IsHexGrid() ? 1.15f : 1) + (addSpacing ? cacheNodeSpacing : 0);
		}
		
		
		public static void ResetGrid(){
			for(int x=0; x<instance.grid.Count; x++){
				for(int z=0; z<instance.grid[x].Count; z++){
					instance.grid[x][z].ResetListState();
				}
			}
		}
		
		[Space(8)]
		public int dimensionX=50;		public static int DimensionX(){ return instance.cacheDimensionX; }
		public int dimensionZ=50;		public static int DimensionZ(){ return instance.cacheDimensionZ; }
		public float unwalkableRate=0.1f;
		public float obstacleRate=0.1f;
		public float wallRate=0.2f;
		
		[Space(8)]	//we need ListWrapper because unity will not serialize a list of list (grid)
		public List<ListWrapper> gridT =new List<ListWrapper>();
		public List<List<Node>> grid=new List<List<Node>>();		
		[HideInInspector] public float offsetX;
		[HideInInspector] public float offsetZ;
		
		public float GetOffsetX(){ 
			if(IsSquareGrid()) offsetX=DimensionX()*GetNodeSize()*.5f-GetNodeSize()*.5f;	
			if(IsHexGrid()) offsetX=DimensionX()*GetNodeSize()*GridGenerator.spaceXHex/2f-0.5f*GetNodeSize()*GridGenerator.spaceXHex;	
			return offsetX;
		}
		public float GetOffsetZ(){ 
			if(IsSquareGrid()) offsetZ=DimensionZ()*GetNodeSize()*.5f-nodeSize*.5f;	
			if(IsHexGrid()) offsetZ=DimensionZ()*GetNodeSize()*GridGenerator.spaceZHex/2f-0.5f*GetNodeSize()*GridGenerator.spaceZHex;	
			return offsetZ;
		}
		
		
		
		
		
		[HideInInspector] public _GridType cacheGridType;
		[HideInInspector] public _GridColliderType cacheColliderType;
		[HideInInspector] public bool cacheDiagonalNeighbour;
		[HideInInspector] public float cacheNodeSize;
		[HideInInspector] public float cacheNodeSpacing;
		[HideInInspector] public int cacheDimensionX;
		[HideInInspector] public int cacheDimensionZ;
		
		public void CacheGridSetting(){
			cacheGridType=gridType;
			cacheColliderType=colliderType;
			cacheDiagonalNeighbour=enableDiagonalNeighbour;
			cacheNodeSize=nodeSize;
			cacheNodeSpacing=nodeSpacing;
			cacheDimensionX=dimensionX;
			cacheDimensionZ=dimensionZ;
		}
		
		
		[Space(8)]	public GameObject masterColliderObj;
		public void SetupMasterCollider(){
			if(!UseMasterCollider()){
				if(masterColliderObj!=null) masterColliderObj.SetActive(false);
				return;
			}
			
			if(masterColliderObj==null){
				masterColliderObj=GameObject.CreatePrimitive(PrimitiveType.Quad);
				masterColliderObj.transform.parent=transform;
				masterColliderObj.transform.position=Vector3.zero;
				masterColliderObj.transform.rotation=Quaternion.Euler(90, 0, 0);
				masterColliderObj.name="Collider";	masterColliderObj.layer=30;
				masterColliderObj.GetComponent<MeshRenderer>().enabled=false;
			}
			
			masterColliderObj.transform.localScale=new Vector3(DimensionX()*GetNodeSize(),  DimensionZ()*GetNodeSize(), 1);
			masterColliderObj.SetActive(true);
		}
		
		
		public static List<Vector3> nodeDirList=new List<Vector3>();
		public static void InitNodeDirList(){
			nodeDirList=new List<Vector3>();
			if(GridManager.IsHexGrid()){
				nodeDirList.Add(new Vector3(.866f, 0, .5f));
				nodeDirList.Add(new Vector3(0, 0, 1));
				nodeDirList.Add(new Vector3(-.866f, 0, .5f));
				nodeDirList.Add(new Vector3(-.866f, 0, -.5f));
				nodeDirList.Add(new Vector3(0, 0, -1));
				nodeDirList.Add(new Vector3(.866f, 0, -.5f));
			}
			else if(GridManager.IsSquareGrid()){
				nodeDirList.Add(new Vector3(1, 0, 0));
				nodeDirList.Add(new Vector3(0, 0, 1));
				nodeDirList.Add(new Vector3(-1, 0, 0));
				nodeDirList.Add(new Vector3(0, 0, -1));
			}
		}
		
		
		
		public static void SetupFogOfWar(){ instance._SetupFogOfWar(); }
		public void _SetupFogOfWar(){
			if(!GameControl.EnableFogOfWar()) return;
			
			List<Unit> unitList=new List<Unit>( UnitManager.GetAllUnits() );
			for(int i=0; i<unitList.Count; i++)
			{

			    var facId = TurnControl.GetTurn();

                if (unitList[i].facID == facId) continue;
				unitList.RemoveAt(i);	i-=1;
			}
			
			for(int x=0; x<grid.Count; x++){
				for(int z=0; z<grid[x].Count; z++){
					bool visible=false;
					for(int i=0; i<unitList.Count; i++){
						visible=CheckLOS(unitList[i].node, grid[x][z], unitList[i].GetSight());
						if(visible) break;
					}
					grid[x][z].SetVisible(visible);
				}
			}
			
			GridIndicator.ShowFog();
		}
		public static bool CheckLOS(Node node1, Node node2, int sight, bool debugging=false){
			if(debugging) Debug.Log("CheckLOS  "+node1.GetPos()+"  "+node2.GetPos()+"   "+GridManager.GetDistance(node1, node2));
			
			if(GridManager.GetDistance(node1, node2)>sight) return false;
			
			if(debugging) Debug.Log("  ___ CheckLOS  ");
			
			List<Node> nList1=new List<Node>{ node1 };
			List<Node> nList2=new List<Node>{ node2 };

			if(GameControl.EnableSideStepping()){
				if(GridManager.IsSquareGrid()){
					nList1.AddRange(GetSideStepNeighbours(node1, node2));
					nList2.AddRange(GetSideStepNeighbours(node2, node1));
				}
				else{
					nList1.AddRange(node1.GetNeighbourList(true));		//n1 is the origin node, 
					//nList2.AddRange(node2.GetNeighbourList(true));
				}
			}

			if(debugging) Debug.Log("  ___ CheckLOS  "+nList1.Count+"   "+nList2.Count);
			
			for(int i=0; i<nList1.Count; i++){
				for(int n=0; n<nList2.Count; n++){
					if(GridManager.GetDistance(nList1[i], nList2[n])>sight) continue;
					bool visible=!LOSCast(nList1[i].GetPos(), nList2[n].GetPos());
					if(debugging) Debug.DrawLine(nList1[i].GetPos(), nList2[n].GetPos(), visible ? Color.white : Color.red, 1);
					if(visible) return true;
				}
			}
			
			if(debugging) Debug.Log("  ___ CheckLOS   pass all     false");
			
			return false;
		}
		public static bool LOSCast(Vector3 pos1, Vector3 pos2, bool debugging=false){
			InitMask();
			return Physics.Linecast(pos1, pos2, losMask);
		}
		private static LayerMask losMask;		public static bool initMask;
		public static void InitMask(){
			if(initMask) return;
			initMask=true;
			losMask=1<<TBTK.GetLayerObsFullCover();
		}
		
		public static List<Node> GetSideStepNeighbours(Node srcNode, Node tgtNode){
			List<Node> list=new List<Node>();
			Node n1=GetSideStepNeighbour(srcNode, tgtNode, 1);		if(n1!=null) list.Add(n1);
			Node n2=GetSideStepNeighbour(srcNode, tgtNode, -1);		if(n2!=null) list.Add(n2);
			return list;
		}
		public static Node GetSideStepNeighbour(Node srcNode, Node tgtNode, float mul){
			Vector3 dir=(tgtNode.GetPos()-srcNode.GetPos()).normalized;
			Vector3 dirP=new Vector3(dir.z, 0, -dir.x);
			
			Node neighbour=srcNode.GetNeighbourFromPos(srcNode.GetPos()+(mul*dirP)*GridManager.GetNodeSize());
			if(neighbour!=null && !neighbour.IsBlocked(srcNode)) return neighbour;
			
			return null;
		}
		
		public static void SetupFogOfWarForDeployment(List<Node> dNodeList, int factionID){
			if(!GameControl.EnableFogOfWar()) return;
			
			//Faction fac=UnitManager.GetFaction(UnitManager.GetDeployingFacIdx());
			//List<Node> dNodeList=GetDeploymentNode(deployFacID);
			
			List<Node> visibleList=new List<Node>();
			
			for(int x=0; x<instance.grid.Count; x++){
				for(int z=0; z<instance.grid[x].Count; z++){
					Node node=instance.grid[x][z];
					
					//Node node=instance.grid[x][z];
					//node.SetVisible((node.unit!=null && node.unit.facID==factionID) || dNodeList.Contains(node));
					
					if(dNodeList.Contains(node) || (node.unit!=null && node.unit.facID==factionID)){
						List<Node> neighbours=node.GetNeighbourList();
						for(int n=0; n<neighbours.Count; n++){
							neighbours[n].SetVisible(true);
							if(!visibleList.Contains(neighbours[n])) visibleList.Add(neighbours[n]);
						}
						
						node.SetVisible(true);
						continue;
					}
					else node.SetVisible(visibleList.Contains(node));
				}
			}
		}
		
		private List<Node> scannedNodeList=new List<Node>();		//list of node under the effect of scan-fog-of-war ability
		public static void AddScannedNode(Node node){ instance.scannedNodeList.Add(node); }
		public static void EndTurn(){ 
			for(int i=0; i<instance.scannedNodeList.Count; i++){
				if(instance.scannedNodeList[i].IterateFogOfWarCD()){
					instance.scannedNodeList.RemoveAt(i);	i-=1;
				}
			}
		}
		
		
		
		private GridGenerator gridGenerator;
		
		private static GridManager instance;
		public static GridManager GetInstance(){ return instance; }
		public void SetupInstance(){ instance=this; }	//called by GridGenerator
		
		public static void Init(){
			if(instance==null) instance=(GridManager)FindObjectOfType(typeof(GridManager));
			
			instance.gridGenerator=instance.transform.GetComponent<GridGenerator>();
			instance.gridGenerator.Init();
			instance.InitGrid();
			
			if(Application.isPlaying) instance.gridGenerator.gridParent.GetComponent<CombineMeshes>().Combine();
			
			GridIndicator indicator=(GridIndicator)FindObjectOfType(typeof(GridIndicator));
			indicator.Awake();
		}
		
		void InitGrid(){
			InitNodeDirList();
			
			for(int i=0; i<gridT.Count; i++) grid.Add(gridT[i].list);
			if(Application.isPlaying) gridT.Clear();
			
			if(Application.isPlaying && generateGridOnStart) gridGenerator.Generate();
			
			for(int x=0; x<grid.Count; x++){
				for(int z=0; z<grid[x].Count; z++){
					if(grid[x][z].unit!=null) grid[x][z].unit.node=grid[x][z];
					if(grid[x][z].collectible!=null) grid[x][z].collectible.node=grid[x][z];
				}
			}
			
			for(int x=0; x<grid.Count; x++){
				for(int z=0; z<grid[x].Count; z++){
					grid[x][z].SetupNeighbour();
				}
			}
			
			for(int x=0; x<grid.Count; x++){
				for(int z=0; z<grid[x].Count; z++){
					grid[x][z].SetupWall();
				}
			}
			
			if(Application.isPlaying){
				if(generateUnitOnStart) gameObject.GetComponent<GridGenerator>().GenerateUnit();
				if(generateCollectibleOnStart) gameObject.GetComponent<GridGenerator>().GenerateCollectible();
			}
			
			if(Application.isPlaying && GameControl.EnableCoverSystem()) instance.StartCoroutine(InitGrid_Delayed());
		}
		private IEnumerator InitGrid_Delayed(){
			yield return null;
			
			for(int x=0; x<grid.Count; x++){
				for(int z=0; z<grid[x].Count; z++){
					grid[x][z].InitCover();
				}
			}
		}
		
		
		
		void Update(){
			if(instance==null && !Application.isPlaying){
				instance=this;
				InitGrid();
			}
		}
		
		
		
		public static List<List<Node>> GetGrid(){ return instance.grid; }
		public static int GetNodeCount(){ return instance.dimensionX * instance.dimensionZ ; }
		
		public static Node GetNode(int x, int z){ 
			if(x<0 || x>=instance.grid.Count) return null;
			if(z<0 || z>=instance.grid[x].Count) return null;
			
			return instance.grid[x][z];
		}
		public static Node GetNode(Vector3 point, GameObject obj){
			if(UseIndividualCollider()){
				for(int x=0; x<instance.grid.Count; x++){
					for(int z=0; z<instance.grid[x].Count; z++){
						if(instance.grid[x][z].objT.gameObject==obj) return instance.grid[x][z];
					}
				}
			}
			
			if(IsSquareGrid()){
				Vector2 coor=new Vector2(point.x+instance.offsetX, point.z+instance.offsetZ)/GetNodeSize();
				return GetNode((int)Mathf.Round(coor.x), (int)Mathf.Round(coor.y));
			}
			else{
				Vector2 coor=new Vector2(point.x+instance.offsetX, point.z+instance.offsetZ)/GetNodeSize();
				int x=(int)Mathf.Round(coor.x/GridGenerator.spaceXHex);
				coor.x/=GridGenerator.spaceXHex;
				
				float offset=(x%2==1 ? 0 : GridGenerator.spaceZHex*GetNodeSize()*0.5f);
				coor.y=(coor.y)/GridGenerator.spaceZHex-offset*0.5f;		
				//Debug.Log(coor.x.ToString("f2")+"   "+coor.y.ToString("f2")+"    "+offset);
				
				return GetNode((int)Mathf.Round(coor.x), (int)Mathf.Round(coor.y));
			}
		}
		
		
		public static int GetDistance(Node n1, Node n2, bool searchAStar=false){
			if(!searchAStar){
				if(IsSquareGrid()){
					if(EnableDiagonalNeighbour()){
						float deltaX=Mathf.Abs(n1.idxX-n2.idxX);
						float deltaZ=Mathf.Abs(n1.idxZ-n2.idxZ);
						return (int)Mathf.Max(deltaX, deltaZ);
					}
					return (int)Mathf.Abs(n1.idxX-n2.idxX)+(int)Mathf.Abs(n1.idxZ-n2.idxZ);
				}
				else{
					float x=Mathf.Abs(n1.x-n2.x);
					float y=Mathf.Abs(n1.y-n2.y);
					float z=Mathf.Abs(n1.z-n2.z);
					return (int)((x + y + z)/2);
				}
			}
			else return AStar.SearchWalkableNode(n1, n2).Count;
		}
		
		public static float GetAngle(Node n1, Node n2, bool round){ return GetAngle(n1.GetPos(), n2.GetPos(), round); }
		public static float GetAngle(Vector3 p1, Vector3 p2, bool round){
			float angle=Utility.Vector2ToAngle(new Vector2(p2.x-p1.x, p2.z-p1.z).normalized);
			if(round){
				if(IsSquareGrid()) angle=Utility.RoundAngleTo90(angle);
				else if(IsHexGrid()) angle=Utility.RoundAngleTo60(angle);
			}
			return angle;
		}
		
		
		public static List<Node> GetDeploymentNode(int facID){ return instance._GetDeploymentNode(facID); }
		public List<Node> _GetDeploymentNode(int facID){
			List<Node> list=new List<Node>();
			for(int x=0; x<grid.Count; x++){
				for(int z=0; z<grid[x].Count; z++){
					if(!grid[x][z].walkable || !grid[x][z].IsEmpty()) continue;
					if(grid[x][z].deployFacID==facID) list.Add(grid[x][z]);
				}
			}
			return list;
		}
		
		
		public List<Node> walkableList=new List<Node>();
		public List<Node> attackableList=new List<Node>();
		public static List<Node> GetWalkableList(){ return instance.walkableList; }
		public static List<Node> GetAttackableList(){ return instance.attackableList; }
		
		public static void SelectUnit(Unit unit){ SetupWalkableList(unit);	SetupAttackableList(unit);	}
		public static List<Node> SetupWalkableList(Unit unit){
			instance.walkableList.Clear();
			if(!unit.CanMove()) return null;
			instance.walkableList=GetNodesWithinDistance(unit.node, unit.GetMoveRange(), true);
			return instance.walkableList;
		}
		public static List<Node> SetupAttackableList(Unit unit){
			instance.attackableList=GetAttackableList(unit, unit.node);
			return instance.attackableList;
			//instance.attackableList.Clear();
			//List<Node> list=GetNodesWithinDistance(unit.node, unit.GetAttackRange(), false);
			//for(int i=0; i<list.Count; i++){
			//	if(list[i].unit!=null && list[i].unit.facID!=unit.facID) instance.attackableList.Add(list[i]);
			//}
		}
		public static void ClearSelectUnit(){
			GridIndicator.HideAll();
			instance.walkableList=new List<Node>();	instance.attackableList=new List<Node>();
		}
		
		public static List<Node> GetAttackableList(Unit unit, Node tgtNode){
			if(!unit.CanAttack()) return new List<Node>();
			
			List<Node> list=GetNodesWithinDistance(tgtNode, unit.GetAttackRange(), false);
			List<Node> tgtList=new List<Node>();
			
			float minAttackRange=unit.GetAttackRangeMin();
			float meleeAttackRange=unit.hasMeleeAttack ? unit.GetAttackRangeMelee() : 0;
			
			for(int i=0; i<list.Count; i++){
				if(list[i].unit==null) continue;
				if(!list[i].IsVisible()) continue; 
				if(OnSameFac(list[i].unit, unit)) continue;
				
				if(minAttackRange>0){
					int dist=GridManager.GetDistance(list[i].unit.node, unit.node);
					if(meleeAttackRange<=0){
						if(minAttackRange>0 && dist<minAttackRange) continue;
					}
					else{
						if(dist>meleeAttackRange && dist<minAttackRange) continue;
					}
				}
				
				if(unit.requireLOSToAttack && !CheckLOS(unit.node, list[i], unit.GetSight())) continue;
				tgtList.Add(list[i]);
			}
			return tgtList;
		}
		
		
		public static bool CanMoveTo(Node node){ return instance.walkableList!=null && instance.walkableList.Contains(node); }
		public static bool CanAttack(Node node){ return node.unit!=null && instance.attackableList!=null && instance.attackableList.Contains(node); }
		
		public static void PreviewAttackableNode(Node node){ instance._PreviewAttackableNode(node); }
		public void _PreviewAttackableNode(Node node){
			bool preview=node!=null && GridManager.CanMoveTo(node);
			preview&=(UnitManager.GetSelectedUnit()!=null && UnitManager.GetSelectedUnit().CanAttack());
			
			if(preview) GridIndicator.PreviewHostile(GetAttackableList(UnitManager.GetSelectedUnit(), node));
			else GridIndicator.ClearPreviewHostile();
		}
		
		
		
		public List<Node> abilityTargetList=new List<Node>();
		public static bool InAbilityTargetList(Node node){ return instance.abilityTargetList.Contains(node); }
		
		public static void SetupAbilityTargetList(Faction fac, Ability ability){
			List<Node> list=new List<Node>();
			for(int x=0; x<instance.grid.Count; x++) list.AddRange(instance.grid[x]);
			SetupAbilityTargetList(ability, list, fac.factionID, null);
		}
		public static void SetupAbilityTargetList(Unit unit, Ability ability){
			//Debug.Log("SetupAbilityTargetList   "+ability.TargetStraightLineOnly());
			
			if(ability.TargetStraightLineOnly()){
				instance.abilityTargetList.Clear();
				
				for(int i=0; i<(IsHexGrid() ? 6 : 4); i++){
					float angle=IsHexGrid() ? (i*60)+30 : i*90 ;
					List<Node> list=GetNodesInALine(unit.node, angle, ability.GetRange());	
					
					for(int n=0; n<list.Count; n++) list[n].abLineParent=list[list.Count-1];
					
					if(ability.type==Ability._AbilityType.Charge){
						for(int n=0; n<list.Count; n++){
							Node prevNode=n==0 ? unit.node : list[n-1] ;
							
							if(ability.targetType==Ability._TargetType.EmptyNode){
								if(list[n].IsBlocked(prevNode)) break;
								instance.abilityTargetList.Add(list[n]);
							}
							else if(ability.targetType==Ability._TargetType.AllNode){
								if(list[n].IsBlocked(prevNode, true)) break;
								instance.abilityTargetList.Add(list[n]);
								if(list[n].unit!=null) break;
							}
							else{
								if(list[n].IsBlocked(prevNode, true)) break;
								if(list[n].unit==null) continue;
								if(ability.targetType==Ability._TargetType.AllUnit) instance.abilityTargetList.Add(list[n]);
								else if(ability.targetType==Ability._TargetType.HostileUnit && !OnSameFac(list[n].unit, unit)) instance.abilityTargetList.Add(list[n]);
								else if(ability.targetType==Ability._TargetType.FriendlyUnit && OnSameFac(list[n].unit, unit)) instance.abilityTargetList.Add(list[n]);
								break;
							}
						}
					}
					else instance.abilityTargetList.AddRange(list);
				}
				
				GridIndicator.ShowAbility(instance.abilityTargetList);
				return;
			}
			
			List<Node> nodeList=GetNodesWithinDistance(unit.node, ability.GetRange());
			SetupAbilityTargetList(ability, nodeList, unit.GetFacID(), unit.node);
		}
		public static void SetupAbilityTargetList(Ability ability, List<Node> list, int facID, Node origin){	
			instance.abilityTargetList.Clear();
			
			if(ability.isUnitAbility && ability.requireLos && origin.unit!=null){
				for(int i=0; i<list.Count; i++){
					if(!CheckLOS(origin, list[i], origin.unit.GetSight())){ list.RemoveAt(i); i-=1; }
				}
			}
			
			if(ability.targetType==Ability._TargetType.AllNode){
				instance.abilityTargetList=list;
			}
			else if(ability.targetType==Ability._TargetType.AllUnit){
				for(int i=0; i<list.Count; i++){
					if(list[i].unit!=null) instance.abilityTargetList.Add(list[i]);
				}
			}
			else if(ability.targetType==Ability._TargetType.HostileUnit){
				for(int i=0; i<list.Count; i++){
					if(list[i].unit!=null && list[i].unit.GetFacID()!=facID) instance.abilityTargetList.Add(list[i]);
				}
			}
			else if(ability.targetType==Ability._TargetType.FriendlyUnit){
				for(int i=0; i<list.Count; i++){
					if(list[i].unit!=null && list[i].unit.GetFacID()==facID) instance.abilityTargetList.Add(list[i]);
				}
			}
			else if(ability.targetType==Ability._TargetType.EmptyNode){
				for(int i=0; i<list.Count; i++){
					if(list[i].unit==null) instance.abilityTargetList.Add(list[i]);
				}
			}
			
			if(!ability.isUnitAbility && ability.targetType==Ability._TargetType.AllNode){
				GridIndicator.ShowAbility(new List<Node>());
			}
			else GridIndicator.ShowAbility(instance.abilityTargetList);
		}
		public static void ClearAbilityTargetList(bool resetIndicator){ 
			GridIndicator.HideAbility();
			instance.abilityTargetList.Clear();
			
			if(resetIndicator){
				GridIndicator.ShowMovable(GetWalkableList());
				GridIndicator.ShowHostile(GetAttackableList());
			}
		}
		
		public static List<Node> GetTargetNodeForNonTargetingFAbility(Ability ability){
			List<Node> list=new List<Node>();
			List<Unit> uList=UnitManager.GetAllUnitList();
			
			//if(ability.targetType==Ability._TargetType.AllNode){
			//	for(int x=0; x<instance.grid.Count; x++) list.AddRange(instance.grid[x]);
			//}
			if(ability.targetType==Ability._TargetType.AllUnit){
				for(int i=0; i<uList.Count; i++) list.Add(uList[i].node);
			}
			else if(ability.targetType==Ability._TargetType.HostileUnit){
				for(int i=0; i<uList.Count; i++){ if(uList[i].GetFacID()!=ability.facID) list.Add(uList[i].node); }
			}
			else if(ability.targetType==Ability._TargetType.FriendlyUnit){
				for(int i=0; i<uList.Count; i++){ if(uList[i].GetFacID()==ability.facID) list.Add(uList[i].node); }
			}
			//else if(ability.targetType==Ability._TargetType.EmptyNode){
			//	for(int x=0; x<instance.grid.Count; x++){
			//		for(int z=0; z<instance.grid[x].Count; z++){
			//			if(instance.grid[x][z].unit==null) list.Add(instance.grid[x][z]);
			//		}
			//	}
			//}
			
			return list;
		}
		
		
		
		public static List<Node> GetNodesInALine(Node srcNode, float angle, int dist, bool walkableOnly=false){
			List<Node> list=new List<Node>();
			Node curNode=srcNode;
			for(int i=0; i<dist; i++){
				List<Node> neighbourList=curNode.GetNeighbourList(walkableOnly);
				for(int n=0; n<neighbourList.Count; n++){
					if(Mathf.Abs(GetAngle(neighbourList[n], curNode, true)-angle)>1) continue;
					curNode=neighbourList[n];
					list.Add(curNode);
					break;
				}
			}
			return list;
		}
		public static List<Node> GetNodesWithinDistance(Node srcNode, int dist, bool walkableOnly=false, bool includeSrc = true){
			List<Node> neighbourList=srcNode.GetNeighbourList(walkableOnly);
			
			List<Node> closeList=new List<Node>();
			List<Node> openList=new List<Node>();
			List<Node> newOpenList=new List<Node>();
			
			for(int m=0; m<neighbourList.Count; m++){
				Node neighbour=neighbourList[m];
				if(!newOpenList.Contains(neighbour)) newOpenList.Add(neighbour);
			}
			
			for(int i=0; i<dist; i++){
				openList=newOpenList;
				newOpenList=new List<Node>();
				
				for(int n=0; n<openList.Count; n++){
					neighbourList=openList[n].GetNeighbourList(walkableOnly);
					for(int m=0; m<neighbourList.Count; m++){
						Node neighbour=neighbourList[m];
						if(!closeList.Contains(neighbour) && !openList.Contains(neighbour) && !newOpenList.Contains(neighbour)){
							newOpenList.Add(neighbour);
						}
					}
				}
				
				for(int n=0; n<openList.Count; n++){
					Node tile=openList[n];
					if(tile!=srcNode && !closeList.Contains(tile)){
						closeList.Add(tile);
					}
				}
			}
			
			
			if(IsSquareGrid() && EnableDiagonalNeighbour() && instance.rangeCalculation==_RangeCalculation.ByDistance){
				float convertedDist=dist*GetNodeSize()+GetNodeSize()*0.5f;
				for(int i=0; i<closeList.Count; i++){
					if(Vector3.Distance(closeList[i].GetPos(), srcNode.GetPos())>=convertedDist){
						closeList.RemoveAt(i); i-=1;
					}
				}
			}

		    if (includeSrc)
		    {
		        closeList.Add(srcNode);
            }
			return closeList;
		}
		
		
		
		public static List<Node> GetSpawnGroup(int facID, int areaID){
			List<Node> list=new List<Node>();
			for(int x=0; x<instance.grid.Count; x++){
				for(int z=0; z<instance.grid[x].Count; z++){
					if(!instance.grid[x][z].walkable || instance.grid[x][z].HasObstacle()) continue;
					if(instance.grid[x][z].spawnGroupFacID==facID && instance.grid[x][z].spawnGroupID==areaID) list.Add(instance.grid[x][z]);
				}
			}
			return list;
		}
		
		
		public static bool OnSameFac(Unit unit1, Unit unit2){ return unit1.GetFacID()==unit2.GetFacID(); }
		
		
		void OnDrawGizmos(){
			for(int x=0; x<grid.Count; x++){
				for(int z=0; z<grid[x].Count; z++){
					//Gizmos.color=grid[x][z].visible ? Color.grey : Color.white;
					if(!grid[x][z].walkable){
						Gizmos.color=new Color(0.4f, 0.4f, 0.4f, 1f);
						Gizmos.DrawSphere(grid[x][z].GetPos(), nodeSize*0.125f);
					}
					else if(!grid[x][z].IsVisible()){
						Gizmos.color=new Color(0.5f, 0.5f, 0.5f, .6f);
						Gizmos.DrawSphere(grid[x][z].GetPos(), nodeSize*0.125f);
					}
					
					if(grid[x][z].deployFacID>=0){
						Gizmos.color=UnitManager.GetFacColor(grid[x][z].deployFacID);
						Gizmos.DrawSphere(grid[x][z].GetPos(), nodeSize*0.175f);
					}
					if(grid[x][z].spawnGroupFacID>=0){
						//Gizmos.DrawSphere(grid[x][z].GetPos(), nodeSize*0.25f);
						//Utility.GizmosDrawCross(grid[x][z].GetPos(), nodeSize*0.25f, UnitManager.GetFacColor(grid[x][z].spawnGroupFacID));
						
						Gizmos.color=UnitManager.GetFacColor(grid[x][z].spawnGroupFacID);
						
						float size=nodeSize*0.25f;		Vector3 pos=grid[x][z].GetPos()+new Vector3(0, .1f, 0);
						Gizmos.DrawLine(pos+new Vector3(1, 0, 1).normalized*size, pos-new Vector3(1, 0, 1).normalized*size);
						Gizmos.DrawLine(pos+new Vector3(-1, 0, 1).normalized*size, pos-new Vector3(-1, 0, 1).normalized*size);
					}
				}
			}
			
			
			if(abilityTargetList.Count>0){
				Gizmos.color=Color.yellow;
				for(int i=0; i<abilityTargetList.Count; i++){
					Gizmos.DrawSphere(abilityTargetList[i].GetPos(), 0.4f);
				}
			}
			else{
				Gizmos.color=Color.green;
				for(int i=0; i<walkableList.Count; i++){
					Gizmos.DrawSphere(walkableList[i].GetPos(), 0.3f);
				}
				
				Gizmos.color=Color.red;
				for(int i=0; i<attackableList.Count; i++){
					Gizmos.DrawSphere(attackableList[i].GetPos(), 0.4f);
				}
			}
		}
		
		
	}

}