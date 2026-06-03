#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CodexIOSAppIcon
{
    private const string IconAssetPath = "Assets/AppIcon/AppIcon-iOS-1024.png";

    [MenuItem("Codex/iOS/Apply App Icon")]
    public static void Apply()
    {
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconAssetPath);
        if (icon == null)
        {
            throw new InvalidOperationException("Missing iOS app icon asset: " + IconAssetPath);
        }

        ConfigureImporter(IconAssetPath);
        ApplyLegacyIcons(icon);
        AssetDatabase.SaveAssets();
        Debug.LogFormat("CODEX_IOS_APP_ICON_APPLIED path={0} size={1}x{2}", IconAssetPath, icon.width, icon.height);
    }

    public static void PostprocessXcodeProject(string xcodeProjectPath)
    {
        if (string.IsNullOrEmpty(xcodeProjectPath)) return;

        string appIconSetPath = Path.Combine(
            xcodeProjectPath,
            "Unity-iPhone",
            "Images.xcassets",
            "AppIcon.appiconset");
        if (!Directory.Exists(appIconSetPath))
        {
            Debug.LogWarning("iOS AppIcon.appiconset was not found: " + appIconSetPath);
            return;
        }

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string sourceIconPath = Path.Combine(projectRoot, IconAssetPath);
        string marketingIconPath = Path.Combine(appIconSetPath, "Icon-AppStore-1024.png");
        WriteOpaquePng(sourceIconPath, marketingIconPath, 1024, 1024);

        foreach (string iconPath in Directory.GetFiles(appIconSetPath, "*.png"))
        {
            int width;
            int height;
            if (TryGetPngSize(iconPath, out width, out height))
            {
                WriteOpaquePng(iconPath, iconPath, width, height);
            }
        }

        WriteContentsJson(appIconSetPath);
        Debug.Log("CODEX_IOS_APP_ICON_XCODE_POSTPROCESSED path=" + appIconSetPath);
    }

    private static void ConfigureImporter(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        bool dirty = false;
        if (importer.textureType != TextureImporterType.Default)
        {
            importer.textureType = TextureImporterType.Default;
            dirty = true;
        }
        if (!importer.isReadable)
        {
            importer.isReadable = true;
            dirty = true;
        }
        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            dirty = true;
        }
        if (importer.alphaSource != TextureImporterAlphaSource.None)
        {
            importer.alphaSource = TextureImporterAlphaSource.None;
            dirty = true;
        }
        if (!importer.DoesSourceTextureHaveAlpha() && importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = false;
            dirty = true;
        }

        TextureImporterPlatformSettings iosSettings = importer.GetPlatformTextureSettings("iPhone");
        if (!iosSettings.overridden || iosSettings.maxTextureSize < 1024 || iosSettings.format != TextureImporterFormat.RGBA32)
        {
            iosSettings.overridden = true;
            iosSettings.maxTextureSize = 1024;
            iosSettings.format = TextureImporterFormat.RGBA32;
            importer.SetPlatformTextureSettings(iosSettings);
            dirty = true;
        }

        if (dirty)
        {
            importer.SaveAndReimport();
        }
    }

    private static void ApplyLegacyIcons(Texture2D icon)
    {
        int[] sizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.iOS, IconKind.Application);
        if (sizes == null || sizes.Length == 0)
        {
            sizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.iOS);
        }
        if (sizes == null || sizes.Length == 0) return;

        Texture2D[] icons = sizes.Select(_ => icon).ToArray();
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iOS, icons, IconKind.Application);
    }

    private static void WriteOpaquePng(string sourcePath, string outputPath, int width, int height)
    {
        byte[] input = File.ReadAllBytes(sourcePath);
        Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!source.LoadImage(input))
        {
            throw new InvalidOperationException("Failed to load icon image: " + sourcePath);
        }

        RenderTexture previous = RenderTexture.active;
        RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, temporary);
        RenderTexture.active = temporary;

        Texture2D opaque = new Texture2D(width, height, TextureFormat.RGB24, false);
        opaque.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        opaque.Apply();

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        File.WriteAllBytes(outputPath, opaque.EncodeToPNG());

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(temporary);
        UnityEngine.Object.DestroyImmediate(source);
        UnityEngine.Object.DestroyImmediate(opaque);
    }

    private static bool TryGetPngSize(string path, out int width, out int height)
    {
        width = 0;
        height = 0;
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        bool loaded = texture.LoadImage(bytes);
        if (loaded)
        {
            width = texture.width;
            height = texture.height;
        }
        UnityEngine.Object.DestroyImmediate(texture);
        return loaded;
    }

    private static void WriteContentsJson(string appIconSetPath)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("{");
        builder.AppendLine("  \"images\" : [");
        AppendImage(builder, "Icon-iPhone-120.png", "iphone", "2x", "60x60", true);
        AppendImage(builder, "Icon-iPhone-180.png", "iphone", "3x", "60x60", true);
        AppendImage(builder, "Icon-iPad-76.png", "ipad", "1x", "76x76", true);
        AppendImage(builder, "Icon-iPad-152.png", "ipad", "2x", "76x76", true);
        AppendImage(builder, "Icon-iPad-167.png", "ipad", "2x", "83.5x83.5", true);
        AppendImage(builder, "Icon-AppStore-1024.png", "ios-marketing", "1x", "1024x1024", false);
        builder.AppendLine("  ],");
        builder.AppendLine("  \"info\" : {");
        builder.AppendLine("    \"author\" : \"xcode\",");
        builder.AppendLine("    \"version\" : 1");
        builder.AppendLine("  },");
        builder.AppendLine("  \"properties\" : {");
        builder.AppendLine("    \"pre-rendered\" : false");
        builder.AppendLine("  }");
        builder.AppendLine("}");
        File.WriteAllText(Path.Combine(appIconSetPath, "Contents.json"), builder.ToString());
    }

    private static void AppendImage(StringBuilder builder, string filename, string idiom, string scale, string size, bool comma)
    {
        builder.AppendLine("    {");
        builder.AppendLine("      \"filename\" : \"" + filename + "\",");
        builder.AppendLine("      \"idiom\" : \"" + idiom + "\",");
        builder.AppendLine("      \"scale\" : \"" + scale + "\",");
        builder.AppendLine("      \"size\" : \"" + size + "\"");
        builder.AppendLine(comma ? "    }," : "    }");
    }

}
#endif
