using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Maze_Project.Sprites;

namespace Maze_Project
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary> 
    /// 

    public class Game1 : Game
    {
        Random rnd = new Random();

        enum ScreenState
        {
            Title,
            MainMenu,
            MainMenuOptions,
            InGame,
            InGameStartFreeze,
            InGameMenu,
            InGameOptions      
        }
        enum GridType
        {
            Quad,
            Hex
        }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        ScreenState currentScreen;
        GridType currentGridType = GridType.Quad;
        GridType nextGridType = GridType.Quad;
        KeyboardState previousKeyboard;
        MouseState previousMouse;

        List<Sprite> sprites = new List<Sprite>();
        List<Opponent> opponents = new List<Opponent>();
        List<PowerUp> powerUps = new List<PowerUp>();
        List<Hex_Cell> hexGrid = new List<Hex_Cell>();
        List<Cell> quadGrid = new List<Cell>();

        List<Button> mainMenuButtons = new List<Button>();
        List<Button> mainMenuOptionsButtons = new List<Button>();
        List<Button> inGameMenuButtons = new List<Button>();
        List<Button> inGameOptionsButtons = new List<Button>();
        List<Texture2D> gemTextures = new List<Texture2D>();
        int[,] ADJlist;
             
        Texture2D blankTexture, beeTexture, flagTexture, woodenTexture, menuTintTexture, blueGemTexture, greenGemTexture, yellowGemTexture;
        SpriteFont menuFont, timerFont;

        //bool speedBoostBool, guidenceBool;//powerup variables
        //float boostElapsed, guidenceElapsed;
        Queue<Guidance_Orb> guidance_Orb_Queue = new Queue<Guidance_Orb>();

        float elapsed=0, timerElapsedTime = 0;     //Animation variables
        readonly float delay = 100f;
        int frames = 0;

        Player player1;
        Opponent opponent1;
        Opponent opponent2;
        Opponent opponent3;

        Sprite flag;
        Hex_Cell hexGoal;
        Cell quadGoal;
        int Columns = 10, nextGridSize = 10;
        readonly int ScreenWidth;
        int cellWidth;
        int Rows;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                IsFullScreen = true,               //1366x768
                PreferredBackBufferWidth = 1366,    //1920x1080
                PreferredBackBufferHeight = 768
            };
            ScreenWidth = graphics.PreferredBackBufferWidth;
            Content.RootDirectory = "Content";
            
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
            previousKeyboard = Keyboard.GetState();
            previousMouse = Mouse.GetState();

            cellWidth = (int)(graphics.PreferredBackBufferWidth / Columns);
            Rows = (int)(graphics.PreferredBackBufferHeight / cellWidth);
            switch (currentGridType)
            {
                case GridType.Quad:
                    quadGrid = GenerateGrid(Columns, Rows, quadGrid);
                    quadGoal = quadGrid[0].GenerateMaze(quadGrid, Columns, Rows, rnd);
                    ADJlist = quadGrid[0].ADJlist;
                    break;
                case GridType.Hex:
                    cellWidth = (int)((graphics.PreferredBackBufferWidth - (cellWidth / 2)) / Columns);
                    Rows = (int)((2 * Math.Sqrt(3) * graphics.PreferredBackBufferHeight - cellWidth) / (3 * cellWidth)); //Comes from Height = Number of cells - (Number of Cells - 1) * Height of Upper Hex Triangle

                    hexGrid = GenerateGrid(Columns, Rows, hexGrid);
                    hexGoal = hexGrid[0].GenerateMaze(hexGrid, Columns,Rows, rnd);
                    ADJlist = hexGrid[0].ADJlist;
                    break;
            }



            currentScreen = ScreenState.Title;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            //Loads textures

            blankTexture = new Texture2D(GraphicsDevice, 1, 1);                  //Creates a 1 by 1 white pixel texure that I can tint and use for walls 
            blankTexture.SetData(new[] { Color.White });
            menuTintTexture = new Texture2D(GraphicsDevice, 1, 1);
            menuTintTexture.SetData(new[] { Color.Black * 0.75f });


            beeTexture = Content.Load<Texture2D>("beesSpritesheet");
            flagTexture = Content.Load<Texture2D>("flagSpritesheet");
            woodenTexture = Content.Load<Texture2D>("Wooden Texture - pexels");
            blueGemTexture = Content.Load<Texture2D>("Blue Gem Spritesheet");
            greenGemTexture = Content.Load<Texture2D>("Green Gem Spritesheet");
            yellowGemTexture = Content.Load<Texture2D>("Yellow Gem Spritesheet");

            gemTextures.Add(blueGemTexture);
            gemTextures.Add(greenGemTexture);
            gemTextures.Add(yellowGemTexture);
            menuFont = Content.Load<SpriteFont>("MenuFont");
            timerFont = Content.Load<SpriteFont>("TimerFont");

            //Main Menu Buttons
            mainMenuButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 100, graphics.PreferredBackBufferHeight / 2 - 100), 200, 50, woodenTexture, "playBut", "Start Game", menuFont));
            mainMenuButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 100, graphics.PreferredBackBufferHeight / 2 - 25),200,50, woodenTexture, "optionsBut", "Options", menuFont));
            mainMenuButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 100, graphics.PreferredBackBufferHeight / 2 + 50), 200, 50, woodenTexture, "backBut", "Back", menuFont));
            //Main Menu Option Buttons
            mainMenuOptionsButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 75, graphics.PreferredBackBufferHeight / 2 - 100), 150, 50, woodenTexture, "gridSizeBut", "Grid Size: " + Columns.ToString(), menuFont));
            mainMenuOptionsButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 100, graphics.PreferredBackBufferHeight / 2 - 100), 25, 50, woodenTexture, "-gridSizeBut", "<", menuFont));
            mainMenuOptionsButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 + 75, graphics.PreferredBackBufferHeight / 2 - 100), 25, 50, woodenTexture, "+gridSizeBut", ">", menuFont));
            mainMenuOptionsButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 100, graphics.PreferredBackBufferHeight / 2 - 25), 200, 50, woodenTexture, "gridBut", "Grid Type: " + currentGridType.ToString(), menuFont));
            mainMenuOptionsButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 100, graphics.PreferredBackBufferHeight / 2 + 50), 200, 50, woodenTexture, "backBut", "Back", menuFont));
            //In Game Menu Buttons
            inGameMenuButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 100, graphics.PreferredBackBufferHeight / 2 - 100), 200, 50, woodenTexture, "resumeBut", "Resume", menuFont));
            inGameMenuButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 100, graphics.PreferredBackBufferHeight / 2 - 25), 200, 50, woodenTexture, "optionsBut", "Options", menuFont));
            inGameMenuButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 100, graphics.PreferredBackBufferHeight / 2 + 50), 200, 50, woodenTexture, "exitBut", "Exit", menuFont));
            //Option Menu Buttons
            inGameOptionsButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 75, graphics.PreferredBackBufferHeight / 2 - 100), 150, 50, woodenTexture, "gridSizeBut", "Grid Size: " + Columns.ToString(), menuFont));
            inGameOptionsButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 100, graphics.PreferredBackBufferHeight / 2 - 100), 25, 50, woodenTexture, "-gridSizeBut", "<", menuFont));
            inGameOptionsButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 + 75, graphics.PreferredBackBufferHeight / 2 - 100), 25, 50, woodenTexture, "+gridSizeBut", ">", menuFont));
            inGameOptionsButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 100, graphics.PreferredBackBufferHeight / 2 - 25), 200, 50, woodenTexture, "gridBut", "Grid Type: " + currentGridType.ToString(), menuFont));
            inGameOptionsButtons.Add(new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 100, graphics.PreferredBackBufferHeight / 2 + 50), 200, 50, woodenTexture, "backBut", "Back", menuFont));

            flag = new Sprite(flagTexture, cellWidth, 3, ScreenWidth);
            switch (currentGridType)
            {
                case GridType.Quad:
                    flag.Position = new Vector2(quadGoal.GridPos.X * cellWidth + (cellWidth / 4), quadGoal.GridPos.Y * cellWidth + (cellWidth / 4));

                    flag.CurrentADJPos = quadGoal.ADJlistPos;

                    opponent1 = new Opponent(beeTexture, cellWidth, 3, 1, currentGridType.ToString(), ScreenWidth, ADJlist, quadGoal);
                    opponent2 = new Opponent(beeTexture, cellWidth, 3, 2, currentGridType.ToString(), ScreenWidth, ADJlist, quadGoal);
                    opponent3 = new Opponent(beeTexture, cellWidth, 3, 3, currentGridType.ToString(), ScreenWidth, ADJlist, quadGoal);

                    foreach (var Cell in quadGrid)
                    {
                        if (Cell.Occupied)
                        {
                            powerUps.Add(new PowerUp(gemTextures, Cell.GridPos, cellWidth, 3, ScreenWidth, currentGridType.ToString(), rnd));
                        }
                    }
                    break;

                case GridType.Hex:
                    if (hexGoal.GridPos.Y % 2 == 0)
                    {
                        flag.Position = new Vector2(((int)(cellWidth * hexGoal.GridPos.X) + (cellWidth / 2) - (cellWidth / 5)), (int)(cellWidth / Math.Sqrt(3)) * (1 + 1.5f * (float)(hexGoal.GridPos.Y)) - (cellWidth / 4));
                    }
                    else
                    {
                        flag.Position = new Vector2(((int)(cellWidth * hexGoal.GridPos.X)) + cellWidth - (cellWidth / 5), (int)(cellWidth / Math.Sqrt(3)) * (1 + 1.5f * (float)(hexGoal.GridPos.Y)) - (cellWidth / 4));
                    }

                    flag.CurrentADJPos = hexGoal.ADJlistPos;

                    opponent1 = new Opponent(beeTexture, cellWidth, 3, 1, currentGridType.ToString(), ScreenWidth, ADJlist, hexGoal);
                    opponent2 = new Opponent(beeTexture, cellWidth, 3, 2, currentGridType.ToString(), ScreenWidth, ADJlist, hexGoal);
                    opponent3 = new Opponent(beeTexture, cellWidth, 3, 3, currentGridType.ToString(), ScreenWidth, ADJlist, hexGoal);

                    foreach (var hexCell in hexGrid)
                    {
                        if (hexCell.Occupied)
                        {
                            powerUps.Add(new PowerUp(gemTextures, hexCell.GridPos, cellWidth, 3, ScreenWidth,currentGridType.ToString(), rnd));
                        }
                    }
                    break;
            }
            flag.sourcRect = new Rectangle(0, 0, 310, 330);
            flag.destRect = new Rectangle((int)flag.Position.X, (int)flag.Position.Y, cellWidth / 2, cellWidth / 2);
            

            player1 = new Player(beeTexture, cellWidth, 3, currentGridType.ToString(), ScreenWidth, flag);


            sprites.Add(player1);
            sprites.Add(opponent1);
            sprites.Add(opponent2);
            sprites.Add(opponent3);
            sprites.Add(flag);

            opponents.Add(opponent1);
            opponents.Add(opponent2);
            opponents.Add(opponent3);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            switch (currentScreen)                          //Allows separate Update loops for different screen states
            {
                case ScreenState.Title:
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousKeyboard.IsKeyUp(Keys.Escape)) //if the player is pressing escape
                    {
                        Exit();
                    }
                    else if (Keyboard.GetState() != previousKeyboard && previousKeyboard.GetPressedKeys().Length == 0 || Mouse.GetState().LeftButton == ButtonState.Released && previousMouse.LeftButton == ButtonState.Pressed)
                    {
                        IsMouseVisible = true;
                        currentScreen = ScreenState.MainMenu;
                    }

                    break;
                case ScreenState.MainMenu:
                    MainMenuUpdate(gameTime);
                    break;
                case ScreenState.MainMenuOptions:
                    MainMenuOptionsUpdate(gameTime);
                    break;
                case ScreenState.InGame:
                    InGameUpdate(gameTime);
                    break;
                case ScreenState.InGameStartFreeze:
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousKeyboard.IsKeyUp(Keys.Escape)) //if the player is pressing escape
                    {
                        Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);       //Sets the position to centre of screen
                        IsMouseVisible = true;
                        currentScreen = ScreenState.InGameMenu;                                   //Open Menu
                    }
                    else if (Keyboard.GetState() != previousKeyboard && previousKeyboard.GetPressedKeys().Length == 0)
                    {
                        currentScreen = ScreenState.InGame;
                    }
                    timerElapsedTime += gameTime.ElapsedGameTime.Milliseconds;
                    if (timerElapsedTime > 5000) //5000 milliseconds = 5 seconds
                    {
                        timerElapsedTime = 0;
                        currentScreen = ScreenState.InGame;
                    }   
                    
                    AnimationsUpdate(gameTime);
                    break;
                case ScreenState.InGameMenu:
                    InGameMenuUpdate(gameTime);
                    break;
                case ScreenState.InGameOptions:
                    OptionsMenuUpdate(gameTime);
                    break;
                default:
                    throw new Exception("Invalid ScreenState");
                
            }
            previousKeyboard = Keyboard.GetState();                                         //Updates the previous Keyboard/Mouse
            previousMouse = Mouse.GetState();

            base.Update(gameTime);
        }
        public void InGameUpdate(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousKeyboard.IsKeyUp(Keys.Escape))
            {
                Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);   //Sets the position to centre of screen
                IsMouseVisible = true;
                currentScreen = ScreenState.InGameMenu;
            }
            #region Collision Logic
            switch (currentGridType)
            {
                case GridType.Quad:
                    
                    if (player1.boundary.Intersects(quadGrid[player1.CurrentADJPos - 1].topRect) && quadGrid[player1.CurrentADJPos - 1].walls[0])
                        player1.Position.Y += player1.LinearVelocity;
                    else if (player1.boundary.Intersects(quadGrid[player1.CurrentADJPos - 1].botRect) && quadGrid[player1.CurrentADJPos - 1].walls[2])
                        player1.Position.Y -= player1.LinearVelocity;
                    if (player1.boundary.Intersects(quadGrid[player1.CurrentADJPos - 1].rightRect) && quadGrid[player1.CurrentADJPos - 1].walls[1])
                        player1.Position.X -= player1.LinearVelocity;
                    else if (player1.boundary.Intersects(quadGrid[player1.CurrentADJPos - 1].leftRect) && quadGrid[player1.CurrentADJPos - 1].walls[3])
                        player1.Position.X += player1.LinearVelocity;


                    if (opponent1.CurrentADJPos == quadGoal.ADJlistPos || opponent3.CurrentADJPos == quadGoal.ADJlistPos || opponent2.CurrentADJPos == quadGoal.ADJlistPos || player1.CurrentADJPos == quadGoal.ADJlistPos) //If anyone gets to the goal then reset go to next level
                    {
                        Columns += 0;
                        if (nextGridType == GridType.Quad)
                        {
                            currentGridType = GridType.Quad;
                            ResetQuadGrid();
                        }
                        else if (nextGridType == GridType.Hex)
                        {
                            currentGridType = GridType.Hex;
                            ResetHexGrid();
                        }
                        currentScreen = ScreenState.InGameStartFreeze;
                    }
                    break;
                case GridType.Hex:
                    for (int i = 0; i < hexGrid.Count; i++)
                    {
                        if ((hexGrid[i].walls[0] && player1.boundary.Intersects(hexGrid[i].topRightRect)))
                        {
                            if (((player1.boundary.X+(player1.boundary.Width/2)) - (hexGrid[i].topRightRect.X + hexGrid[i].topRightRect.Origin.X)) - ((player1.boundary.Y+(player1.boundary.Height/2)) - (hexGrid[i].topRightRect.Y + hexGrid[i].topRightRect.Origin.Y)) < 0)
                            {
                                player1.CurrentADJPos = i + 1;
                                player1.Position.Y += player1.LinearVelocity * 0.7f;
                                player1.Position.X -= player1.LinearVelocity * 0.7f;
                            }
                        }
                        else if (hexGrid[i].walls[3] && player1.boundary.Intersects(hexGrid[i].botLeftRect))
                        {
                            if (((player1.boundary.X + (player1.boundary.Width / 2)) - (hexGrid[i].botLeftRect.X + hexGrid[i].botLeftRect.Origin.X)) - ((player1.boundary.Y + (player1.boundary.Height / 2)) - (hexGrid[i].botLeftRect.Y + hexGrid[i].botLeftRect.Origin.Y)) > 0)
                            {
                                player1.CurrentADJPos = i + 1;
                                player1.Position.Y -= player1.LinearVelocity * 0.7f;
                                player1.Position.X += player1.LinearVelocity * 0.7f;
                            }
                        }
                        if (player1.boundary.Intersects(hexGrid[i].rightRect) && hexGrid[i].walls[1])
                        {
                            if (((player1.boundary.X + (player1.boundary.Width / 2)) - (hexGrid[i].rightRect.X + hexGrid[i].rightRect.Origin.X)) < 0)
                            {
                                player1.CurrentADJPos = i + 1;
                                player1.Position.X -= player1.LinearVelocity;
                            }
                        }
                        else if (player1.boundary.Intersects(hexGrid[i].leftRect) && hexGrid[i].walls[4])
                        {
                            if (((player1.boundary.X + (player1.boundary.Width / 2)) - (hexGrid[i].leftRect.X + hexGrid[i].leftRect.Origin.X)) > 0)
                            {
                                player1.CurrentADJPos = i + 1;
                                player1.Position.X += player1.LinearVelocity;
                            }
                        }
                        if (player1.boundary.Intersects(hexGrid[i].botRightRect) && hexGrid[i].walls[2])
                        {
                            if (((player1.boundary.X + (player1.boundary.Width / 2)) - (hexGrid[i].botRightRect.X + hexGrid[i].botRightRect.Origin.X)) + ((player1.boundary.Y + (player1.boundary.Height / 2)) - (hexGrid[i].botRightRect.Y + hexGrid[i].botRightRect.Origin.Y)) < 0)
                            {
                                player1.CurrentADJPos = i + 1;
                                player1.Position.X -= player1.LinearVelocity * 0.7f;
                                player1.Position.Y -= player1.LinearVelocity * 0.7f;
                            }
                        }
                        else if (player1.boundary.Intersects(hexGrid[i].topLeftRect) && hexGrid[i].walls[5])
                        {
                            if (((player1.boundary.X + (player1.boundary.Width / 2)) - (hexGrid[i].topLeftRect.X + hexGrid[i].topLeftRect.Origin.X)) + ((player1.boundary.Y + (player1.boundary.Height / 2)) - (hexGrid[i].topLeftRect.Y + hexGrid[i].topLeftRect.Origin.Y)) > 0)
                            {
                                player1.CurrentADJPos = i + 1;
                                player1.Position.X += player1.LinearVelocity * 0.7f;
                                player1.Position.Y += player1.LinearVelocity * 0.7f;
                            }
                        }
                    }
                    if (opponent1.CurrentADJPos == hexGoal.ADJlistPos || opponent3.CurrentADJPos == hexGoal.ADJlistPos || opponent2.CurrentADJPos == hexGoal.ADJlistPos || player1.CurrentADJPos == hexGoal.ADJlistPos) //If anyone gets to the goal then reset go to next level
                    {
                        Columns += 0;
                        if (nextGridType == GridType.Quad)
                        {
                            currentGridType = GridType.Quad;
                            ResetQuadGrid();
                        }
                        else if (nextGridType == GridType.Hex)
                        {
                            currentGridType = GridType.Hex;
                            ResetHexGrid();
                        }
                        currentScreen = ScreenState.InGameStartFreeze;
                    }
                    break;
            }
            #endregion

            #region PowerUp Logic

            foreach (PowerUp powerUp in powerUps)
            {
                if (PowerUpCheck(player1, powerUp) || PowerUpCheck(opponent1, powerUp) || PowerUpCheck(opponent2, powerUp) || PowerUpCheck(opponent3, powerUp)) 
                    break;
            }
            #endregion
            player1.Update(gameTime);
            opponent1.Update(gameTime);
            opponent2.Update(gameTime);
            opponent3.Update(gameTime);

            

            //Animations Update
            AnimationsUpdate(gameTime);

        }


        public void AnimationsUpdate(GameTime gameTime)
        {
            elapsed += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            player1.sourcRect = new Rectangle(150 * (frames % 3), 0, 150, 167);
            opponent1.sourcRect = new Rectangle(150 * ((frames + 1) % 3), 0, 150, 167);
            opponent2.sourcRect = new Rectangle(150 * (frames % 3), 0, 150, 167);
            opponent3.sourcRect = new Rectangle(150 * ((frames + 2) % 3), 0, 150, 167);
            flag.sourcRect = new Rectangle(310 * (frames % 3), 0, 310, 330);

            #region PowerUp Timings
            foreach (var powerUp in powerUps)
            {
                powerUp.sourcRect = new Rectangle(80 * ((frames + powerUps.IndexOf(powerUp)) % 6), 0, 80, 92); //Makes powerups rotate in different phases
            }

            if (player1.speedBoostBool)
            {
                player1.boostElapsed += (float) gameTime.ElapsedGameTime.TotalMilliseconds;
                if (player1.boostElapsed >= 5000f)
                {
                    player1.speedBoostBool = false;
                    player1.LinearVelocity /= 1.5f;
                }
            }
            if (player1.guidenceBool)
            {
                foreach(Guidance_Orb orb in guidance_Orb_Queue)
                {
                    if (orb.Parent == 0)
                        orb.Update(player1.CurrentADJPos, player1.Position);
                }

                player1.guidenceElapsed += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (player1.guidenceElapsed >= 5000f)
                {
                    player1.guidenceBool = false;
                    guidance_Orb_Queue.Dequeue();
                }
            }
            foreach(Opponent opponent in opponents)
            {
                if (opponent.speedBoostBool)
                {
                    opponent.boostElapsed += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (opponent.boostElapsed >= 5000f)
                    {
                        opponent.speedBoostBool = false;
                        opponent.LinearVelocity /= 1.5f;
                    }
                }
                if (opponent.guidenceBool)
                {
                    foreach (Guidance_Orb orb in guidance_Orb_Queue)
                    {
                        if (orb.Parent == opponent.Difficulty)
                            orb.Update(opponent.CurrentADJPos, opponent.Position);
                    }
                    opponent.guidenceElapsed += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (opponent.guidenceElapsed >= 5000f)
                    {
                        opponent.guidenceBool = false;
                        guidance_Orb_Queue.Dequeue();
                    }
                }
            }
            #endregion
            if (elapsed >= delay)
            {
                frames++;
                frames = frames % 1000;

                elapsed = 0;
            }

        }
        public void MainMenuUpdate(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousKeyboard.IsKeyUp(Keys.Escape)) //if the player is pressing escape
            {
                IsMouseVisible = false;
                currentScreen = ScreenState.Title;
            }
            foreach(Button button in mainMenuButtons)
            {
                if (button.IsPressedCheck(previousMouse))
                {
                    switch (button.Name)
                    {
                        case "backBut":
                            IsMouseVisible = false;
                            currentScreen = ScreenState.Title;
                            break;
                        case "optionsBut":
                            currentScreen = ScreenState.MainMenuOptions;
                            break;
                        case "playBut":
                            currentGridType = nextGridType;
                            switch (nextGridType)
                            {
                                case GridType.Quad:
                                    ResetQuadGrid();
                                    break;
                                case GridType.Hex:
                                    ResetHexGrid();
                                    break;
                            }

                            currentScreen = ScreenState.InGameStartFreeze;
                            IsMouseVisible = false;
                            break;
                    }
                }
            }
        }
        public void MainMenuOptionsUpdate(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousKeyboard.IsKeyUp(Keys.Escape))
            {
                currentScreen = ScreenState.MainMenu;
            }
            foreach (Button button in mainMenuOptionsButtons)
            {
                if (button.IsPressedCheck(previousMouse))
                {
                    switch (button.Name)
                    {
                        case "backBut":
                            currentScreen = ScreenState.MainMenu;
                            return;//return used over break to exit the foreach loop 
                        case "-gridSizeBut":
                            if (nextGridSize > 4)
                            {
                                nextGridSize--;
                                mainMenuOptionsButtons[0].NewText = "Grid Size: " + nextGridSize.ToString();
                                inGameOptionsButtons[0].NewText = "Grid Size: " + nextGridSize.ToString();
                            }
                            return;
                        case "+gridSizeBut":
                            if (nextGridSize < 50)
                            {
                                nextGridSize++;
                                mainMenuOptionsButtons[0].NewText = "Grid Size: " + nextGridSize.ToString();
                                inGameOptionsButtons[0].NewText = "Grid Size: " + nextGridSize.ToString();
                            }
                            return;
                        case "gridBut":
                            switch (nextGridType)
                            {
                                case GridType.Quad:
                                    currentGridType = GridType.Hex;
                                    nextGridType = GridType.Hex;
                                    ResetHexGrid();
                                    inGameOptionsButtons[3].NewText = "Grid Type: Hex";
                                    mainMenuOptionsButtons[3].NewText = "Grid Type: Hex";
                                    break;
                                case GridType.Hex:
                                    currentGridType = GridType.Quad;
                                    nextGridType = GridType.Quad;
                                    ResetQuadGrid();
                                    inGameOptionsButtons[3].NewText = "Grid Type: Quad";
                                    mainMenuOptionsButtons[3].NewText = "Grid Type: Quad";
                                    break;
                            }
                            return;
                    }
                }
            }
        }
        public void InGameMenuUpdate(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousKeyboard.IsKeyUp(Keys.Escape))
            {
                IsMouseVisible=false;
                currentScreen = ScreenState.InGame;
            }
            foreach (Button button in inGameMenuButtons)
            {
                if (button.IsPressedCheck(previousMouse))
                {
                    switch (button.Name)
                    {
                        case "exitBut":
                            currentScreen = ScreenState.MainMenu;
                            return;//return used over break to exit the foreach loop 
                        case "resumeBut":
                            IsMouseVisible = false;
                            currentScreen = ScreenState.InGame;
                            return;
                        //Add more for each new button and each function. Usually this will just be one line changing the screen state
                        case "optionsBut":
                            currentScreen = ScreenState.InGameOptions;
                            return;
                    }
                }
                
            }
            
        }
        public void OptionsMenuUpdate(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousKeyboard.IsKeyUp(Keys.Escape))
            {
                currentScreen = ScreenState.InGameMenu;
            }
            foreach (Button button in inGameOptionsButtons)
            {
                if (button.IsPressedCheck(previousMouse))
                {
                    switch (button.Name)
                    {
                        case "backBut":
                            currentScreen = ScreenState.InGameMenu;
                            return;//return used over break to exit the foreach loop 
                        case "-gridSizeBut":
                            if (nextGridSize > 4)
                            {
                                nextGridSize --;
                                mainMenuOptionsButtons[0].NewText = "Grid Size: " + nextGridSize.ToString();
                                inGameOptionsButtons[0].NewText = "Grid Size: " + nextGridSize.ToString();
                            }
                            return;
                        case "+gridSizeBut":
                            if (nextGridSize < 50)
                            {
                                nextGridSize++;
                                mainMenuOptionsButtons[0].NewText = "Grid Size: " + nextGridSize.ToString();
                                inGameOptionsButtons[0].NewText = "Grid Size: " + nextGridSize.ToString();
                            }
                            return;
                        case "gridBut":
                            switch (nextGridType)
                            {
                                case GridType.Quad:
                                    nextGridType = GridType.Hex;
                                    inGameOptionsButtons[3].NewText = "Grid Type: Hex";
                                    mainMenuOptionsButtons[3].NewText = "Grid Type: Hex";
                                    break;
                                case GridType.Hex:
                                    nextGridType = GridType.Quad;
                                    inGameOptionsButtons[3].NewText = "Grid Type: Quad";
                                    mainMenuOptionsButtons[3].NewText = "Grid Type: Quad";
                                    break;
                            }
                            return;
                    }
                }

            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            switch (currentScreen)                                               //Separate Screen States to draw difference screens
            {
                case ScreenState.Title:
                    TitleDraw(spriteBatch);
                    break;
                case ScreenState.MainMenu:
                    MenuDraw(spriteBatch, mainMenuButtons);
                    break;
                case ScreenState.MainMenuOptions:
                    MenuDraw(spriteBatch, mainMenuOptionsButtons);
                    break;
                case ScreenState.InGame:
                    GameplayDraw(spriteBatch);
                    break;
                case ScreenState.InGameStartFreeze:
                    GameplayDraw(spriteBatch);
                    spriteBatch.DrawString(timerFont, (5-Math.Floor((timerElapsedTime)/1000)).ToString(), new Vector2(graphics.PreferredBackBufferWidth/2, 50f), Color.Purple);
                  
                    break;
                case ScreenState.InGameMenu:
                    GameplayDraw(spriteBatch);
                    spriteBatch.Draw(menuTintTexture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.Black);
                    MenuDraw(spriteBatch,inGameMenuButtons);
                    break;
                case ScreenState.InGameOptions:
                    GameplayDraw(spriteBatch);
                    spriteBatch.Draw(menuTintTexture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.Black);
                    MenuDraw(spriteBatch, inGameOptionsButtons);
                    break;
                default:
                    throw new Exception("Invalid ScreenState");

            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
        public void GameplayDraw(SpriteBatch spriteBatch)
        {
            switch (currentGridType)
            {
                case GridType.Quad:
                    for (int i = 0; i < quadGrid.Count; i++)
                    {

                        if (quadGrid[i] == quadGoal)
                            spriteBatch.Draw(flagTexture, sourceRectangle: flag.sourcRect, destinationRectangle: flag.destRect, color: Color.White);

                        if (quadGrid[i].walls[0])
                            spriteBatch.Draw(blankTexture, new Rectangle(quadGrid[i].topRect.X, quadGrid[i].topRect.Y, quadGrid[i].topRect.Width, quadGrid[i].topRect.Height), Color.Black);
                        if (quadGrid[i].walls[1])
                            spriteBatch.Draw(blankTexture, new Rectangle(quadGrid[i].rightRect.X, quadGrid[i].rightRect.Y, quadGrid[i].rightRect.Width, quadGrid[i].rightRect.Height), Color.Black);
                            if (quadGrid[i].walls[2])
                            spriteBatch.Draw(blankTexture, new Rectangle(quadGrid[i].botRect.X , quadGrid[i].botRect.Y , quadGrid[i].botRect.Width, quadGrid[i].botRect.Height), Color.Black);
                        if (quadGrid[i].walls[3])
                            spriteBatch.Draw(blankTexture, new Rectangle(quadGrid[i].leftRect.X , quadGrid[i].leftRect.Y, quadGrid[i].leftRect.Width, quadGrid[i].leftRect.Height), Color.Black);
                        
                    }
                    break;
                case GridType.Hex:
                    for (int i = 0; i < hexGrid.Count; i++)
                    {
                        if (hexGrid[i] == hexGoal)
                            spriteBatch.Draw(flagTexture, sourceRectangle: flag.sourcRect, destinationRectangle: flag.destRect, color: Color.White);
                        
                        if (hexGrid[i].walls[0])
                            spriteBatch.Draw(
                                destinationRectangle: new Rectangle(
                                    hexGrid[i].topRightRect.X , 
                                    hexGrid[i].topRightRect.Y - (hexGrid[i].topRightRect.Width/4)+ (hexGrid[i].topRightRect.Height/2), 
                                    hexGrid[i].topRightRect.Width, 
                                    hexGrid[i].topRightRect.Height), 
                                rotation: hexGrid[i].topRightRect.Rotation, 
                                texture: blankTexture, 
                                color: Color.Black);
                        if (hexGrid[i].walls[1])
                            spriteBatch.Draw(destinationRectangle: new Rectangle(hexGrid[i].rightRect.X + (hexGrid[i].rightRect.Width/2) , hexGrid[i].rightRect.Y - (hexGrid[i].rightRect.Width/4), hexGrid[i].rightRect.Width, hexGrid[i].rightRect.Height), rotation: hexGrid[i].rightRect.Rotation, texture: blankTexture, color: Color.Black);
                        if (hexGrid[i].walls[2])
                            spriteBatch.Draw(destinationRectangle: new Rectangle(hexGrid[i].botRightRect.X + (hexGrid[i].botRightRect.Width), hexGrid[i].botRightRect.Y - (hexGrid[i].botRightRect.Width / 4), hexGrid[i].botRightRect.Width, hexGrid[i].botRightRect.Height), rotation: hexGrid[i].botRightRect.Rotation, texture: blankTexture, color: Color.Black);
                        if (hexGrid[i].walls[3])
                            spriteBatch.Draw(destinationRectangle: new Rectangle(hexGrid[i].botLeftRect.X + (hexGrid[i].botLeftRect.Width), hexGrid[i].botLeftRect.Y + (hexGrid[i].botLeftRect.Width / 4), hexGrid[i].botLeftRect.Width, hexGrid[i].botLeftRect.Height), rotation: hexGrid[i].botLeftRect.Rotation, texture: blankTexture, color: Color.Black);
                        if (hexGrid[i].walls[4])
                            spriteBatch.Draw(destinationRectangle: new Rectangle(hexGrid[i].leftRect.X + (hexGrid[i].leftRect.Width / 2), hexGrid[i].leftRect.Y + (hexGrid[i].leftRect.Width / 2), hexGrid[i].leftRect.Width, hexGrid[i].leftRect.Height), rotation: hexGrid[i].leftRect.Rotation, texture: blankTexture, color: Color.Black);
                        if (hexGrid[i].walls[5])
                            spriteBatch.Draw(destinationRectangle: new Rectangle(hexGrid[i].topLeftRect.X , hexGrid[i].topLeftRect.Y + (hexGrid[i].topLeftRect.Width / 4), hexGrid[i].topLeftRect.Width, hexGrid[i].topLeftRect.Height), rotation: hexGrid[i].topLeftRect.Rotation, texture: blankTexture, color: Color.Black);
                    }
                    break;
            }


            /*foreach(Sprite sprite in sprites)
            {
                sprite.Draw(spriteBatch);
            }*/
            
            foreach (var powerUp in powerUps)
            {
                powerUp.Draw(spriteBatch);
            }
            foreach (Guidance_Orb orb in guidance_Orb_Queue)
            {
                orb.Draw(spriteBatch);
            }

            player1.Draw(spriteBatch);

            foreach (Opponent opponent in opponents)
            {
                opponent.Draw(spriteBatch);
            }


            //DEBUG 
            /*spriteBatch.Draw(destinationRectangle: new Rectangle(opponent3.boundary.X + (opponent3.boundary.Width / 2), opponent3.boundary.Y + (opponent3.boundary.Height / 2), opponent3.boundary.Width, opponent3.boundary.Height), sourceRectangle: new Rectangle(0, 0, 2, 2), origin: new Vector2(1, 1), rotation: opponent3.boundary.Rotation, texture: blankTexture, color: Color.Purple);
            spriteBatch.Draw(blankTexture, new Rectangle(hexGrid[16].topRightRect.X, hexGrid[16].topRightRect.Y, hexGrid[16].topRightRect.Width, hexGrid[16].topRightRect.Height), Color.Red);
            spriteBatch.Draw(blankTexture, new Rectangle(new Point(hexGrid[16].topRightRect.X + (int)hexGrid[16].topRightRect.Origin.X, hexGrid[16].topRightRect.Y+ (int)hexGrid[16].topRightRect.Origin.Y),new Point(2)),Color.Purple);
            spriteBatch.Draw(blankTexture, new Rectangle(new Point(player1.boundary.X + (int)(player1.boundary.Width / 2), player1.boundary.Y + (int)(player1.boundary.Height / 2)), new Point(10)), Color.Aquamarine);
            spriteBatch.Draw(texture: blankTexture, destinationRectangle: quadGrid[player1.CurrentADJPos - 1].topRect, color: Color.Purple);
            for (int i = 0; i < hexGrid.Count; i++)
            {


                //if ((player1.boundary.Intersects(hexGrid[i].topRightRect) && hexGrid[i].walls[0]) || (player1.boundary.Intersects(hexGrid[i].rightRect) && hexGrid[i].walls[1]) || (player1.boundary.Intersects(hexGrid[i].botRightRect) && hexGrid[i].walls[2]) || (player1.boundary.Intersects(hexGrid[i].botLeftRect) && hexGrid[i].walls[3]) || (player1.boundary.Intersects(hexGrid[i].leftRect) && hexGrid[i].walls[4]) || (player1.boundary.Intersects(hexGrid[i].topLeftRect) && hexGrid[i].walls[5]))
                if (player1.boundary.Intersects(hexGrid[i].topLeftRect)&&hexGrid[i].walls[5])
                {
                    spriteBatch.Draw(blankTexture, new Rectangle(100, 100, 100, 100), Color.Red);
                }
            }*/




        }
        public void TitleDraw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(timerFont, "Maze Game", new Vector2(graphics.PreferredBackBufferWidth / 2 - 450, graphics.PreferredBackBufferHeight / 2 - 100), Color.Purple);
            spriteBatch.DrawString(menuFont, "Press esc to exit", new Vector2(graphics.PreferredBackBufferWidth / 2 - 100, graphics.PreferredBackBufferHeight / 2 + 200), Color.Purple);

        }
        public void MenuDraw(SpriteBatch spriteBatch, List<Button> buttons)
        {
            foreach (Button button in buttons)
            {
                button.Draw(spriteBatch, previousMouse);
            }
        }
        

        public void ResetQuadGrid()
        {
            Columns = nextGridSize;
            cellWidth = (int)(graphics.PreferredBackBufferWidth / Columns);
            Rows = (int)(graphics.PreferredBackBufferHeight / cellWidth);

            quadGrid = GenerateGrid(Columns, Rows, quadGrid);                                   //Resets the grid
            quadGrid[0].ResetGrid(quadGrid, Columns, Rows);
            quadGoal = quadGrid[0].GenerateMaze(quadGrid, Columns, Rows, rnd);
            ADJlist = quadGrid[0].ADJlist;

            player1.Reset(cellWidth,currentGridType.ToString());                                         //Resets all the players and opponents
            opponent1.Reset(ADJlist, quadGoal, cellWidth, ScreenWidth);
            opponent2.Reset(ADJlist, quadGoal, cellWidth, ScreenWidth);
            opponent3.Reset(ADJlist, quadGoal, cellWidth, ScreenWidth);
            #region flag reset
            flag.Reset(cellWidth);
            flag.Position = new Vector2(quadGoal.GridPos.X * cellWidth + (cellWidth / 5), quadGoal.GridPos.Y * cellWidth + (cellWidth / 5));
            flag.sourcRect = new Rectangle(0, 0, 150, 167);
            flag.destRect = new Rectangle((int)flag.Position.X, (int)flag.Position.Y, cellWidth / 2, cellWidth / 2);
            flag.CurrentADJPos = quadGoal.ADJlistPos;

            #endregion
            #region powerups reset
            guidance_Orb_Queue.Clear();
            powerUps.Clear();
            foreach (var Cell in quadGrid)
            {
                if (Cell.Occupied)
                {
                    powerUps.Add(new PowerUp(gemTextures, Cell.GridPos, cellWidth, 3, ScreenWidth, currentGridType.ToString(), rnd));
                }
            }
            #endregion
        }
        public void ResetHexGrid()
        {
            Columns = nextGridSize;
            cellWidth = (int)(graphics.PreferredBackBufferWidth / Columns);

            cellWidth = (int)((graphics.PreferredBackBufferWidth - (cellWidth / 2)) / Columns);
            Rows = (int)((2 * Math.Sqrt(3) * graphics.PreferredBackBufferHeight - cellWidth) / (3 * cellWidth)); //Comes from Height = Number of cells - (Number of Cells - 1) * Height of Upper Hex Triangle

            hexGrid = GenerateGrid(Columns, Rows, hexGrid);                                    //Resets the grid
            hexGrid[0].ResetGrid(hexGrid, Columns, Rows);
            hexGoal = hexGrid[0].GenerateMaze(hexGrid, Columns, Rows, rnd);
            ADJlist = hexGrid[0].ADJlist;

            player1.Reset(cellWidth,currentGridType.ToString());                                         //Resets all the players and opponents
            opponent1.Reset(ADJlist, hexGoal, cellWidth, ScreenWidth);
            opponent2.Reset(ADJlist, hexGoal, cellWidth, ScreenWidth);
            opponent3.Reset(ADJlist, hexGoal, cellWidth, ScreenWidth);
            #region flag reset
            flag.Reset(cellWidth);

            if (hexGoal.GridPos.Y % 2 == 0)
                flag.Position = new Vector2(((int)(cellWidth * hexGoal.GridPos.X) + (cellWidth / 4)), (int)(cellWidth / Math.Sqrt(3)) * (1 + 1.5f * (float)(hexGoal.GridPos.Y)) - (cellWidth / 4)); //Prev Y: (int)Scale * (int)(NextADJPos / (768 / Scale)) + (int)yOffset)
            else
                flag.Position = new Vector2(((int)(cellWidth * hexGoal.GridPos.X)) + (3 * cellWidth / 4), (int)(cellWidth / Math.Sqrt(3)) * (1 + 1.5f * (float)(hexGoal.GridPos.Y)) - (cellWidth / 4)); //Prev Y: (int)Scale * (int)(NextADJPos / (768 / Scale)) + (int)yOffset)

            flag.destRect = new Rectangle((int)flag.Position.X, (int)flag.Position.Y, cellWidth / 2, cellWidth / 2);
            flag.CurrentADJPos = hexGoal.ADJlistPos;
            #endregion
            #region powerups reset
            guidance_Orb_Queue.Clear();
            powerUps.Clear();
            foreach (var hexCell in hexGrid)
            {
                if (hexCell.Occupied)
                {
                    powerUps.Add(new PowerUp(gemTextures, hexCell.GridPos, cellWidth, 3, ScreenWidth, currentGridType.ToString(), rnd));
                }
            }
            #endregion
        }
        public List<Cell> GenerateGrid(int cols, int rows, List<Cell> quadGrid)
        {
            int c = 1;

            quadGrid = new List<Cell>();
            for (int j = 0; j < rows; j++)
            {
                for (int i = 0; i < cols; i++)
                {
                    Cell cell = new Cell(i, j, cellWidth, blankTexture, graphics.PreferredBackBufferHeight, rnd)
                    {
                        ADJlistPos = c
                    };
                    quadGrid.Add(cell);
                    c += 1;
                }
            }

            return quadGrid;
        }
        public List<Hex_Cell> GenerateGrid(int cols, int rows, List<Hex_Cell> hexGrid)
        {
            int c = 1;
            
            hexGrid = new List<Hex_Cell>();
            for (int j = 0; j < rows; j++)
            {
                for (int i = 0; i < cols; i++)
                {
                    Hex_Cell cell = new Hex_Cell(i, j, cellWidth, blankTexture, graphics.PreferredBackBufferHeight, rnd)
                    {
                        ADJlistPos = c
                    };

                    hexGrid.Add(cell);
                    c += 1;
                }
            }
            //return grid;
            return hexGrid;
        }
        public bool PowerUpCheck(Player player, PowerUp powerUp)
        {
            if (powerUp.Boundary.Intersects(player.boundary))
            {
                switch (powerUp.CurrentType)
                {
                    case PowerUp.PowerUpTypes.Teleport:
                        switch (rnd.Next(0, 3))
                        {
                            case 0:
                                SwitchPlace(player, opponent1);
                                break;
                            case 1:
                                SwitchPlace(player, opponent2);
                                break;
                            case 2:
                                SwitchPlace(player, opponent3);
                                break;
                        }

                        powerUps.Remove(powerUp);
                        return true;
                    case PowerUp.PowerUpTypes.Speed:
                        if (!player.speedBoostBool)                 //Stops speed boosts from stacking
                            player.LinearVelocity *= 1.5f;
                        player.speedBoostBool = true;
                        player.boostElapsed = 0f;

                        powerUps.Remove(powerUp);
                        return true;
                    case PowerUp.PowerUpTypes.Minion:
                        break;
                    case PowerUp.PowerUpTypes.GuidenceOrb:
                        if (!player.guidenceBool)
                        {
                            guidance_Orb_Queue.Enqueue(new Guidance_Orb(blankTexture, cellWidth, ScreenWidth, currentGridType.ToString(), ADJlist, player.CurrentADJPos, 0, flag));
                            player.guidenceBool = true;
                        }
                        player.guidenceElapsed = 0f;
                        powerUps.Remove(powerUp);
                        
                        return true;
                }
            }
            return false;
        }
        public bool PowerUpCheck(Opponent opponent, PowerUp powerUp)
        {
            if (powerUp.Boundary.Intersects(opponent.boundary))
            {
                switch (powerUp.CurrentType)
                {
                    case PowerUp.PowerUpTypes.Teleport:

                        int r;
                        do
                        {
                            r = rnd.Next(0, 3);
                        } while (opponent == opponents[r]);

                        if (rnd.Next(0, 4) == 0)
                            SwitchPlace(player1, opponent);
                        else
                            SwitchPlace(opponent, opponents[r]);


                        powerUps.Remove(powerUp);

                        return true;
                        case PowerUp.PowerUpTypes.Speed:
                            if (!opponent.speedBoostBool)                 //Stops speed boosts from stacking
                                opponent.LinearVelocity *= 1.5f;
                            opponent.speedBoostBool = true;
                            opponent.boostElapsed = 0f;
                            powerUps.Remove(powerUp);

                            return true;
                        case PowerUp.PowerUpTypes.Minion:
                            break;
                        case PowerUp.PowerUpTypes.GuidenceOrb:
                            if (!opponent.guidenceBool)
                            {
                                guidance_Orb_Queue.Enqueue(new Guidance_Orb(blankTexture, cellWidth, ScreenWidth, currentGridType.ToString(), ADJlist, opponent.CurrentADJPos, opponent.Difficulty, flag));
                                opponent.guidenceBool = true;
                            }
                            opponent.guidenceElapsed = 0f;
                            powerUps.Remove(powerUp);
                            
                            break;
                }
                powerUps.Remove(powerUp);
                return true;
            }
            return false;
        }

        public void SwitchPlace(Player Collider, Opponent Target)
        {
            Collider.Position = Target.Position;
            Target.CurrentADJPos = Collider.CurrentADJPos;

            if (currentGridType == GridType.Quad)
            {
                if (Collider.CurrentADJPos % Columns != 0)            //This stops the end column from acting like it is the first column     
                {
                    Target.Position = new Vector2(((int)(cellWidth * (Collider.CurrentADJPos % Columns) - (cellWidth / 2))), (int)cellWidth * (int)(Collider.CurrentADJPos / Columns) + (cellWidth / 2));
                }
                else
                {
                    Target.Position = new Vector2(((int)(cellWidth * (Columns) - (cellWidth / 2))), (int)(cellWidth * (int)(Collider.CurrentADJPos / Columns)) - (cellWidth / 2));
                }

            }
            else //currentGridType is Hex
            {
                int xOffset = 0;
                int yOffset = (int)(cellWidth / Math.Sqrt(3));

                if (Math.Floor(Collider.CurrentADJPos / (decimal)Columns) % 2 != 0 && Collider.CurrentADJPos % Columns != 0)   //Checks to see if its on an odd or even row
                    xOffset = cellWidth / 2;
                else if (Collider.CurrentADJPos % Columns == 0 && Math.Floor(Collider.CurrentADJPos / (decimal)Columns) % 2 == 0)
                    xOffset = cellWidth / 2;

                if (Collider.CurrentADJPos % Columns != 0)            //This stops the end column from acting like it is the first column     
                    Target.Position = new Vector2(((int)(cellWidth * (Collider.CurrentADJPos % Columns) - (cellWidth / 2) + xOffset)), (int)yOffset * (1 + 3 * (float)Math.Floor(Collider.CurrentADJPos / (decimal)Columns / 2) + (((int)(Collider.CurrentADJPos / Columns) % 2) * 1.5f)));
                else
                    Target.Position = new Vector2(((int)(cellWidth * (Columns) - (cellWidth / 2) + xOffset)), (int)yOffset * (1 + 3 * (float)Math.Floor((Collider.CurrentADJPos / (decimal)Columns - 1) / 2) + (((int)(Collider.CurrentADJPos / Columns - 1) % 2) * 1.5f)));

            }

            switch (Target.Difficulty)
            {
                case 1:

                    break;
                case 2:

                    if (currentGridType == GridType.Quad)
                    {
                        Target.PathQueue = Target.HoldLeft(ADJlist, Target.CurrentADJPos, quadGoal.ADJlistPos);
                    }
                    else //currentGridType is Hex
                    {
                        Target.PathQueue = Target.HoldLeft(ADJlist, Target.CurrentADJPos, hexGoal.ADJlistPos);
                    }

                    break;
                case 3:

                    if (currentGridType == GridType.Quad)
                    {
                        Target.PathQueue = Target.ShortestPath(ADJlist, Target.CurrentADJPos, quadGoal.ADJlistPos);
                    }
                    else //currentGridType is Hex
                    {
                        Target.PathQueue = Target.ShortestPath(ADJlist, Target.CurrentADJPos, hexGoal.ADJlistPos);
                    }

                    break;

            }
            Target.nextDirection = Target.NextPosition();
            Target.Direction = new Vector2((float)Math.Cos(Target.nextDirection), (float)Math.Sin(Target.nextDirection));
            if (currentGridType == GridType.Quad)
                Target.NextADJPos = ADJlist[Target.CurrentADJPos, (int)(Math.Round((Target.nextDirection / Math.PI) * 2) + 2)];
            else //currentGridType is Hex
                Target.NextADJPos = ADJlist[Target.CurrentADJPos, (int)(Math.Round((Target.nextDirection / Math.PI) * 3) + 2)];

        }
        public void SwitchPlace(Opponent Collider, Opponent Target)
        {
            int temp = Collider.CurrentADJPos;
            Collider.CurrentADJPos = Target.CurrentADJPos;
            Target.CurrentADJPos = temp;

            if (currentGridType == GridType.Quad)
            {
                if (Target.CurrentADJPos % Columns != 0)            //This stops the end column from acting like it is the first column     
                {
                    Target.Position = new Vector2(((int)(cellWidth * (Target.CurrentADJPos % Columns) - (cellWidth / 2))), (int)cellWidth * (int)(Target.CurrentADJPos / Columns) + (cellWidth / 2));
                }
                else
                {
                    Target.Position = new Vector2(((int)(cellWidth * (Columns) - (cellWidth / 2))), (int)(cellWidth * (int)(Target.CurrentADJPos / Columns)) - (cellWidth / 2));
                }

                if (Collider.CurrentADJPos % Columns != 0)            //This stops the end column from acting like it is the first column     
                {
                    Collider.Position = new Vector2(((int)(cellWidth * (Collider.CurrentADJPos % Columns) - (cellWidth / 2))), (int)cellWidth * (int)(Collider.CurrentADJPos / Columns) + (cellWidth / 2));
                }
                else
                {
                    Collider.Position = new Vector2(((int)(cellWidth * (Columns) - (cellWidth / 2))), (int)(cellWidth * (int)(Collider.CurrentADJPos / Columns)) - (cellWidth / 2));
                }

            }
            else //currentGridType is Hex
            {
                int xOffset = 0;
                int yOffset = (int)(cellWidth / Math.Sqrt(3));

                if (Math.Floor(Target.CurrentADJPos / (decimal)Columns) % 2 != 0 && Target.CurrentADJPos % Columns != 0)   //Checks to see if its on an odd or even row
                    xOffset = cellWidth / 2;
                else if (Target.CurrentADJPos % Columns == 0 && Math.Floor(Target.CurrentADJPos / (decimal)Columns) % 2 == 0)
                    xOffset = cellWidth / 2;

                if (Target.CurrentADJPos % Columns != 0)            //This stops the end column from acting like it is the first column     
                    Target.Position = new Vector2(((int)(cellWidth * (Target.CurrentADJPos % Columns) - (cellWidth / 2) + xOffset)), (int)yOffset * (1 + 3 * (float)Math.Floor(Target.CurrentADJPos / (decimal)Columns / 2) + (((int)(Target.CurrentADJPos / Columns) % 2) * 1.5f)));
                else
                    Target.Position = new Vector2(((int)(cellWidth * (Columns) - (cellWidth / 2) + xOffset)), (int)yOffset * (1 + 3 * (float)Math.Floor((Target.CurrentADJPos / (decimal)Columns - 1) / 2) + (((int)(Target.CurrentADJPos / Columns - 1) % 2) * 1.5f)));

                xOffset = 0;

                if (Math.Floor(Collider.CurrentADJPos / (decimal)Columns) % 2 != 0 && Collider.CurrentADJPos % Columns != 0)   //Checks to see if its on an odd or even row
                    xOffset = cellWidth / 2;
                else if (Collider.CurrentADJPos % Columns == 0 && Math.Floor(Collider.CurrentADJPos / (decimal)Columns) % 2 == 0)
                    xOffset = cellWidth / 2;

                if (Collider.CurrentADJPos % Columns != 0)            //This stops the end column from acting like it is the first column     
                    Collider.Position = new Vector2(((int)(cellWidth * (Collider.CurrentADJPos % Columns) - (cellWidth / 2) + xOffset)), (int)yOffset * (1 + 3 * (float)Math.Floor(Collider.CurrentADJPos / (decimal)Columns / 2) + (((int)(Collider.CurrentADJPos / Columns) % 2) * 1.5f)));
                else
                    Collider.Position = new Vector2(((int)(cellWidth * (Columns) - (cellWidth / 2) + xOffset)), (int)yOffset * (1 + 3 * (float)Math.Floor((Collider.CurrentADJPos / (decimal)Columns - 1) / 2) + (((int)(Collider.CurrentADJPos / Columns - 1) % 2) * 1.5f)));

            }

            switch (Target.Difficulty)
            {
                case 1:

                    break;
                case 2:

                    if (currentGridType == GridType.Quad)
                    {
                        Target.PathQueue = Target.HoldLeft(ADJlist, Target.CurrentADJPos, quadGoal.ADJlistPos);
                    }
                    else //currentGridType is Hex
                    {
                        Target.PathQueue = Target.HoldLeft(ADJlist, Target.CurrentADJPos, hexGoal.ADJlistPos);
                    }

                    break;
                case 3:

                    if (currentGridType == GridType.Quad)
                    {
                        Target.PathQueue = Target.ShortestPath(ADJlist, Target.CurrentADJPos, quadGoal.ADJlistPos);
                    }
                    else //currentGridType is Hex
                    {
                        Target.PathQueue = Target.ShortestPath(ADJlist, Target.CurrentADJPos, hexGoal.ADJlistPos);
                    }

                    break;

            }
            Target.nextDirection = Target.NextPosition();
            Target.Direction = new Vector2((float)Math.Cos(Target.nextDirection), (float)Math.Sin(Target.nextDirection));
            if (currentGridType == GridType.Quad)
                Target.NextADJPos = ADJlist[Target.CurrentADJPos, (int)(Math.Round((Target.nextDirection / Math.PI) * 2) + 2)];
            else //currentGridType is Hex
                Target.NextADJPos = ADJlist[Target.CurrentADJPos, (int)(Math.Round((Target.nextDirection / Math.PI) * 3) + 2)];

            switch (Collider.Difficulty)
            {
                case 1:

                    break;
                case 2:

                    if (currentGridType == GridType.Quad)
                    {
                        Collider.PathQueue = Collider.HoldLeft(ADJlist, Collider.CurrentADJPos, quadGoal.ADJlistPos);
                    }
                    else //currentGridType is Hex
                    {
                        Collider.PathQueue = Collider.HoldLeft(ADJlist, Collider.CurrentADJPos, hexGoal.ADJlistPos);
                    }

                    break;
                case 3:

                    if (currentGridType == GridType.Quad)
                    {
                        Collider.PathQueue = Collider.ShortestPath(ADJlist, Collider.CurrentADJPos, quadGoal.ADJlistPos);
                    }
                    else //currentGridType is Hex
                    {
                        Collider.PathQueue = Collider.ShortestPath(ADJlist, Collider.CurrentADJPos, hexGoal.ADJlistPos);
                    }

                    break;

            }
            Collider.nextDirection = Collider.NextPosition();
            Collider.Direction = new Vector2((float)Math.Cos(Collider.nextDirection), (float)Math.Sin(Collider.nextDirection));
            if (currentGridType == GridType.Quad)
                Collider.NextADJPos = ADJlist[Collider.CurrentADJPos, (int)(Math.Round((Collider.nextDirection / Math.PI) * 2) + 2)];
            else //currentGridType is Hex
                Collider.NextADJPos = ADJlist[Collider.CurrentADJPos, (int)(Math.Round((Collider.nextDirection / Math.PI) * 3) + 2)];

        }


    }
}
