﻿using System.Text;
using seconv.libse.Common;

namespace seconv.libse.SubtitleFormats
{
    public class JsonType8 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 8";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            if (_errorCount >= subtitle.Paragraphs.Count)
            {
                return false;
            }
            var avgDurSecs = subtitle.Paragraphs.Average(p => p.Duration.TotalSeconds);
            return avgDurSecs < 60;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder(@"[");
            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.Append(',');
                }

                sb.Append("{\"start_time\":");
                sb.Append(p.StartTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"end_time\":");
                sb.Append(p.EndTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"text\":\"");
                sb.Append(Json.EncodeJsonText(p.Text));
                sb.Append("\"}");
                count++;
            }
            sb.Append(']');
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

            string allText = sb.ToString().Trim();
            if (!(allText.StartsWith("{", StringComparison.Ordinal) || allText.StartsWith("[", StringComparison.Ordinal)))
            {
                return;
            }

            foreach (string line in allText.Split('{', '}', '[', ']'))
            {
                string s = line.Trim();
                if (s.Length > 10)
                {
                    string start = Json.ReadTag(s, "start_time");
                    string end = Json.ReadTag(s, "end_time");
                    string text = Json.ReadTag(s, "text");
                    if (start != null && end != null && text != null)
                    {
                        if (double.TryParse(start, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var startSeconds) &&
                            double.TryParse(end, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var endSeconds))
                        {
                            subtitle.Paragraphs.Add(new Paragraph(Json.DecodeJsonText(text), startSeconds * TimeCode.BaseUnit, endSeconds * TimeCode.BaseUnit));
                        }
                        else
                        {
                            _errorCount++;
                        }
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }
            subtitle.Renumber();
        }
    }
}
