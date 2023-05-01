

namespace Terwiel.Glaucon
{
    public class NodeRestraint
    {
        public NodeRestraint(int nd, int[] restr, bool active = true)
        {
            NodeNr = nd;
            Restraints = restr;
            Active = active;
        }

        public bool Active;
        public int NodeNr;
        public int[] Restraints = new int[6];
    }
}
