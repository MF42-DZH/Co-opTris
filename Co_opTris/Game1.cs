using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Co_opTris
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //
        // Summary:
        //     Removes the first item of an array passed by reference
        //     and adds in a new value at the end of the array after
        //     shifting the rest of the data back.
        //
        // Parameters:
        //   arr:
        //     The int[] to be modified.
        //
        //   newValue:
        //     The value to be appended to the array.
        public void RemoveFirstAddNew(ref int[] arr, int newValue)
        {
            int LastIndex = arr.Length - 1;

            for (int i = LastIndex; i > 0; i--)
            {
                arr[i - 1] = arr[i];
            }

            arr[LastIndex] = newValue;
        }

        readonly int[] Resolution = new int[2] { 640, 480 };
        Texture2D TetrionTexture;
        Texture2D GenericBoxTexture;
        Texture2D TEXTure;
        BoxDrawing TetrionBox;
        BoxDrawing GenericBox;
        SpriteSheetText.TextObject TextDraw;

        readonly int[][][,] Polyminoes = new int[7][][,];  // Tetrominoes.

        int[,] P1CurrentPolymino = new int[3, 3];
        int[,] P2CurrentPolymino = new int[3, 3];
        int P1PolyminoIndex = 0;
        int P2PolyminoIndex = 0;
        int P1RotState = 0;
        int P2RotState = 0;
        int P1CurLD = 0;
        int P2CurLD = 0;
        int P1CurARE = 0;
        int P2CurARE = 0;
        int[] P1Nexts = { 0, 0, 0 };
        int[] P2Nexts = { 0, 0, 0 };
        int[] P1History = { 6, 4, 6, 4 };
        int[] P2History = { 6, 4, 6, 4 };
        Vector2 MatrixLocation = new Vector2(208, 64);
        Vector2[] P1MinoLocations = new Vector2[4];
        Vector2[] P2MinoLocations = new Vector2[4];
        Vector2 P1CurrentPieceLocation;
        Vector2 P2CurrentPieceLocation;

        int GlobalLD = 30;
        int P1LD = 30;
        int P2LD = 30;

        int GlobalARE = 25;
        int P1ARE = 25;
        int P2ARE = 25;

        int GlobalDAS = 15;
        int P1DAS = 15;
        int P1LDC = 0;
        int P1RDC = 0;
        int P2DAS = 15;
        int P2LDC = 0;
        int P2RDC = 0;

        readonly int[] MarathonGravityTable = { 1, 1, 2, 2, 4, 4, 8, 16, 32, 64,
                                                128, 256, 320, 512, 640, 768, 832, 960, 1152, 1280 };
        readonly int[] DeathGravityTable = { 1 };

        const int GlobalGMAX = 64;
        int GlobalGravity = 1;
        int P1Gravity = 1;
        int P1Elapsed = 0;
        int P2Gravity = 1;
        int P2Elapsed = 0;

        const int MonominoSize = 16;

        /*
         * Matrix (the playfield):
         * Defined as being 14 wide, 21 high in doubles mode.
         */
        const int MatrixWidth = 14;
        const int MatrixHeight = 21;
        public int[,] matrix = new int[21, 14];

        Random PieceRandomiser = new Random();
        Random GeneralRandomiser = new Random();

        // History Randomiser: 4-History 8-Roll
        int StrictHistoryRandomiser(ref int[] history)
        {
            int ReturnValue = -1;

            for (int i = 0; i < 8; i++)
            {
                ReturnValue = PieceRandomiser.Next(0, 7);
                if (!history.Contains(ReturnValue))
                {
                    RemoveFirstAddNew(ref history, ReturnValue);
                    return ReturnValue;
                }
            }

            RemoveFirstAddNew(ref history, ReturnValue);
            return ReturnValue;
        }
        
        // Texture Assets
        Texture2D Monomino;

        // Colours
        readonly Color[] MinoColours = new Color[9] {
            Color.Transparent,
            Color.FromNonPremultiplied(255, 0, 0, 255),  // Red I
            Color.FromNonPremultiplied(0, 0, 255, 255),  // Blue J
            Color.FromNonPremultiplied(255, 127, 0, 255),  // Orange L
            Color.FromNonPremultiplied(255, 255, 0, 255),  // Yellow O
            Color.FromNonPremultiplied(255, 0, 255, 255),  // Magenta S
            Color.FromNonPremultiplied(0, 255, 255, 255),  // Cyan T
            Color.FromNonPremultiplied(0, 255, 0, 255),  // Green Z
            Color.FromNonPremultiplied(240, 240, 240, 255)  // COLOURLESS Bone
        };

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public enum MoveState
        {
            FREE,
            OCCUPIED,
            COLLIDED,
            OUT_OF_BOUNDS
        }

        // Modified this to work with both falling pieces.
        public MoveState SpaceTest(int[,] field, int[,] polymino, int x, int y, int player)
        {
            int len = polymino.GetLength(0);

            for (int py = 0; py < len; py++)
            {
                for (int px = 0; px < len; px++)
                {
                    int coordx = x + px;
                    int coordy = y + py;

                    if (polymino[py, px] != 0)
                    {
                        try
                        {
                            if (coordx < 0 || coordx >= MatrixWidth)
                            {
                                return MoveState.OUT_OF_BOUNDS;
                            }
                            if (coordy >= MatrixHeight || field[coordy, coordx] != 0)
                            {
                                return MoveState.OCCUPIED;
                            }
                            Vector2[] ARRCheck = player == 1 ? P2MinoLocations : P1MinoLocations;
                            for (int k = 0; k < ARRCheck.Length; k++)
                            {
                                if (new Vector2(coordy, coordx) == ARRCheck[k])
                                {
                                    return MoveState.COLLIDED;
                                }
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            return MoveState.OUT_OF_BOUNDS;
                        }
                    }
                }
            }

            return MoveState.FREE;
        }

        private void InitialisePolyminoes()
        {
            // I Rotation States
            Polyminoes[0] = new int[4][,] {
                new int[4, 4]  // State 0
                {
                    { 0, 0, 0, 0 },
                    { 1, 1, 1, 1 },
                    { 0, 0, 0, 0 },
                    { 0, 0, 0, 0 }
                },
                new int[4, 4]  // State 1
                {
                    { 0, 0, 1, 0 },
                    { 0, 0, 1, 0 },
                    { 0, 0, 1, 0 },
                    { 0, 0, 1, 0 }
                },
                new int[4, 4]  // State 2
                {
                    { 0, 0, 0, 0 },
                    { 1, 1, 1, 1 },
                    { 0, 0, 0, 0 },
                    { 0, 0, 0, 0 }
                },
                new int[4, 4]  // State 3
                {
                    { 0, 0, 1, 0 },
                    { 0, 0, 1, 0 },
                    { 0, 0, 1, 0 },
                    { 0, 0, 1, 0 }
                }
            };

            // J Rotation States
            Polyminoes[1] = new int[4][,] {
                new int[3, 3]  // State 0
                {
                    { 0, 0, 0 },
                    { 1, 1, 1 },
                    { 0, 0, 1 }
                },
                new int[3, 3]  // State 1
                {
                    { 0, 1, 0 },
                    { 0, 1, 0 },
                    { 1, 1, 0 }
                },
                new int[3, 3]  // State 2
                {
                    { 0, 0, 0 },
                    { 1, 0, 0 },
                    { 1, 1, 1 }
                },
                new int[3, 3]  // State 3
                {
                    { 0, 1, 1 },
                    { 0, 1, 0 },
                    { 0, 1, 0 }
                }
            };

            // L Rotation States
            Polyminoes[2] = new int[4][,] {
                new int[3, 3]  // State 0
                {
                    { 0, 0, 0 },
                    { 1, 1, 1 },
                    { 1, 0, 0 }
                },
                new int[3, 3]  // State 1
                {
                    { 1, 1, 0 },
                    { 0, 1, 0 },
                    { 0, 1, 0 }
                },
                new int[3, 3]  // State 2
                {
                    { 0, 0, 0 },
                    { 0, 0, 1 },
                    { 1, 1, 1 }
                },
                new int[3, 3]  // State 3
                {
                    { 0, 1, 0 },
                    { 0, 1, 0 },
                    { 0, 1, 1 }
                }
            };

            // O Rotation States
            Polyminoes[3] = new int[4][,] {
                new int[4, 4]  // State 0
                {
                    { 0, 0, 0, 0 },
                    { 0, 1, 1, 0 },
                    { 0, 1, 1, 0 },
                    { 0, 0, 0, 0 }
                },
                new int[4, 4]  // State 1
                {
                    { 0, 0, 0, 0 },
                    { 0, 1, 1, 0 },
                    { 0, 1, 1, 0 },
                    { 0, 0, 0, 0 }
                },
                new int[4, 4]  // State 2
                {
                    { 0, 0, 0, 0 },
                    { 0, 1, 1, 0 },
                    { 0, 1, 1, 0 },
                    { 0, 0, 0, 0 }
                },
                new int[4, 4]  // State 3
                {
                    { 0, 0, 0, 0 },
                    { 0, 1, 1, 0 },
                    { 0, 1, 1, 0 },
                    { 0, 0, 0, 0 }
                }
            };

            // S Rotation States
            Polyminoes[4] = new int[4][,] {
                new int[3, 3]  // State 0
                {
                    { 0, 0, 0 },
                    { 0, 1, 1 },
                    { 1, 1, 0 }
                },
                new int[3, 3]  // State 1
                {
                    { 1, 0, 0 },
                    { 1, 1, 0 },
                    { 0, 1, 0 }
                },
                new int[3, 3]  // State 2
                {
                    { 0, 0, 0 },
                    { 0, 1, 1 },
                    { 1, 1, 0 }
                },
                new int[3, 3]  // State 3
                {
                    { 1, 0, 0 },
                    { 1, 1, 0 },
                    { 0, 1, 0 }
                }
            };

            // T Rotation States
            Polyminoes[5] = new int[4][,] {
                new int[3, 3]  // State 0
                {
                    { 0, 0, 0 },
                    { 1, 1, 1 },
                    { 0, 1, 0 }
                },
                new int[3, 3]  // State 1
                {
                    { 0, 1, 0 },
                    { 1, 1, 0 },
                    { 0, 1, 0 }
                },
                new int[3, 3]  // State 2
                {
                    { 0, 0, 0 },
                    { 0, 1, 0 },
                    { 1, 1, 1 }
                },
                new int[3, 3]  // State 3
                {
                    { 0, 1, 0 },
                    { 0, 1, 1 },
                    { 0, 1, 0 }
                }
            };

            // Z Rotation States
            Polyminoes[6] = new int[4][,] {
                new int[3, 3]  // State 0
                {
                    { 0, 0, 0 },
                    { 1, 1, 0 },
                    { 0, 1, 1 }
                },
                new int[3, 3]  // State 1
                {
                    { 0, 0, 1 },
                    { 0, 1, 1 },
                    { 0, 1, 0 }
                },
                new int[3, 3]  // State 2
                {
                    { 0, 0, 0 },
                    { 1, 1, 0 },
                    { 0, 1, 1 }
                },
                new int[3, 3]  // State 3
                {
                    { 0, 0, 1 },
                    { 0, 1, 1 },
                    { 0, 1, 0 }
                }
            };
        }

        int[,] RotatePlayerPolymino(int player, bool cw)
        {
            if (player == 1)
            {
                int Temp = P1RotState;

                if (cw)
                {
                    if (Temp == 3)
                    {
                        Temp = 0;
                    }
                    else
                    {
                        Temp++;
                    }
                } else
                {
                    if (Temp == 0)
                    {
                        Temp = 3;
                    }
                    else
                    {
                        Temp--;
                    }
                    
                }
                int[,] NewPiece = Polyminoes[P1PolyminoIndex][Temp];
                for (int y = 0; y < NewPiece.GetLength(0); y++)
                {
                    for (int x = 0; x < NewPiece.GetLength(0); x++)
                    {
                        NewPiece[y, x] = NewPiece[y, x] != 0 ? (P1PolyminoIndex + 1) : 0;
                    }
                }
                return NewPiece;
            }
            else
            {
                int Temp2 = P2RotState;
                if (cw)
                {
                    if (Temp2 == 3)
                    {
                        Temp2 = 0;
                    }
                    else
                    {
                        Temp2++;
                    }
                }
                else
                {
                    if (Temp2 == 0)
                    {
                        Temp2 = 3;
                    }
                    else
                    {
                        Temp2--;
                    }

                }
                int[,] NewPiece2 = Polyminoes[P2PolyminoIndex][Temp2];
                for (int y = 0; y < NewPiece2.GetLength(0); y++)
                {
                    for (int x = 0; x < NewPiece2.GetLength(0); x++)
                    {
                        NewPiece2[y, x] = NewPiece2[y, x] != 0 ? (P2PolyminoIndex + 1) : 0;
                    }
                }
                return NewPiece2;
            }
        }

        void SpawnPolymino(int player)
        {
            if (player == 1)
            {
                P1RotState = 0;
                P1IKicksLeft = 1;
                P1TKicksLeft = 1;
                P1PolyminoIndex = P1Nexts[0];
                RemoveFirstAddNew(ref P1Nexts, StrictHistoryRandomiser(ref P1History));

                P1CurLD = P1LD;

                P1CurrentPolymino = (int[,])Polyminoes[P1PolyminoIndex][0].Clone();

                int offset = ((MatrixWidth / 2) - P1CurrentPolymino.GetLength(0)) / 2;
                P1CurrentPieceLocation = new Vector2(offset, 0);
            }
            else
            {
                P2RotState = 0;
                P2IKicksLeft = 1;
                P2TKicksLeft = 1;
                P2PolyminoIndex = P2Nexts[0];
                RemoveFirstAddNew(ref P2Nexts, StrictHistoryRandomiser(ref P2History));

                P2CurLD = P2LD;

                P2CurrentPolymino = (int[,])Polyminoes[P2PolyminoIndex][0].Clone();

                int offset = (MatrixWidth / 2) + ((MatrixWidth / 2) - P2CurrentPolymino.GetLength(0)) / 2;
                offset += (P2PolyminoIndex == 0) || (P2PolyminoIndex == 3) ? 1 : 0;
                P2CurrentPieceLocation = new Vector2(offset, 0);
            }
        }

        public int[,] RemoveLines(int[,] field)  // Clear complete lines in Matrix
        {
            int CompletedLines = 0;

            for (int y = MatrixHeight - 1; y >= 0; y--)
            {
                bool complete = true;


                for (int x = 0; x < MatrixWidth; x++)
                {
                    if (field[y, x] == 0)
                    {
                        complete = false;
                    }
                }

                if (complete)
                {
                    CompletedLines += 1;

                    for (int yc = y; yc > 0; yc--)
                    {
                        for (int x = 0; x < MatrixWidth; x++)
                        {
                            field[yc, x] = field[yc - 1, x];
                        }
                    }

                    for (int i = 0; i < 10; i++)
                    {
                        matrix[0, i] = 0;
                    }

                    y++;
                }
            }

            lines += CompletedLines;

            return field;
        }

        public int[,] Lock(int[,] field, int[,] polymino, int x, int y, int player)  // Lock polymino into matrix.
        {
            int len = polymino.GetLength(0);

            for (int py = 0; py < len; py++)
            {
                for (int px = 0; px < len; px++)
                {
                    int coordx = x + px;
                    int coordy = y + py;

                    if (polymino[py, px] != 0)
                    {
                        field[coordy, coordx] = player == 1 ? (P1PolyminoIndex + 1) : (P2PolyminoIndex + 1);
                    }
                }
            }

            if (player == 1)
            {
                P1CurARE = 0;
                P1Spawn = false;
            }
            else
            {
                P2CurARE = 0;
                P2Spawn = false;
            }

            field = RemoveLines(field);
            return field;
        }

        public void GrabMinoLocations(int[,] polymino, int x, int y, int player)
        {
            int len = polymino.GetLength(0);
            int CurIndex = 0;

            for (int py = 0; py < len; py++)
            {
                for (int px = 0; px < len; px++)
                {
                    int coordx = x + px;
                    int coordy = y + py;

                    if (polymino[py, px] != 0)
                    {
                        if (player == 1)
                        {
                            P1MinoLocations[CurIndex] = new Vector2(coordy, coordx);
                            CurIndex++;
                        }
                        else
                        {
                            P2MinoLocations[CurIndex] = new Vector2(coordy, coordx);
                            CurIndex++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            InitialisePolyminoes();

            for (int i = 0; i < 3; i++)
            {
                P1Nexts[i] = StrictHistoryRandomiser(ref P1History);
                P2Nexts[i] = StrictHistoryRandomiser(ref P2History);
            }

            SpawnPolymino(1);
            SpawnPolymino(2);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            graphics.PreferredBackBufferWidth = Resolution[0];
            graphics.PreferredBackBufferHeight = Resolution[1];
            graphics.ApplyChanges();

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            Monomino = Content.Load<Texture2D>("Monomino");
            TetrionTexture = Content.Load<Texture2D>("TetrionGraphic");
            GenericBoxTexture = Content.Load<Texture2D>("BoxGraphic");
            TEXTure = Content.Load<Texture2D>("FontCoopTris");

            TetrionBox = new BoxDrawing(TetrionTexture, true, true, true, true, Color.LightGray);
            GenericBox = new BoxDrawing(GenericBoxTexture, true, true, true, true, Color.LightGray);

            TextDraw = new SpriteSheetText.TextObject(TEXTure);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        // Keybinds - TODO: Make a thing for these later.
        Keys P1Left = Keys.Left;
        Keys P1Right = Keys.Right;
        Keys P1SonicDrop = Keys.Up;
        Keys P1SoftDrop = Keys.Down;
        Keys P1CCW1 = Keys.Z;
        Keys P1CW = Keys.X;
        Keys P1CCW2 = Keys.C;

        Keys P2Left = Keys.A;
        Keys P2Right = Keys.D;
        Keys P2SonicDrop = Keys.W;
        Keys P2SoftDrop = Keys.S;
        Keys P2CCW1 = Keys.J;
        Keys P2CW = Keys.K;
        Keys P2CCW2 = Keys.L;

        KeyboardState NewKS = new KeyboardState();
        KeyboardState OldKS = new KeyboardState();
        Vector2 OldLTP1 = new Vector2();
        Vector2 OldLTP2 = new Vector2();

        bool P1SD = false;
        bool P2SD = false;

        bool P1Spawn = false;
        bool P2Spawn = false;

        Vector2 UniversalWallkick(int[,] piece, int x, int y, int player, bool IPiece = false)
        {
            Vector2 SpaceTestWK = new Vector2(x + 1, y);
            MoveState RotateCheckW = SpaceTest(matrix, piece, (int)SpaceTestWK.X, (int)SpaceTestWK.Y, player);
            if (RotateCheckW == MoveState.FREE)
            {
                return new Vector2(1, 0);
            } else
            {
                SpaceTestWK = new Vector2(x - 1, y);
                RotateCheckW = SpaceTest(matrix, piece, (int)SpaceTestWK.X, (int)SpaceTestWK.Y, player);

                if (RotateCheckW == MoveState.FREE)
                {
                    return new Vector2(-1, 0);
                }
                else if (IPiece)
                {
                    SpaceTestWK = new Vector2(x + 2, y);
                    RotateCheckW = SpaceTest(matrix, piece, (int)SpaceTestWK.X, (int)SpaceTestWK.Y, player);

                    if (RotateCheckW == MoveState.FREE)
                    {
                        return new Vector2(2, 0);
                    }
                }
            }

            return new Vector2(0, 0);
        }

        Vector2 LJKick (int[,] piece, int x, int y, int player)
        {
            Vector2 FinalResult = new Vector2(0, 0);
            if (player == 1)
            {
                if (P1PolyminoIndex == 1)
                {
                    if (P1RotState == 0)
                    {
                        if (matrix[y, x + 1] == 0 && matrix[y + 2, x + 1] == 0)
                        {
                            FinalResult = UniversalWallkick(piece, x, y, 1);
                        }
                        else if (matrix[y, x + 2] != 0 && matrix[y + 2, x + 1] != 0)
                        {
                            FinalResult = UniversalWallkick(piece, x, y, 1);
                        }
                    }
                    else if (P1RotState == 2)
                    {
                        if (matrix[y, x + 1] == 0 && matrix[y + 1, x + 1] == 0)
                        {
                            FinalResult = UniversalWallkick(piece, x, y, 1);
                        }
                    }
                    else
                    {
                        FinalResult = UniversalWallkick(piece, x, y, 1);
                    }
                }
                else
                {
                    if (P1RotState == 0)
                    {
                        if (matrix[y, x + 1] == 0 && matrix[y + 2, x + 1] == 0)
                        {
                            FinalResult = UniversalWallkick(piece, x, y, 1);
                        }
                        else if (matrix[y, x] != 0 && matrix[y + 2, x + 1] != 0)
                        {
                            FinalResult = UniversalWallkick(piece, x, y, 1);
                        }
                    }
                    else if (P1RotState == 2)
                    {
                        if (matrix[y, x + 1] == 0 && matrix[y + 1, x + 1] == 0)
                        {
                            FinalResult = UniversalWallkick(piece, x, y, 1);
                        }
                    }
                    else
                    {
                        FinalResult = UniversalWallkick(piece, x, y, 1);
                    }
                }
            }
            else
            {
                if (P2PolyminoIndex == 1)
                {
                    if (P2RotState == 0)
                    {
                        if (matrix[y, x + 1] == 0 && matrix[y + 2, x + 1] == 0)
                        {
                            FinalResult = UniversalWallkick(piece, x, y, 2);
                        }
                        else if (matrix[y, x + 2] != 0 && matrix[y + 2, x + 1] != 0)
                        {
                            FinalResult = UniversalWallkick(piece, x, y, 2);
                        }
                    }
                    else if (P2RotState == 2)
                    {
                        if (matrix[y, x + 1] == 0 && matrix[y + 1, x + 1] == 0)
                        {
                            FinalResult = UniversalWallkick(piece, x, y, 2);
                        }
                    }
                    else
                    {
                        FinalResult = UniversalWallkick(piece, x, y, 2);
                    }
                }
                else
                {
                    if (P2RotState == 0)
                    {
                        if (matrix[y, x + 1] == 0 && matrix[y + 2, x + 1] == 0)
                        {
                            FinalResult = UniversalWallkick(piece, x, y, 2);
                        }
                        else if (matrix[y, x] != 0 && matrix[y + 2, x + 1] != 0)
                        {
                            FinalResult = UniversalWallkick(piece, x, y, 2);
                        }
                    }
                    else if (P2RotState == 2)
                    {
                        if (matrix[y, x + 1] == 0 && matrix[y + 1, x + 1] == 0)
                        {
                            FinalResult = UniversalWallkick(piece, x, y, 2);
                        }
                    }
                    else
                    {
                        FinalResult = UniversalWallkick(piece, x, y, 2);
                    }
                }
            }

            return FinalResult;
        }

        int P1IKicksLeft = 1;
        int P2IKicksLeft = 1;
        int P1TKicksLeft = 1;
        int P2TKicksLeft = 1;

        bool StartScreenPassed = false;
        bool ModeScreenPassed = false;
        bool CharScreenPassed = false;
        bool ModsScreenPassed = false;

        Vector2 IKick (int[,] piece, int x, int y, int player)
        {
            Vector2 FinalResult = new Vector2(0, 0);
            if (player == 1)
            {
                if (P1RotState == 1 || P1RotState == 3)
                {
                    FinalResult = UniversalWallkick(piece, x, y, 1, true);
                }
                else
                {
                    try
                    {
                        if ((matrix[y + 2, x + 2] != 0 && matrix[y + 3, x + 2] != 0) || (matrix[y + 2, x + 2] != 0))
                    {
                        if (P1IKicksLeft > 0)
                        {
                            Vector2 SpaceTestWK = new Vector2(x, y - 2);
                            MoveState RotateCheckW = SpaceTest(matrix, piece, (int)SpaceTestWK.X, (int)SpaceTestWK.Y, player);
                            if (RotateCheckW == MoveState.FREE)
                            {
                                FinalResult = new Vector2(0, -2);
                                P1IKicksLeft = 0;
                            }
                        }
                        else
                        {
                            P1CurLD = 0;
                        }
                    }
                        else if (matrix[y + 3, x + 2] != 0 && (matrix[y + 2, x] != 0 || matrix[y + 2, x + 1] != 0 || matrix[y + 3, x] != 0))
                    {
                        if (P1IKicksLeft > 0)
                        {
                            Vector2 SpaceTestWK = new Vector2(x, y - 1);
                            MoveState RotateCheckW = SpaceTest(matrix, piece, (int)SpaceTestWK.X, (int)SpaceTestWK.Y, player);
                            if (RotateCheckW == MoveState.FREE)
                            {
                                FinalResult = new Vector2(0, -1);
                                P1IKicksLeft = 0;
                            }
                        }
                        else
                        {
                            P1CurLD = 0;
                        }
                    }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        if (P1IKicksLeft > 0)
                        {
                            Vector2 SpaceTestWK = new Vector2(x, y - 2);
                            MoveState RotateCheckW = SpaceTest(matrix, piece, (int)SpaceTestWK.X, (int)SpaceTestWK.Y, player);
                            if (RotateCheckW == MoveState.FREE)
                            {
                                FinalResult = new Vector2(0, -2);
                                P1IKicksLeft = 0;
                            }
                        }
                        else
                        {
                            P1CurLD = 0;
                        }
                    }
                }
            }
            else
            {
                if (P2RotState == 1 || P2RotState == 3)
                {
                    FinalResult = UniversalWallkick(piece, x, y, 2, true);
                }
                else
                {
                    try
                    {
                        if ((matrix[y + 2, x + 2] != 0 && matrix[y + 3, x + 2] != 0) || (matrix[y + 2, x + 2] != 0))
                        {
                            if (P2IKicksLeft > 0)
                            {
                                Vector2 SpaceTestWK = new Vector2(x, y - 2);
                                MoveState RotateCheckW = SpaceTest(matrix, piece, (int)SpaceTestWK.X, (int)SpaceTestWK.Y, player);
                                if (RotateCheckW == MoveState.FREE)
                                {
                                    FinalResult = new Vector2(0, -2);
                                    P2IKicksLeft = 0;
                                }
                            }
                            else
                            {
                                P2CurLD = 0;
                            }
                        }
                        else if (matrix[y + 3, x + 2] != 0 && (matrix[y + 2, x] != 0 || matrix[y + 2, x + 1] != 0 || matrix[y + 3, x] != 0))
                        {
                            if (P2IKicksLeft > 0)
                            {
                                Vector2 SpaceTestWK = new Vector2(x, y - 1);
                                MoveState RotateCheckW = SpaceTest(matrix, piece, (int)SpaceTestWK.X, (int)SpaceTestWK.Y, player);
                                if (RotateCheckW == MoveState.FREE)
                                {
                                    FinalResult = new Vector2(0, -1);
                                    P2IKicksLeft = 0;
                                }
                            }
                            else
                            {
                                P2CurLD = 0;
                            }
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        if (P2IKicksLeft > 0)
                        {
                            Vector2 SpaceTestWK = new Vector2(x, y - 2);
                            MoveState RotateCheckW = SpaceTest(matrix, piece, (int)SpaceTestWK.X, (int)SpaceTestWK.Y, player);
                            if (RotateCheckW == MoveState.FREE)
                            {
                                FinalResult = new Vector2(0, -2);
                                P2IKicksLeft = 0;
                            }
                        }
                        else
                        {
                            P2CurLD = 0;
                        }
                    }
                }
            }

            return FinalResult;
        }

        Vector2 TKick(int[,] piece, int x, int y, int player)
        {
            Vector2 FinalResult = new Vector2(0, 0);
            if (player == 1)
            {
                if (P1RotState == 1 || P1RotState == 3)
                {
                    if (matrix[y + 2, x] != 0 && matrix[y + 2, x + 2] != 0)
                    {
                        if (P1TKicksLeft > 0)
                        {
                            Vector2 SpaceTestWK = new Vector2(x, y - 1);
                            MoveState RotateCheckW = SpaceTest(matrix, piece, (int)SpaceTestWK.X, (int)SpaceTestWK.Y, player);
                            if (RotateCheckW == MoveState.FREE)
                            {
                                FinalResult = new Vector2(0, -1);
                                P1TKicksLeft = 0;
                            }
                        }
                        else
                        {
                            P1CurLD = 0;
                        }
                    }
                    else
                    {
                        FinalResult = UniversalWallkick(piece, x, y, 1);
                    }
                }
                else
                {
                    if (matrix[y, x + 1] == 0)
                    {
                        FinalResult = UniversalWallkick(piece, x, y, 1);
                    }
                }
            }
            else
            {
                if (P2RotState == 1 || P2RotState == 3)
                {
                    if (matrix[y + 2, x] != 0 && matrix[y + 2, x + 2] != 0)
                    {
                        if (P2TKicksLeft > 0)
                        {
                            Vector2 SpaceTestWK = new Vector2(x, y - 1);
                            MoveState RotateCheckW = SpaceTest(matrix, piece, (int)SpaceTestWK.X, (int)SpaceTestWK.Y, player);
                            if (RotateCheckW == MoveState.FREE)
                            {
                                FinalResult = new Vector2(0, -1);
                                P2TKicksLeft = 0;
                            }
                        }
                        else
                        {
                            P2CurLD = 0;
                        }
                    }
                    else
                    {
                        FinalResult = UniversalWallkick(piece, x, y, 2);
                    }
                }
                else
                {
                    if (matrix[y, x + 1] == 0)
                    {
                        FinalResult = UniversalWallkick(piece, x, y, 2);
                    }
                }
            }
            return FinalResult;
        }

        int lines = 0;

        void Reset()
        {
            StartScreenPassed = false;
            ModeScreenPassed = false;
            CharScreenPassed = false;
            ModsScreenPassed = false;

            P1IKicksLeft = 1;
            P2IKicksLeft = 1;
            P1TKicksLeft = 1;
            P2TKicksLeft = 1;

            P1SD = false;
            P2SD = false;

            P1Spawn = false;
            P2Spawn = false;

            NewKS = new KeyboardState();
            OldKS = new KeyboardState();

            OldLTP1 = new Vector2();
            OldLTP2 = new Vector2();

            PieceRandomiser = new Random();
            GeneralRandomiser = new Random();

            matrix = new int[21, 14];

            P1PolyminoIndex = 0;
            P2PolyminoIndex = 0;
            P1RotState = 0;
            P2RotState = 0;
            P1CurLD = 0;
            P2CurLD = 0;
            P1CurARE = 0;
            P2CurARE = 0;

            P1CurrentPolymino = new int[3, 3];
            int[,] P2CurrentPolymino = new int[3, 3];

            int[] P1Nexts = { 0, 0, 0 };
            int[] P2Nexts = { 0, 0, 0 };
            int[] P1History = { 6, 4, 6, 4 };
            int[] P2History = { 6, 4, 6, 4 };

            for (int i = 0; i < 3; i++)
            {
                P1Nexts[i] = StrictHistoryRandomiser(ref P1History);
                P2Nexts[i] = StrictHistoryRandomiser(ref P2History);
            }

            GlobalLD = 30;
            P1LD = 30;
            P2LD = 30;

            GlobalARE = 25;
            P1ARE = 25;
            P2ARE = 25;

            GlobalDAS = 15;
            P1DAS = 15;
            P1LDC = 0;
            P1RDC = 0;
            P2DAS = 15;
            P2LDC = 0;
            P2RDC = 0;

            GlobalGravity = 1;
            P1Gravity = 1;
            P1Elapsed = 0;
            P2Gravity = 1;
            P2Elapsed = 0;
            lines = 0;

            SpawnPolymino(1);
            SpawnPolymino(2);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            P1CurARE += 1;
            P2CurARE += 1;

            NewKS = Keyboard.GetState();
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (P1CurARE >= P1ARE && !P1Spawn)
            {
                SpawnPolymino(1);
                P1Spawn = true;
                if (SpaceTest(matrix, P1CurrentPolymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1) != MoveState.FREE)
                {
                    Reset();
                }
            }
            
            if (P2CurARE >= P2ARE && !P2Spawn)
            {
                SpawnPolymino(2);
                P2Spawn = true;
                if (SpaceTest(matrix, P2CurrentPolymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2) != MoveState.FREE)
                {
                    Reset();
                }
            }

            if (P1CurARE >= P1ARE)
            {
                P1Elapsed += P1Gravity;
            }

            if (P2CurARE >= P2ARE)
            {
                P2Elapsed += P2Gravity;
            }

            // Order 1: Rotate.
            if (P1CurARE >= P1ARE)
            {
                if (NewKS.IsKeyDown(P1CCW1) && OldKS.IsKeyUp(P1CCW1))  // P1
                {
                    int[,] NewP1Polymino = RotatePlayerPolymino(1, false);
                    MoveState RotateCheck1 = SpaceTest(matrix, NewP1Polymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
                    if (RotateCheck1 == MoveState.FREE)
                    {
                        P1CurrentPolymino = NewP1Polymino;
                        GrabMinoLocations(P1CurrentPolymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
                        if (P1RotState == 0)
                        {
                            P1RotState = 3;
                        }
                        else
                        {
                            P1RotState--;
                        }
                    }
                    else
                    {
                        if (P1PolyminoIndex != 0 && P1PolyminoIndex != 1 && P1PolyminoIndex != 2 && P1PolyminoIndex != 5)
                        {
                            Vector2 KickLocation = UniversalWallkick(NewP1Polymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P1CurrentPolymino = NewP1Polymino;
                                P1CurrentPieceLocation += KickLocation;

                                if (P1RotState == 0)
                                {
                                    P1RotState = 3;
                                }
                                else
                                {
                                    P1RotState--;
                                }
                            }
                        }
                        else if (P1PolyminoIndex == 1 || P1PolyminoIndex == 2)
                        {
                            Vector2 KickLocation = LJKick(NewP1Polymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P1CurrentPolymino = NewP1Polymino;
                                P1CurrentPieceLocation += KickLocation;

                                if (P1RotState == 0)
                                {
                                    P1RotState = 3;
                                }
                                else
                                {
                                    P1RotState--;
                                }
                            }
                        }
                        else if (P1PolyminoIndex == 5)
                        {
                            Vector2 KickLocation = TKick(NewP1Polymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P1CurrentPolymino = NewP1Polymino;
                                P1CurrentPieceLocation += KickLocation;

                                if (P1RotState == 0)
                                {
                                    P1RotState = 3;
                                }
                                else
                                {
                                    P1RotState--;
                                }
                            }
                        }
                        else if (P1PolyminoIndex == 0)
                        {
                            Vector2 KickLocation = IKick(NewP1Polymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P1CurrentPolymino = NewP1Polymino;
                                P1CurrentPieceLocation += KickLocation;

                                if (P1RotState == 0)
                                {
                                    P1RotState = 3;
                                }
                                else
                                {
                                    P1RotState--;
                                }
                            }
                        }
                    }
                }
                if (NewKS.IsKeyDown(P1CW) && OldKS.IsKeyUp(P1CW))
                {
                    int[,] NewP1Polymino = RotatePlayerPolymino(1, true);
                    MoveState RotateCheck1 = SpaceTest(matrix, NewP1Polymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
                    if (RotateCheck1 == MoveState.FREE)
                    {
                        P1CurrentPolymino = NewP1Polymino;
                        GrabMinoLocations(P1CurrentPolymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
                        if (P1RotState == 3)
                        {
                            P1RotState = 0;
                        }
                        else
                        {
                            P1RotState++;
                        }
                    }
                    else
                    {
                        if (P1PolyminoIndex != 0 && P1PolyminoIndex != 1 && P1PolyminoIndex != 2 && P1PolyminoIndex != 5)
                        {
                            Vector2 KickLocation = UniversalWallkick(NewP1Polymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P1CurrentPolymino = NewP1Polymino;
                                P1CurrentPieceLocation += KickLocation;

                                if (P1RotState == 3)
                                {
                                    P1RotState = 0;
                                }
                                else
                                {
                                    P1RotState++;
                                }
                            }

                        }
                        else if (P1PolyminoIndex == 1 || P1PolyminoIndex == 2)
                        {
                            Vector2 KickLocation = LJKick(NewP1Polymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P1CurrentPolymino = NewP1Polymino;
                                P1CurrentPieceLocation += KickLocation;

                                if (P1RotState == 3)
                                {
                                    P1RotState = 0;
                                }
                                else
                                {
                                    P1RotState++;
                                }
                            }
                        }
                        else if (P1PolyminoIndex == 5)
                        {
                            Vector2 KickLocation = TKick(NewP1Polymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P1CurrentPolymino = NewP1Polymino;
                                P1CurrentPieceLocation += KickLocation;

                                if (P1RotState == 3)
                                {
                                    P1RotState = 0;
                                }
                                else
                                {
                                    P1RotState++;
                                }
                            }
                        }
                        else if (P1PolyminoIndex == 0)
                        {
                            Vector2 KickLocation = IKick(NewP1Polymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P1CurrentPolymino = NewP1Polymino;
                                P1CurrentPieceLocation += KickLocation;

                                if (P1RotState == 3)
                                {
                                    P1RotState = 0;
                                }
                                else
                                {
                                    P1RotState++;
                                }
                            }
                        }
                    }
                }
                if (NewKS.IsKeyDown(P1CCW2) && OldKS.IsKeyUp(P1CCW2))
                {
                    int[,] NewP1Polymino = RotatePlayerPolymino(1, false);
                    MoveState RotateCheck1 = SpaceTest(matrix, NewP1Polymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
                    if (RotateCheck1 == MoveState.FREE)
                    {
                        P1CurrentPolymino = NewP1Polymino;
                        GrabMinoLocations(P1CurrentPolymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
                        if (P1RotState == 0)
                        {
                            P1RotState = 3;
                        }
                        else
                        {
                            P1RotState--;
                        }
                    }
                    else
                    {
                        if (P1PolyminoIndex != 0 && P1PolyminoIndex != 1 && P1PolyminoIndex != 2 && P1PolyminoIndex != 5)
                        {
                            Vector2 KickLocation = UniversalWallkick(NewP1Polymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P1CurrentPolymino = NewP1Polymino;
                                P1CurrentPieceLocation += KickLocation;

                                if (P1RotState == 0)
                                {
                                    P1RotState = 3;
                                }
                                else
                                {
                                    P1RotState--;
                                }
                            }

                        }
                        else if (P1PolyminoIndex == 1 || P1PolyminoIndex == 2)
                        {
                            Vector2 KickLocation = LJKick(NewP1Polymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P1CurrentPolymino = NewP1Polymino;
                                P1CurrentPieceLocation += KickLocation;

                                if (P1RotState == 0)
                                {
                                    P1RotState = 3;
                                }
                                else
                                {
                                    P1RotState--;
                                }
                            }
                        }
                        else if (P1PolyminoIndex == 5)
                        {
                            Vector2 KickLocation = TKick(NewP1Polymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P1CurrentPolymino = NewP1Polymino;
                                P1CurrentPieceLocation += KickLocation;

                                if (P1RotState == 0)
                                {
                                    P1RotState = 3;
                                }
                                else
                                {
                                    P1RotState--;
                                }
                            }
                        }
                        else if (P1PolyminoIndex == 0)
                        {
                            Vector2 KickLocation = IKick(NewP1Polymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P1CurrentPolymino = NewP1Polymino;
                                P1CurrentPieceLocation += KickLocation;

                                if (P1RotState == 0)
                                {
                                    P1RotState = 3;
                                }
                                else
                                {
                                    P1RotState--;
                                }
                            }
                        }
                    }
                }
            }
            if (P2CurARE >= P2ARE)
            {
                if (NewKS.IsKeyDown(P2CCW1) && OldKS.IsKeyUp(P2CCW1))  // P2
                {
                    int[,] NewP2Polymino = RotatePlayerPolymino(2, false);
                    MoveState RotateCheck1 = SpaceTest(matrix, NewP2Polymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);
                    if (RotateCheck1 == MoveState.FREE)
                    {
                        P2CurrentPolymino = NewP2Polymino;
                        GrabMinoLocations(P2CurrentPolymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);
                        if (P2RotState == 0)
                        {
                            P2RotState = 3;
                        }
                        else
                        {
                            P2RotState--;
                        }
                    }
                    else
                    {
                        if (P2PolyminoIndex != 0 && P2PolyminoIndex != 1 && P2PolyminoIndex != 2 && P2PolyminoIndex != 5)
                        {
                            Vector2 KickLocation = UniversalWallkick(NewP2Polymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P2CurrentPolymino = NewP2Polymino;
                                P2CurrentPieceLocation += KickLocation;

                                if (P2RotState == 0)
                                {
                                    P2RotState = 3;
                                }
                                else
                                {
                                    P2RotState--;
                                }
                            }
                        }
                        else if (P2PolyminoIndex == 1 || P2PolyminoIndex == 2)
                        {
                            Vector2 KickLocation = LJKick(NewP2Polymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P2CurrentPolymino = NewP2Polymino;
                                P2CurrentPieceLocation += KickLocation;

                                if (P2RotState == 0)
                                {
                                    P2RotState = 3;
                                }
                                else
                                {
                                    P2RotState--;
                                }
                            }
                        }
                        else if (P2PolyminoIndex == 5)
                        {
                            Vector2 KickLocation = TKick(NewP2Polymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P2CurrentPolymino = NewP2Polymino;
                                P2CurrentPieceLocation += KickLocation;

                                if (P2RotState == 0)
                                {
                                    P2RotState = 3;
                                }
                                else
                                {
                                    P2RotState--;
                                }
                            }
                        }
                        else if (P2PolyminoIndex == 0)
                        {
                            Vector2 KickLocation = IKick(NewP2Polymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P2CurrentPolymino = NewP2Polymino;
                                P2CurrentPieceLocation += KickLocation;

                                if (P2RotState == 0)
                                {
                                    P2RotState = 3;
                                }
                                else
                                {
                                    P2RotState--;
                                }
                            }
                        }
                    }
                }
                if (NewKS.IsKeyDown(P2CW) && OldKS.IsKeyUp(P2CW))
                {
                    int[,] NewP2Polymino = RotatePlayerPolymino(2, true);
                    MoveState RotateCheck1 = SpaceTest(matrix, NewP2Polymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);
                    if (RotateCheck1 == MoveState.FREE)
                    {
                        P2CurrentPolymino = NewP2Polymino;
                        GrabMinoLocations(P2CurrentPolymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);
                        if (P2RotState == 3)
                        {
                            P2RotState = 0;
                        }
                        else
                        {
                            P2RotState++;
                        }
                    }
                    else
                    {
                        if (P2PolyminoIndex != 0 && P2PolyminoIndex != 1 && P2PolyminoIndex != 2 && P2PolyminoIndex != 5)
                        {
                            Vector2 KickLocation = UniversalWallkick(NewP2Polymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P2CurrentPolymino = NewP2Polymino;
                                P2CurrentPieceLocation += KickLocation;

                                if (P2RotState == 3)
                                {
                                    P2RotState = 0;
                                }
                                else
                                {
                                    P2RotState++;
                                }
                            }

                        }
                        else if (P2PolyminoIndex == 1 || P2PolyminoIndex == 2)
                        {
                            Vector2 KickLocation = LJKick(NewP2Polymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P2CurrentPolymino = NewP2Polymino;
                                P2CurrentPieceLocation += KickLocation;

                                if (P2RotState == 3)
                                {
                                    P2RotState = 0;
                                }
                                else
                                {
                                    P2RotState++;
                                }
                            }
                        }
                        else if (P2PolyminoIndex == 5)
                        {
                            Vector2 KickLocation = TKick(NewP2Polymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P2CurrentPolymino = NewP2Polymino;
                                P2CurrentPieceLocation += KickLocation;

                                if (P2RotState == 3)
                                {
                                    P2RotState = 0;
                                }
                                else
                                {
                                    P2RotState++;
                                }
                            }
                        }
                        else if (P2PolyminoIndex == 0)
                        {
                            Vector2 KickLocation = IKick(NewP2Polymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P2CurrentPolymino = NewP2Polymino;
                                P2CurrentPieceLocation += KickLocation;

                                if (P2RotState == 3)
                                {
                                    P2RotState = 0;
                                }
                                else
                                {
                                    P2RotState++;
                                }
                            }
                        }
                    }
                }
                if (NewKS.IsKeyDown(P2CCW2) && OldKS.IsKeyUp(P2CCW2))
                {
                    int[,] NewP2Polymino = RotatePlayerPolymino(2, false);
                    MoveState RotateCheck1 = SpaceTest(matrix, NewP2Polymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);
                    if (RotateCheck1 == MoveState.FREE)
                    {
                        P2CurrentPolymino = NewP2Polymino;
                        GrabMinoLocations(P2CurrentPolymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);
                        if (P2RotState == 0)
                        {
                            P2RotState = 3;
                        }
                        else
                        {
                            P2RotState--;
                        }
                    }
                    else
                    {
                        if (P2PolyminoIndex != 0 && P2PolyminoIndex != 1 && P2PolyminoIndex != 2 && P2PolyminoIndex != 5)
                        {
                            Vector2 KickLocation = UniversalWallkick(NewP2Polymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P2CurrentPolymino = NewP2Polymino;
                                P2CurrentPieceLocation += KickLocation;

                                if (P2RotState == 0)
                                {
                                    P2RotState = 3;
                                }
                                else
                                {
                                    P2RotState--;
                                }
                            }

                        }
                        else if (P2PolyminoIndex == 1 || P2PolyminoIndex == 2)
                        {
                            Vector2 KickLocation = LJKick(NewP2Polymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P2CurrentPolymino = NewP2Polymino;
                                P2CurrentPieceLocation += KickLocation;

                                if (P2RotState == 0)
                                {
                                    P2RotState = 3;
                                }
                                else
                                {
                                    P2RotState--;
                                }
                            }
                        }
                        else if (P2PolyminoIndex == 5)
                        {
                            Vector2 KickLocation = TKick(NewP2Polymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P2CurrentPolymino = NewP2Polymino;
                                P2CurrentPieceLocation += KickLocation;

                                if (P2RotState == 0)
                                {
                                    P2RotState = 3;
                                }
                                else
                                {
                                    P2RotState--;
                                }
                            }
                        }
                        else if (P2PolyminoIndex == 0)
                        {
                            Vector2 KickLocation = IKick(NewP2Polymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);

                            if (KickLocation != new Vector2(0, 0))
                            {
                                P2CurrentPolymino = NewP2Polymino;
                                P2CurrentPieceLocation += KickLocation;

                                if (P2RotState == 0)
                                {
                                    P2RotState = 3;
                                }
                                else
                                {
                                    P2RotState--;
                                }
                            }
                        }
                    }
                }
            }
            // Order 2: Movement
            // P1
            if (P1CurARE >= P1ARE)
            {
                if (NewKS.IsKeyDown(P1SoftDrop) && !P1SD)
                {
                    P1Elapsed += GlobalGMAX;
                }
                if (NewKS.IsKeyUp(P1SoftDrop))
                {
                    P1SD = false;
                }

                if (NewKS.IsKeyDown(P1SonicDrop) && OldKS.IsKeyUp(P1SonicDrop))
                {
                    for (int i = 1; i <= MatrixHeight; i++)
                    {
                        Vector2 P1NewPieceLocation = P1CurrentPieceLocation + new Vector2(0, i);

                        MoveState LockCheck1 = SpaceTest(matrix, P1CurrentPolymino, (int)P1NewPieceLocation.X, (int)P1NewPieceLocation.Y, 1);
                        if (LockCheck1 == MoveState.FREE)
                        {
                            if (i != MatrixHeight)
                            {
                                continue;
                            }
                            else
                            {
                                P1NewPieceLocation = P1CurrentPieceLocation + new Vector2(0, i - 1);
                                P1CurrentPieceLocation = P1NewPieceLocation;
                                GrabMinoLocations(P1CurrentPolymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
                            }
                        }
                        else
                        {
                            P1NewPieceLocation = P1CurrentPieceLocation + new Vector2(0, i - 1);
                            P1CurrentPieceLocation = P1NewPieceLocation;
                            GrabMinoLocations(P1CurrentPolymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
                            break;
                        }
                    }
                }

                if (NewKS.IsKeyDown(P1Left) && NewKS.IsKeyDown(P1Right))
                {
                    P1LDC = 0;
                    P1RDC = 0;
                }
                else if ((NewKS.IsKeyDown(P1Left) && OldKS.IsKeyUp(P1Left)) || (NewKS.IsKeyDown(P1Right) && OldKS.IsKeyUp(P1Right)))
                {
                    Vector2 P1NewPieceLocation = P1CurrentPieceLocation + new Vector2(NewKS.IsKeyDown(P1Left) ? -1 : 1, 0);

                    MoveState MoveCheck1 = SpaceTest(matrix, P1CurrentPolymino, (int)P1NewPieceLocation.X, (int)P1NewPieceLocation.Y, 1);
                    if (MoveCheck1 == MoveState.FREE)
                    {
                        P1CurrentPieceLocation = P1NewPieceLocation;
                        GrabMinoLocations(P1CurrentPolymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
                    }
                }
                if (NewKS.IsKeyDown(P1Left) && P1LDC < P1DAS || NewKS.IsKeyDown(P1Right) && P1RDC < P1DAS)
                {
                    if (NewKS.IsKeyDown(P1Left))
                    {
                        P1LDC++;
                    }
                    else
                    {
                        P1RDC++;
                    }
                }
                else if ((P1LDC >= P1DAS && NewKS.IsKeyDown(P1Left)) || (P1RDC >= P1DAS && NewKS.IsKeyDown(P1Right)))
                {
                    Vector2 P1NewPieceLocation = P1CurrentPieceLocation + new Vector2(NewKS.IsKeyDown(P1Left) ? -1 : 1, 0);

                    MoveState MoveCheck1 = SpaceTest(matrix, P1CurrentPolymino, (int)P1NewPieceLocation.X, (int)P1NewPieceLocation.Y, 1);
                    if (MoveCheck1 == MoveState.FREE)
                    {
                        P1CurrentPieceLocation = P1NewPieceLocation;
                        GrabMinoLocations(P1CurrentPolymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
                    }
                }
                else
                {
                    P1LDC = 0;
                    P1RDC = 0;
                }
            }

            // P2
            if (P2CurARE >= P2ARE)
            {
                if (NewKS.IsKeyDown(P2SoftDrop) && !P2SD)
                {
                    P2Elapsed += GlobalGMAX;
                }
                if (NewKS.IsKeyUp(P2SoftDrop))
                {
                    P2SD = false;
                }

                if (NewKS.IsKeyDown(P2SonicDrop) && OldKS.IsKeyUp(P2SonicDrop))
                {
                    for (int i = 1; i <= MatrixHeight; i++)
                    {
                        Vector2 P2NewPieceLocation = P2CurrentPieceLocation + new Vector2(0, i);

                        MoveState LockCheck2 = SpaceTest(matrix, P2CurrentPolymino, (int)P2NewPieceLocation.X, (int)P2NewPieceLocation.Y, 2);
                        if (LockCheck2 == MoveState.FREE)
                        {
                            if (i != MatrixHeight)
                            {
                                continue;
                            }
                            else
                            {
                                P2NewPieceLocation = P2CurrentPieceLocation + new Vector2(0, i - 1);
                                P2CurrentPieceLocation = P2NewPieceLocation;
                                GrabMinoLocations(P2CurrentPolymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);
                            }
                        }
                        else
                        {
                            P2NewPieceLocation = P2CurrentPieceLocation + new Vector2(0, i - 1);
                            P2CurrentPieceLocation = P2NewPieceLocation;
                            GrabMinoLocations(P2CurrentPolymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);
                            break;
                        }
                    }
                }

                if (NewKS.IsKeyDown(P2Left) && NewKS.IsKeyDown(P2Right))
                {
                    P2LDC = 0;
                    P2RDC = 0;
                }
                else if ((NewKS.IsKeyDown(P2Left) && OldKS.IsKeyUp(P2Left)) || (NewKS.IsKeyDown(P2Right) && OldKS.IsKeyUp(P2Right)))
                {
                    Vector2 P2NewPieceLocation = P2CurrentPieceLocation + new Vector2(NewKS.IsKeyDown(P2Left) ? -1 : 1, 0);

                    MoveState MoveCheck1 = SpaceTest(matrix, P2CurrentPolymino, (int)P2NewPieceLocation.X, (int)P2NewPieceLocation.Y, 2);
                    if (MoveCheck1 == MoveState.FREE)
                    {
                        P2CurrentPieceLocation = P2NewPieceLocation;
                        GrabMinoLocations(P2CurrentPolymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);
                    }
                }
                if (NewKS.IsKeyDown(P2Left) && P2LDC < P2DAS || NewKS.IsKeyDown(P2Right) && P2RDC < P2DAS)
                {
                    if (NewKS.IsKeyDown(P2Left))
                    {
                        P2LDC++;
                    }
                    else
                    {
                        P2RDC++;
                    }
                }
                else if ((P2LDC >= P2DAS && NewKS.IsKeyDown(P2Left)) || (P2RDC >= P2DAS && NewKS.IsKeyDown(P2Right)))
                {
                    Vector2 P2NewPieceLocation = P2CurrentPieceLocation + new Vector2(NewKS.IsKeyDown(P2Left) ? -1 : 1, 0);

                    MoveState MoveCheck1 = SpaceTest(matrix, P2CurrentPolymino, (int)P2NewPieceLocation.X, (int)P2NewPieceLocation.Y, 2);
                    if (MoveCheck1 == MoveState.FREE)
                    {
                        P2CurrentPieceLocation = P2NewPieceLocation;
                        GrabMinoLocations(P2CurrentPolymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);
                    }
                }
                else
                {
                    P2LDC = 0;
                    P2RDC = 0;
                }
            }
            // Order 3: Gravity
            if (P1Elapsed >= GlobalGMAX)
            {
                if (P1Gravity / GlobalGMAX > 1)
                {
                    for (int i = 1; i <= P1Gravity / GlobalGMAX; i++)
                    {
                        Vector2 P1NewPieceLocation = P1CurrentPieceLocation + new Vector2(0, i);

                        MoveState LockCheck1 = SpaceTest(matrix, P1CurrentPolymino, (int)P1NewPieceLocation.X, (int)P1NewPieceLocation.Y, 1);
                        if (LockCheck1 == MoveState.FREE)
                        {
                            if (i != (P1Gravity / GlobalGMAX))
                            {
                                continue;
                            }
                            else
                            {
                                P1NewPieceLocation = P1CurrentPieceLocation + new Vector2(0, i - 1);
                                P1CurrentPieceLocation = P1NewPieceLocation;
                                GrabMinoLocations(P1CurrentPolymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
                            }
                        }
                        else
                        {
                            P1NewPieceLocation = P1CurrentPieceLocation + new Vector2(0, i - 1);
                            P1CurrentPieceLocation = P1NewPieceLocation;
                            GrabMinoLocations(P1CurrentPolymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
                            break;
                        }
                    }
                }
                else
                {
                    Vector2 P1NewPieceLocation = P1CurrentPieceLocation + new Vector2(0, 1);

                    MoveState LockCheck1 = SpaceTest(matrix, P1CurrentPolymino, (int)P1NewPieceLocation.X, (int)P1NewPieceLocation.Y, 1);
                    if (LockCheck1 == MoveState.FREE)
                    {
                        P1CurrentPieceLocation = P1NewPieceLocation;
                        GrabMinoLocations(P1CurrentPolymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
                    }
                }

                P1Elapsed = 0;
            }
            if (P2Elapsed >= GlobalGMAX)
            {
                if (P2Gravity / GlobalGMAX > 1)
                {
                    for (int i = 1; i <= P2Gravity / GlobalGMAX; i++)
                    {
                        Vector2 P2NewPieceLocation = P2CurrentPieceLocation + new Vector2(0, i);

                        MoveState LockCheck2 = SpaceTest(matrix, P2CurrentPolymino, (int)P2NewPieceLocation.X, (int)P2NewPieceLocation.Y, 2);
                        if (LockCheck2 == MoveState.FREE)
                        {
                            if (i != (P2Gravity / GlobalGMAX))
                            {
                                continue;
                            }
                            else
                            {
                                P2NewPieceLocation = P2CurrentPieceLocation + new Vector2(0, i - 1);
                                P2CurrentPieceLocation = P2NewPieceLocation;
                                GrabMinoLocations(P2CurrentPolymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);
                            }
                        }
                        else
                        {
                            P2NewPieceLocation = P2CurrentPieceLocation + new Vector2(0, i - 1);
                            P2CurrentPieceLocation = P2NewPieceLocation;
                            GrabMinoLocations(P2CurrentPolymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);
                            break;
                        }
                    }
                }
                else
                {
                    Vector2 P2NewPieceLocation = P2CurrentPieceLocation + new Vector2(0, 1);

                    MoveState LockCheck1 = SpaceTest(matrix, P2CurrentPolymino, (int)P2NewPieceLocation.X, (int)P2NewPieceLocation.Y, 2);
                    if (LockCheck1 == MoveState.FREE)
                    {
                        P2CurrentPieceLocation = P2NewPieceLocation;
                        GrabMinoLocations(P2CurrentPolymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);
                    }
                }

                P2Elapsed = 0;
            }

            // Order 4: LockChecks
            // P1
            if (P1CurARE >= P1ARE)
            {
                Vector2 LockPlaceTest1 = P1CurrentPieceLocation + new Vector2(0, 1);
                MoveState LockPlace1 = SpaceTest(matrix, P1CurrentPolymino, (int)LockPlaceTest1.X, (int)LockPlaceTest1.Y, 1);
                if (LockPlace1 != MoveState.FREE && LockPlace1 != MoveState.COLLIDED && NewKS.IsKeyUp(P1SoftDrop))
                {
                    P1CurLD--;
                }
                else if (LockPlace1 != MoveState.FREE && LockPlace1 != MoveState.COLLIDED && !P1SD)
                {
                    P1CurLD = 0;
                }
                if (LockPlaceTest1.Y > OldLTP1.Y)
                {
                    P1CurLD = P1LD;
                }
                OldLTP1 = LockPlaceTest1;

                if (P1CurLD <= 0)
                {
                    matrix = Lock(matrix, P1CurrentPolymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
                    if (NewKS.IsKeyDown(P1SoftDrop))
                    {
                        P1SD = true;
                    }
                    GrabMinoLocations(P1CurrentPolymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
                }
            }

            // P2
            if (P2CurARE >= P2ARE)
            {
                Vector2 LockPlaceTest2 = P2CurrentPieceLocation + new Vector2(0, 1);
                MoveState LockPlace2 = SpaceTest(matrix, P2CurrentPolymino, (int)LockPlaceTest2.X, (int)LockPlaceTest2.Y, 2);
                if (LockPlace2 != MoveState.FREE && LockPlace2 != MoveState.COLLIDED && NewKS.IsKeyUp(P2SoftDrop))
                {
                    P2CurLD--;
                }
                else if (LockPlace2 != MoveState.FREE && LockPlace2 != MoveState.COLLIDED && !P2SD)
                {
                    P2CurLD = 0;
                }

                if (P2CurLD <= 0)
                {
                    matrix = Lock(matrix, P2CurrentPolymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);
                    if (NewKS.IsKeyDown(P2SoftDrop))
                    {
                        P2SD = true;
                    }
                    GrabMinoLocations(P2CurrentPolymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);
                }
                if (LockPlaceTest2.Y > OldLTP2.Y)
                {
                    P2CurLD = P2LD;
                }
                OldLTP2 = LockPlaceTest2;
            }

            GrabMinoLocations(P1CurrentPolymino, (int)P1CurrentPieceLocation.X, (int)P1CurrentPieceLocation.Y, 1);
            GrabMinoLocations(P2CurrentPolymino, (int)P2CurrentPieceLocation.X, (int)P2CurrentPieceLocation.Y, 2);

            // TODO: Add your update logic here
            OldKS = Keyboard.GetState();
            base.Update(gameTime);
        }

        Vector2 GhostPiece1;
        Vector2 GhostPiece2;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            TetrionBox.Draw(spriteBatch, new Vector2(MatrixLocation.X - MonominoSize, MatrixLocation.Y), MatrixHeight + 1, MatrixWidth + 2);
            GenericBox.Draw(spriteBatch, new Vector2(MatrixLocation.X - MonominoSize * 8, MatrixLocation.Y), 6, 8);
            GenericBox.Draw(spriteBatch, new Vector2(MatrixLocation.X + MonominoSize * (MatrixWidth), MatrixLocation.Y), 6, 8);

            if (P1Spawn)
            {
                for (int i = (int)P1CurrentPieceLocation.Y; i < MatrixHeight; i++)
                {
                    GhostPiece1 = new Vector2(P1CurrentPieceLocation.X, i);
                    MoveState LockCheck1 = SpaceTest(matrix, P1CurrentPolymino, (int)GhostPiece1.X, (int)GhostPiece1.Y, 1);
                    if (LockCheck1 != MoveState.FREE)
                    {
                        GhostPiece1 = new Vector2(P1CurrentPieceLocation.X, i - 1);
                        break;
                    }
                }
            }

            if (P2Spawn)
            {
                for (int i = (int)P2CurrentPieceLocation.Y; i < MatrixHeight; i++)
                {
                    GhostPiece2 = new Vector2(P2CurrentPieceLocation.X, i);
                    MoveState LockCheck1 = SpaceTest(matrix, P2CurrentPolymino, (int)GhostPiece2.X, (int)GhostPiece2.Y, 2);
                    if (LockCheck1 != MoveState.FREE)
                    {
                        GhostPiece2 = new Vector2(P2CurrentPieceLocation.X, i - 1);
                        break;
                    }
                }
            }
            
            // TODO: Add your drawing code here
            spriteBatch.Begin();
            TextDraw.Draw(spriteBatch, "Lines: " + lines, new Vector2(MatrixLocation.X, MatrixLocation.Y + (MonominoSize * (MatrixHeight + 1))));

            for (int y = 0; y < MatrixHeight; y++)  // Draw Matrix
            {
                for (int x = 0; x < MatrixWidth; x++)
                {
                    if (y != 0 && matrix[y, x] != 0)
                    {
                        Color Base = MinoColours[matrix[y, x]];
                        Color Block = Color.FromNonPremultiplied((int)(Base.R * 0.65), (int)(Base.G * 0.65), (int)(Base.B * 0.65), 255);
                        spriteBatch.Draw(Monomino, new Vector2(MonominoSize * x + MatrixLocation.X, MonominoSize * y + MatrixLocation.Y), Block);
                    }
                }
            }

            int[,] P1NextMino = Polyminoes[P1Nexts[0]][0];
            int P1NextMinoLen = P1NextMino.GetLength(0);
            for (int y = 0; y < P1NextMinoLen; y++)
            {
                for (int x = 0; x < P1NextMinoLen; x++)
                {
                    int col = P1NextMino[y, x];
                    Color BaseColor = col != 0 ? MinoColours[P1Nexts[0] + 1] : MinoColours[0];
                    int offset = (int)(((double)(4 - P1NextMinoLen) / 2) * MonominoSize);
                    int offsetY = P1Nexts[0] == 0 ? MonominoSize / 2 : 0;
                    spriteBatch.Draw(Monomino, new Vector2(MatrixLocation.X - (6 * MonominoSize) + offset + x * MonominoSize, MatrixLocation.Y + MonominoSize + y * MonominoSize + offsetY), BaseColor);
                }
            }

            int[,] P2NextMino = Polyminoes[P2Nexts[0]][0];
            int P2NextMinoLen = P2NextMino.GetLength(0);
            for (int y = 0; y < P2NextMinoLen; y++)
            {
                for (int x = 0; x < P2NextMinoLen; x++)
                {
                    int col = P2NextMino[y, x];
                    Color BaseColor = col != 0 ? MinoColours[P2Nexts[0] + 1] : MinoColours[0];
                    int offset = (int)(((double)(4 - P2NextMinoLen) / 2) * MonominoSize);
                    int offsetY = P2Nexts[0] == 0 ? MonominoSize / 2 : 0;
                    spriteBatch.Draw(Monomino, new Vector2(MatrixLocation.X + (MatrixWidth * MonominoSize) + (2 * MonominoSize) + offset + x * MonominoSize, MatrixLocation.Y + MonominoSize + y * MonominoSize + offsetY), BaseColor);
                }
            }

            int P1SideLen = P1CurrentPolymino.GetLength(0);
            if (P1Spawn)
            {
                for (int y = 0; y < P1SideLen; y++)  // Draw P1's ghost piece
                {
                    for (int x = 0; x < P1SideLen; x++)
                    {
                        int col = P1CurrentPolymino[y, x];
                        Color BaseColor = Color.FromNonPremultiplied(255, 255, 255, col != 0 ? 127 : 0);
                        spriteBatch.Draw(Monomino, new Vector2((MatrixLocation.X + MonominoSize * x) + (GhostPiece1.X * MonominoSize), (MatrixLocation.Y + MonominoSize * y) + (GhostPiece1.Y * MonominoSize)), BaseColor);
                    }
                }
            }

            int P2SideLen = P2CurrentPolymino.GetLength(0);
            if (P2Spawn)
            {
                for (int y = 0; y < P2SideLen; y++)  // Draw P2's ghost piece
                {
                    for (int x = 0; x < P2SideLen; x++)
                    {
                        int col2 = P2CurrentPolymino[y, x];
                        Color BaseColor2 = Color.FromNonPremultiplied(255, 255, 255, col2 != 0 ? 127 : 0);
                        spriteBatch.Draw(Monomino, new Vector2((MatrixLocation.X + MonominoSize * x) + (GhostPiece2.X * MonominoSize), (MatrixLocation.Y + MonominoSize * y) + (GhostPiece2.Y * MonominoSize)), BaseColor2);
                    }
                }
            }

            for (int y = 0; y < P1SideLen; y++)  // Draw P1's falling piece
            {
                for (int x = 0; x < P1SideLen; x++)
                {
                    int col = P1CurrentPolymino[y, x];
                    Color BaseColor = col != 0 ? MinoColours[P1PolyminoIndex + 1] : MinoColours[0];
                    BaseColor = Color.FromNonPremultiplied((int)(BaseColor.R * ((double)P1CurLD / P1LD)), (int)(BaseColor.G * ((double)P1CurLD / P1LD)), (int)(BaseColor.B * ((double)P1CurLD / P1LD)), col != 0 && P1Spawn ? 255 : 0);
                    spriteBatch.Draw(Monomino, new Vector2((MatrixLocation.X + MonominoSize * x) + (P1CurrentPieceLocation.X * MonominoSize), (MatrixLocation.Y + MonominoSize * y) + (P1CurrentPieceLocation.Y * MonominoSize)), BaseColor);
                }
            }

            for (int y = 0; y < P2SideLen; y++)  // Draw P2's falling piece
            {
                for (int x = 0; x < P2SideLen; x++)
                {
                    int col2 = P2CurrentPolymino[y, x];
                    Color BaseColor2 = col2 != 0 ? MinoColours[P2PolyminoIndex + 1] : MinoColours[0];
                    BaseColor2 = Color.FromNonPremultiplied((int)(BaseColor2.R * ((double)P2CurLD / P2LD)), (int)(BaseColor2.G * ((double)P2CurLD / P2LD)), (int)(BaseColor2.B * ((double)P2CurLD / P2LD)), col2 != 0 && P2Spawn ? 255 : 0);
                    spriteBatch.Draw(Monomino, new Vector2((MatrixLocation.X + MonominoSize * x) + (P2CurrentPieceLocation.X * MonominoSize), (MatrixLocation.Y + MonominoSize * y) + (P2CurrentPieceLocation.Y * MonominoSize)), BaseColor2);
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
