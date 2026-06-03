using System;
using System.Collections.Generic;
using UnityEngine;
using Utage;
using UtageExtensions;
using UI;

public class TitleAnimationCommand : AdvCommand
{
	private Sprite title_bg;

	private Sprite title_text;

	private Sprite chapter_bg;

	private string chapterBgPath;

	private string chapterTitleBgPath;

	private string chapterTitlePath;

	private TitleAnimationType particleType;

	private int offset;

	private float scale;

	private UI_TitleAnimation animation;

	public TitleAnimationCommand(StringGridRow row)
		: base(row)
	{
		chapterBgPath = ParseCellOptional<string>(AdvColumnName.Arg1, "");
		chapterTitleBgPath = ParseCellOptional<string>(AdvColumnName.Arg2, "");
		chapterTitlePath = ParseCellOptional<string>(AdvColumnName.Arg3, "");
		particleType = ParseAnimationType(ParseCellOptional<string>(AdvColumnName.Arg4, "Liang"));
		offset = ParseCellOptional<int>(AdvColumnName.Arg5, 0);
		scale = ParseCellOptional<float>(AdvColumnName.Arg6, 1f);
	}

	public override void DoCommand(AdvEngine engine)
	{
		title_bg = LoadSprite("title/chapterbg/" + chapterBgPath);
		chapter_bg = LoadSprite("title/titlebg/" + chapterTitleBgPath);
		title_text = LoadLocalizedTitleSprite(chapterTitlePath, particleType);

		CustomCommander commander = null;
		if (engine != null)
		{
			commander = engine.GetComponentInChildren<CustomCommander>(true);
		}
		if (commander == null)
		{
			commander = WrapperFindObject.FindObjectOfTypeIncludeInactive<CustomCommander>();
		}
		if (commander == null) return;

		ClearCameraFadeEffects(engine);

		float commandScale = Mathf.Approximately(scale, 0) ? 1f : scale;
		float titleScale = commander.GetTitleAnimationScale(chapterTitlePath);
		animation = commander.ShowTitleAnimation(title_bg, chapter_bg, title_text, particleType, false, offset, commandScale * titleScale);
	}

	public override bool Wait(AdvEngine engine)
	{
		return animation != null && animation.IsPlaying;
	}

	private Sprite LoadLocalizedTitleSprite(string spriteName, TitleAnimationType type)
	{
		if (string.IsNullOrEmpty(spriteName)) return null;

		foreach (string folder in GetLanguageFolders())
		{
			foreach (string candidate in GetTitleSpriteCandidates(spriteName, type))
			{
				Sprite sprite = LoadSprite("title/textsprite/" + folder + "/" + candidate);
				if (sprite != null) return sprite;
			}
		}

		return null;
	}

	private string[] GetLanguageFolders()
	{
		string current = LanguageManagerBase.Instance != null ? LanguageManagerBase.Instance.CurrentLanguage : "";
		string folder = LanguageToFolder(current);

		return new[]
		{
			folder,
			"sc",
			"tc",
			"english",
			"japanese",
			"russian"
		};
	}

	private string LanguageToFolder(string language)
	{
		if (string.IsNullOrEmpty(language)) return "sc";

		string lower = language.ToLowerInvariant();
		if (lower.Contains("tc") || lower.Contains("traditional")) return "tc";
		if (lower.Contains("english") || lower == "en") return "english";
		if (lower.Contains("japanese") || lower == "ja") return "japanese";
		if (lower.Contains("russian") || lower == "ru") return "russian";
		return "sc";
	}

	private Sprite LoadSprite(string path)
	{
		if (string.IsNullOrEmpty(path)) return null;

		Sprite sprite = Resources.Load<Sprite>(path);
		if (sprite != null) return sprite;

		Sprite[] sprites = Resources.LoadAll<Sprite>(path);
		if (sprites != null && sprites.Length > 0) return sprites[0];

		int splitIndex = path.LastIndexOf('/');
		if (splitIndex <= 0 || splitIndex >= path.Length - 1) return null;

		string folder = path.Substring(0, splitIndex);
		string assetName = path.Substring(splitIndex + 1);
		Sprite[] folderSprites = Resources.LoadAll<Sprite>(folder);
		if (folderSprites == null) return null;

		foreach (Sprite folderSprite in folderSprites)
		{
			if (folderSprite != null && string.Equals(folderSprite.name, assetName, StringComparison.OrdinalIgnoreCase))
			{
				return folderSprite;
			}
		}

		return null;
	}

	private TitleAnimationType ParseAnimationType(string raw)
	{
		if (string.IsNullOrEmpty(raw)) return TitleAnimationType.Liang;

		switch (raw.Trim().ToLowerInvariant())
		{
			case "be":
			case "bad":
			case "badend":
				return TitleAnimationType.BadEnd;
			case "ne":
			case "normal":
			case "normalend":
				return TitleAnimationType.NormalEnd;
			case "te":
			case "true":
			case "trueend":
				return TitleAnimationType.TrueEnd;
		}

		TitleAnimationType type;
		return Enum.TryParse(raw, true, out type) ? type : TitleAnimationType.Liang;
	}

	private IEnumerable<string> GetTitleSpriteCandidates(string spriteName, TitleAnimationType type)
	{
		List<string> candidates = new List<string>();
		AddCandidate(candidates, spriteName);
		AddCandidate(candidates, GetAliasTitleSpriteName(spriteName, type));
		AddCandidate(candidates, GetLegacyTitleSpriteName(spriteName, type));
		return candidates;
	}

	private string GetAliasTitleSpriteName(string spriteName, TitleAnimationType type)
	{
		if (string.IsNullOrEmpty(spriteName)) return null;

		if (!spriteName.StartsWith("chapter_title_E", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		string suffix = spriteName.Substring("chapter_title_E".Length);
		switch (type)
		{
			case TitleAnimationType.BadEnd:
				return "chapter_title_BE" + suffix;
			case TitleAnimationType.NormalEnd:
				return "chapter_title_NE" + suffix;
			case TitleAnimationType.TrueEnd:
				return "chapter_title_TE" + suffix;
			default:
				return null;
		}
	}

	private string GetLegacyTitleSpriteName(string spriteName, TitleAnimationType type)
	{
		if (string.IsNullOrEmpty(spriteName)) return null;

		switch (type)
		{
			case TitleAnimationType.BadEnd:
				return ReplaceTitlePrefix(spriteName, "chapter_title_BE", "chapter_title_E");
			case TitleAnimationType.NormalEnd:
				return ReplaceTitlePrefix(spriteName, "chapter_title_NE", "chapter_title_E");
			case TitleAnimationType.TrueEnd:
				return ReplaceTitlePrefix(spriteName, "chapter_title_TE", "chapter_title_E");
			default:
				return null;
		}
	}

	private string ReplaceTitlePrefix(string spriteName, string fromPrefix, string toPrefix)
	{
		if (string.IsNullOrEmpty(spriteName) || !spriteName.StartsWith(fromPrefix, StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		return toPrefix + spriteName.Substring(fromPrefix.Length);
	}

	private void AddCandidate(List<string> candidates, string candidate)
	{
		if (string.IsNullOrEmpty(candidate)) return;
		if (candidates.Contains(candidate)) return;
		candidates.Add(candidate);
	}

	private void ClearCameraFadeEffects(AdvEngine engine)
	{
		if (engine == null || engine.CameraManager == null) return;

		foreach (CameraRoot cameraRoot in engine.CameraManager.CameraList)
		{
			if (cameraRoot == null) continue;

			LetterBoxCamera letterBoxCamera = cameraRoot.LetterBoxCamera;
			if (letterBoxCamera == null) continue;

			GameObject cameraObject = letterBoxCamera.gameObject;
			cameraObject.SafeRemoveComponent<ColorFade>();
			cameraObject.SafeRemoveComponent<RuleFade>();
		}
	}
}
