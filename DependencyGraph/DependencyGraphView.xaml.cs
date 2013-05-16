using System.Windows.Controls;

namespace Endava.DependencyGraph
{
	/// <summary>
	/// Interaction logic for DependencyGraphView.xaml
	/// </summary>
	public partial class DependencyGraphView : UserControl
	{
		public DependencyGraphView()
		{
			InitializeComponent();

			FarseerPhysicsGame game = new FarseerPhysicsGame(this, Game, DebugCanvas, txtFPS, txtDebug);
			game.Run();
		}
	}
}
