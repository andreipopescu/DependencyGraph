using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MvvmFoundation.Wpf;

namespace Endava.DependencyGraph
{
	public class AttributeBase//: ObservableObject
	{
		public string Name {get; set; }

		public AttributeBase(string name)
		{
			Name = name;
		}
	}

	public class AttributeSkill : AttributeBase
	{
		public float? Weight { get; set; }
		public Group Group { get; set; }

		public AttributeSkill(string name, float? weight) : base(name)
		{
			Weight = weight;
		}

		public AttributeSkill(string name, float? weight, Group group) : base(name)
		{
			Weight = weight;
			Group = group;
		}
	}

	public class AttributeTemporal : AttributeBase
	{
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }

		public AttributeTemporal(string name, DateTime? start, DateTime? end) : base(name)
		{
			Start = start;
			End = end;
		}
	}
}
