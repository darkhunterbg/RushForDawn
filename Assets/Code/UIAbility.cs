﻿using System;
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

		public Image ButtonImage;

		public Color NormalColor;
		public Color InvertedColor;

		public bool Toggled;

		public Ability Ability { get; private set; }

		public bool Disable { get; set; } = false;

		public Tooltip.AbilityTooltipMode Mode;

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
			if (Mode != Tooltip.AbilityTooltipMode.Default) {
				Button.interactable = !Disable;
				return;
			}
			ButtonImage.color = Toggled ? InvertedColor : NormalColor;
			Icon.color = Toggled ? NormalColor : InvertedColor;

			Button.interactable = !Disable && Ability.User.PlayerControllable && Ability.CanExecute(BattleGameState.Instance.Actors);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{

			GameController.Instance.Tooltip.ShowAbility(Ability, Mode);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			GameController.Instance.Tooltip.Hide();
		}
	}
}
