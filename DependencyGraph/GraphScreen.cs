using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Endava.DependencyGraph.ScreenSystem;
using System.Linq;
using System.Collections.Generic;

namespace Endava.DependencyGraph
{
    public class GraphScreen : GameScreen
    {

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Dependency graph";
        }

        public string GetDetails()
        {
            return string.Empty;
        }

        #endregion

        public override void Initialize()
        {
            World = new World(Vector2.Zero);
            base.Initialize();
        }


        public override void LoadContent()
        {
            List<Node> employlist = GraphBuilder.CreateNodes();

            List<Body> bodys = new List<Body>();
            List<DistanceJoint> joints = new List<DistanceJoint>();

            SetBodiesAndJoints(employlist, bodys, joints);

            base.LoadContent();
        }

        private void SetBodiesAndJoints(List<Node> nodes, List<Body> bodies, List<DistanceJoint> joints)
        {
            // sets bodies
            for (int i = 0; i < nodes.Count; i++)
            {
				BodyDescription bodyDescription = new BodyDescription(nodes[i].Name, nodes[i].Size, FigureType.Node);
				bodies.Add(BodyFactory.CreateNode(World, (nodes[i].Size / 3), new Vector2(), bodyDescription));
            }

            // sets joints
            int counter = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < nodes.Count; j++)
                {
                    if ((nodes[j].IsConnected != true) && (nodes[i].Name != nodes[j].Name))
                    {
                        //float length = CalculateNodeDistance(nodes[i], nodes[j]);
                        KeyValuePair<float, string> commonSkills = CalculateNodeDistance(nodes[i], nodes[j]);

                        if (commonSkills.Key > 0)
                        {
                            joints.Add(JointFactory.CreateConnector(World, bodies[i], bodies[j], 3f));
                            joints[counter].CollideConnected = true;
                            joints[counter].Length = commonSkills.Key;
                            joints[counter].UserData = commonSkills.Value;
                            counter++;
                        }
                    }
                }

                nodes[i].IsConnected = true;
            }
        }

        public KeyValuePair<float, string> CalculateNodeDistance(Node pers1, Node pers2)
        {
            const float correction = 1;
            float Pers1CommonSkillTotal = 0;
            float Pers2CommonSkillTotal = 0;
            string commonSkills = string.Empty;

            List<AttributeSkill> pers1attr = pers1.Attributes.OfType<AttributeSkill>().ToList();
            List<AttributeSkill> pers2attr = pers2.Attributes.OfType<AttributeSkill>().ToList();

            for (int i = 0; i < pers1attr.Count; i++)
            {
                if (pers2attr.Exists(e => e.Name == pers1attr[i].Name))                       
                {
                    Pers1CommonSkillTotal += pers1attr[i].Weight ?? 0;
                    Pers2CommonSkillTotal += (pers2attr.Where(w => w.Name == pers1attr[i].Name)).FirstOrDefault().Weight ?? 0;
                    commonSkills += pers1attr[i].Name + "\n";
                }
            }

            if (Pers1CommonSkillTotal == 0 || Pers2CommonSkillTotal == 0)
            {
                return new KeyValuePair<float,string>(0, string.Empty);
            }

            float pers1SkillSum = pers1attr.Sum(s => s.Weight) ?? 0;
            float pers2SkillSum = pers2attr.Sum(s => s.Weight) ?? 0;

            double result = (pers1.Size * pers2.Size) / ((Pers1CommonSkillTotal / pers1SkillSum) * (Pers2CommonSkillTotal / pers2SkillSum));

            //return (float)System.Math.Sqrt(result * correction);
            return new KeyValuePair<float, string>((float)System.Math.Sqrt(result * correction), commonSkills);
        }
    }
}