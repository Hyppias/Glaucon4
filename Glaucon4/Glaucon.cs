#region FileHeader
// Project: Glaucon4
// Filename:   Glaucon.cs
// Last write: 4/30/2023 2:29:30 PM
// Creation:   4/24/2023 11:59:08 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Xml.Serialization;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Terwiel.Glaucon
{

    [XmlRoot("Glaucon")]
    public partial class Glaucon
    {
        #region Methods

        /// <Param name="endforces">resulting member end forces vector</Param>
        /// ///
        /// <Param name="reactions">resulting reaction forces vector</Param>
        /// <Param name="deflection">resulting node deflections vector</Param>
        /// <returns>0 if all OK, 1 if not</returns>
        public int Execute(ref DenseMatrix deflection, ref DenseMatrix reactions,
            ref DenseMatrix endforces)
        {
            StartClock(0);

            Control.NativeProviderPath = nativeProviderPath;

            Control.UseNativeMKL();

            try
            {
                InitStaticMesh(); // general plot data and undeformed mesh
                                  // everything is read, start calculation:
                if (Param.Analyze)
                {
                    K = new DenseMatrix(DoF); // global stiffness matrix

                    foreach (var ldc in LoadCases)
                    {
                        ldc.Solve(Members);

                        // only if there are loadcases to investigate  add the deformed mesh
                        // That is, write plot commands to the control script and
                        // write a separate script with coordinates etc., per load case
                        DeformedMeshLC(ldc);
                    }

                    deflection = LoadCases[0].Displacements;
                    reactions = LoadCases[0].Reactions;
                    endforces = LoadCases[0].Q;

                    if (Param.ModalMethod != None)
                    {
                        // K was set up during load case processing:
                        K.PermuteColumns(Perm.Inverse());
                        K.PermuteRows(Perm.Inverse());
                        ModalAnalysis();
                        // now we have eigenFreq
                        ModalMesh();
                        Animate(0);
                    }
                }

                // matrix condensation of stiffness and mass
                if (CondensedNodes.Count > 0)
                {
                    switch (Param.CondensationMethod)
                    {
                        case Static:
                            // condensation only
                            StaticCondensation();
                            break;
                        case Modal:
                            var Cfreq = eigenFreq[MatchedCondenseModes[0]];
                            PazCondensation(Cfreq);
                            WriteMatrix(COND, "", "Mc", "Mc", Mc);
                            break;
                        case Dynamic:
                            ModalCondensation();
                            WriteMatrix(COND, "", "Mc", "Mc", Mc);
                            break;
                    }

                    WriteMatrix(COND, "", "Kc", "Kc", Kc);
                }

                try
                {
                    switch (Param.OutputFormat)
                    {
                        case XML:
                            WriteXML(Param.OutputPath + BaseFile + ".xml");
                            break;

                        case HTML:
                            BuildHTML(Param.OutputPath + BaseFile + ".html");
                            break;
                        case CSV:
                            BuildCSV(Param.OutputPath + BaseFile + ".csv");
                            break;
                        case Latex:
                            BuildLatex(Param.OutputPath + BaseFile + ".tex");
                            break;
                        case Excel:
                            BuildExcel(Param.OutputPath + BaseFile + ".xslx");
                            break;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Error writing output" + e.Message, e.InnerException);
                }
            }
            catch (Exception ex)
            {
                Lg(ex.Message);
                return 1;
            }

            StopClock(0, "Elapsed computing time:");

            return 0; // all OK
        } // end Execute

        public static T Sq<T>(T a)
        {
            return (dynamic)a * a;
        }

        [Conditional("DEBUG")]
        public static void StartClock(int nr)
        {
            if (sw[nr] == null)
            {
                sw[nr] = Stopwatch.StartNew();
            }
        }

        [Conditional("DEBUG")]
        public static void StopClock(int Nr, string message)
        {
            if (sw[Nr] != null)
            {
                sw[Nr].Stop();
                Debug.WriteLine($"{message} takes {sw[Nr].ElapsedMilliseconds} msec");
                sw[Nr] = null;
            }
        }

        #endregion

        #region PublicFields

        /// <summary>
        /// number of condensed degrees of freedom.
        /// </summary>
        public int Cdof;

        public double TotalMass;
        public double StructMass;

        /// <summary>
        /// list of DoF's to condense.
        /// </summary>
        public int[] DoFToCondense;

        public List<ExtraNodeInertia> ExtraNodeInertias;
        public List<ExtraElementMass> ExtraElementMasses;
        public List<CondensedNode> CondensedNodes;
        public int[] AnimatedModes; // anim[]
        public int[] MatchedCondenseModes;

        #endregion


        #region StaticFields
        /// <summary>
        /// The parameters for Glaucon.
        /// </summary>
        public static Parameters Param;

#if DEBUG

        /// <summary>
        /// Where the MatLab file goes: current folder
        /// </summary>
        private static string matlab = @".";

        /// <summary>
        /// A prefix for the MatLab file.
        /// </summary>
        private static string prefix;

#endif



        /// <summary>
        /// the System stiffness matrix
        /// </summary>
        public static DenseMatrix SSM;

        /// <summary>
        /// Gets or sets the System mass matrix
        /// </summary>

        public static DenseMatrix SMM;

        /// <summary>
        /// for rearranging the stiffness, displacements and forces matrices:
        /// </summary>

        private static Permutation perm;

        /// <summary>
        /// reaction data, total no. of restraints , partition of matrices
        /// </summary>
        private static int matrixPart;

        /// <summary>
        /// The number of degrees of freedom of the construction.
        /// </summary>

        public static int DoF; // number of Degrees of Freedom

        /// <summary>
        /// Set to true if the construction has no
        /// restraints (= free-floating).
        /// </summary>
        private static bool constructionUnrestrained;

        /// <summary>
        /// TODO The index.
        /// </summary>
        //private static int[] index;

        /// <summary>
        /// global restraints vector.
        /// six restraints per node.
        /// </summary>
        //private static bool[] restraints;

        #endregion

        #region PrivateFields

        /// <summary>
        /// Gets or sets the eigenfrequencies
        /// </summary>
        private DenseVector frequencies;

        #endregion

        #region Fields

#if DEBUG
        // selectively write matrices
        private const bool EIGEN = false;
        private const bool LOADCASE = true;
        private const bool MINMAX = false;
        private const bool MEMBER = false;
        private const bool COND = false;
        private const bool MODAL = false;
        private const bool GAMMA = false;

        public static Stopwatch[] sw = new Stopwatch[10];

#endif

        private string nativeProviderPath =
            @"E:\Users\erik\Documents\Visual Studio 2022\Projects\Glaucon4\Glaucon4\bin\Debug\net6.0-windows";

        // destination for the matrices to be written:
        private string debugPath;
        public static string Prefix, Mprefix;
        public double MachinePrecision;
        private CultureInfo culture;

        public double TraceK, TraceM;

        public int[] Index, RevIndex;

        private const bool ReArrange = true;

        public static List<string> Errors = new();

        public DenseMatrix Kc, Mc; // condensed stiffness and mass matrices

        public static double
            // x-increment for peak forces.
            // Redefined from Frame3DD: if <0 then no min/max force calculation
            // to avoid FP equal comparisons.
            // So: less than 0: no min/max
            // positive including 0 do min/max
            deltaX = -1, Shift; // shift-factor for rigid-body-modes

        // for rearranging the stiffness, displacements and forces matrices:
        public static Permutation Perm;

        /// <summary>
        /// reaction data, total no. of restraints , partition of matrices
        /// </summary>
        //private static int matrixPart;

        public static int
            //BandWidth,
            MinRestraints, // nr of restrained DOFs
            MatrixPart; // reaction data, total no. of Restraints , partition of matrices

        public string BaseFile, ProgramName, ProgramVersion;

        #endregion

        #region Properties

        [XmlAttribute("AnimationModes"), JsonProperty("AnimationModes")]
        [Description("mode number to animate")]
        public int[] AnimationModes { get; set; }

        //[XmlAttribute("Pan"), JsonProperty("Pan")]
        //[Description("pan rate during animation")]
        //public double Pan { get; set; }

        [JsonProperty("Dx")]
        [Description("Dx")]
        public int DeltaX { get; set; }

        public int ModalMethod = ALL;

        //[XmlAttribute("CondModesCnt"), JsonIgnore]
        //[Description("number of condensed nodes")]
        //public int nC { get; set; } // number of condensed nodes

        [XmlAttribute("Dimensionality"), JsonIgnore]
        [Description("dimensions of the construction: X, Y, Z")]
        public string Space { get; set; }

        [XmlElement("Title"), JsonProperty("Title")]
        [Description("Description of the construction")]
        public string Title { get; set; }

        [XmlArray("LoadCases"), JsonProperty("LoadCases")]
        [XmlArrayItem("Loadcase")]
        [Description("Load case")]
        public List<LoadCase> LoadCases { get; set; }

        [XmlArray("Nodes")]
        [XmlArrayItem("Node"), JsonProperty("Nodes")]
        [Description("List of structure nodes")]
        public List<Node> Nodes { get; set; }

        [XmlArray("Members")]
        [JsonProperty("Members")]
        [XmlArrayItem("Member")]
        [Description("list of structure members")]
        public List<Member> Members { get; set; }

        [XmlArray("NodeRestraints")]
        [JsonProperty("NodeRestraints")]
        [XmlArrayItem("NodeRestraint")]
        [Description("list of NodeRestraints")]
        public List<NodeRestraint> NodeRestraints { get; set; }

        [XmlArray("NodeToCondense")]
        [JsonProperty("NodeToCondense")]
        [XmlArrayItem("NodeToCondense")]
        [Description("list of nodes to condense")]
        public List<CondensedNode> NodesToCondense { get; set; }

        public List<NodeRestraint> NodesRestraints;

        public int[] Restraints;

        #endregion

        #region Constructors

        public Glaucon()
        {
        }

        public Glaucon(
            string title,
            List<Node> nodes,
            List<(int, int[])> restraints,
            List<Member> members,
            Parameters parm
            )
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var fieVersionInfo = FileVersionInfo.GetVersionInfo(executingAssembly.Location);
            ProgramName = executingAssembly.GetName().Name;
            ProgramVersion = fieVersionInfo.FileVersion;
            culture = new CultureInfo("en-US");
            Param = parm;

            try
            {
                Control.NativeProviderPath = nativeProviderPath;
                Control.UseNativeMKL();
            }
            catch (Exception ex)
            {
                Errors.Add(ex.Message);
            }

            //Debug.WriteLine(Control.LinearAlgebraProvider.ToString());
            Debug.Write(Control.Describe());
            //Debug.WriteLine($"Machine precision {MachinePrecision = NativeMethods.MachinePrecision('E')}");
            //Debug.WriteLine($"Nr. of digits in mantissa {NativeMethods.MachinePrecision('n')}");
            //Debug.WriteLine($"Base of the machine {NativeMethods.MachinePrecision('B')}");
            BaseFile = Path.GetFileNameWithoutExtension(Param.InputFileName);
            if (!Directory.Exists(Param.InputPath))
            {
                throw new DirectoryNotFoundException($"{Param.InputPath} does not exist");
            }

            if (!File.Exists($"{Param.InputPath}{Param.InputFileName}"))
            {
                throw new FileNotFoundException($"{Param.InputFileName} not found.");
            }

            if (!Directory.Exists(Param.OutputPath))
            {
                Directory.CreateDirectory(Param.OutputPath);
            }

            if (parm.InputSource.Equals("OBJECT"))
            {

                ProcessInput();
            }
            else
            {
                Param = ReadJsonFile<Parameters>($"{parm.InputPath}Params.json", $"{parm.InputPath}ParamSchemaFile.json");
                ReadDefaultInput(this, $"{parm.InputPath}DefaultInputData.json");
                ProcessJsonInput();
            }

            if (Param.RenumNodes)
            {
                RenumNodes();
            }

        }

        #endregion

        #region Methods

        void ProcessJsonInput()
        {

        }

        void ProcessInput()
        {

#if DEBUG
            var s = new Stopwatch();
            s.Start();
#endif
            //Lg(Param.title);
            // correct restraints:
            var dim = new int[3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 1; j < Nodes.Count; j++)
                {
                    var node = Nodes[j];
                    if (node.Nr >= Nodes.Count) // check node number, while we are here
                    {
                        throw new ArgumentException($"Wrong node nr {node.Nr}");
                    }
                    if (Nodes[0].Coord[i] != Nodes[j].Coord[i])
                    {
                        dim[i] = 1;// dimension X, Y or Z is present
                        Lg($"{("XYZ"[i])} dimension is present");
                        break;
                    }
                }
            }
            for (int j = 0; j < Nodes.Count; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (dim[i] == 0)
                    {
                        // if diemnsion[i] is not present, the corresponding DoF must be 'fixed' (=1)
                        Nodes[j].Restraints[i] = Nodes[j].Restraints[i + 3] = 1;
                        Lg($"Node {Nodes[j].Nr}, Restraints {i} and {i + 3} corrected.");
                    }
                }
            }

            DoF = Nodes.Count * 6;// total number of degrees of freedom ( 6 per node)
            Lg($"{DoF} dregrees of freedom.");

            //SupportedNodesCount = NodeRestraints.Count; // Nr of restrained nodes
            Lg($"{NodeRestraints.Count} restrained nodes.");
            // now set up the DoF vector:
            matrixPart = DoF;
            Restraints = new int[DoF];
            // NodeRestraints is a list of only restrained nodes
            foreach (var nr in NodeRestraints)
            {
                var nodeNr = nr.NodeNr;
                var restr = nr.Restraints; // int[6]
                for (var j = 0; j < 6; j++)
                {
                    // read restraints: 0 = not restrained (free), other value = fixed = false.
                    var n = restr[j];
                    // Restraints is  int[DoF]
                    Restraints[(nodeNr * 6) + j] = n;
                    // one of the 6 degrees of freedon of the construction may not be
                    // unconstrained:
                    matrixPart -= n == 0 ? 0 : 1;
                }
            }

            // partition stiffness matrix / force vector:
            // index[i] says where a vector element or matrix row/column should go when permuting.
            var index = new int[DoF]; // use for Permutation
            var j1 = 0;
            var j2 = 0;
            for (var i = 0; i < DoF; i++)
            {
                index[i] = Restraints[i] != 0 ? j1++ : matrixPart + j2++;
            }

            perm = new Permutation(index);

            Lg($"{Members.Count} members.");

            if (Param.Validate)
            {
                for (var i = 0; i < Nodes.Count; i++)
                {
                    if (!Nodes[i].Active)
                    {
                        throw new ArgumentOutOfRangeException("Node Not Used");
                    }
                }
                foreach (var mbr in Members)
                {
                    for (var j = mbr.Nr + 1; j < Members.Count; j++)
                    {
                        if (mbr.NodeA == Members[j].NodeA && mbr.NodeB == Members[j].NodeB)
                        {
                            throw new ArgumentOutOfRangeException($"Two members are the same: {mbr.Nr + 1} and {j + 1}");
                        }
                    }
                }
                foreach (var lc in LoadCases)
                {
                    foreach (var displ in lc.PrescrDisplacements)
                    {
                        var restr = Nodes[displ.NodeNr ].Restraints;
                        for (int i = 0; i < 6; i++)
                        {
                            if(restr[i] == 0 &&  Globals.AlmostEqual(displ.Displacements[i], 1e-12))
                            {
                                string m = "";
                                restr[i] = 1;
                                Lg(m = $"Load case {lc.Nr+1}, displacement {i+1} for node {displ.NodeNr+1} too small.\n"
                                    + " Is set as fixed");
                                throw new ArgumentOutOfRangeException(m);
                            }
                        }
                    }
                }
            }

            // Parallel.ForEach(members, mb => { mb.SetupGamma(nodes[mb.NodeA], nodes[mb.NodeB]); });

            // Param.Xincr = buffer.ReadDouble();
            // Only now can we set up the shear deformation factors:

            //Parallel.ForEach(members, (mb => { mb.SetupShearDeformationFactors(); }));

            // # of load cases

            Lg($"{LoadCases.Count} load cases.");

            // number of desired dynamic modes of vibration = 3
            if (Param.DynamicModesCount > 0)
            {
                foreach (var eni in ExtraNodeInertias)
                {
                    var nd = eni.NodeNr;
                    Debug.Assert(nd < Nodes.Count, $"Node number error reading mode {nd + 1} mass");
                    Nodes[nd].ReadNodeExtraMass(eni.Inertias);
                }
                foreach (var emm in ExtraElementMasses)
                {
                    var mbr = Members[emm.MemberNr];
                    mbr.EMs = emm.Mass;
                }
                foreach (var mbr in Members)
                {
                    totalMass += mbr.TotalMass;
                    structMass += mbr.Mass;
                }
            }
            
            ReadCondensationData();



#if DEBUG
            s.Stop();
            Debug.WriteLine($"Elapsed computing time for processing input: {s.ElapsedMilliseconds} msec.");
#endif
        }
    }

#if false
    /// <summary>
    /// Input has been read. Now prepare for calculation
    /// </summary>
    private void ProcessInput2()
    {
        StartClock(1);
        try
        {
            Space = "";
            for (var j = 0; j < 3; j++)
            {
                double c = Nodes[0].Coord[j];
                for (var i = 1; i < nN; i++)
                {
                    if (Nodes[i].Coord[j] != c)
                    {
                        Space += "XYZ"[j];
                        break;
                    }
                }
            }

            if (MatrixPart == DoF)
            {
                throw new InvalidOperationException("Construction is not restrained");
            }

            // cannot do analysis on unrestrained structure.
            // partition stiffness matrix / force vector:
            // index[i] says where a vector element or matrix row/column should go when permutating.
            Index = new int[DoF]; // use for Permutation
            RevIndex = new int[DoF];
            var j1 = 0;
            var j2 = 0;
            for (var i = 0; i < DoF; i++)
            {
                Index[i] = Restraints[i] ? MatrixPart + j2 : j1;
                RevIndex[Restraints[i] ? MatrixPart + j2++ : j1++] = i;
            }

            Perm = new Permutation(Index);

            //BandWidth *= 6;

            if (Param.Validate)
            {
                for (var i = 0; i < Nodes.Count; i++)
                {
                    if (!Nodes[i].active)
                    {
                        throw new Exception($"Node {i + 1} is not used.");
                    }
                }

                for (var i = 0; i < nE - 1; i++)
                {
                    var mbr = Members[i];
                    for (var j = i + 1; j < nE; j++)
                    {
                        if (mbr.NodeA == Members[j].NodeA && mbr.NodeB == Members[j].NodeB)
                        {
                            throw new Exception($"Member {i + 1} and member {j + 1} have same nodes.");
                        }
                    }
                }
            }

            // Only now can we set up the Shear deformation factors,
            // which are by default resp. 0 and 1:
            if (IncludeShear)
            {
                foreach (var mbr in Members) //Parallel.ForEach(Members, (mb =>
                {
                    mbr.SetupShearDeformationFactors();
                }
            }

            for (int i = 0; i < LoadCases.Count; i++)
            {
                // create and read the entire load case:
                LoadCases[i] = new LoadCase(i, Members) { IncludeShear = this.IncludeShear };
                if (deltaX > 0)
                {
                    LoadCases[i].IntForces = new DenseMatrix[Members.Count];
                    LoadCases[i].TransvDispl = new DenseMatrix[Members.Count];
                    foreach (var mbr in Members)
                    {
                        mbr.SetupMinMaxCalculation(LoadCases[i]);
                    }
                }
            }

            if (Param.ModalMethod != None)
            {
                nM = buffer.ReadInt32(); // number of desired dynamic modes of vibration
                if (nM > 0)
                {
                    ReadMassData();
                }
                else
                {
                    Param.ModalMethod = None;
                }
            }

            // nr of modes to animate
            nA = buffer.ReadInt32();
            Anim = new int[nA];
            for (var i = 0; i < nA; i++)
            {
                Anim[i] = buffer.ReadInt32() - 1;
            }

            Pan = buffer.ReadSingle();

            ReadCondensationData();
        }

        catch (InvalidOperationException ex)
        {
            throw ex;
        }
        catch (Exception ex)
        {
            Errors.Add("\t" + ex.Message + Environment.NewLine + ex.StackTrace);
            var e = ex.InnerException;
            while (e != null)
            {
                Errors.Add("\t" + e.Message + Environment.NewLine);
                e = e.InnerException;
            }
#if DEBUG
            Debug.WriteLine(Errors);
#endif
        }

        StopClock(1, "Calculation");
    }

#endif
#if false
        private void ReadBuffer(byte[] bf)
        {
            {
                StartClock(1);
                buffer = new BinaryReader(new MemoryStream(bf));

                try
                {
                    Title = buffer.ReadString();
                    nN = buffer.ReadInt32();
                    Nodes = new Node[nN];
                    //var xyz = new [] { 0, 0, 0 };
                    for (var i = 0; i < nN; i++)
                    {
                        var n = buffer.ReadInt32() - 1;
                        if (n < 0 || n >= nN)
                        {
                            throw new ArgumentOutOfRangeException($"Node number {n + 1} out of range.");
                        }

                        Nodes[n] = new Node(n);
                        // the node list now has its nodes in numeric order
                    }

                    DoF = 6 * nN; // total number of degrees of freedom ( 6 per node)

                    Restraints = new bool[DoF]; // allocate and clear memory for Restraints

                    nR = buffer.ReadInt32(); // Nr of restrained nodes
                    if (nR <= 0)
                    {
                        throw new InvalidOperationException("there are no restrained nodes");
                    }

                    MatrixPart = DoF;
                    var free = new bool[6];
                    MinRestraints = 0;
                    for (var i = 0; i < nR; i++)
                    {
                        // int n;
                        var nodeNr = buffer.ReadInt32() - 1; // node Nr base 0)
                        var Nr = Nodes[nodeNr].NodeRestraints; // node's Restraints

                        for (var j = 0; j < 6; j++)
                        {
                            // read Restraints: 0 = false = free, other value = fixed = false.
                            var n = buffer.ReadInt32();
                            Restraints[nodeNr * 6 + j] = Nr[j] = n != 0;
                            MinRestraints += n == 0 ? 0 : 1;
                            free[j] = free[j] || Nr[j];
                            MatrixPart -= n == 0 ? 0 : 1;
                        }
                    }

                    nE = buffer.ReadInt32(); //number of frame elements
                    Members = new Member[nE];

                    for (var i = 0; i < nE; i++)
                    {
                        var n = buffer.ReadInt32() - 1; // Member number base 0
                        Members[n] = new Member(n, Nodes);
                    }

                    //Parallel.ForEach(Members, mb =>
                    //{
                    //    mb.SetupGamma(Nodes[mb.NodeA], Nodes[mb.NodeB]);
                    //});
                    IncludeShear = buffer.ReadBoolean();
                    IncludeGeometricStability = buffer.ReadBoolean();
                    Param.DeformationExaggeration = buffer.ReadSingle();
                    Scale = buffer.ReadSingle();

                    deltaX = buffer.ReadSingle(); // if neg.: no internal max/min force calculation

                    // # of load cases
                    nL = buffer.ReadInt32();
                    LoadCases = new LoadCase[nL];

                    for (var i = 0; i < nL; i++)
                    {
                        // create and read the entire load case:
                        LoadCases[i] = new LoadCase(i, Members);
                    }

                    if (Param.ModalMethod != None)
                    {
                        nM = buffer.ReadInt32(); // number of desired dynamic modes of vibration
                        if (nM > 0)
                        {
                            ReadMassData();
                        }
                        else
                        {
                            Param.ModalMethod = None;
                        }
                    }

                    // nr of modes to animate
                    nA = buffer.ReadInt32();

                    for (var i = 0; i < nA; i++)
                    {
                        Anim[i] = buffer.ReadInt32() - 1;
                    }

                    Pan = buffer.ReadSingle();

                    ReadCondensationData();
                }

                catch (InvalidOperationException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    Errors.Add("\t" + ex.Message + Environment.NewLine + ex.StackTrace);
                    var e = ex.InnerException;
                    while (e != null)
                    {
                        Errors.Add("\t" + e.Message + Environment.NewLine);
                        e = e.InnerException;
                    }
#if DEBUG
                    Debug.WriteLine(Errors);
#endif
                }

                StopClock(1, "Calculation");
            }

        }
#endif
    #endregion Methods
}

