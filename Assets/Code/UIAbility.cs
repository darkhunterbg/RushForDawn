using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Code
{
	public class UIAbility : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		public Image Icon;
		public Button Button;

		public Ability Ability;

		public bool Disable = false;

		public event Action<UIAbility> OnClicked = null;

		public void Awake()
		{
			Button.onClick.AddListener(() =>
			{
				OnClicked?.Invoke(this);
			});
		}


		public void SetAbility(Ability ability)
		{
			Ability = ability;
			Icon.sprite = Ability.Icon;
		}

		public void Update()
		{
			Button.interactable = !Disable && Ability.CanExecute(BattleGameState.Instance.Actors);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{

			GameController.Instance.Tooltip.ShowAbility(Ability);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			GameController.Instance.Tooltip.Hide();
		}
	}
}
