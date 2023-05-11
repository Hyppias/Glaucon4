
namespace Terwiel.Glaucon
{
    public class CondensedNode
    {
        public CondensedNode(int nodeNr, int[] doFToCondense, bool active = true)
        {
            NodeNr = nodeNr;
            DoFs = doFToCondense;
            Active = active;
        }
        public bool Active; 
        public int NodeNr;
        
        // This is an array containing the 6 DoFs that may or
        // may not be condensed out.
        // only the 1's are stored in the array DoFsToCondense and counted. 
        public int[] DoFs = new int[6];
    }
}
