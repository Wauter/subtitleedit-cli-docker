﻿using seconv.libse.Common;
using seconv.libse.ContainerFormats.Mp4;

namespace seconv.libse.SubtitleFormats
{
    /// <summary>
    /// CMFT - "Common Media application Format Text"
    /// </summary>
    public class Cmaft : SubtitleFormat
    {

        public override string Extension => ".cmft";

        public const string NameOfFormat = "CMAF Text";

        public override string Name => NameOfFormat;

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".cmft", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var parser = new CmafParser(fileName);
                    return parser.Subtitle.Paragraphs.Count > 0;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not implemented!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var parser = new CmafParser(fileName);
            subtitle.Paragraphs.Clear();
            subtitle.Paragraphs.AddRange(parser.Subtitle.Paragraphs);
        }
    }
}
