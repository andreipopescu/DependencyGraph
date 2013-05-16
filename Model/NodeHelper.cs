using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Endava.DependencyGraph
{
	public class NodeHelper
	{
		private static Random random = new Random();

		public static bool ContainsKeyValue(Dictionary<Node, Node> dictionary, Node expectedKey, Node expectedValue)
		{
			Node actualValue;
			if (!dictionary.TryGetValue(expectedKey, out actualValue))
			{
				return false;
			}
			return actualValue == expectedValue;
		}

		public static bool NodesAreConnected(Node node1, Node node2)
		{
			var na = (from t in NodeDependency.ConnectedNodes
					  where t.StartNode == node1 && t.EndNode == node2 || t.StartNode == node2 && t.EndNode == node1
					  select t);

			return na.Count() > 0;
		}
			
		public static double GetRandomAngle()
		{
			int min = 0;
			int max = 179;
	
			//convert to Radians
			var angle = random.Next(min, max) * (Math.PI / 180);
			return angle;
		}

		/// <summary>
		/// Calculates a value that needs to be added to the X and Y axis in order to position the end Node.
		/// NOTE: the Node's center doesn't equals the Node's location on X and Y
		/// </summary>
		public static double GetNodeCenter(double startNodeSize, double endNodeSize)
		{
			var nodeCenter = (startNodeSize / 2) - (endNodeSize / 2);

			return nodeCenter;
		}

		/// <summary>
		/// Calculates distance between the Nodes based on the attraction force.
		/// </summary>
		public static double GetDistanceBetweenNodes(double startNodeSize, double endNodeSize, float? attractionForce)
		{
			var c = Math.Sqrt(startNodeSize * endNodeSize / (double)attractionForce);

			return c;
		}
	}
}
