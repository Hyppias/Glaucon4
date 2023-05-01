

using System.ComponentModel.Design;

namespace Terwiel.Glaucon
{

    public class ExtraNodeInertia
    {
        public ExtraNodeInertia(int nodeNr, double[] inertia, bool active = true)
        {
            NodeNr = nodeNr;
            Inertias = inertia;
            Active = active;
        }
        public bool Active;
        public int NodeNr;
        public double[] Inertias = new double[4]; // 4x
    }
}
