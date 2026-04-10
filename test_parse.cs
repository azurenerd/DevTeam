using System.Text.RegularExpressions;

class Test {
    static void Main() {
        // The actual regex from CodeFileParser.cs line 161
        var pattern = @'(?:^|\n)\s*(?:\*\*)?FILE:\s*(?:)?(?<path>[^\n*]+?)(?:)?(?:\*\*)?\s*\n\s*`(?<lang>\w*)\n(?<content>.*?)`';
        var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        
        // Example of AI output with malformed filename
        var aiOutput = @'FILE: src/@response{config}.cs
`csharp
public class Config { }
`';
        
        var match = regex.Match(aiOutput);
        if (match.Success) {
            Console.WriteLine(""Path captured: "" + match.Groups[""path""].Value.Trim());
        }
    }
}
