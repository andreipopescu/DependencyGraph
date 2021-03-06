using System;
using System.Diagnostics;
//using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Transform = FarseerPhysics.Common.Transform;

namespace Endava.DependencyGraph
{
    /// <summary>
    /// A debug view that works in Silverlight.
    /// A debug view shows you what happens inside the physics engine. You can view
    /// bodies, joints, fixtures and more.
    /// </summary>
    public class DebugViewGraph : DebugView, IDisposable
    {
        private const int MaxContactPoints = 2048;

        public Color DefaultShapeColor = Color.FromArgb(255, 230, 179, 179);

        public Color InactiveShapeColor = Color.FromArgb(255, 128, 128, 77);
        public Color KinematicShapeColor = Color.FromArgb(255, 128, 128, 230);
        public Color SleepingShapeColor = Color.FromArgb(255, 153, 153, 153);
        public Color StaticShapeColor = Color.FromArgb(255, 128, 230, 128);
        private Canvas _debugCanvas;
        private int _pointCount;
        private ContactPoint[] _points = new ContactPoint[MaxContactPoints];
        private TextBlock _txtDebug;

        public DebugViewGraph(Canvas debugCanvas, TextBlock txtDebug, World world)
            : base(world)
        {
            _debugCanvas = debugCanvas;
            _txtDebug = txtDebug;
            if (world != null)
                world.ContactManager.PreSolve += PreSolve;
            Transform = new TransformGroup();

            //Default flags
            AppendFlags(DebugViewFlags.Shape);
            AppendFlags(DebugViewFlags.Joint);
        }

        public TransformGroup Transform { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            World.ContactManager.PreSolve -= PreSolve;
        }

        #endregion

        private void PreSolve(Contact contact, ref Manifold oldManifold)
        {
            if ((Flags & DebugViewFlags.ContactPoints) == DebugViewFlags.ContactPoints)
            {
                Manifold manifold = contact.Manifold;

                if (manifold.PointCount == 0)
                {
                    return;
                }

                Fixture fixtureA = contact.FixtureA;

                FixedArray2<PointState> state1, state2;
                Collision.GetPointStates(out state1, out state2, ref oldManifold, ref manifold);

                Vector2 normal;
                FixedArray2<Vector2> points;
                contact.GetWorldManifold(out normal, out points);

                for (int i = 0; i < manifold.PointCount && _pointCount < MaxContactPoints; ++i)
                {
                    if (fixtureA == null)
                    {
                        _points[i] = new ContactPoint();
                    }
                    ContactPoint cp = _points[_pointCount];
                    cp.Position = points[i];
                    cp.Normal = normal;
                    cp.State = state2[i];
                    _points[_pointCount] = cp;
                    ++_pointCount;
                }
            }
        }

        /// <summary>
        /// Call this to draw shapes and other debug draw data.
        /// </summary>
        public void DrawDebugData()
        {
            if ((Flags & DebugViewFlags.ContactPoints) == DebugViewFlags.ContactPoints)
            {
                const float axisScale = 0.3f;

                for (int i = 0; i < _pointCount; ++i)
                {
                    ContactPoint point = _points[i];

                    if (point.State == PointState.Add)
                    {
                        // Add
                        DrawPoint(point.Position, 0.1f, Color.FromArgb(255, 77, 243, 77));
                    }
                    else if (point.State == PointState.Persist)
                    {
                        // Persist
                        DrawPoint(point.Position, 0.1f, Color.FromArgb(255, 77, 77, 243));
                    }

                    if ((Flags & DebugViewFlags.ContactNormals) == DebugViewFlags.ContactNormals)
                    {
                        Vector2 p1 = point.Position;
                        Vector2 p2 = p1 + axisScale * point.Normal;
                        DrawSegment(p1, p2, Color.FromArgb(255, 102, 230, 102));
                    }
                }

                _pointCount = 0;
            }

            if ((Flags & DebugViewFlags.PolygonPoints) == DebugViewFlags.PolygonPoints)
            {
                foreach (Body body in World.BodyList)
                {
                    foreach (Fixture f in body.FixtureList)
                    {
                        PolygonShape polygon = f.Shape as PolygonShape;
                        if (polygon != null)
                        {
                            Transform xf;
                            body.GetTransform(out xf);

                            for (int i = 0; i < polygon.Vertices.Count; i++)
                            {
                                Vector2 tmp = MathUtils.Multiply(ref xf, polygon.Vertices[i]);
                                DrawPoint(tmp, 0.1f, Colors.Red);
                            }
                        }
                    }
                }
            }

            if ((Flags & DebugViewFlags.DebugPanel) == DebugViewFlags.DebugPanel)
            {
                DrawDebugPanel();
            }

            if ((Flags & DebugViewFlags.Joint) == DebugViewFlags.Joint)
            {
                foreach (Joint j in World.JointList)
                {
                    DrawJoint(j);
                }
            }

            if ((Flags & DebugViewFlags.Shape) == DebugViewFlags.Shape)
            {
                //int index = 0;

                foreach (Body b in World.BodyList)
                {
                    Transform xf;
                    b.GetTransform(out xf);

                    Color color = Colors.Transparent;
                    if (b.UserData != null)
                    {
                        color = ((BodyDescription)b.UserData).Color;
                    }

                    foreach (Fixture f in b.FixtureList)
                    {
                        DrawShape(f, xf, DefaultShapeColor, color, b.UserData);
                        //if (b.Enabled == false)
                        //{
                        //    DrawShape(f, xf, InactiveShapeColor);
                        //}
                        //else if (b.BodyType == BodyType.Static)
                        //{
                        //    DrawShape(f, xf, StaticShapeColor);
                        //}
                        //else if (b.BodyType == BodyType.Kinematic)
                        //{
                        //    DrawShape(f, xf, KinematicShapeColor);
                        //}
                        //else if (b.Awake == false)
                        //{
                        //    DrawShape(f, xf, SleepingShapeColor);
                        //}
                        //else
                        //{
                        //    DrawShape(f, xf, DefaultShapeColor);
                        //}
                    }
                }
            }

            if ((Flags & DebugViewFlags.Pair) == DebugViewFlags.Pair)
            {
                Color color = Color.FromArgb(255, 77, 230, 230);
                for (int i = 0; i < World.ContactManager.ContactList.Count; i++)
                {
                    Contact c = World.ContactManager.ContactList[i];

                    Fixture fixtureA = c.FixtureA;
                    Fixture fixtureB = c.FixtureB;

                    AABB aabbA;
                    fixtureA.GetAABB(out aabbA, 0);
                    AABB aabbB;
                    fixtureB.GetAABB(out aabbB, 0);

                    Vector2 cA = aabbA.Center;
                    Vector2 cB = aabbB.Center;

                    DrawSegment(cA, cB, color);
                }
            }

            if ((Flags & DebugViewFlags.AABB) == DebugViewFlags.AABB)
            {
                Color color = Color.FromArgb(255, 230, 77, 230);
                IBroadPhase bp = World.ContactManager.BroadPhase;

                foreach (Body b in World.BodyList)
                {
                    if (b.Enabled == false)
                    {
                        continue;
                    }

                    foreach (Fixture f in b.FixtureList)
                    {
                        for (int t = 0; t < f.ProxyCount; ++t)
                        {
                            FixtureProxy proxy = f.Proxies[t];
                            AABB aabb;
                            bp.GetFatAABB(proxy.ProxyId, out aabb);
                            Vector2[] vs = new Vector2[4];
                            vs[0] = new Vector2(aabb.LowerBound.X, aabb.LowerBound.Y);
                            vs[1] = new Vector2(aabb.UpperBound.X, aabb.LowerBound.Y);
                            vs[2] = new Vector2(aabb.UpperBound.X, aabb.UpperBound.Y);
                            vs[3] = new Vector2(aabb.LowerBound.X, aabb.UpperBound.Y);

                            DrawPolygon(vs, 4, color);
                        }
                    }
                }
            }

            if ((Flags & DebugViewFlags.CenterOfMass) == DebugViewFlags.CenterOfMass)
            {
                foreach (Body b in World.BodyList)
                {
                    Transform xf;
                    b.GetTransform(out xf);
                    xf.Position = b.WorldCenter;
                    DrawTransform(ref xf);
                }
            }
        }

        private void DrawDebugPanel()
        {
            if (_txtDebug != null)
            {
                int fixtures = 0;
                for (int i = 0; i < World.BodyList.Count; i++)
                {
                    fixtures += World.BodyList[i].FixtureList.Count;
                }

                StringBuilder output = new StringBuilder();
                output.AppendLine("Bodies: " + World.BodyList.Count);
                output.AppendLine("Fixtures: " + fixtures);
                output.AppendLine("Contacts: " + World.ContactList.Count);
                output.AppendLine("Joints: " + World.JointList.Count);
                output.AppendLine("Proxies: " + World.ProxyCount);
                output.AppendLine("Breakable: " + World.BreakableBodyList.Count);
                output.AppendLine("Controllers: " + World.ControllerList.Count);

                output.AppendLine();

                output.AppendLine("Controllers: " + World.ControllersUpdateTime);
                output.AppendLine("Contacts: " + World.ContactsUpdateTime);
                output.AppendLine("Solve: " + World.SolveUpdateTime);
                output.AppendLine("CCD: " + World.ContinuousPhysicsTime);
                output.AppendLine("Total: " + World.UpdateTime);

                _txtDebug.Text = output.ToString();
            }
        }

        private void DrawJoint(Joint joint)
        {
            Body b1 = joint.BodyA;
            Body b2 = joint.BodyB;
            Transform xf1, xf2;
            b1.GetTransform(out xf1);

            Vector2 x2 = new Vector2();

            // WIP David
            if (!joint.IsFixedType())
            {
                b2.GetTransform(out xf2);
                x2 = xf2.Position;
            }
            Vector2 p2 = joint.WorldAnchorB;
            Vector2 x1 = xf1.Position;
            Vector2 p1 = joint.WorldAnchorA;
            Color color = Color.FromArgb(255, 128, 205, 205);

		    if (joint.JointType != JointType.FixedMouse)
		    {
			    DrawSegment(x1, p1, color);
			    DrawSegment(p1, p2, color);
			    DrawSegment(x2, p2, color);
		    }
        }

        private void DrawShape(Fixture fixture, Transform xf, Color color, Color fillcolor, object Userdata)
        {
            switch (fixture.ShapeType)
            {
                case ShapeType.Circle:
                    {
                        CircleShape circle = (CircleShape)fixture.Shape;

                        Vector2 center = MathUtils.Multiply(ref xf, circle.Position);
                        float radius = circle.Radius;
                        Vector2 axis = xf.R.Col1;

                        DrawSolidCircle(center, radius, axis, color);
                    }
                    break;

                case ShapeType.Polygon:
                    {
                        PolygonShape poly = (PolygonShape)fixture.Shape;
                        int vertexCount = poly.Vertices.Count;
                        Debug.Assert(vertexCount <= Settings.MaxPolygonVertices);
                        Vector2[] vertices = new Vector2[Settings.MaxPolygonVertices];

                        for (int i = 0; i < vertexCount; ++i)
                        {
                            vertices[i] = MathUtils.Multiply(ref xf, poly.Vertices[i]);
                        }

                        if (fixture.UserData == null)
                        {
                            fixture.UserData = string.Empty; 
                        }

	                    var bodyDescription = fixture.Body.UserData as BodyDescription;
	                    if (bodyDescription == null)
	                    {
		                    DrawSolidPolygon(vertices, vertexCount, color);
	                    }
	                    else
	                    {
		                    switch (bodyDescription.FigureType)
		                    {
			                    case FigureType.Node:
									DrawNode(vertices, vertexCount, fillcolor, color, bodyDescription, fixture.Body);
				                    break;
			                    case FigureType.Tooltip:
									DrawTooltip(vertices, vertexCount, color, true, bodyDescription, fixture.Body);
				                    break;
		                    }
	                    }
                    }
                    break;

                case ShapeType.Edge:
                    {
                        EdgeShape edge = (EdgeShape)fixture.Shape;
                        Vector2 v1 = MathUtils.Multiply(ref xf, edge.Vertex1);
                        Vector2 v2 = MathUtils.Multiply(ref xf, edge.Vertex2);
                        DrawSegment(v1, v2, color);
                    }
                    break;

                case ShapeType.Loop:
                    {
                        LoopShape loop = (LoopShape)fixture.Shape;
                        int count = loop.Vertices.Count;

                        Vector2 v1 = MathUtils.Multiply(ref xf, loop.Vertices[count - 1]);
                        for (int i = 0; i < count; ++i)
                        {
                            Vector2 v2 = MathUtils.Multiply(ref xf, loop.Vertices[i]);
                            DrawSegment(v1, v2, color);
                            v1 = v2;
                        }
                    }
                    break;
            }
        }

		public void DrawTooltip(Vector2[] vertices, int count, Color color, bool outline, BodyDescription description, Body body)
		{
			if (count == 2)
			{
				DrawPolygon(vertices, count, color);
				return;
			}

			Color colorFill = Color.FromArgb((byte)(outline ? 128 : 255), color.R, color.G, color.B);

			Polygon poly = new Polygon();
			poly.Fill = new SolidColorBrush(colorFill);

			for (int i = 0; i < count; i++)
			{
				poly.Points.Add(Transform.Transform(new Point(vertices[i].X, vertices[i].Y)));
			}
			_debugCanvas.Children.Add(poly);

			if (outline)
			{
				DrawPolygon(vertices, count, color);
			}

			TextBlock text = SetTooltipText(description, body);
			if (text != null)
			{
				_debugCanvas.Children.Add(text);
			}
		}

		private TextBlock SetTooltipText(BodyDescription description, Body body)
		{
			if (!string.IsNullOrEmpty(description.Text) && description.Text != "Static")
			{
				TextBlock text = new TextBlock();
				text.Text = description.Text;
				var textSize = (Size) description.Size;
				text.Width = textSize.Width * 8;
				text.FontWeight = FontWeights.SemiBold;

				text.TextWrapping = TextWrapping.WrapWithOverflow;
				text.TextAlignment = TextAlignment.Left;
				//text.Background = new SolidColorBrush(Colors.AliceBlue);

				Point position = Transform.Transform(new Point(body.Position.X, body.Position.Y));
				Canvas.SetLeft(text, position.X - textSize.Width * 3.7);
				Canvas.SetTop(text, position.Y - textSize.Height * 8);
				return text;
			}
			else
				return null;
		}

        public void DrawNode(Vector2[] vertices, int count, Color fillColor, Color strokeColor, BodyDescription bodyDescription, Body body)
        {
            Polygon poly = new Polygon();
            poly.Fill = new SolidColorBrush(fillColor);
            poly.Stroke = new SolidColorBrush(strokeColor);


            for (int i = 0; i < count; i++)
            {
                poly.Points.Add(Transform.Transform(new Point(vertices[i].X, vertices[i].Y)));
            }

            _debugCanvas.Children.Add(poly);

            //_debugCanvas.Children.Add(SetPieChart());

			TextBlock text = SetNodeText(bodyDescription, body);
            if (text != null)
            {
                _debugCanvas.Children.Add(text);
            }
        }

		private TextBlock SetNodeText(BodyDescription bodyDescription, Body body)
		{
			if (!string.IsNullOrEmpty(bodyDescription.Text) && bodyDescription.Text != "Static")
            {
				var radius = (float)bodyDescription.Size;

                TextBlock text = new TextBlock();
				text.Text = bodyDescription.Text.Replace(' ', '\n');
                text.Width = radius * 13;
				text.FontSize = text.Width / 6;
                text.TextWrapping = TextWrapping.WrapWithOverflow;
                text.TextAlignment = TextAlignment.Center;
				//text.Background = new SolidColorBrush(Colors.AliceBlue);

                Point position = Transform.Transform(new Point(body.Position.X, body.Position.Y));
                Canvas.SetLeft(text, position.X - (radius * 6.5));
				Canvas.SetTop(text, position.Y - (radius * 3));

                return text;
            }

			return null;
		}

	    public override void DrawPolygon(Vector2[] vertices, int count, float red, float green, float blue)
        {
            DrawPolygon(vertices, count, Color.FromArgb(255, (byte)(red * 255), (byte)(green * 255), (byte)(blue * 255)));
        }

        public void DrawPolygon(Vector2[] vertices, int count, Color color)
        {
            Polygon poly = new Polygon();
            poly.Fill = new SolidColorBrush(Colors.Transparent);
            poly.Stroke = new SolidColorBrush(color);

            for (int i = 0; i < count; i++)
            {
                poly.Points.Add(Transform.Transform(new Point(vertices[i].X, vertices[i].Y)));
            }
            _debugCanvas.Children.Add(poly);
        }

        public override void DrawSolidPolygon(Vector2[] vertices, int count, float red, float green, float blue)
        {
            DrawSolidPolygon(vertices, count,
                             Color.FromArgb(255, (byte)(red * 255), (byte)(green * 255), (byte)(blue * 255)), true);
        }

        public void DrawSolidPolygon(Vector2[] vertices, int count, Color color)
        {
            DrawSolidPolygon(vertices, count, color, true);
        }

        public void DrawSolidPolygon(Vector2[] vertices, int count, Color color, bool outline)
        {
            if (count == 2)
            {
                DrawPolygon(vertices, count, color);
                return;
            }

            Color colorFill = Color.FromArgb((byte)(outline ? 128 : 255), color.R, color.G, color.B);

            Polygon poly = new Polygon();
            poly.Fill = new SolidColorBrush(colorFill);

            for (int i = 0; i < count; i++)
            {
                poly.Points.Add(Transform.Transform(new Point(vertices[i].X, vertices[i].Y)));
            }
            _debugCanvas.Children.Add(poly);

            if (outline)
            {
                DrawPolygon(vertices, count, color);
            }
        }

        public override void DrawCircle(Vector2 center, float radius, float red, float green, float blue)
        {
            DrawCircle(center, radius,
                       Color.FromArgb(255, (byte)(red * 255), (byte)(green * 255), (byte)(blue * 255)));
        }

        public void DrawCircle(Vector2 center, float radius, Color color)
        {
            Ellipse circle = new Ellipse();
            circle.Fill = new SolidColorBrush(Colors.Transparent);
            circle.Stroke = new SolidColorBrush(color);

            circle.Width = Math.Abs(radius * 2 * ((TranslateTransform)Transform.Children[1]).X);
            circle.Height = Math.Abs(radius * 2 * ((TranslateTransform)Transform.Children[1]).Y);

            Point c = Transform.Transform(new Point(center.X, center.Y));

            Canvas.SetLeft(circle, c.X - circle.Width / 2);
            Canvas.SetTop(circle, c.Y - circle.Height / 2);

            _debugCanvas.Children.Add(circle);
        }

        public void DrawText(Vector2 position, string description)
        {
            TextBlock text = new TextBlock();
            text.Text = description;


            Canvas.SetLeft(text, position.X);
            Canvas.SetLeft(text, position.Y);

            _debugCanvas.Children.Add(text);
        }

        public override void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, float red, float green,
                                             float blue)
        {
            DrawSolidCircle(center, radius, axis,
                            Color.FromArgb(255, (byte)(red * 255), (byte)(green * 255), (byte)(blue * 255)));
        }

        public void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, Color color)
        {
            Color colorFill = Color.FromArgb(128, color.R, color.G, color.B);

            Ellipse circle = new Ellipse();
            circle.Fill = new SolidColorBrush(colorFill);

            circle.Width = Math.Abs(radius * 2 * ((TranslateTransform)Transform.Children[1]).X);
            circle.Height = Math.Abs(radius * 2 * ((TranslateTransform)Transform.Children[1]).Y);

            Point c = Transform.Transform(new Point(center.X, center.Y));

            Canvas.SetLeft(circle, c.X - circle.Width / 2);
            Canvas.SetTop(circle, c.Y - circle.Height / 2);

            _debugCanvas.Children.Add(circle);

            DrawCircle(center, radius, color);

            DrawSegment(center, center + axis * radius, color);
        }

        public override void DrawSegment(Vector2 start, Vector2 end, float red, float green, float blue)
        {
            DrawSegment(start, end, Color.FromArgb(255, (byte)(red * 255), (byte)(green * 255), (byte)(blue * 255)));
        }

        public void DrawSegment(Vector2 start, Vector2 end, Color color)
        {
            Line line = new Line();
            line.Stroke = new SolidColorBrush(color);
	        line.StrokeThickness = 3;

            Point start2 = Transform.Transform(new Point(start.X, start.Y));
            Point end2 = Transform.Transform(new Point(end.X, end.Y));

            line.X1 = start2.X;
            line.Y1 = start2.Y;
            line.X2 = end2.X;
            line.Y2 = end2.Y;

            _debugCanvas.Children.Add(line);
        }

        public void DrawSegment(Vector2 start, Vector2 end, Color color, float thickness)
        {
            Line line = new Line();
            line.Stroke = new SolidColorBrush(color);
            line.StrokeThickness = thickness;

            Point start2 = Transform.Transform(new Point(start.X, start.Y));
            Point end2 = Transform.Transform(new Point(end.X, end.Y));

            line.X1 = start2.X;
            line.Y1 = start2.Y;
            line.X2 = end2.X;
            line.Y2 = end2.Y;

            _debugCanvas.Children.Add(line);
        }

        public override void DrawTransform(ref Transform transform)
        {
            const float axisScale = 0.4f;
            Vector2 p1 = transform.Position;

            Vector2 p2 = p1 + axisScale * transform.R.Col1;
            DrawSegment(p1, p2, Colors.Red);

            p2 = p1 + axisScale * transform.R.Col2;
            DrawSegment(p1, p2, Colors.Green);
        }

        public void DrawPoint(Vector2 p, float size, Color color)
        {
            Vector2[] verts = new Vector2[4];
            float hs = size / 2.0f;
            verts[0] = p + new Vector2(-hs, -hs);
            verts[1] = p + new Vector2(hs, -hs);
            verts[2] = p + new Vector2(hs, hs);
            verts[3] = p + new Vector2(-hs, hs);

            DrawSolidPolygon(verts, 4, color, true);
        }

        public void DrawAABB(ref AABB aabb, Color color)
        {
            Vector2[] verts = new Vector2[4];
            verts[0] = new Vector2(aabb.LowerBound.X, aabb.LowerBound.Y);
            verts[1] = new Vector2(aabb.UpperBound.X, aabb.LowerBound.Y);
            verts[2] = new Vector2(aabb.UpperBound.X, aabb.UpperBound.Y);
            verts[3] = new Vector2(aabb.LowerBound.X, aabb.UpperBound.Y);

            DrawPolygon(verts, 4, color);
        }

        #region Nested type: ContactPoint

        private struct ContactPoint
        {
            public Vector2 Normal;
            public Vector2 Position;
            public PointState State;
        }

        #endregion
    }
}