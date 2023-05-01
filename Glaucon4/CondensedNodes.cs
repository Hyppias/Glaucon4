
namespace Terwiel.Glaucon
{
    public class CondensedNode
    {
        public CondensedNode(int nodeNr, int[] doFToCondense, bool active = true)
        {
            NodeNr = nodeNr;
            DoFToCondense = doFToCondense;
            Active = active;
        }
        public bool Active; 
        public int NodeNr;
        public int[] DoFToCondense = new int[6];
    }
}
