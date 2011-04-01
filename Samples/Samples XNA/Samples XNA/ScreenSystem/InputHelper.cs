﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace FarseerPhysics.SamplesFramework
{
    /// <summary>
    ///   an enum of all available mouse buttons.
    /// </summary>
    public enum MouseButtons
    {
        LeftButton,
        MiddleButton,
        RightButton,
        ExtraButton1,
        ExtraButton2
    }

    public class InputHelper
    {
        private readonly List<GestureSample> _gestures = new List<GestureSample>();
        private GamePadState _currentGamePadState;
        private KeyboardState _currentKeyboardState;
        private MouseState _currentMouseState;
        private GamePadState _currentVirtualState;

        private GamePadState _lastGamePadState;
        private KeyboardState _lastKeyboardState;
        private MouseState _lastMouseState;
        private GamePadState _lastVirtualState;

        private Vector2 _cursor;
        private bool _cursorIsValid;
        private bool _cursorIsVisible;
        private bool _cursorMoved;
        private Sprite _cursorSprite;
        private Sprite _socketSprite;
        private Sprite _stickSprite;
        private Texture2D _texButtons;
        private Vector2 _buttonOrigin;

        private ScreenManager _manager;
        private Viewport _viewport;

        /// <summary>
        ///   Constructs a new input state.
        /// </summary>
        public InputHelper(ScreenManager manager)
        {
            _currentKeyboardState = new KeyboardState();
            _currentGamePadState = new GamePadState();
            _currentMouseState = new MouseState();
            _currentVirtualState = new GamePadState();

            _lastKeyboardState = new KeyboardState();
            _lastGamePadState = new GamePadState();
            _lastMouseState = new MouseState();
            _lastVirtualState = new GamePadState();

            _manager = manager;

            _cursorIsVisible = false;
            _cursorMoved = false;
#if WINDOWS_PHONE
            _cursorIsValid = false;
#else
            _cursorIsValid = true;
#endif
            _cursor = Vector2.Zero;
        }

        public GamePadState GamePadState
        {
            get { return _currentGamePadState; }
        }

        public KeyboardState KeyboardState
        {
            get { return _currentKeyboardState; }
        }

        public MouseState MouseState
        {
            get { return _currentMouseState; }
        }

        public GamePadState VirtualState
        {
            get { return _currentVirtualState; }
        }

        public GamePadState PreviousGamePadState
        {
            get { return _lastGamePadState; }
        }

        public KeyboardState PreviousKeyboardState
        {
            get { return _lastKeyboardState; }
        }

        public MouseState PreviousMouseState
        {
            get { return _lastMouseState; }
        }

        public GamePadState PreviousVirtualState
        {
            get { return _lastVirtualState; }
        }

        public bool ShowCursor
        {
            get { return _cursorIsVisible && _cursorIsValid; }
            set { _cursorIsVisible = value; }
        }

        public Vector2 Cursor
        {
            get { return _cursor; }
        }

        public bool IsCursorMoved
        {
            get { return _cursorMoved; }
        }

        public bool IsCursorValid
        {
            get { return _cursorIsValid; }
        }

        public void LoadContent()
        {
            _cursorSprite = new Sprite(_manager.Content.Load<Texture2D>("Common/cursor"));
            _socketSprite = new Sprite(_manager.Content.Load<Texture2D>("Common/socket"));
            _stickSprite = new Sprite(_manager.Content.Load<Texture2D>("Common/stick"));
            _texButtons = _manager.Content.Load<Texture2D>("Common/buttons");
            _buttonOrigin = new Vector2(20f, 20f);

            _viewport = _manager.GraphicsDevice.Viewport;
        }

        /// <summary>
        ///   Reads the latest state of the keyboard and gamepad and mouse/touchpad.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            _lastKeyboardState = _currentKeyboardState;
            _lastGamePadState = _currentGamePadState;
            _lastMouseState = _currentMouseState;
            _lastVirtualState = _currentVirtualState;

            _currentKeyboardState = Keyboard.GetState();
            _currentGamePadState = GamePad.GetState(PlayerIndex.One);
            _currentMouseState = Mouse.GetState();

#if XBOX
            _currentVirtualState= GamePad.GetState(PlayerIndex.One);
#elif WINDOWS
            if (GamePad.GetState(PlayerIndex.One).IsConnected)
            {
                _currentVirtualState = GamePad.GetState(PlayerIndex.One);
            }
            else
            {
                _currentVirtualState = HandleVirtualStickWin();
            }
#elif WINDOWS_PHONE
            _currentVirtualState= HandleVirtualStickWP7();
#endif

            _gestures.Clear();
            while (TouchPanel.IsGestureAvailable)
            {
                _gestures.Add(TouchPanel.ReadGesture());
            }

            // Update cursor
            Vector2 oldCursor = _cursor;
            if (_currentGamePadState.IsConnected && _currentGamePadState.ThumbSticks.Left != Vector2.Zero)
            {
                Vector2 temp = _currentGamePadState.ThumbSticks.Left;
                _cursor += temp * new Vector2(300f, -300f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                Mouse.SetPosition((int)_cursor.X, (int)_cursor.Y);
            }
            else
            {
                _cursor.X = _currentMouseState.X;
                _cursor.Y = _currentMouseState.Y;
            }
            _cursor.X = MathHelper.Clamp(_cursor.X, 0f, _viewport.Width);
            _cursor.Y = MathHelper.Clamp(_cursor.Y, 0f, _viewport.Height);

            if (_cursorIsValid && oldCursor != _cursor)
            {
                _cursorMoved = true;
            }
            else
            {
                _cursorMoved = false;
            }

#if WINDOWS
            if (_viewport.Bounds.Contains(_currentMouseState.X, _currentMouseState.Y))
            {
                _cursorIsValid = true;
            }
            else
            {
                _cursorIsValid = false;
            }
#elif WINDOWS_PHONE
            if (_currentMouseState.LeftButton == ButtonState.Pressed)
            {
                _cursorIsValid = true;
            }
            else
            {
                _cursorIsValid = false;
            }
#endif
        }

        public void Draw()
        {
            if (_cursorIsVisible && _cursorIsValid)
            {
                _manager.SpriteBatch.Begin();
                _manager.SpriteBatch.Draw(_cursorSprite.Texture, _cursor, null, Color.White, 0f, _cursorSprite.Origin, 1f, SpriteEffects.None, 0f);
                _manager.SpriteBatch.End();
            }
#if WINDOWS_PHONE

#endif
        }

        private GamePadState HandleVirtualStickWin()
        {
            Buttons _virtualButtons;


            GamePadButtons _buttons = new GamePadButtons();

          /*      GamePadState _state = new GamePadState(Vector2.Zero, Vector2.Zero, 0f, 0f, );
         /*   Vector2 force = _agentForce * new Vector2(input.GamePadState.ThumbSticks.Right.X,
                                                      -input.GamePadState.ThumbSticks.Right.Y);
            float torque = _agentTorque * (input.GamePadState.Triggers.Right - input.GamePadState.Triggers.Left);

            _userAgent.ApplyForce(force);
            _userAgent.ApplyTorque(torque);

            float forceAmount = _agentForce * 0.6f;

            force = Vector2.Zero;
            torque = 0;

            if (input.KeyboardState.IsKeyDown(Keys.A))
            {
                force += new Vector2(-forceAmount, 0);
            }
            if (input.KeyboardState.IsKeyDown(Keys.S))
            {
                force += new Vector2(0, forceAmount);
            }
            if (input.KeyboardState.IsKeyDown(Keys.D))
            {
                force += new Vector2(forceAmount, 0);
            }
            if (input.KeyboardState.IsKeyDown(Keys.W))
            {
                force += new Vector2(0, -forceAmount);
            }
            if (input.KeyboardState.IsKeyDown(Keys.Q))
            {
                torque -= _agentTorque;
            }
            if (input.KeyboardState.IsKeyDown(Keys.E))
            {
                torque += _agentTorque;
            }

            _userAgent.ApplyForce(force);
            _userAgent.ApplyTorque(torque);*/


            return new GamePadState();
        }

        private GamePadState HandleVirtualStickWP7()
        {
            return new GamePadState();
        }

        /// <summary>
        ///   Helper for checking if a key was newly pressed during this update.
        /// </summary>
        public bool IsNewKeyPress(Keys key)
        {
            return (_currentKeyboardState.IsKeyDown(key) &&
                    _lastKeyboardState.IsKeyUp(key));
        }

        public bool IsNewKeyRelease(Keys key)
        {
            return (_lastKeyboardState.IsKeyDown(key) &&
                    _currentKeyboardState.IsKeyUp(key));
        }

        /// <summary>
        ///   Helper for checking if a button was newly pressed during this update.
        /// </summary>
        public bool IsNewButtonPress(Buttons button)
        {
            return (_currentGamePadState.IsButtonDown(button) &&
                    _lastGamePadState.IsButtonUp(button));
        }

        public bool IsNewButtonRelease(Buttons button)
        {
            return (_lastGamePadState.IsButtonDown(button) &&
                    _currentGamePadState.IsButtonUp(button));
        }

        /// <summary>
        ///   Helper for checking if a mouse button was newly pressed during this update.
        /// </summary>
        public bool IsNewMouseButtonPress(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.LeftButton:
                    return (_currentMouseState.LeftButton == ButtonState.Pressed &&
                            _lastMouseState.LeftButton == ButtonState.Released);
                case MouseButtons.RightButton:
                    return (_currentMouseState.RightButton == ButtonState.Pressed &&
                            _lastMouseState.RightButton == ButtonState.Released);
                case MouseButtons.MiddleButton:
                    return (_currentMouseState.MiddleButton == ButtonState.Pressed &&
                            _lastMouseState.MiddleButton == ButtonState.Released);
                case MouseButtons.ExtraButton1:
                    return (_currentMouseState.XButton1 == ButtonState.Pressed &&
                            _lastMouseState.XButton1 == ButtonState.Released);
                case MouseButtons.ExtraButton2:
                    return (_currentMouseState.XButton2 == ButtonState.Pressed &&
                            _lastMouseState.XButton2 == ButtonState.Released);
                default:
                    return false;
            }
        }


        /// <summary>
        /// Checks if the requested mouse button is released.
        /// </summary>
        /// <param name="button">The button.</param>
        public bool IsNewMouseButtonRelease(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.LeftButton:
                    return (_lastMouseState.LeftButton == ButtonState.Pressed &&
                            _currentMouseState.LeftButton == ButtonState.Released);
                case MouseButtons.RightButton:
                    return (_lastMouseState.RightButton == ButtonState.Pressed &&
                            _currentMouseState.RightButton == ButtonState.Released);
                case MouseButtons.MiddleButton:
                    return (_lastMouseState.MiddleButton == ButtonState.Pressed &&
                            _currentMouseState.MiddleButton == ButtonState.Released);
                case MouseButtons.ExtraButton1:
                    return (_lastMouseState.XButton1 == ButtonState.Pressed &&
                            _currentMouseState.XButton1 == ButtonState.Released);
                case MouseButtons.ExtraButton2:
                    return (_lastMouseState.XButton2 == ButtonState.Pressed &&
                            _currentMouseState.XButton2 == ButtonState.Released);
                default:
                    return false;
            }
        }

        /// <summary>
        ///   Checks for a "menu select" input action.
        /// </summary>
        public bool IsMenuSelect()
        {
            return IsNewKeyPress(Keys.Space) ||
                   IsNewKeyPress(Keys.Enter) ||
                   IsNewButtonPress(Buttons.A) ||
                   IsNewButtonPress(Buttons.Start) ||
                   IsNewMouseButtonPress(MouseButtons.LeftButton);
        }

        public bool IsMenuPressed()
        {
            return _currentKeyboardState.IsKeyDown(Keys.Space) ||
                   _currentKeyboardState.IsKeyDown(Keys.Enter) ||
                   _currentGamePadState.IsButtonDown(Buttons.A) ||
                   _currentGamePadState.IsButtonDown(Buttons.Start) ||
                   _currentMouseState.LeftButton == ButtonState.Pressed;
        }

        public bool IsMenuReleased()
        {
            return IsNewKeyRelease(Keys.Space) ||
                   IsNewKeyRelease(Keys.Enter) ||
                   IsNewButtonRelease(Buttons.A) ||
                   IsNewButtonRelease(Buttons.Start) ||
                   IsNewMouseButtonRelease(MouseButtons.LeftButton);
        }

        /// <summary>
        ///   Checks for a "menu cancel" input action.
        /// </summary>
        public bool IsMenuCancel()
        {
            return IsNewKeyPress(Keys.Escape) ||
                   IsNewButtonPress(Buttons.Back);
        }
    }
}