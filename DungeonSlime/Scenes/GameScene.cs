using System;
using DungeonSlime.UI;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using Gum.Forms.Controls;
using MonoGameGum.GueDeriving;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;

namespace DungeonSlime.Scenes;

public class GameScene : Scene
{
    // Defines the slime sprite
    private AnimatedSprite _slime;

    // Define the bat sprite
    private AnimatedSprite _bat;

    // Tracks the position of the slime
    private Vector2 _slimePosition;

    // Speed multiplier when moving
    private const float MOVEMENT_SPEED = 5.0f;

    // Tracks the position of the bat
    private Vector2 _batPosition;

    // Tracks the velocity of the bat
    private Vector2 _batVelocity;

    // defines the tilemap to draw
    private Tilemap _tilemap;

    // Defines the bounds of the room that the slime and bat are contained within
    private Rectangle _roomBounds;

    // The sound effect to play when the bat bounces off the edge of the screen
    private SoundEffect _bounceSoundEffect;
    
    // The sound effect to play when the slime eats a bat
    private SoundEffect _collectSoundEffect;

    // The SpriteFont Description used to draw text.
    private SpriteFont _font;

    // Tracks the players score
    private int _score;

    // Defines the position to draw the score text at.
    private Vector2 _scoreTextPosition;

    // Defines the origin used when drawing the score text
    private Vector2 _scoreTextOrigin;

    private Panel _pausePanel;

    private AnimatedButton _resumeButton;
    private SoundEffect _uiSoundEffect;

    // Reference to the texture atlas that we can pass to UI elements when they are created
    private TextureAtlas _atlas;

    private void PauseGame()
    {
        // Make the pause UI element visible
        _pausePanel.IsVisible = true;

        // set the resume button to have focus
        _resumeButton.IsFocused = true;
    }

    private void CreatePausePanel()
    {
        _pausePanel = new Panel();
        _pausePanel.Anchor(Anchor.Center);
        _pausePanel.Visual.WidthUnits = DimensionUnitType.Absolute;
        _pausePanel.Visual.HeightUnits = DimensionUnitType.Absolute;
        _pausePanel.Visual.Height = 70;
        _pausePanel.Visual.Width = 264;
        _pausePanel.IsVisible = false;
        _pausePanel.AddToRoot();

        TextureRegion backgroundRegion = _atlas.GetRegion("panel-background");

        NineSliceRuntime background = new NineSliceRuntime();
        background.Dock(Dock.Fill);
        background.Texture = backgroundRegion.Texture;
        background.TextureAddress = TextureAddress.Custom;
        background.TextureHeight = backgroundRegion.Height;
        background.TextureLeft = backgroundRegion.SourceRectangle.Left;
        background.TextureTop = backgroundRegion.SourceRectangle.Top;
        background.TextureWidth = backgroundRegion.Width;
        _pausePanel.AddChild(background);
      
        var textInstance = new TextRuntime();
        textInstance.Text = "PAUSED";
        textInstance.CustomFontFile = @"fonts/04b_30.fnt";
        textInstance.UseCustomFont = true;
        textInstance.FontScale = 0.5f;
        textInstance.X = 10f;
        textInstance.Y = 10f;
        _pausePanel.AddChild(textInstance);

        _resumeButton = new AnimatedButton(_atlas);
        _resumeButton.Text = "RESUME";
        _resumeButton.Anchor(Anchor.BottomLeft);
        _resumeButton.Visual.X = 9f;
        _resumeButton.Visual.Y = -9f;
        _resumeButton.Visual.Width = 80;
        _resumeButton.Click += HandleResumeButtonClicked;
        _pausePanel.AddChild(_resumeButton);

        var quitButton = new AnimatedButton(_atlas);
        quitButton.Text = "QUIT";
        quitButton.Anchor(Anchor.BottomRight);
        quitButton.Visual.X = -9f;
        quitButton.Visual.Y = -9f;        
        quitButton.Click += HandleQuitButtonClicked;

        _pausePanel.AddChild(quitButton);
    }

    private void HandleResumeButtonClicked(object sender, EventArgs e)
    {
        // A UI interaction occured, play the sound effect
        Core.Audio.PlaySoundEffect(_uiSoundEffect);

        // Make the pause panel invisible to result the game
        _pausePanel.IsVisible = false;
    }

    private void HandleQuitButtonClicked(object sender, EventArgs e)
    {
        // A UI interaction occured, play the sound effect
        Core.Audio.PlaySoundEffect(_uiSoundEffect);

        // Go back to title scene
        Core.ChangeScene(new TitleScene());
    }

    private void InitializeUI()
    {
        GumService.Default.Root.Children.Clear();
        CreatePausePanel();
    }


    public override void Initialize()
    {
        base.Initialize();

        Core.ExitOnEscape = false;
        
        Rectangle screenBounds =  Core.GraphicsDevice.PresentationParameters.Bounds;

        _roomBounds = new Rectangle(
            (int)_tilemap.TileWidth,
            (int)_tilemap.TileHeight,
            screenBounds.Width - (int)_tilemap.TileWidth * 2,
            screenBounds.Height - (int)_tilemap.TileHeight * 2
        );

        // Initial slime position will be the center tile of the tilemap
        int centerRow = _tilemap.Rows / 2;
        int centerColumn = _tilemap.Columns / 2;
        _slimePosition = new Vector2(centerColumn * _tilemap.TileWidth, centerRow * _tilemap.TileHeight);

        // Initial bat position will be in the top left corner of the room
        _batPosition = new Vector2(_roomBounds.Left, _roomBounds.Top);
        
        // Set the position of the score text to align to the left edge of the
        // room bounds, and to vertically be at the center of the first tile.
        _scoreTextPosition = new Vector2(_roomBounds.Left, _tilemap.TileHeight * 0.5f);

        // Set the origin of the text so it is left-centered
        float _scoreTextYOrigin = _font.MeasureString("Score").Y * 0.5f;
        _scoreTextOrigin = new Vector2(0, _scoreTextYOrigin);

        // Assign the initial random velocity to the bat
        AssignRandomBatVelocity();   

        InitializeUI();    
    }

    public override void LoadContent()
    {
        _atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");

        // Create the slime sprite from the atlas
        _slime = _atlas.CreateAnimatedSprite("slime-animation");
        _slime.Scale = new Vector2(4.0f, 4.0f);

        // Create the bat sprite from the atlas
        _bat = _atlas.CreateAnimatedSprite("bat-animation");
        _bat.Scale = new Vector2(4.0f, 4.0f);

        // Set the initial position of the bat to be 10px to the right of the slime
        //_batPosition = new Vector2(_slime.Width + 10, 0);
        
       // Create the tilemap from the XML configuration file.
        _tilemap = Tilemap.FromFile(Content, "images/tilemap-definitions.xml");
        _tilemap.Scale = new Vector2(4.0f, 4.0f);

        // Load the bounce sound effect
        _bounceSoundEffect = Content.Load<SoundEffect>("audio/bounce");

        // Load the colect sound effect
        _collectSoundEffect = Content.Load<SoundEffect>("audio/collect");

        // Load the font
        _font = Content.Load<SpriteFont>("fonts/04B_30");

        // Load the sound effect to play when ui actions occur
        _uiSoundEffect = Core.Content.Load<SoundEffect>("audio/ui");
    }

    public override void Update(GameTime gameTime)
    {        
        // Ensure the UI is always updated
        GumService.Default.Update(gameTime);

        // if the game is paused, do not continue
        if(_pausePanel.IsVisible)
        {
            return;
        }

        _slime.Update(gameTime);
        _bat.Update(gameTime);

        // Check for keyboard input and handle it.
        CheckKeyboardInput();

        // Check for gamepad input and handle it.
        CheckGamePadInput();

        // Creating a bound circle for the slime
        Circle slimeBounds = new Circle(
            (int)(_slimePosition.X + (_slime.Width * 0.5f)),
            (int)(_slimePosition.Y + (_slime.Height * 0.5f)),
            (int)(_slime.Width * 0.5f)
        );

        // Use distance based checks to determine if the slime is within the
        // bounds of the game screen, and if it is outside that screen edge
        // move it back inside
        if(slimeBounds.Left < _roomBounds.Left)
        {
            _slimePosition.X = _roomBounds.Left;
        }
        else if(slimeBounds.Right > _roomBounds.Right)
        {
            _slimePosition.X = _roomBounds.Right - _slime.Width;
        }

        if(slimeBounds.Top < _roomBounds.Top)
        {
            _slimePosition.Y = _roomBounds.Top;
        }
        else if(slimeBounds.Bottom > _roomBounds.Bottom)
        {
            _slimePosition.Y = _roomBounds.Bottom - _slime.Height;
        }

        // Calculate the new position of the bat based on the velocity
        Vector2 newBatPosition = _batPosition + _batVelocity;

        // Create a bounding circle for the bat
        Circle batBounds = new Circle(
            (int)(newBatPosition.X + (_bat.Width * 0.5f)),
            (int)(newBatPosition.Y + (_bat.Height * 0.5f)),
            (int)(_bat.Width * 0.5f)
        );

        Vector2 normal = Vector2.Zero;

        // Use distance based checks to determine if the bat is within the
        // bounds of the game screen, and if it is outside that screen edge,
        // reflect it about the screen edge normal
        if(batBounds.Left < _roomBounds.Left)
        {
            normal.X = Vector2.UnitX.X;
            newBatPosition.X = _roomBounds.Left;
        }
        else if(batBounds.Right > _roomBounds.Right)
        {
            normal.X = -Vector2.UnitX.X;
            newBatPosition.X = _roomBounds.Right - _bat.Width;
        }

        if(batBounds.Top < _roomBounds.Top)
        {
            normal.Y = Vector2.UnitY.Y;
            newBatPosition.Y = _roomBounds.Top;
        }
        else if(batBounds.Bottom > _roomBounds.Bottom)
        {
            normal.Y = -Vector2.UnitY.Y;
            newBatPosition.Y = _roomBounds.Bottom - _bat.Height;
        }

        // If the normal is anything but Vector2.Zero, this means the bat had
        // moved outside the screen edge so we should reflect it about the normal
        if(normal != Vector2.Zero)
        {
            normal.Normalize();
            _batVelocity = Vector2.Reflect(_batVelocity, normal);

            // Play the bounce soud effect
            Core.Audio.PlaySoundEffect(_bounceSoundEffect);
        }

        _batPosition = newBatPosition;

        if(slimeBounds.Intersects(batBounds))
        {
            // Choose a random row and column based on the total number of each
            int column = Random.Shared.Next(1, _tilemap.Columns -1);
            int row = Random.Shared.Next(1, _tilemap.Rows -1);

            // Change the bat position by setting the x and y values equal to
            // the column and row multiplied by the width and height
            _batPosition = new Vector2(column * _bat.Width, row * _bat.Height);

            // Assign a new Random velocity to the bat
            AssignRandomBatVelocity();

            // Play the collect sound effect
            Core.Audio.PlaySoundEffect(_collectSoundEffect);

            // Increase the player's score
            _score += 100;
        }

        base.Update(gameTime);
    }

    private void CheckKeyboardInput()
    {  
        // get ta reference to the keyboard
        KeyboardInfo keyboard = Core.Input.Keyboard;

        // If the escape key is pressed, return to the title screen
        if(Core.Input.Keyboard.WasKeyJustPressed(Keys.Escape))
        {
            //Core.ChangeScene(new TitleScene());
            PauseGame();
        }

        // if the space key is held down, the movement speed increases with 1.5
        float speed = MOVEMENT_SPEED;
        if (keyboard.IsKeyDown(Keys.Space))
        {
            speed *= 1.5f;
        }

        // if the W or Up keys are down, move the slime up on the screen
        if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
        {
            _slimePosition.Y -= speed;
        }

        // if the S or Down keys are down, move the slime down on the screen
        if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
        {
            _slimePosition.Y += speed;
        }

        // if the A or Left keys are down, move the slime left on the screen
        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
        {
            _slimePosition.X -= speed;
        }

        // if the D or Right keys are down, move the slime right on the screen
        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
        {
            _slimePosition.X += speed;
        }

        // If the M key is pressed, toggle mute state for audio
        if(keyboard.WasKeyJustPressed(Keys.M))
        {
            Core.Audio.ToggleMute();
        }

        // if the + button is pressed, increase the volume
        if(keyboard.WasKeyJustPressed(Keys.OemPlus))
        {
            Core.Audio.SongVolume += 0.1f;
            Core.Audio.SoundEffectVolume += 0.1f;
        }

        // if the - button is pressed, decrease the volume
        if(keyboard.WasKeyJustPressed(Keys.OemMinus))
        {
            Core.Audio.SongVolume -= 0.1f;
            Core.Audio.SoundEffectVolume -= 0.1f;
        }

    }
    
    private void CheckGamePadInput()
    {
        GamePadInfo gamePadOne = Core.Input.GamePads[(int)PlayerIndex.One];

        if(gamePadOne.WasButtonJustPressed(Buttons.Start))
        {
            PauseGame();
        }

        // if A button is held down, the movement speed increases by 1.5
        // and the gamepad vibrates as feedback to the player
        float speed = MOVEMENT_SPEED;
        if (gamePadOne.IsButtonDown(Buttons.A))
        {
            speed *= 1.5f;
            gamePadOne.SetVibration(1.0f, TimeSpan.FromSeconds(1));
        }
        else
        {
            gamePadOne.StopVibration();
        }

        // CHeck thumbstick first since it has priority over which gamepad input
        // is movement.  It has priority since the thumbstick values provide a
        // more granular analog value that can be used for movement
        if (gamePadOne.LeftThumbStick != Vector2.Zero)
        {
            _slimePosition.X += gamePadOne.LeftThumbStick.X * speed;
            _slimePosition.Y -= gamePadOne.LeftThumbStick.Y * speed;
        }
        else
        {
            if (gamePadOne.IsButtonDown(Buttons.DPadUp))
            {
                _slimePosition.Y -= speed;
            }

            if (gamePadOne.IsButtonDown(Buttons.DPadDown))
            {
                _slimePosition.Y += speed;
            }

            if (gamePadOne.IsButtonDown(Buttons.DPadLeft))
            {
                _slimePosition.X -= speed;
            }
            
            if(gamePadOne.IsButtonDown(Buttons.DPadRight))
            {
                _slimePosition.X += speed;
            }
        }
    }

    public void AssignRandomBatVelocity()
    {
        // Generate a random angle
        float angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);

        // convert angle to direction vector
        float x = (float)Math.Cos(angle);
        float y = (float)Math.Sin(angle);
        Vector2 direction = new Vector2(x, y);

        // Multiply the direction vector by the movement speed
        _batVelocity = direction * MOVEMENT_SPEED;
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.CornflowerBlue);

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw the tilemap
        _tilemap.Draw(Core.SpriteBatch);

        // draw the slime texture region at a scale of 4.0
        _slime.Draw(Core.SpriteBatch, _slimePosition);
        
        // draw the bat texture region 10px to the right of the slime
        _bat.Draw(Core.SpriteBatch, _batPosition);

        // draw the score
        Core.SpriteBatch.DrawString(
            _font,     // spritefont
            $"Score: {_score}",   // text
            _scoreTextPosition,    // position
            Color.White,          // color
            0.0f,                  // rotation
            _scoreTextOrigin,     // origin
            1.0f,                 // scale
            SpriteEffects.None,   // effect
            0.0f                  // layerDepth
        );

        // Always end the sprite batch when finished
        Core.SpriteBatch.End();

        // Draw the Gum UI
        GumService.Default.Draw();       
    }
}
