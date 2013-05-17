using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Endava.DependencyGraph
{
	public enum FigureType
	{
		Default = -1,
		Tooltip = 0,
		Node = 1,
	}

	public class BodyDescription
	{
		public BodyDescription(string text, float radius, FigureType figure)
		{
			Text = text;
			Radius = radius;
			FigureType = figure;
		}
		public string Text { get; set; }
		public float Radius { get; set; }
		public FigureType FigureType { get; set; }
	}
}
