﻿using FarseerPhysics.Dynamics;
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

        private void SetBodiesAndJoints(List<Node> employlist, List<Body> bodys, List<DistanceJoint> joints)
        {
            // sets bodys
            for (int i = 0; i < employlist.Count; i++)
            {
                bodys.Add(BodyFactory.CreateNode(World, (employlist[i].Size / 3), 25f, new Vector2(), employlist[i].Name));
                bodys[i].BodyType = BodyType.Dynamic;
                bodys[i].FixedRotation = true;
                bodys[i].UserData = employlist[i].Size;
            }

            // sets joints
            int counter = 0;
            for (int i = 0; i < employlist.Count; i++)
            {
                for (int j = 0; j < employlist.Count; j++)
                {
                    if ((employlist[j].IsConnected != true) && (employlist[i].Name != employlist[j].Name))
                    {
                        float lenght = CalculateNodeDistance(employlist[i], employlist[j]);

                        if (lenght > 0)
                        {
                            joints.Add(JointFactory.CreateConnector(World, bodys[i], bodys[j], 3f));
                            joints[counter].CollideConnected = true;
                            joints[counter].Length = lenght;
                            counter++;
                        }
                    }
                }

                employlist[i].IsConnected = true;
            }
        }

        public float CalculateNodeDistance(Node pers1, Node pers2)
        {
            const float correction = 1;
            float Pers1CommonSkillTotal = 0;
            float Pers2CommonSkillTotal = 0;

            List<AttributeSkill> pers1attr = pers1.Attributes.OfType<AttributeSkill>().ToList();
            List<AttributeSkill> pers2attr = pers2.Attributes.OfType<AttributeSkill>().ToList();

            for (int i = 0; i < pers1attr.Count; i++)
            {
                if (pers2attr.Exists(e => e.Name == pers1attr[i].Name))                       
                {
                    Pers1CommonSkillTotal += pers1attr[i].Weight ?? 0;
                    Pers2CommonSkillTotal += (pers2attr.Where(w => w.Name == pers1attr[i].Name)).FirstOrDefault().Weight ?? 0;
                }
            }

            if (Pers1CommonSkillTotal == 0 || Pers2CommonSkillTotal == 0)
            {
                return 0;
            }

            float pers1SkillSum = pers1attr.Sum(s => s.Weight) ?? 0;
            float pers2SkillSum = pers2attr.Sum(s => s.Weight) ?? 0;

            double result = (pers1.Size * pers2.Size) / ((Pers1CommonSkillTotal / pers1SkillSum) * (Pers2CommonSkillTotal / pers2SkillSum));

            return (float)System.Math.Sqrt(result * correction);
        }
    }
}