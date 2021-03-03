using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code
{
	public class GameOverScreen : MonoBehaviour
	{

		public Button RetryButton;
		public Button MainMenuButton;

		public Text Description;

		public bool Victory;

		public GameObject VictoryRoot;
		public GameObject LostRoot;

		public void Awake()
		{
			RetryButton.onClick.AddListener(() =>
			{
				GameController.Instance.NewGame();
			});

			MainMenuButton.onClick.AddListener(() =>
			{
				GameController.Instance.ToMainMenu();
			});
		}

		void OnEnable()
		{
			if (Victory) {
				VictoryRoot.gameObject.SetActive(true);
				LostRoot.gameObject.SetActive(false);
				RetryButton.gameObject.SetActive(false);
			} else {
				VictoryRoot.gameObject.SetActive(false);
				LostRoot.gameObject.SetActive(true);

				int hoursLeft = GameController.Instance.Levels.Count - GameController.Instance.Level;

				Description.text = "EGG DESTROYED " + (hoursLeft > 1 ? $"{hoursLeft} HOURS BEFORE DAWN" : "LAST HOUR BEFORE DAWN");
				RetryButton.gameObject.SetActive(true);
			}


		}
	}
}
