<Query Kind="Program" />

void Main()
{
    var files =
        Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "output", "html"))
            .Select(path =>
                new
                {
                    Path = path,
                    Contents = 
                        Regex.Replace(
                            File.ReadAllText(path),
                            @"<div class=""pageHeader"" id=""PageHeader"">.*?</div>",
                            @"<div class=""pageHeader"" id=""PageHeader""><a href=""/"">Rock Framework</a></div>")
                });

    foreach (var file in files)
    {
        File.WriteAllText(file.Path, file.Contents);
    }
}