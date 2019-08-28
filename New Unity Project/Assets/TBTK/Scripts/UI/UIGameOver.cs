using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace TBTK{

	public class UIGameOver : UIScreen {
		
		public UIButton buttonContinue;
		public UIButton buttonRestart;
		public UIButton buttonMainMenu;
		
		private static UIGameOver instance;
		
		public override void Awake(){
			base.Awake();
			
			instance=this;
		}
		
		public override void Start(){
			buttonContinue.Init();
			buttonContinue.button.onClick.AddListener(delegate { OnContinueButton(); });
			
			buttonRestart.Init();
			buttonRestart.button.onClick.AddListener(delegate { OnRestartButton(); });
			
			buttonMainMenu.Init();
			buttonMainMenu.button.onClick.AddListener(delegate { OnMenuButton(); });
			
			thisObj.SetActive(false);
		}
		
		
		public void OnContinueButton(){
			UIControl.NextLevel();
		}
		public void OnRestartButton(){
			UIControl.RestartLevel();
		}
		public void OnMenuButton(){
			UIControl.MainMenu();
		}
		
		
		public static void Show(bool playerWon){ instance._Show(playerWon); }
		public void _Show(bool playerWon){
			base.Show();
		}
		
	}

}