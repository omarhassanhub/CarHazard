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

namespace CarvHazard
{

    public class Game1 : Microsoft.Xna.Framework.Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Texture2D mCar;
        private int mMoveCarX = 160;

        private Texture2D mCar2;
        private int mMoveCarX2 = 160;
        private Vector2 mCarPosition2 = new Vector2(450, 180);

        private Texture2D mCar3;
        private int mMoveCarX3 = 160;
        private Vector2 mCarPosition3 = new Vector2(650, 500);

        private int mVelocityY;
        private Texture2D mBackground;
        private Texture2D mRoad;
        private Texture2D mRoad2;
        private Texture2D mRoad3;
        private Texture2D mHazard;
        private KeyboardState mPreviousKeyboardState;
        private Vector2 mCarPosition = new Vector2(180, 280);
        
        private double mNextHazardAppearsIn;
        private int mCarsRemaining;
        private int mHazardsPassed;
        private int mIncreaseVelocity;
        private double mExitCountDown = 10;
        private int[] mRoadY = new int[2];
        private int[] mRoad2Y = new int[2];
        private int[] mRoad3Y = new int[2];
        private List<Hazard> mHazards = new List<Hazard>();
        private Random mRandom = new Random();
        private SpriteFont mFont;

        List<Explosion> explosionList = new List<Explosion>();
        SoundManager sm = new SoundManager();
        SoundManager sm2 = new SoundManager();

        private enum State
        {
            TitleScreen,
            Running,
            Crash,
            GameOver,
            Success
        }

        private State mCurrentState = State.TitleScreen;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 800;
        }


        protected override void Initialize()
        {
            base.Initialize();
        }


        protected override void LoadContent()
        {

            spriteBatch = new SpriteBatch(GraphicsDevice);

            mCar = Content.Load<Texture2D>("ImgAud/Car");
            mCar2 = Content.Load<Texture2D>("ImgAud/Car2");
            mCar3 = Content.Load<Texture2D>("ImgAud/heli3");
            mBackground = Content.Load<Texture2D>("ImgAud/Background");
            mRoad = Content.Load<Texture2D>("ImgAud/Road");
            mRoad2 = Content.Load<Texture2D>("ImgAud/Road2");
            mRoad3 = Content.Load<Texture2D>("ImgAud/Road3");
            mHazard = Content.Load<Texture2D>("ImgAud/Hazard");
            mFont = Content.Load<SpriteFont>("ImgAud/MyFont");
            sm.LoadContent(Content);
            sm2.LoadContent(Content);
            MediaPlayer.Play(sm2.bgMusic);
        }


        protected override void UnloadContent()
        {

        }

        protected void StartGame()
        {
            mRoadY[0] = 0;
            mRoadY[1] = -1 * mRoad.Height;
            mRoad2Y[0] = 0;
            mRoad2Y[1] = -1 * mRoad2.Height;
            mRoad3Y[0] = 0;
            mRoad3Y[1] = -1 * mRoad3.Height;
            mHazardsPassed = 0;
            mCarsRemaining = 3;
            mVelocityY = 3;
            mNextHazardAppearsIn = 1.5;
            mIncreaseVelocity = 5;
            mHazards.Clear();
            mCurrentState = State.Running;
        }


        protected override void Update(GameTime gameTime)
        {
            KeyboardState aCurrentKeyboardState = Keyboard.GetState();

            //Exit game
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                aCurrentKeyboardState.IsKeyDown(Keys.Escape) == true)
            {
                this.Exit();
            }

            switch (mCurrentState)
            {
                case State.TitleScreen:
                case State.Success:
                case State.GameOver:
                    {
                        ExitCountdown(gameTime);

                        if (aCurrentKeyboardState.IsKeyDown(Keys.Enter) == true && mPreviousKeyboardState.IsKeyDown(Keys.Enter) == false)
                        {
                            StartGame();
                        }
                        break;
                    }

                case State.Running:
                    {
                        //Switch lane if "enter" is pressed
                        if (aCurrentKeyboardState.IsKeyDown(Keys.Enter) == true && mPreviousKeyboardState.IsKeyDown(Keys.Enter) == false)
                        {
                            mCarPosition.X += mMoveCarX;
                            mMoveCarX *= -1;
                            //mCarPosition2.X += mMoveCarX2;
                            
                        }
                        if ( true )
                        {
                            mCarPosition2.Y += -2;
                            mMoveCarX2 *= -1;
                            mCarPosition3.Y += -2;
                            mMoveCarX3 *= -1;
                        
                        }
                        
                            
                        
                        ScrollRoad();
                        ScrollRoad2();
                        ScrollRoad3();
                        foreach (Hazard aHazard in mHazards)
                        {
                            if (CheckCollision(aHazard) == true)
                            {

                                break;
                            }

                            MoveHazard(aHazard);
                        }

                        UpdateHazards(gameTime);
                        break;
                    }
                case State.Crash:
                    {
                        //Continue driving if user presses "enter"
                        if (aCurrentKeyboardState.IsKeyDown(Keys.Enter) == true && mPreviousKeyboardState.IsKeyDown(Keys.Enter) == false)
                        {
                            mHazards.Clear();
                            mCurrentState = State.Running;
                        }

                        break;
                    }
            }

            mPreviousKeyboardState = aCurrentKeyboardState;

            ManageExplosions();

            base.Update(gameTime);

            foreach (Explosion ex in explosionList)
            {
                ex.Update(gameTime);
            }
        }


        private void ScrollRoad()
        {
            //Scroll road
            for (int aIndex = 0; aIndex < mRoadY.Length; aIndex++)
            {
                if (mRoadY[aIndex] >= this.Window.ClientBounds.Height)
                {
                    int aLastRoadIndex = aIndex;
                    for (int aCounter = 0; aCounter < mRoadY.Length; aCounter++)
                    {
                        if (mRoadY[aCounter] < mRoadY[aLastRoadIndex])
                        {
                            aLastRoadIndex = aCounter;
                        }
                    }
                    mRoadY[aIndex] = mRoadY[aLastRoadIndex] - mRoad.Height;
                }
            }

            for (int aIndex = 0; aIndex < mRoadY.Length; aIndex++)
            {
                mRoadY[aIndex] += mVelocityY;
            }
        }

        private void ScrollRoad2()
        {
            //Scroll road
            for (int aIndex = 0; aIndex < mRoad2Y.Length; aIndex++)
            {
                if (mRoad2Y[aIndex] >= this.Window.ClientBounds.Height)
                {
                    int aLastRoadIndex = aIndex;
                    for (int aCounter = 0; aCounter < mRoad2Y.Length; aCounter++)
                    {
                        if (mRoad2Y[aCounter] < mRoad2Y[aLastRoadIndex])
                        {
                            aLastRoadIndex = aCounter;
                        }
                    }
                    mRoad2Y[aIndex] = mRoad2Y[aLastRoadIndex] - mRoad2.Height;
                }
            }

            for (int aIndex = 0; aIndex < mRoad2Y.Length; aIndex++)
            {
                mRoad2Y[aIndex] += mVelocityY;
            }
        }

        private void ScrollRoad3()
        {
            //Scroll road
            for (int aIndex = 0; aIndex < mRoad3Y.Length; aIndex++)
            {
                if (mRoad3Y[aIndex] >= this.Window.ClientBounds.Height)
                {
                    int aLastRoadIndex = aIndex;
                    for (int aCounter = 0; aCounter < mRoad3Y.Length; aCounter++)
                    {
                        if (mRoad3Y[aCounter] < mRoad3Y[aLastRoadIndex])
                        {
                            aLastRoadIndex = aCounter;
                        }
                    }
                    mRoad3Y[aIndex] = mRoad3Y[aLastRoadIndex] - mRoad3.Height;
                }
            }

            for (int aIndex = 0; aIndex < mRoad3Y.Length; aIndex++)
            {
                mRoad3Y[aIndex] += mVelocityY;
            }
        }

        private void MoveHazard(Hazard theHazard)
        {
            theHazard.Position.Y += mVelocityY;
            if (theHazard.Position.Y > graphics.GraphicsDevice.Viewport.Height && theHazard.Visible == true)
            {
                theHazard.Visible = false;
                mHazardsPassed += 1;

                if (mHazardsPassed >= 100)
                {
                    mCurrentState = State.Success;
                    mExitCountDown = 10;
                }

                mIncreaseVelocity -= 1;
                if (mIncreaseVelocity < 0)
                {
                    mIncreaseVelocity = 5;
                    mVelocityY += 1;
                }
            }
        }

        private void UpdateHazards(GameTime theGameTime)
        {
            mNextHazardAppearsIn -= theGameTime.ElapsedGameTime.TotalSeconds;
            if (mNextHazardAppearsIn < 0)
            {
                int aLowerBound = 24 - (mVelocityY * 2);
                int aUpperBound = 30 - (mVelocityY * 2);

                if (mVelocityY > 10)
                {
                    aLowerBound = 6;
                    aUpperBound = 8;
                }

                mNextHazardAppearsIn = (double)mRandom.Next(aLowerBound, aUpperBound) / 10;
                AddHazard();
            }
        }

        private void AddHazard()
        {
            int aRoadPosition = mRandom.Next(1, 3);
            int aPosition = 150;
            if (aRoadPosition == 2)
            {
                aPosition = 270;
            }

            bool aAddNewHazard = true;
            foreach (Hazard aHazard in mHazards)
            {
                if (aHazard.Visible == false)
                {
                    aAddNewHazard = false;
                    aHazard.Visible = true;
                    aHazard.Position = new Vector2(aPosition, -mHazard.Height);
                    break;
                }
            }

            if (aAddNewHazard == true)
            {
                //Add hazard to left side of Road
                Hazard aHazard = new Hazard();
                aHazard.Position = new Vector2(aPosition, -mHazard.Height);

                mHazards.Add(aHazard);
            }
        }

        private bool CheckCollision(Hazard theHazard)
        {
            BoundingBox aHazardBox = new BoundingBox(new Vector3(theHazard.Position.X, theHazard.Position.Y, 0), new Vector3(theHazard.Position.X + (mHazard.Width * .4f), theHazard.Position.Y + ((mHazard.Height - 50) * .4f), 0));
            BoundingBox aCarBox = new BoundingBox(new Vector3(mCarPosition.X, mCarPosition.Y, 0), new Vector3(mCarPosition.X + (mCar.Width * .2f), mCarPosition.Y + (mCar.Height * .2f), 0));
            BoundingBox aCarBox2 = new BoundingBox(new Vector3(mCarPosition2.X, mCarPosition2.Y, 0), new Vector3(mCarPosition2.X + (mCar2.Width * .2f), mCarPosition2.Y + (mCar2.Height * .2f), 0));
            BoundingBox aCarBox3 = new BoundingBox(new Vector3(mCarPosition3.X, mCarPosition3.Y, 0), new Vector3(mCarPosition3.X + (mCar3.Width * .2f), mCarPosition3.Y + (mCar3.Height * .2f), 0));

            if (aHazardBox.Intersects(aCarBox) == true)
            {

                explosionList.Add(new Explosion(Content.Load<Texture2D>("ImgAud/explosion3"), new Vector2(theHazard.Position.X, theHazard.Position.Y)));

                sm2.explodeSound.Play();
                mCurrentState = State.Crash;
                mCarsRemaining -= 1;


                if (mCarsRemaining < 0)
                {
                    mCurrentState = State.GameOver;
                    mExitCountDown = 10;
                }
                return true;
            }

            return false;
        }

        private void ExitCountdown(GameTime theGameTime)
        {
            mExitCountDown -= theGameTime.ElapsedGameTime.TotalSeconds;
            if (mExitCountDown < 0)
            {
                this.Exit();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.Draw(mBackground, new Rectangle(graphics.GraphicsDevice.Viewport.X, graphics.GraphicsDevice.Viewport.Y, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height), Color.White);

            switch (mCurrentState)
            {
                case State.TitleScreen:
                    {

                        DrawTextCentered("KEEP CALM AND PASS THE HAZARDS!", 200);
                        DrawTextCentered("Press 'Enter' to begin", 260);
                        DrawTextCentered("Program Exits in " + ((int)mExitCountDown).ToString(), 475);

                        break;
                    }

                default:
                    {
                        DrawRoad();
                        DrawRoad2();
                        DrawRoad3();
                        DrawHazards();

                        spriteBatch.Draw(mCar, mCarPosition, new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.2f, SpriteEffects.None, 0);
                        spriteBatch.Draw(mCar2, mCarPosition2, new Rectangle(0, 0, mCar2.Width, mCar2.Height), Color.White, 0, new Vector2(0, 0), 0.2f, SpriteEffects.None, 0);
                        spriteBatch.Draw(mCar3, mCarPosition3, new Rectangle(0, 0, mCar3.Width, mCar3.Height), Color.White, 0, new Vector2(0, 0), 0.2f, SpriteEffects.None, 0);

                        foreach (Explosion ex in explosionList)
                        {
                            ex.Draw(spriteBatch);
                        }

                        spriteBatch.DrawString(mFont, "Cars:", new Vector2(28, 520), Color.Brown, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
                        for (int aCounter = 0; aCounter < mCarsRemaining; aCounter++)
                        {
                            spriteBatch.Draw(mCar, new Vector2(25 + (30 * aCounter), 550), new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.05f, SpriteEffects.None, 0);
                            spriteBatch.Draw(mCar2, new Vector2(25 + (30 * aCounter), 550), new Rectangle(0, 0, mCar2.Width, mCar2.Height), Color.White, 0, new Vector2(0, 0), 0.05f, SpriteEffects.None, 0);
                            spriteBatch.Draw(mCar3, new Vector2(25 + (30 * aCounter), 550), new Rectangle(0, 0, mCar3.Width, mCar3.Height), Color.White, 0, new Vector2(0, 0), 0.05f, SpriteEffects.None, 0);
                        }

                        spriteBatch.DrawString(mFont, "Hazards: " + mHazardsPassed.ToString(), new Vector2(5, 25), Color.Brown, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);

                        if (mCurrentState == State.Crash)
                        {

                            DrawTextDisplayArea();

                            DrawTextCentered("You Crashed the Hazard!", 200);
                            DrawTextCentered("Press 'Enter' to continue.", 260);
                        }
                        else if (mCurrentState == State.GameOver)
                        {
                            DrawTextDisplayArea();

                            DrawTextCentered("Game Over.", 200);
                            DrawTextCentered("Press 'Enter' to re-try.", 260);
                            DrawTextCentered("Program Exits in " + ((int)mExitCountDown).ToString(), 320);

                        }
                        else if (mCurrentState == State.Success)
                        {
                            DrawTextDisplayArea();

                            DrawTextCentered("Brilliant!", 200);
                            DrawTextCentered("Press 'Enter' to play again.", 260);
                            DrawTextCentered("Exit in " + ((int)mExitCountDown).ToString(), 400);
                        }

                        break;
                    }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawRoad()
        {
            for (int aIndex = 0; aIndex < mRoadY.Length; aIndex++)
            {
                if (mRoadY[aIndex] > mRoad.Height * -1 && mRoadY[aIndex] <= this.Window.ClientBounds.Height)
                {
                    spriteBatch.Draw(mRoad, new Rectangle((int)((this.Window.ClientBounds.Width - mRoad.Width) / 2 - 200), mRoadY[aIndex], mRoad.Width, mRoad.Height + 5), Color.White);

                }
            }
        }

        private void DrawRoad2()
        {
            for (int aIndex = 0; aIndex < mRoad2Y.Length; aIndex++)
            {
                if (mRoad2Y[aIndex] > mRoad2.Height * -1 && mRoad2Y[aIndex] <= this.Window.ClientBounds.Height)
                {
                    spriteBatch.Draw(mRoad2, new Rectangle((int)((this.Window.ClientBounds.Width - mRoad2.Width) / 2 - -159), mRoad2Y[aIndex], mRoad2.Width, mRoad2.Height + 5), Color.White);
                }
            }
        }

        private void DrawRoad3()
        {
            for (int aIndex = 0; aIndex < mRoad3Y.Length; aIndex++)
            {
                if (mRoad3Y[aIndex] > mRoad3.Height * -1 && mRoad3Y[aIndex] <= this.Window.ClientBounds.Height)
                {
                    spriteBatch.Draw(mRoad3, new Rectangle((int)((this.Window.ClientBounds.Width - mRoad3.Width) / 2 - -405), mRoad3Y[aIndex], mRoad3.Width, mRoad3.Height + 5), Color.White);
                }
            }
        }

        private void DrawHazards()
        {
            foreach (Hazard aHazard in mHazards)
            {
                if (aHazard.Visible == true)
                {
                    spriteBatch.Draw(mHazard, aHazard.Position, new Rectangle(0, 0, mHazard.Width, mHazard.Height), Color.White, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, 0);
                }
            }
        }

        private void DrawTextDisplayArea()
        {
            int aPositionX = (int)((graphics.GraphicsDevice.Viewport.Width / 2) - (300 / 2));
            spriteBatch.Draw(mBackground, new Rectangle(aPositionX, 175, 300, 200), Color.White);
        }

        private void DrawTextCentered(string theDisplayText, int thePositionY)
        {
            Vector2 aSize = mFont.MeasureString(theDisplayText);
            int aPositionX = (int)((graphics.GraphicsDevice.Viewport.Width / 2) - (aSize.X / 3));

            spriteBatch.DrawString(mFont, theDisplayText, new Vector2(aPositionX, thePositionY), Color.Beige, 0, new Vector2(0, 0), 0.7f, SpriteEffects.None, 0);
            spriteBatch.DrawString(mFont, theDisplayText, new Vector2(aPositionX + 1, thePositionY + 1), Color.Brown, 0, new Vector2(0, 0), 0.7f, SpriteEffects.None, 0);
        }


        public void ManageExplosions()
        {
            for (int i = 0; i < explosionList.Count; i++)
            {
                if (!explosionList[i].isVisible)
                {
                    explosionList.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
