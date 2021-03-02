using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code
{
	public class UIShopItem : MonoBehaviour
	{
		public UIAbility Ability;
		public TMPro.TMP_Text Cost;

		public Action<UIShopItem> OnItemBought;

		public void Init(Ability ability)
		{
			Ability.SetAbility(ability);
			Cost.text = ability.ScrapCost.ToString();
			Ability.ShopMode = true;

			Ability.OnClicked += Ability_OnClicked;
		}


		private void Ability_OnClicked(UIAbility ability)
		{
			OnItemBought?.Invoke(this);
		}

		public void Update()
		{
			Ability.Disable = GameController.Instance.Scrap < Ability.Ability.ScrapCost ||
				!(BattleGameState.Instance.PlayerParty.Any(p => p.IsAlive && p.Class == Ability.Ability.Class));
		}
	}
}
