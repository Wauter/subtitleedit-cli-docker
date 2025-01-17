﻿using System.Text;
using seconv.libse.Common;

namespace seconv.libse.SubtitleFormats
{
    public class JsonType5 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 5";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.Append("{\"text_tees\":[");
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                sb.Append(p.StartTime.TotalMilliseconds);
                sb.Append(',');
                sb.Append(p.EndTime.TotalMilliseconds);
                if (i < subtitle.Paragraphs.Count - 1)
                {
                    sb.Append(',');
                }
            }
            sb.Append("],");

            sb.Append("\"text_target\":[");
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                sb.Append("[\"w1\",\"w3\"],[\"w1\",\"w3\"]");
                if (i < subtitle.Paragraphs.Count - 1)
                {
                    sb.Append(',');
                }
            }
            sb.Append("],");

            sb.Append("\"text_content\":[");
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                sb.Append('[');
                Paragraph p = subtitle.Paragraphs[i];
                var lines = p.Text.Replace(Environment.NewLine, "\n").Split('\n');
                for (int j = 0; j < lines.Length; j++)
                {
                    sb.Append('"');
                    sb.Append(Json.EncodeJsonText(lines[j]));
                    sb.Append('"');
                    if (j < lines.Length - 1)
                    {
                        sb.Append(',');
                    }
                }
                sb.Append("],");
                if (i < subtitle.Paragraphs.Count - 1)
                {
                    sb.Append("[\"\",\"\"],");
                }
                else
                {
                    sb.Append("[\"\",\"\"]");
                }
            }
            sb.Append("],");

            sb.Append("\"text_styles\":[");
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                sb.Append("[\"s1\",\"s2\"],[\"s1\",\"s2\"]");
                if (i < subtitle.Paragraphs.Count - 1)
                {
                    sb.Append(',');
                }
            }
            sb.Append("],");

            sb.Append("\"timerange\":[");
            var timeRangeParagraph = subtitle.GetParagraphOrDefault(0);
            if (timeRangeParagraph == null)
            {
                sb.Append('0');
            }
            else
            {
                sb.Append(timeRangeParagraph.StartTime.TotalMilliseconds);
            }

            sb.Append(',');
            timeRangeParagraph = subtitle.GetParagraphOrDefault(subtitle.Paragraphs.Count - 1);
            if (timeRangeParagraph == null)
            {
                sb.Append('0');
            }
            else
            {
                sb.Append(timeRangeParagraph.EndTime.TotalMilliseconds);
            }

            sb.Append(']');

            sb.Append('}');

            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            foreach (string s in lines)
            {
                sb.Append(s);
            }

            var allText = sb.ToString();
            if (!allText.Contains("text_tees"))
            {
                return;
            }

            var times = Json.ReadArray(allText, "text_tees");
            var texts = Json.ReadArray(allText, "text_content");

            for (var i = 0; i < Math.Min(times.Count, texts.Count); i++)
            {
                var text = texts[i];
                if (text.StartsWith('['))
                {
                    var textLines = Json.ReadArray("{\"text\":" + texts[i] + "}", "text");
                    var textSb = new StringBuilder();
                    foreach (string line in textLines)
                    {
                        var t = Json.DecodeJsonText(line);
                        if (t.StartsWith("[\"", StringComparison.Ordinal) && t.EndsWith("\"]", StringComparison.Ordinal))
                        {
                            var innerSb = new StringBuilder();
                            var innerTextLines = Json.ReadArray("{\"text\":" + t + "}", "text");
                            foreach (string innerLine in innerTextLines)
                            {
                                innerSb.Append(' ');
                                innerSb.Append(innerLine);
                            }
                            textSb.AppendLine(innerSb.ToString().Trim());
                        }
                        else
                        {
                            textSb.AppendLine(t);
                        }
                    }
                    text = textSb.ToString().Trim();
                    text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                }

                if (int.TryParse(times[i], out var startMs))
                {
                    var p = new Paragraph(text, startMs, 0);
                    if (i + 1 < times.Count && int.TryParse(times[i + 1], out var endMs))
                    {
                        p.EndTime.TotalMilliseconds = endMs;
                    }
                    subtitle.Paragraphs.Add(p);
                }
                else
                {
                    _errorCount++;
                }
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }
    }
}
