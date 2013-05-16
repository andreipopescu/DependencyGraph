using System.Windows.Input;
using System.Windows.Controls;

namespace Endava.DependencyGraph.ScreenSystem
{
    /// <summary>
    /// This class is used to keep track of Keyboard and Mouse states throughout the game
    /// </summary>
    public class InputState
    {
        private UserControl _userControl;

        public InputState()
        {
            LastMouseState = new MouseState();
            CurrentMouseState = new MouseState();
        }

        public MouseState LastMouseState { get; set; }
        public MouseState CurrentMouseState { get; set; }

        public void Attach(UserControl userControl)
        {
            _userControl = userControl;
            userControl.MouseMove += target_MouseMove;
            userControl.MouseLeftButtonDown += target_MouseLeftButtonDown;
            userControl.MouseLeftButtonUp += target_MouseLeftButtonUp;
	        userControl.MouseRightButtonDown += target_MouseRightButtonDown;
        }


		private void target_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			LastMouseState.IsRightButtonDown = CurrentMouseState.IsRightButtonDown;
			CurrentMouseState.IsRightButtonDown = true;
		}

        private void target_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LastMouseState.IsLeftButtonDown = CurrentMouseState.IsLeftButtonDown;
            CurrentMouseState.IsLeftButtonDown = false;
        }

        private void target_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LastMouseState.IsLeftButtonDown = CurrentMouseState.IsLeftButtonDown;
            CurrentMouseState.IsLeftButtonDown = true;
        }

        private void target_MouseMove(object sender, MouseEventArgs e)
        {
            LastMouseState.X = CurrentMouseState.X;
            LastMouseState.Y = CurrentMouseState.Y;
			
			//CurrentMouseState.X = e.StylusDevice.GetStylusPoints(_userControl)[0].X;
			//CurrentMouseState.Y = e.StylusDevice.GetStylusPoints(_userControl)[0].Y;
	        var position = e.GetPosition(_userControl);
	        CurrentMouseState.X = position.X;
			CurrentMouseState.Y = position.Y;
        }

		public void Update(GameTime gameTime)
        {
            LastMouseState = new MouseState(CurrentMouseState);
        }
    }
}
