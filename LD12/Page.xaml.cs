using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Threading;
using FarseerGames.FarseerPhysics.Collisions;

namespace LD12
{
    public partial class Page : UserControl
    {
        private PhysicsSimulator physicsSimulator;
        private List<IGameObject> GameObjects;
        DateTime lastUpdateTime = DateTime.MinValue;
        double leftoverUpdateTime = 0;
        TimeSpan elapsedTime;
        bool running = false;
        bool isMouseInDropZone = false;
        bool isMouseButtonDown = true;
        bool canAddNewBrick = true;
        bool winCountDown = true;
        IGameObject winnerBrick;
        DateTime winnerClock;
        Rectangle floorRectangle;
        Int32 level = 0;

        public Page()
        {
            InitializeComponent();

            GameObjects = new List<IGameObject>();

            physicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            physicsSimulator.MaxContactsToDetect = 5;
            physicsSimulator.MaxContactsToDetect = 2; //for stacked objects, simultaneous collision are the bottlenecks so limit them to 2 per geometric pair.
            physicsSimulator.Iterations = 10;
            physicsSimulator.BiasFactor = .4f;
            physicsSimulator.FrictionType = FrictionType.Minimum;

            lastUpdateTime = DateTime.Now;
        }

        private void Restart()
        {
            foreach (IGameObject iGameObject in GameObjects)
            {
                iGameObject.UIElement.Visibility = Visibility.Collapsed;
                TheCanvas.Children.Remove(iGameObject.UIElement);
                iGameObject.Delete();
            }
            GameObjects.Clear();

            floorRectangle = new Rectangle(physicsSimulator);
            floorRectangle.Body.IsStatic = true;
            floorRectangle.Width = 375;
            floorRectangle.Height = 74;
            floorRectangle.X = 190.0f;
            floorRectangle.Y = 266.0f;
            floorRectangle.Body.Mass = 100000;
            TheCanvas.Children.Add(floorRectangle);
            GameObjects.Add(floorRectangle);
        }
        private void BuildNextLevel()
        {
            Restart();

            Level.Text = string.Format("Level {0}", ++level);

            double newWinPosition = 175.0f;
            if (winLine != null)
            {
                double y = (double)winLine.GetValue(Canvas.TopProperty);
                newWinPosition = y - (y * 0.05f);
            }
            winLine.SetValue(Canvas.TopProperty, newWinPosition);

            if (level > 8)
            {
                physicsSimulator.Gravity = new Vector2(0, 250);
            }
        }

        private void MainStoryboard_Completed(object sender, EventArgs e)
        {
            textBlockCount.Text = string.Format("No. of blocks - {0}", GameObjects.Count - 1);

            // the game loop
            elapsedTime = DateTime.Now - lastUpdateTime;
            lastUpdateTime = DateTime.Now;
            double secs = (elapsedTime.TotalMilliseconds / 1000.0) + leftoverUpdateTime;
            while (secs > .01)
            {
                physicsSimulator.Update(.01f);

                List<IGameObject> delete = new List<IGameObject>();
                foreach (IGameObject iGameObject in GameObjects)
                {
                    if (iGameObject.Body.Position.Y != Canvas.GetTop(iGameObject.UIElement))
                    {
                        Canvas.SetTop(iGameObject.UIElement, iGameObject.Body.Position.Y);
                    }

                    if (iGameObject.Body.Position.X != (double)Canvas.GetLeft(iGameObject.UIElement))
                    {
                        Canvas.SetLeft(iGameObject.UIElement, iGameObject.Body.Position.X);
                    }

                    double theAngle = (iGameObject.Body.Rotation * 360) / (2 * Math.PI);
                    if (theAngle != iGameObject.RotateTransform.Angle)
                    {
                        iGameObject.RotateTransform.Angle = theAngle;
                    }

                    if (iGameObject.Body.Position.X > 600
                        || iGameObject.Body.Position.X < -10
                        || iGameObject.Body.Position.Y > 600
                        || iGameObject.Body.Position.Y < -10)
                    {
                        delete.Add(iGameObject);
                    }

                    double y = (double)winLine.GetValue(Canvas.TopProperty);
                    if (winCountDown == false
                        && iGameObject.Y < y)
                    {
                        winCountDown = true;
                        winnerBrick = iGameObject;
                        winnerClock = DateTime.Now;
                    }

                    if (winCountDown == true)
                    {
                        Int32 seconds = (DateTime.Now - winnerClock).Seconds;
                        textBlockCount.Text = string.Format("Win in {0} seconds.", 3 - seconds);
                    }
                }

                foreach (IGameObject iGameObject in delete)
                {
                    iGameObject.UIElement.Visibility = Visibility.Collapsed;
                    GameObjects.Remove(iGameObject);
                    TheCanvas.Children.Remove(iGameObject.UIElement);
                    iGameObject.Delete();
                }

                secs -= .01;
            }
            leftoverUpdateTime = secs;

            double winLineY = (double)winLine.GetValue(Canvas.TopProperty);
            if (winCountDown == true
                && winnerBrick != null
                && winnerBrick.Y < winLineY)
            {
                if (winnerClock.AddSeconds(3) <= DateTime.Now)
                {
                    running = false;
                    textBlockCount.Text = "You Win!";
                    if ((double)winLine.GetValue(Canvas.TopProperty) < 1.0)
                    {
                        Play.Content = "You are a Grand Champion!";
                        Play.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Play.Content = "Next Level";
                        Play.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                winCountDown = false;
            }
            if (winCountDown == false
                && running == true
                && GameObjects.Count > 85 )
            {
                Play.Content = "Try Again";
                Play.Visibility = Visibility.Visible;
                textBlockCount.Text = "Too many bricks!";
                running = false;
            }

            if (running == true)
            {
                MainStoryboard.Begin();
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (running == false)
            {
                Play.Content = "";
                Play.Visibility = Visibility.Collapsed;
                if (GameObjects.Count > 85)
                {
                    Restart();
                }
                else
                {
                    BuildNextLevel();
                }
                winCountDown = false;
                running = true;
                lastUpdateTime = DateTime.Now;
                MainStoryboard.Begin();
            }
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (floorRectangle != null)
            {
                if (e.GetPosition(this.TheCanvas).Y < 230
                    && e.GetPosition(this.TheCanvas).X < 450)
                {
                    isMouseInDropZone = true;
                }
                else
                {
                    isMouseInDropZone = false;
                }
            }
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (canAddNewBrick == true
                && isMouseButtonDown == true
                && isMouseInDropZone == true)
            {
                canAddNewBrick = false;

                Rectangle reactangle = new Rectangle(physicsSimulator);
                reactangle.Width = 10;
                reactangle.Height = 10;
                reactangle.X = (float)e.GetPosition(this.TheCanvas).X;
                reactangle.Y = (float)e.GetPosition(this.TheCanvas).Y;
                TheCanvas.Children.Add(reactangle);
                GameObjects.Add(reactangle);

                canAddNewBrick = true;
            }

            isMouseButtonDown = false;
        }

        private void TheCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseButtonDown = true;
        }
    }
}
