using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmoustCourseWork
{
    public class Node
    {
        public int[] State { get; }
        public int Depth { get; }
        public int F { get; set; }

        public Node(int[] state, int depth, int h)
        {
            State = state;
            Depth = depth;
            F = depth + h;
        }
    }
}
