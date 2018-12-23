using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Co_opTris
{
    class BoxDrawing
    {
        public Texture2D Texture { get; set; }
        public Color color { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public bool LeftEdgeMode { get; set; }
        public bool RightEdgeMode { get; set; }
        public bool TopEdgeMode { get; set; }
        public bool BottomEdgeMode { get; set; }

        public BoxDrawing(Texture2D texture, bool leftEdge, bool rightEdge, bool topEdge, bool bottomEdge, Color tint)
        {
            Texture = texture;
            Rows = 3;
            Columns = 3;
            color = tint;
            LeftEdgeMode = leftEdge;
            RightEdgeMode = rightEdge;
            TopEdgeMode = topEdge;
            BottomEdgeMode = bottomEdge;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 location, int rows, int columns, Color tint = new Color())
        {
            int width = Texture.Width / Columns;
            int height = Texture.Height / Rows;

            if (tint != new Color())
            {
                color = tint;
            }

            Rectangle TLRect = new Rectangle(width * 0, height * 0, width, height);
            Rectangle TMRect = new Rectangle(width * 1, height * 0, width, height);
            Rectangle TRRect = new Rectangle(width * 2, height * 0, width, height);
            Rectangle MLRect = new Rectangle(width * 0, height * 1, width, height);
            Rectangle MMRect = new Rectangle(width * 1, height * 1, width, height);
            Rectangle MRRect = new Rectangle(width * 2, height * 1, width, height);
            Rectangle BLRect = new Rectangle(width * 0, height * 2, width, height);
            Rectangle BMRect = new Rectangle(width * 1, height * 2, width, height);
            Rectangle BRRect = new Rectangle(width * 2, height * 2, width, height);

            spriteBatch.Begin();

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    Rectangle sourceRectangle = new Rectangle();

                    Rectangle destinationRectangle = new Rectangle((int)location.X + (width * column), (int)location.Y + (height * row), width, height);

                    if (row == 0)
                    {
                        if (column != 0 && column != columns - 1)
                        {
                            if (TopEdgeMode)
                            {
                                sourceRectangle = TMRect;
                            } else
                            {
                                sourceRectangle = MMRect;
                            }
                        }
                        else if (column == 0)
                        {
                            if (TopEdgeMode && LeftEdgeMode)
                            {
                                sourceRectangle = TLRect;
                            }
                            else if (TopEdgeMode)
                            {
                                sourceRectangle = TMRect;
                            }
                            else if (LeftEdgeMode)
                            {
                                sourceRectangle = MLRect;
                            }
                            else
                            {
                                sourceRectangle = MMRect;
                            }
                        }
                        else
                        {
                            if (TopEdgeMode && RightEdgeMode)
                            {
                                sourceRectangle = TRRect;
                            }
                            else if (TopEdgeMode)
                            {
                                sourceRectangle = TMRect;
                            }
                            else if (RightEdgeMode)
                            {
                                sourceRectangle = MRRect;
                            }
                            else
                            {
                                sourceRectangle = MMRect;
                            }
                        }
                    }
                    else if (row == rows - 1)
                    {
                        if (column != 0 && column != columns - 1)
                        {
                            if (BottomEdgeMode)
                            {
                                sourceRectangle = BMRect;
                            }
                            else
                            {
                                sourceRectangle = MMRect;
                            }
                        }
                        else if (column == 0)
                        {
                            if (BottomEdgeMode && LeftEdgeMode)
                            {
                                sourceRectangle = BLRect;
                            }
                            else if (BottomEdgeMode)
                            {
                                sourceRectangle = BMRect;
                            }
                            else if (LeftEdgeMode)
                            {
                                sourceRectangle = MLRect;
                            }
                            else
                            {
                                sourceRectangle = MMRect;
                            }
                        }
                        else
                        {
                            if (BottomEdgeMode && RightEdgeMode)
                            {
                                sourceRectangle = BRRect;
                            }
                            else if (BottomEdgeMode)
                            {
                                sourceRectangle = BMRect;
                            }
                            else if (RightEdgeMode)
                            {
                                sourceRectangle = MRRect;
                            }
                            else
                            {
                                sourceRectangle = MMRect;
                            }
                        }
                    } else
                    {
                        if (column != 0 && column != columns - 1)
                        {
                            sourceRectangle = MMRect;
                        }
                        else if (column == 0)
                        {
                            if (LeftEdgeMode)
                            {
                                sourceRectangle = MLRect;
                            }
                            else
                            {
                                sourceRectangle = MMRect;
                            }
                        }
                        else
                        {
                            if (RightEdgeMode)
                            {
                                sourceRectangle = MRRect;
                            }
                            else
                            {
                                sourceRectangle = MMRect;
                            }
                        }
                    }

                    spriteBatch.Draw(Texture, destinationRectangle, sourceRectangle, color);
                }
            }

            spriteBatch.End();
        }
    }
}