
namespace Terwiel.Glaucon
{
    using System.ComponentModel;

    /// <summary>
    /// Material properties for Glaucon
    /// </summary>
    public class Material
    {
        /// <summary>
        /// Density of the material (kg/m3)
        /// </summary>
        [Description("Density"),]
        public double Density { get; set; }

        /// <summary>
        /// Modulus of elasticity (young's) (N/mm2)
        /// </summary>
        [Description("Elastic modulus"),]
        public double E { get; set; }

        /// <summary>
        /// Shear modulus (N/mm2)
        /// </summary>
        [Description("Shear modulus"),]
        public double G { get; set; }
    }
}