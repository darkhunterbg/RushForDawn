using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code
{
	public class BattleWonScreen : MonoBehaviour
	{
		public Button NextBattleButton;
		public Button UpgradesButton;
		public Button MainMenuButton;

		public GameObject UpgradesScreen;
		public GameObject ShopItemsRoot;
		public UIShopItem ShopItemPrefab;

		public GameObject RepairItemsRoot;

		public Text DescriptionText;
		public Text TitleText;

		private List<Ability> _shopAbilities;

		public int ShowItems = 5;

		public void Awake()
		{
			NextBattleButton.onClick.AddListener(NextBattleClicked);
			UpgradesButton.onClick.AddListener(UpgradesClicked);
			MainMenuButton.onClick.AddListener(() => GameController.Instance.ToMainMenu());

			UpgradesScreen.gameObject.SetActive(false);
		}

		public void NextBattleClicked()
		{
			GameController.Instance.NewBattle();
		}

		public void Init(bool firstTime)
		{
			TitleText.gameObject.SetActive(!firstTime);
			UpgradesButton.gameObject.SetActive(!firstTime);

			UpgradesScreen.gameObject.SetActive(false);

			int hoursLeft = GameController.Instance.Levels.Count - GameController.Instance.Level;

			DescriptionText.text = hoursLeft > 1 ? $"{hoursLeft} HOURS LEFT UNTIL DAWN" : "LAST HOUR BEFORE DAWN";

			var party = GameController.Instance.Party.ToList();
			//var invalidClass = party.Select(s => s.Class).ToList();

			var validAbilities = GameController.Instance.ShopAbilities.Where
				(s => party.All(p => !p.AbilityDefinisions.Any(q => q.Name == s.Name))).ToList();

			//validAbilities.RemoveAll(c => invalidClass.Contains(c.Class));

			_shopAbilities = new List<Ability>();

			for (int i = 0; i < ShowItems; ++i) {
				if (validAbilities.Count == 0)
					break;

				int r = UnityEngine.Random.Range(0, validAbilities.Count);
				_shopAbilities.Add(validAbilities[r]);
				validAbilities.Remove(validAbilities[r]);
			}

			_shopAbilities = _shopAbilities.Distinct().ToList();

			foreach (var item in ShopItemsRoot.GetComponentsInChildren<UIShopItem>()) {
				Destroy(item.gameObject);
			}

			foreach (var ability in _shopAbilities) {
				var obj = Instantiate(ShopItemPrefab, ShopItemsRoot.transform);
				obj.Init(ability);
				obj.OnItemBought = ItemBought;
			}

			var repairControllers = RepairItemsRoot.GetComponentsInChildren<UIRepairController>(includeInactive: true);
			for (int i = 0; i < party.Count; ++i) {
				repairControllers[i].Init(party[i]);
				repairControllers[i].gameObject.SetActive(true);
				repairControllers[i].OnRepaired = OnRepaired;
				repairControllers[i].OnDismantled = OnDismantled;
				repairControllers[i].OnReconstruct = OnReconstructed;
			}
		}

		public void UpgradesClicked()
		{
			UpgradesScreen.gameObject.SetActive(true);
		}

		private void ItemBought(UIShopItem item)
		{
			GameController.Instance.Scrap -= item.Ability.Ability.ScrapCost;

			var actor = GameController.Instance.Party.FirstOrDefault(p => p.Class == item.Ability.Ability.Class);

			if (!string.IsNullOrEmpty(item.Ability.Ability.Replaces)) {
				actor.AbilityDefinisions.RemoveAll(a => a.Name.Contains(item.Ability.Ability.Replaces));
			}

			actor.AbilityDefinisions.Add(item.Ability.Ability);

			GameObject.Destroy(item.gameObject);

			var repairControllers = RepairItemsRoot.GetComponentsInChildren<UIRepairController>();
			foreach (var c in repairControllers)
				c.Refresh();

			GameController.Instance.Tooltip.Hide();
		}

		public void Update()
		{
			if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape)) {
				Back();
			}

		}

		public void Back()
		{
			if (UpgradesScreen.activeInHierarchy) {
				UpgradesScreen.gameObject.SetActive(false);
			}
		}

		private void OnDismantled(Actor actor)
		{
			var repairControllers = RepairItemsRoot.GetComponentsInChildren<UIRepairController>();
			foreach (var c in repairControllers)
				c.Refresh();
			//for (int i = 0; i < party.Count; ++i) {
			//	repairControllers[i].Init(party[i]);
			//	repairControllers[i].gameObject.SetActive(true);
			//	repairControllers[i].OnRepaired = OnRepaired;
			//	repairControllers[i].OnDismantled = OnDismantled;
			//}
			//for (int i = party.Count; i < repairControllers.Length; ++i) {
			//	repairControllers[i].gameObject.SetActive(false);
			//}
		}

		private void OnReconstructed(Actor actor)
		{
			var repairControllers = RepairItemsRoot.GetComponentsInChildren<UIRepairController>();
			foreach (var c in repairControllers)
				c.Refresh();
		}

		private void OnRepaired(Actor actor, int value)
		{
			var repairControllers = RepairItemsRoot.GetComponentsInChildren<UIRepairController>();
			foreach (var c in repairControllers)
				c.Refresh();
		}
	}
}
