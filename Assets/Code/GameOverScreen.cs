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

		public void Awake()
		{
			RetryButton.onClick.AddListener(() =>
			{
				GameController.Instance.NewGame();
			});
		}

		void OnEnable()
		{
			int hoursLeft = GameController.Instance.Levels.Count - GameController.Instance.Level;

			Description.text = "EGG DESTROYED " + (hoursLeft > 1 ? $"{hoursLeft} HOURS BEFORE DAWN" : "LAST HOUR BEFORE DAWN");

		}
	}
}
