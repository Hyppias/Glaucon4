using MathNet.Numerics.LinearAlgebra.Double;


namespace Terwiel.Glaucon
{
    public partial class Glaucon
    {
        public partial class LoadCase
        {
            public abstract class GenericLoad<T>
            {
                public bool Active { get; set; } = true;
                public abstract DenseVector GetLoadVector();
            }
        }
    }
}
