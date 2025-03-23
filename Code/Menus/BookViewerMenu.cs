using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace SunberryVillage.Menus;
internal class BookViewerMenu : LetterViewerMenu
{
	internal List<string> Pages = [];
	internal List<FormatModel> PageFormats = [];

	internal Texture2D defaultTexture;
	public BookViewerMenu(string text) : base(text) { }

	public BookViewerMenu(int secretNoteIndex) : base(secretNoteIndex) { }

	public BookViewerMenu(List<string> pages, string mail, string mailTitle, bool fromCollection = false) : base(mail, mailTitle, fromCollection)
	{
		defaultTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
		Pages = pages;

		Regex leadingNewlines = new(@"^\^+", RegexOptions.Compiled);
		Regex trailingNewlines = new(@"\^+$", RegexOptions.Compiled);

		foreach (string pageString in Pages)
		{
			FormatModel pageFormat = ParseCustomFormatting(pageString, out string parsed) ?? new FormatModel {customTextColor = null, letterTexture = defaultTexture };

			parsed = leadingNewlines.Replace(parsed, "");
			parsed = trailingNewlines.Replace(parsed, "");

			List<string> parsedPages = SpriteText.getStringBrokenIntoSectionsOfHeight(parsed, width - 64, height - 128);
			
			foreach (string parsedPage in parsedPages)
			{
				pageFormat.letterTexture ??= defaultTexture;
				PageFormats.Add(pageFormat);
			}
		}

		ApplyCustomFormattingPerPage();
	}

	public static BookViewerMenu GetMenu(List<string> pages, string bookTitle)
	{
		string book = string.Join("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^", pages);
		return new BookViewerMenu(pages, book, bookTitle);
	}

	public override void OnPageChange()
	{
		base.OnPageChange();

		// apply custom formatting per page rather than to the letter as a whole
		ApplyCustomFormattingPerPage();
	}

	private void ApplyCustomFormattingPerPage()
	{
		if (-1 >= page || page >= PageFormats.Count)
			return;

		FormatModel format = PageFormats[page];
		customTextColor = format.customTextColor;
		usingCustomBackground = format.usingCustomBackground;
		whichBG = format.whichBG;
		letterTexture = format.letterTexture;
	}

	public FormatModel ParseCustomFormatting(string text, out string result)
	{
		FormatModel format = new();

        text = Dialogue.applyGenderSwitchBlocks(Game1.player.Gender, text);
        for (int index = text.IndexOf("[", StringComparison.Ordinal); index >= 0; index = text.IndexOf("[", index + 1, StringComparison.Ordinal))
        {
            int endIndex = text.IndexOf("]", index, StringComparison.Ordinal);
            
            if (endIndex < 0)
	            continue;

            bool validTag = false;
            try
            {
	            string[] split = ArgUtility.SplitBySpace(text.Substring(index + 1, endIndex - index - 1));
	            string text2 = split[0];
	            switch (text2)
	            {
		            case "letterbg":
			            switch (split.Length)
			            {
				            case 2:
					            format.whichBG = int.Parse(split[1]);
					            break;
				            case 3:
					            format.usingCustomBackground = true;
					            format.letterTexture = Game1.temporaryContent.Load<Texture2D>(split[1]);
					            format.whichBG = int.Parse(split[2]);
					            break;
			            }

			            validTag = true;
			            break;
		            case "textcolor":
		            {
			            string colorString = split[1].ToLower();
			            string[] colorLookup = ["black", "blue", "red", "purple", "white", "orange", "green", "cyan", "gray", "jojablue"
			            ];
			            format.customTextColor = null;
			            for (int i = 0; i < colorLookup.Length; i++)
			            {
				            if (colorString != colorLookup[i])
					            continue;

				            format.customTextColor = SpriteText.getColorFromIndex(i);
				            break;
			            }

			            validTag = true;
			            break;
		            }
	            }
            }
            catch (Exception e)
            {
				Log.Error($"Failed while parsing book \"{mailTitle}\": {e}");
            }

            if (!validTag)
	            continue;

            text = text.Remove(index, endIndex - index + 1);
            index--;
        }

        result = text;
        return format;
	}
}

public class FormatModel
{
	internal Color? customTextColor;
	internal bool usingCustomBackground;
	internal int whichBG;
	internal Texture2D letterTexture;
}