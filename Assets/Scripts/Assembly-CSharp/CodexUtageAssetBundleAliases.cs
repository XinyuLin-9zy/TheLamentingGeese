public static class CodexUtageAssetBundleAliases
{
	public static readonly Entry[] Entries =
	{
		new Entry("texture/bg/青楼外街道3.asset", "texture/bg/青楼外街道2.asset"),
		new Entry("texture/bg/end1.asset", "texture/event/end1/1.asset"),
		new Entry("texture/bg/end2.asset", "texture/event/end2/1.asset"),
		new Entry("sound/bgm/Amb_CangYing.asset", "sound/ambience/amb_summernight.asset"),
		new Entry("sound/se/Se_PaoBu_ChenZhong.asset", "sound/se/se_沉重脚步.asset"),
		new Entry("sound/se/Se_洗手.asset", "sound/se/se_洗脸.asset"),
	};

	public sealed class Entry
	{
		public Entry(string requestBundleName, string sourceBundleName)
		{
			RequestBundleName = requestBundleName;
			SourceBundleName = sourceBundleName;
		}

		public string RequestBundleName { get; private set; }
		public string SourceBundleName { get; private set; }
	}
}
