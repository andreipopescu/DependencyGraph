using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Border = Endava.DependencyGraph.Components.Border;

namespace Endava.DependencyGraph.ScreenSystem
{
	/// <summary>
	/// Enum describes the screen transition state.
	/// </summary>
	public enum ScreenState
	{
		TransitionOn,
		Active,
		TransitionOff,
		Hidden,
	}

	/// <summary>
	/// A screen is a single layer that has update and draw logic, and which
	/// can be combined with other layers to build up a complex menu system.
	/// For instance the main menu, the options menu, the "are you sure you
	/// want to quit" message box, and the main game itself are all implemented
	/// as screens.
	/// </summary>
	public abstract class GameScreen : IDisposable
	{
		private FixedMouseJoint _fixedMouseJoint;
		private bool _otherScreenHasFocus;
		public bool firstRun = true;
		private bool tooltip = false;

		protected GameScreen()
		{
			DebugCanvas = null;
			TxtDebug = null;
			Transform = new TransformGroup();

			#region Transitions
			//ScreenState = ScreenState.TransitionOn;
			//TransitionPosition = 1;
			//TransitionOffTime = TimeSpan.FromSeconds(0.5);
			//TransitionOnTime = TimeSpan.FromSeconds(0.5);
			#endregion
		}

		public Canvas DebugCanvas { get; set; }
		public TextBlock TxtDebug { get; set; }
		public TransformGroup Transform { get; set; }

		public World World { get; set; }

		public DebugViewGraph DebugView { get; set; }

		public bool DebugViewEnabled { get; set; }

		/// <summary>
		/// Normally when one screen is brought up over the top of another,
		/// the first screen will transition off to make room for the new
		/// one. This property indicates whether the screen is only a small
		/// popup, in which case screens underneath it do not need to bother
		/// transitioning off.
		/// </summary>
		public bool IsPopup { get; protected set; }

		/// <summary>
		/// Indicates how long the screen takes to
		/// transition on when it is activated.
		/// </summary>
		public TimeSpan TransitionOnTime { get; protected set; }

		/// <summary>
		/// Indicates how long the screen takes to
		/// transition off when it is deactivated.
		/// </summary>
		public TimeSpan TransitionOffTime { get; protected set; }

		/// <summary>
		/// Gets the current position of the screen transition, ranging
		/// from zero (fully active, no transition) to one (transitioned
		/// fully off to nothing).
		/// </summary>
		public float TransitionPosition { get; protected set; }

		/// <summary>
		/// Gets the current alpha of the screen transition, ranging
		/// from 255 (fully active, no transition) to 0 (transitioned
		/// fully off to nothing).
		/// </summary>
		public byte TransitionAlpha
		{
			get { return (byte)(255 - TransitionPosition * 255); }
		}

		/// <summary>
		/// Gets the current screen transition state.
		/// </summary>
		public ScreenState ScreenState { get; protected set; }

		/// <summary>
		/// There are two possible reasons why a screen might be transitioning
		/// off. It could be temporarily going away to make room for another
		/// screen that is on top of it, or it could be going away for good.
		/// This property indicates whether the screen is exiting for real:
		/// if set, the screen will automatically remove itself as soon as the
		/// transition finishes.
		/// </summary>
		public bool IsExiting { get; protected set; }

		/// <summary>
		/// Checks whether this screen is active and can respond to user input.
		/// </summary>
		public bool IsActive
		{
			get
			{
				return true;
				//return !_otherScreenHasFocus &&
				//       (ScreenState == ScreenState.TransitionOn ||
				//        ScreenState == ScreenState.Active);
			}
		}

		/// <summary>
		/// Gets the manager that this screen belongs to.
		/// </summary>
		public ScreenManager ScreenManager { get; internal set; }

		#region IDisposable Members

		public virtual void Dispose()
		{
		}

		#endregion

		public virtual void Initialize()
		{
			if (DebugCanvas != null)
			{
				Transform.Children.Add(new ScaleTransform(DebugCanvas.ActualWidth / ScreenManager.ScreenWidth, -DebugCanvas.ActualHeight / ScreenManager.ScreenHeight));
				Transform.Children.Add(new TranslateTransform(DebugCanvas.ActualWidth / 2, DebugCanvas.ActualHeight / 2));

				//DebugView
				DebugView = new DebugViewGraph(DebugCanvas, TxtDebug, World);
				DebugView.DefaultShapeColor = Colors.White;
				DebugView.SleepingShapeColor = Colors.LightGray;
				DebugView.Transform = Transform;
			}
		}

		/// <summary>
		/// Load graphics content for the screen.
		/// </summary>
		public virtual void LoadContent()
		{
			if (World != null)
			{
				new Border(World, ScreenManager.ScreenWidth, ScreenManager.ScreenHeight, 2);
			}
		}

		/// <summary>
		/// Unload content for the screen.
		/// </summary>
		public virtual void UnloadContent()
		{
		}

		/// <summary>
		/// Allows the screen to run logic, such as updating the transition position.
		/// Unlike <see cref="HandleInput"/>, this method is called regardless of whether the screen
		/// is active, hidden, or in the middle of a transition.
		/// </summary>
		public virtual void Update(GameTime gameTime, bool otherScreenHasFocus,
								   bool coveredByOtherScreen)
		{
			_otherScreenHasFocus = otherScreenHasFocus;

			#region Transitions
			/*
			if (IsExiting)
			{
				// If the screen is going away to die, it should transition off.
				ScreenState = ScreenState.TransitionOff;

				if (!UpdateTransition(gameTime, TransitionOffTime, 1))
				{
					// When the transition finishes, remove the screen.
					ScreenManager.RemoveScreen(this);

					IsExiting = false;
				}
			}
			else if (coveredByOtherScreen)
			{
				// If the screen is covered by another, it should transition off.
				if (UpdateTransition(gameTime, TransitionOffTime, 1))
				{
					// Still busy transitioning.
					ScreenState = ScreenState.TransitionOff;
				}
				else
				{
					// Transition finished!
					ScreenState = ScreenState.Hidden;
				}
			}
			else
			{
				// Otherwise the screen should transition on and become active.
				if (UpdateTransition(gameTime, TransitionOnTime, -1))
				{
					// Still busy transitioning.
					ScreenState = ScreenState.TransitionOn;
				}
				else
				{
					// Transition finished!
					ScreenState = ScreenState.Active;
				}
			}
			*/
			#endregion

			if (!coveredByOtherScreen && !otherScreenHasFocus)
			{
				if (World != null)
				{
					// variable time step but never less then 30 Hz
					World.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, (1f / 30f)));

					Settings.VelocityIterations = 5;
					Settings.PositionIterations = 3;
				}
			}
		}

		/// <summary>
		/// Allows the screen to handle user input. Unlike Update, this method
		/// is only called when the screen is active, and not when some other
		/// screen has taken the focus.
		/// </summary>
		public virtual void HandleInput(InputState input)
		{
			//Mouse
			Point p = Transform.Inverse.Transform(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y));
			Vector2 position = new Vector2((float)p.X, (float)p.Y);
			
			if (input.CurrentMouseState.IsLeftButtonDown == false && input.LastMouseState.IsLeftButtonDown)
			{
				MouseUp();
			}
			else if (input.CurrentMouseState.IsLeftButtonDown && input.LastMouseState.IsLeftButtonDown == false)
			{
				MouseDown(position);
			}

			MouseMove(position);

			//Enter DebugView
			if (input.CurrentMouseState.IsRightButtonDown)
			{
				DebugViewEnabled = !DebugViewEnabled;
				Settings.EnableDiagnostics = DebugViewEnabled;
				if (DebugViewEnabled == false)
					TxtDebug.Text = "";
			}

			if (DebugView != null)
			{
				if (DebugViewEnabled)
					DebugView.AppendFlags(DebugViewFlags.DebugPanel);
				else
					DebugView.RemoveFlags(DebugViewFlags.DebugPanel);
			}
		}

		private void MouseMove(Vector2 p)
		{
			if (_fixedMouseJoint != null)
			{
				_fixedMouseJoint.WorldAnchorB = p;
			}
		}

		private bool IsMouseOnConnector(Vector2 p, Joint joint)
		{
			//convert pixels to Vector2 measures
            float tolerance = joint.Thickness / 2.5f;

			Vector2 startPoint = joint.WorldAnchorA;
			Vector2 endPoint = joint.WorldAnchorB;

			Math.Abs(joint.WorldAnchorA.X);

			Vector2 leftPoint;
			Vector2 rightPoint;

			// Normalize start/end to left right to make the offset calc simpler.
			if (startPoint.X <= endPoint.X)
			{
				leftPoint = startPoint;
				rightPoint = endPoint;
			}
			else
			{
				leftPoint = endPoint;
				rightPoint = startPoint;
			}

			// If point is out of bounds, no need to do further checks.                  
			if (p.X + tolerance < leftPoint.X || rightPoint.X < p.X - tolerance)
			{
				return false;
			}

			if (p.Y + tolerance < Math.Min(leftPoint.Y, rightPoint.Y) || Math.Max(leftPoint.Y, rightPoint.Y) < p.Y - tolerance)
			{
				return false;
			}

			double deltaX = rightPoint.X - leftPoint.X;
			double deltaY = rightPoint.Y - leftPoint.Y;

			// If the line is straight, the earlier boundary check is enough to determine that the point is on the line.
			// Also prevents division by zero exceptions.
			if ((Math.Abs(deltaX) <= tolerance / 7) || (Math.Abs(deltaY) <= tolerance / 10))
			{
				return true;
			}
			double slope = deltaY / deltaX;
			double offset = leftPoint.Y - leftPoint.X * slope;
			double calculatedY = p.X * slope + offset;

			// Check calculated Y matches the points Y coord with some easing.
            bool lineContains = p.Y - tolerance <= calculatedY && calculatedY <= p.Y + tolerance;

            return lineContains;
		}

		private void MouseDown(Vector2 p)
		{
			if (_fixedMouseJoint != null)
			{
				return;
			}

			// Make a small box.
			AABB aabb;
			//Vector2 d = new Vector2(0.001f, 0.001f);
			Vector2 d = new Vector2(1f, 1f);

			aabb.LowerBound = p - d;
			aabb.UpperBound = p + d;

			Fixture savedFixture = null;

			if (tooltip)
			{
				foreach (var body in World.BodyList)
				{
					var bodyDescription = body.UserData as BodyDescription;
					if (bodyDescription != null && bodyDescription.FigureType == FigureType.Tooltip)
					{
						World.BodyList.Remove(body);
						break;
					}
				}
			}

			// Query the world for overlapping shapes.
			World.QueryAABB(
				fixture =>
				{
					Body body = fixture.Body;
					if (body.BodyType == BodyType.Dynamic)
					{
						bool inside = fixture.TestPoint(ref p);
						if (inside)
						{
							savedFixture = fixture;

							// We are done, terminate the query.
							return false;
						}
					}

					// Continue the query.
					return true;
				}, ref aabb);

			if (savedFixture != null)
			{
				Body body = savedFixture.Body;
				_fixedMouseJoint = new FixedMouseJoint(body, p);
				_fixedMouseJoint.MaxForce = 1000.0f * body.Mass;
				World.AddJoint(_fixedMouseJoint);
				body.Awake = true;
			}
			// Check for connectors if no fixture is found
			else
			{
				foreach (var joint in World.JointList)
				{
					if (IsMouseOnConnector(p, joint))
					{
						BodyFactory.CreateTooltip(World, p, joint.UserData.ToString());
						tooltip = true;
						return;
					}
				}
			}
		}

		private void MouseUp()
		{
			if (_fixedMouseJoint != null)
			{
				World.RemoveJoint(_fixedMouseJoint);
				_fixedMouseJoint = null;
			}
		}

		/// <summary>
		/// This is called when the screen should draw itself.
		/// </summary>
		public virtual void Draw(GameTime gameTime)
		{
			if (World != null)
			{
				if (DebugView != null)
				{
					DebugView.DrawDebugData();
				}
			}
		}

		#region Transitions

		/// <summary>
		/// Helper for updating the screen transition position.
		/// </summary>
		private bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
		{
			// How much should we move by?
			float transitionDelta;

			if (time == TimeSpan.Zero)
				transitionDelta = 1;
			else
				transitionDelta = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds);

			// Update the transition position.
			TransitionPosition += transitionDelta * direction;

			// Did we reach the end of the transition?
			if ((TransitionPosition <= 0) || (TransitionPosition >= 1))
			{
				TransitionPosition = MathHelper.Clamp(TransitionPosition, 0, 1);
				return false;
			}
			// Otherwise we are still busy transitioning.
			return true;
		}

		#endregion

		/// <summary>
		/// Tells the screen to go away. Unlike <see cref="ScreenManager"/>.RemoveScreen, which
		/// instantly kills the screen, this method respects the transition timings
		/// and will give the screen a chance to gradually transition off.
		/// </summary>
		public void ExitScreen()
		{
			ScreenManager.RemoveScreen(this);

			#region Transitions
			//TxtDebug.Text = "";
			//if (TransitionOffTime == TimeSpan.Zero)
			//{
			//    // If the screen has a zero transition time, remove it immediately.
			//    ScreenManager.RemoveScreen(this);
			//}
			//else
			//{
			//    // Otherwise flag that it should transition off and then exit.
			//    IsExiting = true;
			//}
			#endregion
		}
	}
}