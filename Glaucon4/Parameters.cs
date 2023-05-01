


namespace Terwiel.Glaucon
{

    /// <summary>
    /// a list of parameters used by Glaucon:
    /// </summary>
    public struct Parameters
    {
        #region PublicFields

        public bool KeepLog;
        public string LogFilename ;
        public double PanRate; // for animation
        /// <summary>
        /// limit above which members are in danger of buckling.
        /// </summary>
        public double StrainLimit;
        public double Scale;
        public double Shift;
        /// <summary>
        /// account for geometric (in)stability?
        /// </summary>
        public bool AccountForGeomStability ;
        public bool ConsistentMassMatrix;
        /// <summary>
        /// Equilibrium tolerance for geometric stability calculation
        /// </summary>
        public double EquilibriumTolerance;

        /// <summary>
        /// actual nr. of iterations of geometric stability calculation
        /// </summary>
        public int Iterations;

        /// <summary>
        /// maximum iteration loops for geometric stability calculation
        /// </summary>
        public int MaximumIterations;

        /// <summary>
        /// actual equilibrium error
        /// </summary>
        public double EquilibriumError;

        /// <summary>
        /// iteration tolerance
        /// </summary>
        public double Tolerance;

        /// <summary>
        /// execute the analysis?
        /// </summary>
        public bool Analyze;

        /// <summary>
        /// do some input validation?
        /// </summary>
        public bool Validate ;

        /// <summary>
        /// account for shear effects?
        /// </summary>
        public bool AccountForShear;

        /// <summary>
        /// use a lumped or else a consistent mass matrix?
        /// </summary>
        public bool LumpedMassMatrix;

        /// <summary>
        /// minimum eigenvalue wanted
        /// </summary>
        public double MinEigenvalue;

        /// <summary>
        /// maximum eigenvalue wanted
        /// </summary>
        public double MaxEigenvalue ;

        /// <summary>
        /// limit vibration time to look for
        /// </summary>
        public double MaxVibrationTime;

        /// <summary>
        /// nr. of frequencies found
        /// </summary>
        public int FrequenciesFound;

        /// <summary>
        /// are there members with a critical axial strain? (buckling danger)
        /// This number should be 0
        /// </summary>
        public int AxialStrainWarning;

        /// <summary>
        /// The q-Loads given are in local coordinate system
        /// </summary>
        public bool QLoadsLocal;

        /// <summary>
        /// duration of the analysis
        /// </summary>
        public long Ticks;

        /// <summary>
        /// step size for peak values of internal forces. If less than or equal to 0, No internal peak forces are calculated
        /// </summary>
        public double XIncrement ;

        /// <summary>
        /// Find eigenfrequencies?
        /// </summary>
        public bool DoModal;
        public string Title;
        public double ModalExaggeration;
        /// <summary>
        /// which method to use for modal analysis:
        /// 1 = FEAST algorithm:
        /// <see cref="http://www.feast-solver.org/"/>
        /// 2 (default) = LAPACKE_dsygv:  get ALL eigenvalues and optionally eigenvectors
        /// 3 = LAPACKE_dsygvx: computes selected eigenvalues, and optionally, eigenvectors
        /// </summary>
        public int ModalMethod ;
        public int CondensationMethod;
        public double DeformationExaggeration;
        public int DynamicModesCount;
        #endregion

        public double ModalConvergenceTol { get; set; }
        public double ResidualTolerance { get; set; }

        // uniformly distr. loads may be input relative to the
        // local (true) or to the global (false) coordiante system:
        public bool UnifLoadsLocal { get; set; }

        public int OutputFormat { get; set; }

        public bool RenumNodes { get; set; }
        public int MaxSegmentCount { get; set; }
        public int MinimumIterations { get; set; }

        public int LoopCount { get; set; }
        public int Decimals { get; set; }
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public string InputFileName { get; set; }
        public int InputSource { get; set; }

    }

}
