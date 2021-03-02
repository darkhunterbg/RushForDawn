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
		public Button ReconstructButton;
		public GameObject DismantleRoot;
		public GameObject ReconstructRoot;

		public Text RepairAllCostText;
		public Text DismantleGainText;
		public Text ReconstructCostText;

		public Actor Actor { get; private set; }
		public Action<Actor> OnDismantled;
		public Action<Actor, int> OnRepaired;
		public Action<Actor> OnReconstruct;

		public GameObject AbilitiesRoot;

		public void Awake()
		{
			Repair1Button.onClick.AddListener(() => Repair(1));
			Repair5Button.onClick.AddListener(() => Repair(5));
			Repair10Button.onClick.AddListener(() => Repair(10));
			RepairAllButton.onClick.AddListener(() => Repair(Actor.MissingHealth));
			DismantleButton.onClick.AddListener(() => Dismantle());
			ReconstructButton.onClick.AddListener(() => Reconstruct());
		}

		public void Init(Actor actor)
		{
			Actor = actor;

			Icon.sprite = actor.Icon;
			Name.text = actor.Name.ToUpper();

			UpdateHealth();

			if (actor.Class == ClassType.Egg) {
				DismantleRoot.gameObject.SetActive(false);
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
			UpdateHealth();
			OnDismantled(Actor);

		}
		private void Repair(int value)
		{
			GameController.Instance.Scrap -= value;
			Actor.CurrentHealth += value;
			UpdateHealth();
			OnRepaired(Actor, value);
		}
		private void Reconstruct()
		{
			int scrap = (Actor.MaxHealth + Actor.AbilityDefinisions.Sum(s => s.ScrapCost));
			GameController.Instance.Scrap -= (scrap * 3) / 4;
			Actor.SetToMaxHP();
			Actor.gameObject.SetActive(true);
			UpdateHealth();
			OnReconstruct(Actor);
		}

		public void Refresh()
		{
			if (Actor.Class != ClassType.Egg) {

				DismantleRoot.gameObject.SetActive(Actor.IsAlive);
				ReconstructRoot.gameObject.SetActive(!Actor.IsAlive);
				int cost = (Actor.MaxHealth + Actor.AbilityDefinisions.Sum(s => s.ScrapCost));
				DismantleGainText.text = $"[+{cost / 2} SCRAP]";
				ReconstructCostText.text = $"[{(cost * 3) / 4} SCRAP]";
				ReconstructButton.interactable = GameController.Instance.Scrap >= (cost * 3) / 4;
			}

			RepairAllCostText.text = $"[{Actor.MissingHealth} SCRAP]";

			Repair1Button.interactable = Actor.IsAlive && Actor.MissingHealth >= 1 && GameController.Instance.Scrap >= 1;
			Repair5Button.interactable = Actor.IsAlive && Actor.MissingHealth >= 5 && GameController.Instance.Scrap >= 5;
			Repair10Button.interactable = Actor.IsAlive && Actor.MissingHealth >= 10 && GameController.Instance.Scrap >= 10;
			RepairAllButton.interactable = Actor.IsAlive && Actor.MissingHealth > 0 && GameController.Instance.Scrap >= Actor.MissingHealth;

			Icon.color = Actor.IsAlive ? Color.white : Color.black;

			var abilities = AbilitiesRoot.GetComponentsInChildren<UIAbility>(includeInactive: true);
			for (int i = 0; i < Actor.AbilityDefinisions.Count; ++i) {
				abilities[i].SetAbility(Actor.AbilityDefinisions[i]);
				abilities[i].gameObject.SetActive(true);
				abilities[i].Mode = Tooltip.AbilityTooltipMode.ShopOwned;
				abilities[i].Disable = true;
			}
			for (int j = Actor.AbilityDefinisions.Count; j < abilities.Length; ++j) {
				abilities[j].gameObject.SetActive(false);
			}
		}
	}
}
