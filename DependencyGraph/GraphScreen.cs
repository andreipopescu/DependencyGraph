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
            #region ...
            //Body _circle1, _circle2, _circle3;
            //Vector2 _circle1Position, _circle3Position;
            //DistanceJoint _joint1, _joint2;

            //_circle1 = BodyFactory.CreateNode(World, 2.5f, 15f, new Vector2(20, 20), "circle1");
            //_circle1.BodyType = BodyType.Dynamic;
            //_circle1.FixedRotation = true;
            //_circle1.UserData = "FirstNode";

            //_circle2 = BodyFactory.CreateNode(World, 2f, 25f, new Vector2(5, 5), "circle2");
            //_circle2.BodyType = BodyType.Dynamic;
            //_circle2.FixedRotation = true;
            //_circle2.UserData = "SecondNode";

            //_joint2 = JointFactory.CreateConnector(World, _circle1, _circle2, 3f);
            //_joint2.Length = 20f;

            //_circle3 = BodyFactory.CreateNode(World, 2f, 25f, new Vector2(15, 15), "circle3");
            //_circle3.BodyType = BodyType.Dynamic;
            //_circle3.FixedRotation = true;
            //_circle3.UserData = "TherdNode";

            //_joint1 = JointFactory.CreateConnector(World, _circle1, _circle3, 5f);
            //_joint1.CollideConnected = true;
            //_joint1.Length = 7f;

            //Body myCirlce = BodyFactory.CreateNode(World, 4f, 25f, new Vector2(), "My_New Circle");
            //myCirlce.BodyType = BodyType.Dynamic;
            //myCirlce.FixedRotation = true;
            //myCirlce.UserData = "ForthNode";

            //DistanceJoint newJoint = JointFactory.CreateConnector(World, myCirlce, _circle1, 3f);
            //newJoint.CollideConnected = true;
            //newJoint.Length = 15f;

            //Body myCircle2 = BodyFactory.CreateNode(World, 4f, 27f, new Vector2(), "Second Circle");
            //myCircle2.BodyType = BodyType.Dynamic;
            //myCircle2.FixedRotation = true;
            //myCircle2.UserData = "FifthNode";

            //DistanceJoint joint = JointFactory.CreateConnector(World, myCircle2, _circle1, 3f);
            //joint.CollideConnected = true;
            //joint.Length = 17f;

            //DistanceJoint joint2 = JointFactory.CreateConnector(World, myCircle2, myCirlce, 3f);
            //joint2.CollideConnected = true;
            //joint2.Length = 9f;
            #endregion

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

        public float CalculateNodeDistance(Employ pers1, Employ pers2)
        {
            const float correction = 1;
            float Pers1CommonSkillTotal = 0;
            float Pers2CommonSkillTotal = 0;

            foreach (var skill in pers1.Skills)
            {
                if (pers2.Skills.ContainsKey(skill.Key))
                {
                    Pers1CommonSkillTotal += skill.Value;
                    Pers2CommonSkillTotal += pers2.Skills[skill.Key];
                }
            }

            if (Pers1CommonSkillTotal == 0 || Pers2CommonSkillTotal == 0)
            {
                return 0;
            }

            double result = (pers1.Experience * pers2.Experience) / ((Pers1CommonSkillTotal / pers1.SkillsSum) * (Pers2CommonSkillTotal / pers2.SkillsSum));
            return (float)System.Math.Sqrt(result * correction);
        }

        public List<KeyValuePair<Employ, bool>> GetEmploys()
        {
            Employ Pers1 = new Employ("George Washington");
            Pers1.Experience = 10;
            Pers1.Skills = new Dictionary<string, int>();
            Pers1.Skills.Add(".NET", 2);
            Pers1.Skills.Add("C#", 3);
            Pers1.Skills.Add("SQL", 5);

            Employ Pers2 = new Employ("Samantha");
            Pers2.Experience = 8;
            Pers2.Skills = new Dictionary<string, int>();
            Pers2.Skills.Add(".NET", 5);
            Pers2.Skills.Add("SQL", 2);
            Pers2.Skills.Add("C#", 1);

            Employ Pers3 = new Employ("Jack");
            Pers3.Experience = 5;
            Pers3.Skills = new Dictionary<string, int>();
            Pers3.Skills.Add("Java", 3);


            Employ Pers4 = new Employ("Joe");
            Pers4.Experience = 6;
            Pers4.Skills = new Dictionary<string, int>();
            Pers4.Skills.Add("Java", 3);
            Pers4.Skills.Add("NoSQL", 5);

            Employ Pers5 = new Employ("Tania");
            Pers5.Experience = 8;
            Pers5.Skills = new Dictionary<string, int>();
            Pers5.Skills.Add("Java", 2);


            List<KeyValuePair<Employ, bool>> employs = new List<KeyValuePair<Employ, bool>>();
            employs.Add(new KeyValuePair<Employ, bool>(Pers1, false));
            employs.Add(new KeyValuePair<Employ, bool>(Pers2, false));
            employs.Add(new KeyValuePair<Employ, bool>(Pers3, false));
            employs.Add(new KeyValuePair<Employ, bool>(Pers4, false));
            employs.Add(new KeyValuePair<Employ, bool>(Pers5, false));

            return employs;
        }

        public class Employ
        {
            public Employ() { }

            public Employ(string Name)
            {
                this.Name = Name;
            }

            public string Name { get; set; }
            public int Experience { get; set; }
            public Dictionary<string, int> Skills;
            public int SkillsSum
            {
                get
                {
                    return Skills.Sum(s => s.Value);
                }
            }
        }
    }
}