using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

namespace TBTK{
	
	[CustomEditor(typeof(CameraControl))]
	public class I_CameraControlEditor : TBEditorInspector {
		
		//private CameraControl instance;
		
		public override void Awake(){
			base.Awake();
			//instance = (CameraControl)target;
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			//EditorGUILayout.Space();
			
			EditorGUIUtility.labelWidth=160;
			
			DrawDefaultInspector();
			//TurnControl.inspector=DefaultInspector(TurnControl.inspector, 0);
			
			EditorGUIUtility.labelWidth=0;
		}
		
	}
	
}
