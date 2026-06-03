// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// クロスフェード可能なRawImage表示
	/// </summary>
	[AddComponentMenu("Utage/Lib/UI/UguiCrossFadeRawImage")]
	public class UguiCrossFadeRawImage : MonoBehaviour, IMeshModifier, IMaterialModifier
	{
		public Texture FadeTexture
		{
			get
			{
				return fadeTexture;
			}
			set
			{
				if (fadeTexture == value)
					return;

				fadeTexture = value;
				Target.SetVerticesDirty();
				Target.SetMaterialDirty();
			}
		}
		[SerializeField]
		Texture fadeTexture;


		float Strengh
		{
			get { return strengh; }
			set
			{
				strengh = value;
				Target.SetMaterialDirty();
			}
		}


		[SerializeField, Range(0, 1.0f)]
		float strengh = 1;

		public virtual Graphic Target { get { return this.GetComponentCache(ref target); } }
		protected Graphic target;

		public Timer Timer
		{
			get
			{
				if (timer == null)
				{
					timer = this.gameObject.AddComponent<Timer>();
				}
				return timer;
			}
		}
		Timer timer;

		protected Material lastMaterial;
		public Material Material
		{
			get
			{
				return Target.material;
			}
			set
			{
				Target.material = value;
			}
		}
		protected Material corssFadeMaterial;

		protected virtual void Awake()
		{
			lastMaterial = Target.material;
			Material sourceMaterial = lastMaterial != null ? lastMaterial : Target.defaultMaterial;
			corssFadeMaterial = new Material(ShaderManager.CrossFade);
			if (sourceMaterial != null)
			{
				CopyUiState(sourceMaterial, corssFadeMaterial);
				corssFadeMaterial.name = sourceMaterial.name + " (CrossFade)";
			}
			Material = corssFadeMaterial;
		}

		static void CopyUiState(Material sourceMaterial, Material destinationMaterial)
		{
			CopyFloatIfExists(sourceMaterial, destinationMaterial, "_StencilComp");
			CopyFloatIfExists(sourceMaterial, destinationMaterial, "_Stencil");
			CopyFloatIfExists(sourceMaterial, destinationMaterial, "_StencilOp");
			CopyFloatIfExists(sourceMaterial, destinationMaterial, "_StencilWriteMask");
			CopyFloatIfExists(sourceMaterial, destinationMaterial, "_StencilReadMask");
			CopyFloatIfExists(sourceMaterial, destinationMaterial, "_ColorMask");
			CopyFloatIfExists(sourceMaterial, destinationMaterial, "_UseUIAlphaClip");

			if (sourceMaterial.HasProperty("_Color") && destinationMaterial.HasProperty("_Color"))
			{
				destinationMaterial.SetColor("_Color", sourceMaterial.GetColor("_Color"));
			}

			if (sourceMaterial.IsKeywordEnabled("UNITY_UI_ALPHACLIP"))
			{
				destinationMaterial.EnableKeyword("UNITY_UI_ALPHACLIP");
			}
			else
			{
				destinationMaterial.DisableKeyword("UNITY_UI_ALPHACLIP");
			}
		}

		static void CopyFloatIfExists(Material sourceMaterial, Material destinationMaterial, string propertyName)
		{
			if (!sourceMaterial.HasProperty(propertyName) || !destinationMaterial.HasProperty(propertyName))
			{
				return;
			}

			destinationMaterial.SetFloat(propertyName, sourceMaterial.GetFloat(propertyName));
		}

		void OnDestroy()
		{
			Material = lastMaterial;
			Destroy(corssFadeMaterial);
			Destroy(timer);
		}

#if UNITY_EDITOR
		void OnValidate()
		{
			Target.SetVerticesDirty();
			Target.SetMaterialDirty();
		}
#endif

		public void ModifyMesh(Mesh mesh)
		{
			using (var helper = new VertexHelper(mesh))
			{
				ModifyMesh(helper);
				helper.FillMesh(mesh);
			}
		}

		public void ModifyMesh(VertexHelper vh)
		{
			Texture tex = Target.mainTexture;
			if (tex == null) return;

			RebuildVertex(vh);
		}

		public virtual void RebuildVertex(VertexHelper vh)
		{
			UIVertex vert = new UIVertex();
			for (int i = 0; i < vh.currentVertCount; i++)
			{
				vh.PopulateUIVertex(ref vert, i);
				vert.uv1 = vert.uv0;
				vh.SetUIVertex(vert, i);
			}
		}


		public Material GetModifiedMaterial(Material baseMaterial)
		{
			if (baseMaterial == null) return baseMaterial;
			baseMaterial.SetFloat("_Strength", Strengh);
			baseMaterial.SetTexture("_FadeTex", FadeTexture);
			return baseMaterial;
		}

		internal void CrossFade(Texture fadeTexture, float time, Action onComplete)
		{
			this.FadeTexture = fadeTexture;
			Target.material.EnableKeyword("CROSS_FADE");

			Timer.StartTimer(
				time,
				x => Strengh = x.Time01Inverse,
				x =>
				{
					Target.material.DisableKeyword("CROSS_FADE");
					onComplete();
				});
		}
		
		internal void Restart(float time, Action onComplete)
		{
			Timer.StartTimer(
				time,
				x => Strengh = x.Time01Inverse,
				x =>
				{
					Target.material.DisableKeyword("CROSS_FADE");
					onComplete();
				});
		}
	}
}
