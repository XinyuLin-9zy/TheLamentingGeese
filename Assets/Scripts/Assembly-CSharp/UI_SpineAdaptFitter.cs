using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SkeletonGraphic))]
public class UI_SpineAdaptFitter : MonoBehaviour
{
	private RectTransform parent;

	private SkeletonGraphic graphic;

	private RectTransform rectTransform;

	private Vector2 lastParentSize;

	private bool isApplying;

	private const float Overscan = 1.18f;

	private const float MeshCropScale = 1.2f;

	private void Awake()
	{
		CacheComponents();
	}

	private void OnEnable()
	{
		SetScale();
	}

	private void Start()
	{
		SetScale();
		Canvas.ForceUpdateCanvases();
		SetScale();
	}

	private void OnRectTransformDimensionsChange()
	{
		if (!isActiveAndEnabled) return;
		if (isApplying) return;

		SetScale();
	}

	private void SetScale()
	{
		if (isApplying) return;

		CacheComponents();
		if (rectTransform == null || parent == null || graphic == null) return;

		Vector2 parentSize = parent.rect.size;
		if (parentSize.x <= 0f || parentSize.y <= 0f) return;

		if (lastParentSize == parentSize && graphic.layoutScaleMode == SkeletonGraphic.LayoutMode.EnvelopeParent)
		{
			return;
		}
		lastParentSize = parentSize;

		isApplying = true;
		try
		{
			graphic.raycastTarget = false;
			graphic.Initialize(false);
			if (graphic.IsValid)
			{
				graphic.layoutScaleMode = SkeletonGraphic.LayoutMode.None;
				graphic.MatchRectTransformWithBounds();
			}

			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.sizeDelta = parentSize * (Overscan - 1f);
			rectTransform.localScale = Vector3.one;
			rectTransform.localRotation = Quaternion.identity;

			graphic.layoutScaleMode = SkeletonGraphic.LayoutMode.EnvelopeParent;
			if (graphic.IsValid)
			{
				graphic.Update(0f);
				graphic.UpdateMesh();
				ApplyMeshCoverScale(parentSize);
			}
		}
		finally
		{
			isApplying = false;
		}
	}

	private void ApplyMeshCoverScale(Vector2 parentSize)
	{
		if (graphic == null || rectTransform == null) return;
		if (parentSize.x <= 0f || parentSize.y <= 0f) return;

		Mesh mesh = graphic.GetLastMesh();
		if (mesh == null || mesh.vertexCount <= 0) return;

		mesh.RecalculateBounds();
		Bounds bounds = mesh.bounds;
		float boundsWidth = Mathf.Abs(bounds.size.x);
		float boundsHeight = Mathf.Abs(bounds.size.y);
		if (boundsWidth <= 0.01f || boundsHeight <= 0.01f) return;

		Vector2 targetSize = parentSize * Overscan;
		float coverScale = Mathf.Max(targetSize.x / boundsWidth, targetSize.y / boundsHeight);
		if (float.IsNaN(coverScale) || float.IsInfinity(coverScale)) return;

		coverScale = Mathf.Clamp(coverScale * MeshCropScale, 0.25f, 8f);
		rectTransform.localScale = new Vector3(coverScale, coverScale, 1f);
		rectTransform.anchoredPosition = new Vector2(-bounds.center.x * coverScale, -bounds.center.y * coverScale);
	}

	private void CacheComponents()
	{
		if (rectTransform == null) rectTransform = transform as RectTransform;
		if (graphic == null) graphic = GetComponent<SkeletonGraphic>();
		if (parent == null && rectTransform != null) parent = rectTransform.parent as RectTransform;
	}
}
