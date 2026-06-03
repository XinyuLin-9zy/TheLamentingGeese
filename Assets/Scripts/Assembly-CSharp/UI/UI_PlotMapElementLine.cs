using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class UI_PlotMapElementLine : MonoBehaviour
	{
		public UI_PlotChapterElement unit1;

		public ElementPointPos pos1;

		public UI_PlotChapterElement unit2;

		public ElementPointPos pos2;

		[SerializeField]
		private GameObject normalLine;

		[SerializeField]
		private GameObject activeLine;

		public void SetPosition()
		{
			EnsureLineReferences();

			RectTransform rectTransform = transform as RectTransform;
			RectTransform parentRect = rectTransform != null ? rectTransform.parent as RectTransform : null;
			RectTransform pointA = unit1 != null ? unit1.GetPoint(pos1) : null;
			RectTransform pointB = unit2 != null ? unit2.GetPoint(pos2) : null;
			if (rectTransform == null || parentRect == null || pointA == null || pointB == null)
			{
				SetLineState(false);
				return;
			}

			Vector2 localA = parentRect.InverseTransformPoint(GetWorldCenter(pointA));
			Vector2 localB = parentRect.InverseTransformPoint(GetWorldCenter(pointB));
			Vector2 delta = localB - localA;
			float length = delta.magnitude;
			if (length <= 0.01f)
			{
				gameObject.SetActive(false);
				return;
			}

			gameObject.SetActive(true);
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.anchoredPosition = (localA + localB) * 0.5f;
			rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
			rectTransform.localScale = Vector3.one;

			Vector2 size = rectTransform.sizeDelta;
			rectTransform.sizeDelta = new Vector2(length, Mathf.Max(2f, size.y));

			StretchLineChild(normalLine);
			StretchLineChild(activeLine);
			SetLineState(!unit1.IsLocking && !unit2.IsLocking);
		}

		private static Vector3 GetWorldCenter(RectTransform rectTransform)
		{
			return rectTransform.TransformPoint(rectTransform.rect.center);
		}

		private void EnsureLineReferences()
		{
			if (normalLine != null || activeLine != null) return;

			foreach (Transform child in transform)
			{
				string childName = child.name ?? "";
				if (activeLine == null && IsActiveLineName(childName))
				{
					activeLine = child.gameObject;
					continue;
				}
				if (normalLine == null && IsNormalLineName(childName))
				{
					normalLine = child.gameObject;
				}
			}

			if (normalLine == null && transform.childCount == 1)
			{
				normalLine = transform.GetChild(0).gameObject;
			}
		}

		private static bool IsActiveLineName(string value)
		{
			return Contains(value, "Active") || Contains(value, "Unlock") || Contains(value, "On");
		}

		private static bool IsNormalLineName(string value)
		{
			return Contains(value, "Normal") || Contains(value, "Lock") || Contains(value, "Off");
		}

		private static bool Contains(string value, string fragment)
		{
			return !string.IsNullOrEmpty(value)
				&& value.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private void SetLineState(bool active)
		{
			if (normalLine != null) normalLine.SetActive(!active);
			if (activeLine != null) activeLine.SetActive(active);
			if (normalLine == null && activeLine == null)
			{
				Graphic graphic = GetComponent<Graphic>();
				if (graphic != null)
				{
					Color color = active
						? new Color(1f, 0.88f, 0.52f, Mathf.Max(0.86f, graphic.color.a))
						: new Color(0.75f, 0.75f, 0.78f, Mathf.Max(0.46f, graphic.color.a));
					graphic.color = color;
					graphic.raycastTarget = false;
				}
			}
		}

		private static void StretchLineChild(GameObject line)
		{
			if (line == null) return;

			RectTransform rectTransform = line.transform as RectTransform;
			if (rectTransform != null)
			{
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.one;
				rectTransform.offsetMin = Vector2.zero;
				rectTransform.offsetMax = Vector2.zero;
				rectTransform.localRotation = Quaternion.identity;
				rectTransform.localScale = Vector3.one;
			}

			foreach (Graphic graphic in line.GetComponentsInChildren<Graphic>(true))
			{
				if (graphic != null) graphic.raycastTarget = false;
			}
		}
	}
}
