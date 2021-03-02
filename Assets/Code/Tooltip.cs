using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code
{
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


		public List<Keyword> Keywords;

		private Canvas _canvas;

		public void Start()
		{
			_canvas = GetComponentInParent<Canvas>();
		}

		public void ShowAbility(Ability ability)
		{
			TitleText.text = ability.Name;
			string text = string.Format(ability.Description, ability.Effects.Select(s => s.Value.ToString()).ToArray());
			foreach (var word in Keywords) {
				text = text.Replace($"[{word.Word}]", $"<color=#{ColorUtility.ToHtmlStringRGB(word.Color)}>{word.Word}</color>");
			}

			ContentText.text = text;
			HealthCost.text = ability.Cost.ToString();
			HealthCostRoot.gameObject.SetActive(true);

			gameObject.SetActive(true);
		}

		public void Hide()
		{
			gameObject.SetActive(false);
		}

		public void Update()
		{
			Vector2 movePos;
			Vector2 size = ((RectTransform)transform).rect.size;

			RectTransformUtility.ScreenPointToLocalPointInRectangle(
	_canvas.transform as RectTransform,
	Input.mousePosition, _canvas.worldCamera,
	out movePos);

			transform.position = _canvas.transform.TransformPoint(movePos + new Vector2(size.x / 2, size.y / 2) + new Vector2(16, -16));// - new Vector3(0, 2, 0);
		}

	}
}
