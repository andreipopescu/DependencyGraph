using System;
using System.Windows.Controls;
using Endava.DependencyGraph.Components;
using Endava.DependencyGraph.ScreenSystem;

namespace Endava.DependencyGraph
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class FarseerPhysicsGame : Game
    {
        public FarseerPhysicsGame(UserControl userControl, Canvas drawingCanvas, Canvas debugCanvas, TextBlock txtFPS, TextBlock txtDebug)
            : base(userControl, drawingCanvas, debugCanvas, txtDebug)
        {
            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 16);
            IsFixedTimeStep = true;

            //new-up components and add to Game.Components
            ScreenManager = new ScreenManager(this);
            Components.Add(ScreenManager);

            if (txtFPS != null)
            {
                FrameRateCounter frameRateCounter = new FrameRateCounter(ScreenManager, txtFPS);
                Components.Add(frameRateCounter);
            }

			GraphScreen graph = new GraphScreen();

			ScreenManager.MainMenuScreen.AddMainMenuItem("", graph);
			ScreenManager.GoToMainMenu();

			//ScreenManager.AddScreen(graph);
		}

        public ScreenManager ScreenManager { get; set; }
    }
}