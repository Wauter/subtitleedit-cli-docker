﻿using System.Text;
using System.Text.RegularExpressions;
using seconv.libse.Common;

namespace seconv.libse.SubtitleFormats
{
    public class UnknownSubtitle103 : SubtitleFormat
    {
        public override string Extension => ".txt";

        public override string Name => "Unknown 103";

        private static readonly Regex RegexTimeCodeLine = new Regex(@"^ {15}.+\s+\d\d:\d\d:\d\d\s*", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCode = new Regex(@"\d\d:\d\d:\d\d", RegexOptions.Compiled);

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            string writeFormat = string.Empty.PadLeft(16, ' ') + "{0} ";
            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var p = subtitle.Paragraphs[index];
                var start = p.StartTime.Hours.ToString().PadLeft(2, '0') + ":" +
                            p.StartTime.Minutes.ToString().PadLeft(2, '0') + ":" +
                            p.StartTime.Seconds.ToString().PadLeft(2, '0');
                var text = new StringBuilder(Utilities.UnbreakLine(HtmlUtil.RemoveHtmlTags(p.Text, true)));
                while (text.Length < (73 - 17))
                {
                    text.Append(" ");
                }
                sb.AppendLine(string.Format(writeFormat, text) + start);
            }
            sb.AppendLine();

            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            var timeCodeSplitChars = new[] { ':' };
            foreach (string line in lines)
            {
                if (line.Length >= 70 && RegexTimeCodeLine.IsMatch(line))
                {
                    var match = RegexTimeCode.Match(line);
                    string start = match.Value;
                    var text = line.Substring(14, match.Index - 14).Trim();
                    string[] startParts = start.Split(timeCodeSplitChars, StringSplitOptions.RemoveEmptyEntries);
                    var tc = new TimeCode(int.Parse(startParts[0]), int.Parse(startParts[1]), int.Parse(startParts[2]), 0);
                    var p = new Paragraph(text, tc.TotalMilliseconds, tc.TotalMilliseconds);
                    subtitle.Paragraphs.Add(p);
                }
                else if (subtitle.Paragraphs.Count > 0 && line.Trim().Length > 0)
                {
                    _errorCount++;
                }
            }

            subtitle.RecalculateDisplayTimes(Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds, null, Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds, true);
            subtitle.Renumber();
        }
    }
}
