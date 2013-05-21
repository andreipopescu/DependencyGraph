using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

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
		public BodyDescription(string text, object size, FigureType figure, Color color)
		{
			Text = text;
			Size = size;
			FigureType = figure;
            Color = color;
		}
		public string Text { get; set; }
		public object Size { get; set; }
		public FigureType FigureType { get; set; }
        public Color Color { get; set; }
	}
}
