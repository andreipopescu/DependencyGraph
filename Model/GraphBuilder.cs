using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;

namespace Endava.DependencyGraph
{
	public class GraphBuilder
	{
		private static Random random = new Random();

		public static List<Node> CreateNodes()
		{
            List<Node> nodes = new List<Node>();
			NodeDependency.ConnectedNodes = new List<NodeDependency.ConnectedNode>();

			//groups
			Group group1 = new Group("Group1");
			Group group2 = new Group("Group2");

			//node1
			var size1 = new AttributeSize(12);
			var skill1 = new AttributeSkill("ASP.NET", 10, group1);
			var skill12 = new AttributeSkill("Oracle", 5, group1);
			var skill13 = new AttributeSkill("WCF", 8, group1);
			var skill14 = new AttributeSkill("LINQ", 3, group2);
			var skill15 = new AttributeSkill("Windows Forms", 4, group2);

			nodes.Add(new Node("Ryan Kelvin", new List<AttributeBase>() {size1, skill1, skill12, skill13, skill14, skill15 }));

			//node2
			var size2 = new AttributeSize(8);
			var skill2 = new AttributeSkill("ASP.NET", 1, group2);
			var skill22 = new AttributeSkill("Oracle", 8, group1);
			var skill23 = new AttributeSkill("Java", 6, group2);
			var skill24 = new AttributeSkill("SQL", 3, group1);
			var skill25 = new AttributeSkill("Windows Forms", 4, group2);

			nodes.Add(new Node("Taylor Murphy", new List<AttributeBase>() { size2, skill2, skill22, skill23, skill24, skill25 }));

			//node3
			var size3 = new AttributeSize(8);
			var skill3 = new AttributeSkill("WCF", 14, group2);
			var skill33 = new AttributeSkill("Java", 12, group2);
			var skill34 = new AttributeSkill("SQL", 4, group2);

			nodes.Add(new Node("Anna Williams", new List<AttributeBase>() {size3, skill33, skill3, skill34 }));

			//node4

			var skill4 = new AttributeSkill("LINQ", 4, group2);
			var size4 = new AttributeSize(8);
			nodes.Add(new Node("Thomas Stewart", new List<AttributeBase>() {size4, skill4 }));

			return nodes;
		}

		public static void BuildGraph(double width, double height)
		{
			float? attractionForce;
			string description;

            List<Node> nodesToProcess = CreateNodes();
			
			while(nodesToProcess.Count > 0)
			{
				var startNode = nodesToProcess.First();

                //BodyFactory.CreateNode(

				nodesToProcess.Remove(startNode);

                List<AttributeSkill> startNodeSkills = (from t in startNode.Attributes
                                                        select t).OfType<AttributeSkill>().ToList();

				for(int j = 0; j < nodesToProcess.Count; j++)
				{
					var endNode = nodesToProcess[j];

					//get the skill attributes of each Node
					List<AttributeSkill> endNodeSkills = (from t in endNode.Attributes
														  select t).OfType<AttributeSkill>().ToList();
					
					//match skills based on their Names
					var commonSkills = from s1 in startNodeSkills
									   from s2 in endNodeSkills
									   where s1.Name == s2.Name
									   select new { skill1 = s1, skill2 = s2 };

					if (commonSkills.Count() > 0)
					{
						description = "";
						attractionForce = 0;

						var startNodeWeight = startNodeSkills.Sum(t => t.Weight);
						var endNodeWeight = endNodeSkills.Sum(t => t.Weight);

						foreach (var skillPair in commonSkills)
						{
							description += skillPair.skill1.Name + Environment.NewLine;
							//increase attraction force between the Nodes
							attractionForce += (skillPair.skill1.Weight / startNodeWeight) * (skillPair.skill2.Weight / endNodeWeight);
						}

						var nodeDistance = NodeHelper.GetDistanceBetweenNodes(startNode.Size, endNode.Size, attractionForce);


					}
				}
			}
		}

		/*
		/// <summary>
		/// Position the endNode and the startNode maintaining the distance to the base (connected) Node
		/// Applied generalized Pythagoras' theorem
		/// </summary>
		/// 			 A
		///				/|
		///			   / |
		///			b /  | c
		///			 /   |
		///		  C /____| B
		///			  a	
		private static void PositionConnectedNodes(ref Node startNode, ref Node endNode, double c)
		{
			//distance from base Node to start Node (e.g. node1 -> node2)
			var startNodeName = startNode.Name;
			var baseToStart = NodeDependency.ConnectedNodes.Where(n => n.EndNode.Name == startNodeName).First();
			var b = baseToStart.Distance;

			//distance from base Node to end Node (e.g. node1 -> node3)
			var endNodeName = endNode.Name;
			var baseToEnd = NodeDependency.ConnectedNodes.Where(n => n.EndNode.Name == endNodeName).First();
			var a = baseToEnd.Distance;
			
			var cosineFormula = (c * c - a * a - b * b) / (-2 * a * b);
			var angleC = Math.Acos(cosineFormula);

			var Ax = b * Math.Cos(angleC);
			var Ay = b * Math.Sin(angleC);

			var nodeCenter1 = NodeHelper.GetNodeCenter(baseToEnd.StartNode.Size, startNode.Size);
			startNode.X = baseToEnd.StartNode.X + nodeCenter1 + Ax;
			startNode.Y = baseToEnd.StartNode.Y + nodeCenter1 + Ay;

			var nodeCenter2 = NodeHelper.GetNodeCenter(baseToEnd.StartNode.Size, baseToEnd.EndNode.Size);
			endNode.Y = baseToEnd.StartNode.Y + nodeCenter2;
			endNode.X = baseToEnd.StartNode.X + nodeCenter2 + a;
		}

		//TODO: provide location when atractionForce is null
		/// <summary>
		/// Position endNode on X and Y axis relative to startNode, based on the attractionForce.
		/// Applied Pythagoras' theorem
		/// </summary>
		private static void PositionEndNode(Node startNode, ref Node endNode, double c)
		{
			double x, y; //catheti; c - ipothenuse
			
			//position the end Node relative to start Node's center at a random angle
			var angle = NodeHelper.GetRandomAngle();

			x = c * Math.Sin(angle);
			y = Math.Sqrt(c * c - x * x);

			var endNodeLocation = new Point(x, y);
			RotateNode(startNode, endNode, ref endNodeLocation);

			double nodeCenter = NodeHelper.GetNodeCenter(startNode.Size, endNode.Size);

			endNode.X = endNodeLocation.X + nodeCenter;
			endNode.Y = endNodeLocation.Y + nodeCenter;
		}

		//TODO: ensure endNode position is not outside the window
		/// <summary>
		/// Rotate endNode around startNode in order to randomize the position where endNode appears
		/// </summary>
		private static void RotateNode(Node startNode, Node endNode, ref Point endNodeLocation)
		{
			switch (random.Next(1, 5))
			{
				case 1:
					endNodeLocation.X = startNode.X + endNodeLocation.X;
					endNodeLocation.Y = startNode.Y + endNodeLocation.Y;
					break;

				case 2:
					endNodeLocation.X = startNode.X - endNodeLocation.X;
					endNodeLocation.Y = startNode.Y - endNodeLocation.Y;
					break;

				case 3:
					endNodeLocation.X = startNode.X - endNodeLocation.X;
					endNodeLocation.Y = startNode.Y + endNodeLocation.Y;
					break;

				case 4:
					endNodeLocation.X = startNode.X + endNodeLocation.X;
					endNodeLocation.Y = startNode.Y - endNodeLocation.Y;
					break;
			}
		}
		*/
	}
}