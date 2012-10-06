using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics.Dynamics;

namespace LD12
{
    public interface IGameObject
    {
        float X { set; get; }
        float Y { set; get; }
        void Delete();
        bool CheckIfNeedsToBeDeleted();
        float Width { set; get; }
        float Height { set; get; }
        Body Body { get; }
        RotateTransform RotateTransform { get; }
        UIElement UIElement { get; }
    }
}
