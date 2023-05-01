

namespace Terwiel.Glaucon
{
    using MathNet.Numerics.LinearAlgebra.Double;

    public partial class Glaucon
    {
        public partial class LoadCase
        {
            /// <summary>
            /// A point load acting on/at a node.
            /// This is the load on a node.
            /// <see cref="http://svn.code.sourceforge.net/p/frame3dd/code/trunk/doc/Frame3DD-manual.html"/>
            /// </summary>
            public partial class NodeLoad
            {
                /// <summary>
                /// three components of the point load: X, Y ans Z.
                /// </summary>
                public DenseVector Load = new(6);

                /// <summary>
                /// The member nr that the load is on.
                /// </summary>
                public int NodeNr;                

                public DenseVector GetLoadVector(double Ksz, double Ksy, double Ln)
                {
                   return null;
                }
            }
        }
    }
}
