using System;
using System.Text;
using System.Threading.Tasks;
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

		public Node(string name, float size, float x, float y, List<AttributeBase> attributes)
		{
			Name = name;
			Size = size;
			X = x;
			Y = y;
			Color = "LightGreen";
			Attributes = attributes;
			IsConnected = false;
		}

		public string Name { get; set; }
		public float Size { get; set; }
		public string Color { get; set; }
		public List<AttributeBase> Attributes { get; set; }
		public bool IsConnected { get; set; }

		#region Node location

		double _x, _y;

		public double X
		{
			get { return _x; }
			set
			{
				if (value == _x)
					return;

				_x = value;
				base.RaisePropertyChanged("X");
			}
		}

		public double Y
		{
			get { return _y; }
			set
			{
				if (value == _y)
					return;

				_y = value;
				base.RaisePropertyChanged("Y");
			}
		}

		#endregion
	}
	
	/*
	public struct NodeSize
	{
		private double _width, _height;

		public NodeSize(double radius)
		{
			_width = radius;
			_height = radius;
		}

		public NodeSize(double width, double height)
		{
			_width = width;
			_height = height;
		}

		public double Width
		{
			get { return _width; }
			set { _width = value; }
		}

		public double Height
		{
			get { return _height; }
			set { _height = value; }
		}
	}
	*/
}
