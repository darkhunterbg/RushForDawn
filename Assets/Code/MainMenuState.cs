using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code
{
	public class MainMenuState : MonoBehaviour
	{
		public Button Easy;
		public Button Normal;
		public Button Hard;
		public Button Start;
		public GameObject TutorialRoot;

		public void Awake()
		{
			TutorialRoot.gameObject.SetActive(false);

			Easy.onClick.AddListener(() => StartGame(0));
			Normal.onClick.AddListener(() => StartGame(1));
			Hard.onClick.AddListener(() => StartGame(2));
			Start.onClick.AddListener(() => GameController.Instance.NewGame());
		}

		private void StartGame(int difficulty)
		{
			GameController.Instance.GameDifficulty = GameController.Instance.Difficulties[difficulty];

			TutorialRoot.gameObject.SetActive(true);
		
		}
	}
}
