# Subtitle Edit cli 

Code is based on SE 3.6.7

Imaged based formats/OCR was removed.
To update, the `SubtitleFormats` should be copied from latest SE.

How to compile: `dotnet build seconv.csproj`

How to run: `./seconv <pattern> <name-of-format-without-spaces> [<optional-parameters>]`.
E.g.: `./seconv *.sub subrip` - for more info see https://www.nikse.dk/subtitleedit/help#commandline

This was made due to https://github.com/SubtitleEdit/subtitleedit/issues/3568
