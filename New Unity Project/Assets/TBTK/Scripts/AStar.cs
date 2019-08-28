using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TBTK{

	public class AStar{
		
		//search for a path, through walkable tile only
		//for normal movement, return the path in a list of hexTile
		public static List<Node> SearchWalkableNode(Node originNode, Node destNode, bool returnNearest=true){
			
			GridManager.ResetGrid();
			
			List<Node> closeList=new List<Node>();
			List<Node> openList=new List<Node>();
			
			Node currentNode=originNode;
			
			float currentLowestF=Mathf.Infinity;
			int id=0;
			int i=0;
			
			while(true){
				
				//if we have reach the destination
				if(currentNode==destNode) break;
				
				//move currentNode to closeList;
				closeList.Add(currentNode);
				currentNode.listState=Node._ListState.Close;
				
				//loop through the neighbour of current loop, calculate  score and stuff
				currentNode.ProcessNeighbour(destNode.GetPos());
				
				//put all neighbour in openlist
				foreach(Node neighbour in currentNode.GetNeighbourList(true)){
					if(neighbour.IsBlocked(currentNode)) continue;
					if(neighbour.listState==Node._ListState.Unassigned || neighbour==destNode){
						//~ //set the node state to open
						neighbour.listState=Node._ListState.Open;
						openList.Add(neighbour);
					}
				}
				
				//clear the current node, before getting a new one, so we know if there isnt any suitable next node
				currentNode=null;
				
				
				currentLowestF=Mathf.Infinity;
				id=0;
				for(i=0; i<openList.Count; i++){
					if(openList[i].scoreF<currentLowestF){
						currentLowestF=openList[i].scoreF;
						currentNode=openList[i];
						id=i;
					}
				}
				
				
				//if there's no node left in openlist, path doesnt exist
				if(currentNode==null){
					if(!returnNearest) return new List<Node>();
					break;
				}

				openList.RemoveAt(id);
			}
			
			if(currentNode==null){
				float tileSize=GridManager.GetNodeSize();//*GridManager.GetGridToTileSizeRatio();
				currentLowestF=Mathf.Infinity;
				for(i=0; i<closeList.Count; i++){
					float dist=Vector3.Distance(destNode.GetPos(), closeList[i].GetPos());
					if(dist<currentLowestF){
						currentLowestF=dist;
						currentNode=closeList[i];
						if(dist<tileSize*1.5f) break;
					}
				}
			}
			
			List<Node> path=new List<Node>();
			while(currentNode!=null){
				if(currentNode==originNode || currentNode==currentNode.parent) break;
				path.Add(currentNode);
				currentNode=currentNode.parent;
			}
			
			path=InvertNodeArray(path);
			
			ResetGraph(destNode, openList, closeList);
			
			return path;
		}
		
		
		
		/*
		//search the shortest path through all tile reagardless of status
		//this is used to accurately calculate the distance between 2 tiles in term of tile
		//distance calculated applies for line traverse thru walkable tiles only, otherwise it can be calculated using the coordinate
		public static int GetDistance(Tile srcTile, Tile targetTile){
			List<Tile> closeList=new List<Tile>();
			List<Tile> openList=new List<Tile>();
			
			Tile currentTile=srcTile;
			if(srcTile==null) Debug.Log("src tile is null!!!");
			
			float currentLowestF=Mathf.Infinity;
			int id=0;
			int i=0;
			
			while(true){
				//if we have reach the destination
				if(currentTile==targetTile) break;
				
				//move currentNode to closeList;
				closeList.Add(currentTile);
				currentTile.aStar.listState=Node._ListState.Close;
				
				//loop through all neighbours, regardless of status 
				//currentTile.ProcessAllNeighbours(targetTile);
				currentTile.aStar.ProcessWalkableNeighbour(targetTile);
				
				//put all neighbour in openlist
				foreach(Tile neighbour in currentTile.aStar.GetNeighbourList()){
					if(neighbour.unit!=null && neighbour!=targetTile) continue;
					if(neighbour.aStar.listState==Node._ListState.Unassigned) {
						//set the node state to open
						neighbour.aStar.listState=Node._ListState.Open;
						openList.Add(neighbour);
					}
				}
				
				currentTile=null;
				
				currentLowestF=Mathf.Infinity;
				id=0;
				for(i=0; i<openList.Count; i++){
					if(openList[i].aStar.scoreF<currentLowestF){
						currentLowestF=openList[i].aStar.scoreF;
						currentTile=openList[i];
						id=i;
					}
				}
				
				if(currentTile==null) return -1;
				
				openList.RemoveAt(id);
			}
			
			
			int counter=0;
			while(currentTile!=null){
				counter+=1;
				currentTile=currentTile.aStar.parent;
			}
			
			ResetGraph(targetTile, openList, closeList);
			
			return counter-1;
		}
		*/
		
		
		
		private static List<Vector3> InvertArray(List<Vector3> p){
			List<Vector3> pInverted=new List<Vector3>();
			for(int i=0; i<p.Count; i++){
				pInverted.Add(p[p.Count-(i+1)]);
			}
			return pInverted;
		}
		
		private static List<Node> InvertNodeArray(List<Node> p){
			List<Node> pInverted=new List<Node>();
			for(int i=0; i<p.Count; i++){
				pInverted.Add(p[p.Count-(i+1)]);
			}
			return pInverted;
		}
		
		
		//reset all the tile, called after a search is complete
		private static void ResetGraph(Node hNode, List<Node> oList, List<Node> cList){
			hNode.ResetListState();
			hNode.parent=null;
			
			foreach(Node node in oList){
				node.ResetListState();
				node.parent=null;
			}
			foreach(Node node in cList){
				node.ResetListState();
				node.parent=null;
			}
		}
	}
	
}
