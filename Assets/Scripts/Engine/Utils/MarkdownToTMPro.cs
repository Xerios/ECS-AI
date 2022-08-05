using System.Text.RegularExpressions;

public static class MarkdownToTMPro
{
    // const string H1_PATTERN = @"(^#\s)(.+?)(\r\n)";
    // const string H2_PATTERN = @"(^##\s)(.+?)(\r\n)";
    // const string QUOTE_PATTERN = @"(^>\s)(.+?)(\r\n)";
    // const string UNORDERED_LIST_PATTERN = @"(^\*\s)(.+?)(\r\n)";
    const string BOLD_PATTERN = @"\*(.+?)\*";
    const string BOLD_PATTERN_REPLACE = @"<b><color=#ffa500ff>$1</color></b>";

    // Must do this AFTER bold
    // TODO, test:  @"\*([^\r\n\t\v *]+)\*(?!\*)"
    // const string ITALICS_PATTERN = @"(\*)(\S+\S)(\*)";
    // const string ORDERED_LIST_PATTERN = @"(^\d\.\s)(.+?)(\r?\n?)";

    // const string H1_TMP_PREFIX = "<indent=0%><size=46><color=#ffa500ff> <u>";
    // const string H1_TMP_SUFFIX = "</u>:</color></size>";

    // // QUOTE==H2
    // // <indent=3%> <size=36><color=#ffa500ff> <u>Class Fixes</u>:</color></size>
    // const string H2_QUOTE_TMP_PREFIX = "<indent=3%> <size=36><color=#ffa500ff> <u>";
    // const string H2_QUOTE_TMP_SUFFIX = "</u>:</color></size>";

    // // <indent=4%> - <indent=6%>Added ability for devs to swap regions without a patch if servers go unstable.
    // const string TMP_UOLIST_PREFIX = "<indent=4%> - <indent=6%>";
    // const string TMP_OLIST_PREFIX_1 = "<indent=4%> ";
    // const string TMP_OLIST_PREFIX_2 = "<indent=6%>";


    public static string Convert (string value)
    {
        return Regex.Replace(value, BOLD_PATTERN, BOLD_PATTERN_REPLACE, RegexOptions.Multiline);
    }
}