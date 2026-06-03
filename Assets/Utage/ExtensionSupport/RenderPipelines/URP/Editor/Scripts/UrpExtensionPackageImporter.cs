#if UTAGE_URP_EDITOR
using UnityEditor;

namespace Utage.RenderPipeline.Urp
{
	public class UrpExtensionPackageImporter
	{
		//エディタ起動時やスクリプトリロード時に、追加パッケージの情報を登録
		[InitializeOnLoadMethod]
		static void Initialize()
		{
			//デフォルトアセット用のパッケージ
			ExtensionPackageManager.Instance.AddPackage(new ExtensionPackage("d823085cc08011a418659a5eb6fabaec",1));
#if URP_17_OR_NEWER
			//Urp17以降用のパッケージ
			//シェーダーグラフのアセットなど
			ExtensionPackageManager.Instance.AddPackage(new ExtensionPackage("ba83337a383980049a8d1796c51d7c7c", 1));
#endif
		}
	}
}
#endif
