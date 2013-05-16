using System;
using System.Collections.Generic;
using System.Text;

namespace Endava.DependencyGraph
{
	public static class NodeDependency
	{
		public struct ConnectedNode
		{
			public Node StartNode { get; set; }
			public Node EndNode { get; set; }
			public string Description { get; set; }
			public double Distance { get; set; }
		}

		public static List<ConnectedNode> ConnectedNodes { get; set; }

		public static void AddConnectedNodes(Node node1, Node node2, string desc, double distance)
		{
			if (ConnectedNodes == null)
			{
				ConnectedNodes = new List<NodeDependency.ConnectedNode>();
			}

			ConnectedNodes.Add(new ConnectedNode { StartNode = node1, EndNode = node2, Description = desc, Distance = distance });
		}
	}
}
