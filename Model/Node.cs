using System;
using System.Linq;
using System.Collections.Generic;
using MvvmFoundation.Wpf;
using System.Windows.Media;

namespace Endava.DependencyGraph
{
    public class Node
    {
        public string Name { get; set; }
        public List<AttributeBase> Attributes { get; set; }
        public bool IsConnected { get; set; }

        public Node(string name)
        {
            Name = name;
            IsConnected = false;
        }

        public Node(string name, List<AttributeBase> attributes)
        {
            Name = name;
            //Color = getColor();
            Attributes = attributes;
            IsConnected = false;
        }

        public float Size
        {
            get { return Attributes.OfType<AttributeSize>().First().Size; }
        }

        public Color Color
        {
            get
            {
                string group = GetPrimeGroup(Attributes.OfType<AttributeSkill>());

                switch (group)
                {
                    case "Group1":
                        return Colors.Orange;
                        break;
                    case "Group2":
                        return Colors.YellowGreen;
                        break;
                    default:
                        return Colors.Transparent;
                        break;
                }
            }
        }

        private string GetPrimeGroup(IEnumerable<AttributeSkill> skills)
        {
            Dictionary<string, float?> weight = new Dictionary<string, float?>();

            foreach (var skill in skills)
            {
                if (!weight.ContainsKey(skill.Group.Name))
                {
                    weight.Add(skill.Group.Name, skill.Weight);
                }
                else
                {
                    weight[skill.Group.Name] += skill.Weight;
                }
            }

            return weight.OrderByDescending(o => o.Value).First().Key;
        }
    }
}