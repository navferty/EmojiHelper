namespace EmojiHelper;

public class AppSettings
{
    public string? Setting1 { get; set; }
    public int Setting2 { get; set; }
}

public class AppSettingsInner1
{
    public const string SectionName = "folder1";
    public string? Setting1 { get; set; }
    public string? Setting2 { get; set; }
}

public class AppSettingsInner2
{
    public const string SectionName = "folder2";
    public string? Setting1 { get; set; }
    public string? Setting2 { get; set; }
}
