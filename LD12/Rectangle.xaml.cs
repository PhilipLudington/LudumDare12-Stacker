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
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.FarseerPhysics.Collisions;

namespace LD12
{
    public partial class Rectangle : UserControl, IGameObject
    {
        PhysicsSimulator physicsSimulator;
        Geom rectangleGeom;

        public Rectangle(PhysicsSimulator physicsSimulator)
        {
            InitializeComponent();

            this.physicsSimulator = physicsSimulator;

            ReBuildGeometry();
        }
        public bool CheckIfNeedsToBeDeleted()
        {
            if (Body.Position.X > 400
                || Body.Position.X < -10
                || Body.Position.Y > 300
                || Body.Position.Y < -10)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Delete()
        {
            physicsSimulator.Remove(rectangleGeom);
            physicsSimulator.Remove(body);
        }

        #region Private Helpers
        private void ReBuildGeometry()
        {
            Body oldBody = body;

            body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, (float)rectangle.Width, (float)rectangle.Height, 1);
            double x = (double)rectangle.GetValue(Canvas.LeftProperty);
            double y = (double)rectangle.GetValue(Canvas.TopProperty);
            body.Position = new Vector2((float)x, (float)y);

            if (oldBody != null)
            {
                body.IsStatic = oldBody.IsStatic;
                physicsSimulator.Remove(oldBody);
            }

            if (rectangleGeom != null)
            {
                physicsSimulator.Remove(rectangleGeom);
            }

            rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, body, Width, Height);
            rectangleGeom.RestitutionCoefficient = 0.0f;
            rectangleGeom.FrictionCoefficient = 1000000.90f;
        }
        #endregion

        #region IGameObject Members
        public float Y
        {
            set
            {
                rectangle.SetValue(Canvas.TopProperty, (double)value);
                Body.Position = new Vector2(Body.Position.X, value);
            }
            get
            {
                return (float)Body.Position.Y;
            }
        }
        public float X
        {
            set
            {
                rectangle.SetValue(Canvas.LeftProperty, (double)value);
                Body.Position = new Vector2(value, Body.Position.Y);
            }
            get
            {
                return (float)Body.Position.X;
            }
        }
        public new float Width
        {
            set
            {
                rectangle.Width = value;
                translateTransform.X = -value / 2;
                ReBuildGeometry();
            }
            get
            {
                return (float)rectangle.Width;
            }
        }
        public new float Height
        {
            set
            {
                rectangle.Height = value;
                translateTransform.Y = -value / 2;
                ReBuildGeometry();
            }
            get
            {
                return (float)rectangle.Height;
            }
        }
        public Body Body
        {
            get { return body; }
        }
        private Body body;

        public RotateTransform RotateTransform
        {
            get { return rotateTransform; }
        }

        public UIElement UIElement
        {
            get { return rectangle; }
        }
        #endregion
    }
}
