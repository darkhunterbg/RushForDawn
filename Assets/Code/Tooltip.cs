using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code
{
	[DefaultExecutionOrder(1000)]
	public class Tooltip : MonoBehaviour
	{
		[Serializable]
		public class Keyword
		{
			public string Word;
			public Color Color = Color.white;

		}

		public TMPro.TMP_Text TitleText;
		public TMPro.TMP_Text ContentText;

		public TMPro.TMP_Text HealthCost;
		public GameObject HealthCostRoot;


		public Color ReplaceColor;
		public Color ClassColor;

		public List<Keyword> Keywords;

		private Canvas _canvas;

		public object Object { get; private set; }

		public void Start()
		{
			_canvas = GetComponentInParent<Canvas>();
		}

		public void ShowAbility(Ability ability, bool shopMode)
		{
			TitleText.text = ability.Name;
			string text = string.Format(ability.Description, ability.Effects.Select(s => s.Value.ToString()).ToArray());
			

			if (shopMode) {

				if(ability.Class!= ClassType.None)
					text = $"<color=#{ColorUtility.ToHtmlStringRGB(ClassColor)}>{ability.Class}</color> ability\n\n{text}";

				if (!string.IsNullOrEmpty(ability.Replaces)) {
					text += $"\n\n Replaces any <color=#{ColorUtility.ToHtmlStringRGB(ReplaceColor)}>{ability.Replaces}</color> ability.";
				}
				text += $"\n\n Cost {ability.ScrapCost} [Scrap].";
			}

			foreach (var word in Keywords) {
				text = text.Replace($"[{word.Word}]", $"<color=#{ColorUtility.ToHtmlStringRGB(word.Color)}>{word.Word}</color>");
			}

			ContentText.text = text;
			HealthCost.text = ability.GetActualCost().ToString();
			HealthCostRoot.gameObject.SetActive(true);

			Object = ability;

			UpdatePosition();

			gameObject.SetActive(true);
		}
		public void ShowBuff(Buff buff, int stack = 1)
		{
			TitleText.text = buff.Name;
			string text = string.Format(buff.Description, stack);
			foreach (var word in Keywords) {
				text = text.Replace($"[{word.Word}]", $"<color=#{ColorUtility.ToHtmlStringRGB(word.Color)}>{word.Word}</color>");
			}

			ContentText.text = text;

			HealthCostRoot.gameObject.SetActive(false);

			Object = buff;


			UpdatePosition();

			gameObject.SetActive(true);
		}

		public void Hide()
		{
			Object = null;

			gameObject.SetActive(false);
		}

		public void Update()
		{
			UpdatePosition();
		}

		private void UpdatePosition()
		{
			if (_canvas == null)
				_canvas = GetComponentInParent<Canvas>();

			Vector2 movePos;
			Vector2 size = ((RectTransform)transform).rect.size;

			RectTransformUtility.ScreenPointToLocalPointInRectangle(
	_canvas.transform as RectTransform,
	Input.mousePosition, _canvas.worldCamera,
	out movePos);

			var set = movePos + new Vector2(0, 0) + new Vector2(32, 16);

			if (set.y > 500)
				set.y = 500;

			if (set.y < -180)
				set.y = -180;

			transform.position = _canvas.transform.TransformPoint(set);
		}

	}
}
