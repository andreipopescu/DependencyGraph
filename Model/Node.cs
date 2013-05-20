using System;
using System.Linq;
using System.Collections.Generic;
using MvvmFoundation.Wpf;

namespace Endava.DependencyGraph
{
	public class Node : ObservableObject
	{
		public Node(string name)
		{
			Name = name;
			IsConnected = false;
		}

		private string Color { get; set; }

		public Node(string name, List<AttributeBase> attributes)
		{
			Name = name;
			Color = getColor();
			Attributes = attributes;
			IsConnected = false;
		}

		public float Size
		{
			get { return Attributes.OfType<AttributeSize>().First().Size; }
		}

		private string getColor()
		{
			// grupate skillurile dupa grup, facuta suma, si scos maximum
			//Attributes.OfType<AttributeSkill>().Select(x => x.Weight).Distinct()
			
			return String.Empty;
		}

		public string Name { get; set; }
		public List<AttributeBase> Attributes { get; set; }
		public bool IsConnected { get; set; }
	}
}