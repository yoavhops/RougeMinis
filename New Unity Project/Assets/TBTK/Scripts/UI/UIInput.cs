using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace TBTK{

	public class UIInput : MonoBehaviour {
		
		public Transform touchModeIndicator;
		
		private static UIInput instance;
		
		void Awake(){
			instance=this;
			
			touchModeIndicator.gameObject.SetActive(false);
			routeIndicator.gameObject.SetActive(false);
		}
		
		
		private Node prevNode;
		
		void Update(){
			//for debug
			#if UNITY_EDITOR
				if(Input.GetKeyDown(KeyCode.X)){
					GameControl.GameOver(UnitManager.GetFaction(0));
				}
			#endif
			
			RouteIndicatorTextureScroll();
			
			Node node=GetNodeFromCursor();
			
			bool newNode=prevNode!=node;
			
			if(newNode){
				prevNode=node;
				if(node!=null && node.walkable) GridIndicator.SetCursor(node);		//show hovering tile
			}
			
			if(Input.GetMouseButtonDown(1) && UIControl.EnableUnitInfo() && node!=null && node.unit!=null && !UnitManager.DeployingUnit()){
				UIUnitInfo.Show(node.unit);
				UITooltip.HideTooltip();
			}
			
			
			if(GameControl.IsGameOver()) return;
			if(GameControl.ActionInProgress()) return;
			if(UnitManager.DeployingUnit()) return;
			if(UI.IsCursorOnUI()) return;
			
			if(!UIControl.InTouchMode() && newNode && node!=null && !GameControl.ActionInProgress() && UnitManager.GetSelectedUnit()!=null){
				ShowTooltipNPreview(node);
				
				if(!AbilityManager.IsWaitingForTarget()){
					if(GridManager.CanMoveTo(node)) SetRoute(node);
					else ClearRoute();
				}
			}
			
			
			if(Input.GetMouseButtonDown(0) && node!=null) OnNode(node);
			
			
			if(Input.GetKeyDown(KeyCode.Escape)){
				if(AbilityManager.IsWaitingForTarget()){}	//quit ability target mode is handled in UIAbility script
				else UIPauseMenu.Show();
			}
			
			if(Input.GetKeyDown(KeyCode.Tab)){
				UnitManager.SelectNextUnit();
				ClearTouchModeCursor();
			}
			
			if(Input.GetKeyDown(KeyCode.Return)){
				if(!UnitManager.DeployingUnit()) GameControl.EndTurn();
			}


		    Unit sUnit = UnitManager.GetSelectedUnit();

		    if (sUnit != null)
		    {
		        if (Input.GetKeyDown(KeyCode.Keypad6) ||
		            Input.GetKeyDown(KeyCode.Alpha6))
		        {
		            var wantedRot = Quaternion.Euler(0, 90, 0);
		            sUnit.thisT.rotation = wantedRot;
		        }
		        if (Input.GetKeyDown(KeyCode.Keypad4) ||
		            Input.GetKeyDown(KeyCode.Alpha4))
                {
		            var wantedRot = Quaternion.Euler(0, 270, 0);
		            sUnit.thisT.rotation = wantedRot;
		        }
		        if (Input.GetKeyDown(KeyCode.Keypad8) ||
		            Input.GetKeyDown(KeyCode.Alpha8))
                {
		            var wantedRot = Quaternion.Euler(0, 0, 0);
		            sUnit.thisT.rotation = wantedRot;
		        }
		        if (Input.GetKeyDown(KeyCode.Keypad2) ||
		            Input.GetKeyDown(KeyCode.Alpha2))
                {
		            var wantedRot = Quaternion.Euler(0, 180, 0);
		            sUnit.thisT.rotation = wantedRot;
		        }
            }

        }
		
		private void ShowTooltipNPreview(Node node){
			if(AbilityManager.IsWaitingForTarget()) return;
			
			if(GridManager.CanAttack(node)){
				Attack attack=new Attack(UnitManager.GetSelectedUnit(), node.unit);
				UITooltip.ShowAttackInfo(attack);
			}
			else UITooltip.HideTooltip();
			
			GridManager.PreviewAttackableNode(node);
		}
		
		
		
		void OnNode(Node node){
			if(AbilityManager.IsWaitingForTarget()){
				if(GridManager.InAbilityTargetList(node)){
					if(CheckTouchMode(node)) AbilityManager.AbilityTargetSelected(node);
				}
				else{
					ClearTouchModeCursor();
					UIMessage.DisplayMessage("Invalid Target");
				}
				return;
			}
			
			Unit sUnit=UnitManager.GetSelectedUnit();
			if(sUnit!=null){
				if(node.unit!=null){
					if(sUnit.GetFacID()==node.unit.GetFacID()){
						UnitManager.SelectUnit(node.unit);
						ClearTouchModeCursor();		//for touch mode
					}
					else{
						if(GridManager.CanAttack(node) && CheckTouchMode(node)) GameControl.UnitAttack(sUnit, node);
					}
				}
				else{
					if(GridManager.CanMoveTo(node) && CheckTouchMode(node)){
						GameControl.UnitMove(sUnit, node);
						ClearRoute();
					}
				}
			}
			else if(node.unit!=null){
				if(UnitManager.GetCurrentFactionID()==node.unit.GetFacID()){
					UnitManager.SelectUnit(node.unit);
					ClearTouchModeCursor();			//for touch mode
				}
			}
		}
		
		private Node lastNode;
		private bool CheckTouchMode(Node node){
			if(!UIControl.InTouchMode()) return true;
			
			if(lastNode!=node){
				lastNode=node;
				touchModeIndicator.position=node.GetPos();
				touchModeIndicator.gameObject.SetActive(true);
				ShowTooltipNPreview(node);
				
				if(GridManager.CanMoveTo(node)) SetRoute(node);
				
				return false;
			}
			
			lastNode=null;	touchModeIndicator.gameObject.SetActive(false);
			return true;
		}
		private void _ClearTouchModeCursor(){
			if(!UIControl.InTouchMode()) return;
			UITooltip.HideTooltip();
			lastNode=null;	touchModeIndicator.gameObject.SetActive(false);
		}
		public static void ClearTouchModeCursor(){ instance._ClearTouchModeCursor(); }
		
		
		
		public static Node GetNodeFromCursor(){
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			LayerMask mask=1<<TBTK.GetLayerNode();
			
			if(Physics.Raycast(ray, out hit, Mathf.Infinity, mask)){
				return GridManager.GetNode(hit.point, hit.collider.gameObject);
			}
			
			return null;
		}
		
		
		public LineRenderer routeIndicator;
		public void SetRoute(Node node){
			Unit sUnit=UnitManager.GetSelectedUnit();
			
			if(sUnit==null) return;
			
			List<Node> path=AStar.SearchWalkableNode(sUnit.node, node);
			path.Insert(0, sUnit.node);
			
			Vector3[] pos=new Vector3[path.Count];
			for(int i=0; i<path.Count; i++) pos[i]=path[i].GetPos()+new Vector3(0, 0.25f, 0);
			
			routeIndicator.positionCount=pos.Length;
			routeIndicator.SetPositions(pos);
			
			routeIndicator.gameObject.SetActive(true);
			
			touchModeIndicator.position=node.GetPos();
			touchModeIndicator.gameObject.SetActive(true);
		}
		public void ClearRoute(){
			routeIndicator.gameObject.SetActive(false);
			touchModeIndicator.gameObject.SetActive(false);
		}
		
		private Vector2 uvOffset = Vector2.zero;
		public void RouteIndicatorTextureScroll(){
			Material mat=routeIndicator.materials[0];
			uvOffset -= ( new Vector2( 1, 0f ) * Time.deltaTime );
			mat.SetTextureOffset( "_MainTex", uvOffset );
		}
		
	}

}