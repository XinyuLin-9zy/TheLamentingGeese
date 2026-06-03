using Utage;

public class ChapterCommand : AdvCommand
{
	private string ChapterName;

	public ChapterCommand(StringGridRow row)
		: base(row)
	{
		ChapterName = ParseCellOptional<string>(AdvColumnName.Arg1, "");
	}

	public override void DoCommand(AdvEngine engine)
	{
		PlotMapProgressStore.CurrentChapterName = ChapterName;
	}
}
