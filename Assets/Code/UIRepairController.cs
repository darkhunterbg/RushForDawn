using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code
{
	public class UIRepairController : MonoBehaviour
	{
		public Image Icon;
		public TMPro.TMP_Text Name;
		public TMPro.TMP_Text Health;

		public Button Repair1Button;
		public Button Repair5Button;
		public Button Repair10Button;
		public Button RepairAllButton;
		public Button DismantleButton;
		public GameObject DismantleRoot;

		public Text RepairAllCostText;
		public Text DismantleGainText;

		public Actor Actor { get; private set; }
		public Action<Actor> OnDismantled;
		public Action<Actor,int> OnRepaired;

		public void Awake()
		{
			Repair1Button.onClick.AddListener(() => Repair(1));
			Repair5Button.onClick.AddListener(() => Repair(5));
			Repair10Button.onClick.AddListener(() => Repair(10));
			RepairAllButton.onClick.AddListener(() => Repair(Actor.MissingHealth));
			DismantleButton.onClick.AddListener(() => Dismantle());
		}

		public void Init(Actor actor)
		{
			Actor = actor;

			Icon.sprite = actor.Icon;
			Name.text = actor.Name.ToUpper();

			UpdateHealth();

			if (actor.Class == ClassType.Egg) {
				DismantleRoot.gameObject.SetActive(false);
			} else {
				DismantleRoot.gameObject.SetActive(true);
			}

			Refresh();
		}

		private void UpdateHealth()
		{
			Health.text = $"{Actor.CurrentHealth}/{Actor.MaxHealth}";
		}

		private void Dismantle()
		{
			Actor.CurrentHealth = 0;
			Actor.gameObject.SetActive(false);
			int scrap = (Actor.MaxHealth + Actor.AbilityDefinisions.Sum(s => s.ScrapCost)) / 2;
			GameController.Instance.Scrap += scrap;
			OnDismantled(Actor);

		}
		private void Repair(int value)
		{
			GameController.Instance.Scrap -= value;
			Actor.CurrentHealth += value;
			UpdateHealth();
			OnRepaired(Actor, value);
		}

		public void Refresh()
		{
			if (Actor.Class != ClassType.Egg) {
				int cost = (Actor.MaxHealth + Actor.AbilityDefinisions.Sum(s => s.ScrapCost)) / 2;
				DismantleGainText.text = $"[+{cost} SCRAP]";
			}

			RepairAllCostText.text = $"[{Actor.MissingHealth} SCRAP]";

			Repair1Button.interactable = Actor.MissingHealth >= 1 && GameController.Instance.Scrap >= 1;
			Repair5Button.interactable = Actor.MissingHealth >= 5 && GameController.Instance.Scrap >= 5;
			Repair10Button.interactable = Actor.MissingHealth >= 10 && GameController.Instance.Scrap >= 10;
			RepairAllButton.interactable = Actor.MissingHealth > 0 && GameController.Instance.Scrap >= Actor.MissingHealth;
		}
	}
}
