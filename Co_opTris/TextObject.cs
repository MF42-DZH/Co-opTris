using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpriteSheetText
{
    public class TextObject
    {
        private Texture2D TextTexture { get; set; }

        readonly private string[] AlphabetBlock = { "A", "B", "C", "D", "E", "F", "G",
                                                  "H", "I", "J", "K", "L", "M", "N",
                                                  "O", "P", "Q", "R", "S", "T", "U",
                                                  "V", "W", "X", "Y", "Z" };

        readonly private string[] NumbersBlock = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        readonly private string[] SymbolBlock = { ".", "!", ":", ",", "?" };

        const int TotalLength = 42;

        // Text class constructor.
        public TextObject(Texture2D texture)
        {
            TextTexture = texture;
        }

        public void Draw(SpriteBatch spriteBatch, string text, Vector2 location, Color color)
        {
            text = text.ToUpper();
            int width = TextTexture.Width / (AlphabetBlock.Length + NumbersBlock.Length + SymbolBlock.Length + 1);
            int height = TextTexture.Height;
            int indexOfChar = TotalLength;

            for (int currentCharacter = 0; currentCharacter < text.Length; currentCharacter++)
            {
                indexOfChar = TotalLength;

                string CC = text[currentCharacter].ToString();
                if (AlphabetBlock.Contains(CC))
                {
                    indexOfChar = Array.IndexOf(AlphabetBlock, CC);
                }
                else if (NumbersBlock.Contains(CC))
                {
                    indexOfChar = AlphabetBlock.Length + Array.IndexOf(NumbersBlock, CC);
                }
                else if (SymbolBlock.Contains(CC))
                {
                    indexOfChar = NumbersBlock.Length + AlphabetBlock.Length + Array.IndexOf(SymbolBlock, CC);
                }

                spriteBatch.Draw(TextTexture,
                                 new Rectangle((int)location.X + (currentCharacter * width), (int)location.Y, width, height),
                                 new Rectangle(indexOfChar * width, 0, width, height), color);
            }
        }

        // Overload for no given colour.
        public void Draw(SpriteBatch spriteBatch, string text, Vector2 location)
        {
            text = text.ToUpper();
            int width = TextTexture.Width / (AlphabetBlock.Length + NumbersBlock.Length + SymbolBlock.Length + 1);
            int height = TextTexture.Height;
            int indexOfChar = TotalLength;

            for (int currentCharacter = 0; currentCharacter < text.Length; currentCharacter++)
            {
                indexOfChar = TotalLength;

                string CC = text[currentCharacter].ToString();
                if (AlphabetBlock.Contains(CC))
                {
                    indexOfChar = Array.IndexOf(AlphabetBlock, CC);
                }
                else if (NumbersBlock.Contains(CC))
                {
                    indexOfChar = AlphabetBlock.Length + Array.IndexOf(NumbersBlock, CC);
                }
                else if (SymbolBlock.Contains(CC))
                {
                    indexOfChar = NumbersBlock.Length + AlphabetBlock.Length + Array.IndexOf(SymbolBlock, CC);
                }

                spriteBatch.Draw(TextTexture,
                                 new Rectangle((int)location.X + (currentCharacter * width), (int)location.Y, width, height),
                                 new Rectangle(indexOfChar * width, 0, width, height), Color.White);
            }
        }
    }
}
