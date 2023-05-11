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
using System.Drawing.Text;
using System.Globalization;
using System.Reflection;
using System.Xml.Serialization;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Windows.UI.Core;

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
                // general plot data and undeformed mesh
                InitStaticMesh();
                // everything is ready, start calculation:
                if (Param.Analyze)
                {
                    K = new DenseMatrix(DoF); // global stiffness matrix
                    SSM = new DenseMatrix(DoF);

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
                        SMM = new DenseMatrix(DoF);
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
        public static int Cdof;

        public double TotalMass;
        public double StructMass;

        /// <summary>
        /// list of DoF's to condense.
        /// </summary>
        public static int[] DoFsToCondense; // Size = DoF

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
        public static Permutation Perm;

        /// <summary>
        /// reaction data, total no. of restraints , partition of matrices
        /// </summary>
        public static int FreeCount;

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
        private int[] GlobalRestraints;

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

        // use Permutation
        private const bool ReArrange = true;

        public static List<string> Errors = new();

        public static DenseMatrix Kc, Mc; // condensed stiffness and mass matrices

        public static double
            // x-increment for peak forces.
            // Redefined from Frame3DD: if <0 then no min/max force calculation
            // to avoid FP equal comparisons.
            // So: less than 0: no min/max
            // positive including 0 do min/max
            //deltaX = -1, 
            Shift; // shift-factor for rigid-body-modes

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

        //[JsonProperty("Dx")]
        //[Description("Dx")]
        //public int DeltaX { get; set; }

        public int ModalMethod = ALL;

        [XmlAttribute("Dimensionality"), JsonIgnore]
        [Description("dimensions of the construction: X, Y, Z")]
        public char[] Space;

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

        [XmlArray("NodesRestraints")]
        [JsonProperty("NodesRestraints")]
        [XmlArrayItem("NodesRestraint")]
        [Description("list of node restraints")]
        public List<NodeRestraint> NodesRestraints { get; set; }

        [XmlArray("NodesToCondense")]
        [JsonProperty("NodesToCondense")]
        [XmlArrayItem("NodeToCondense")]
        [Description("list of nodes to condense")]
        public List<CondensedNode> NodesToCondense { get; set; }

        //  public List<NodeRestraint> NodesRestraints;



        #endregion

        #region Constructors

        public Glaucon()
        {
        }

        public void ProcessGlaucon(
            //string title,
            //List<Node> nodes,
            //List<(int, int[])> restraints,
            //List<Member> members,
            //List<LoadCase> loadCases,
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

            if (parm.InputSource == OBJECT)
            {
                ProcessInput();
            }
            else
            { // InputSource = JSON

                //BaseFile = Path.GetFileNameWithoutExtension(Param.InputFileName);
                if (!Directory.Exists(Param.InputPath))
                {
                    throw new DirectoryNotFoundException($"{Param.InputPath} does not exist");
                }

                if (!File.Exists($"{Param.InputPath}{Param.InputFileName}"))
                {
                    throw new FileNotFoundException($"{Param.InputFileName} not found.");
                }

                if (string.IsNullOrEmpty(Param.OutputPath))
                {
                    Directory.CreateDirectory(Param.OutputPath);
                }
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
            //// adjust node, restraint and member numbers to base 0:           
            ArrangeNodesAndNumbers();
            
            Space = new char[3];
            int k = 0;
            for (int i = 0; i < 3; i++)
            {
                if (Nodes.Any(x => x.Coord[i] != Nodes.First().Coord[i]))
                {
                    Space[k++] = "XYZ"[i];
                }
            }

            DoF = Nodes.Count * 6;// total number of degrees of freedom ( 6 per node)
            Lg($"{DoF} dregrees of freedom.");

            Lg($"{NodesRestraints.Count} restrained nodes with one or more restrained DoFs.");

            int[] index = PartitionSystemMatrices();

            // the lower part of this array comprise the free-to-move DoFs,
            // corresponding to the Kff partition in McGuire, formula 3.7 page 43 

            Perm = new Permutation(index);

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
                // check for duplicates
                foreach (var mbr in Members)
                {
                    for (var j = mbr.Nr + 1; j < Members.Count; j++)
                    {
                        // duplicate members?
                        if ((mbr.nA == Members[j].nA && mbr.nA == Members[j].nB) ||
                                (mbr.nA == Members[j].nB && mbr.nA == Members[j].nA))
                        {
                            throw new ArgumentOutOfRangeException($"Two members are the same: {mbr.Nr + 1} and {j + 1}");
                        }
                    }
                }
                foreach (var lc in LoadCases)
                {
                    foreach (var displ in lc.PrescrDisplacements)
                    {
                        var restr = Nodes[displ.NodeNr].Restraints;
                        for (int i = 0; i < 6; i++)
                        {
                            if (restr[i] == 0 && Globals.AlmostEqual(displ.Displacements[i], 1e-12))
                            {
                                string m = "";
                                restr[i] = 1;
                                Lg(m = $"Load case {lc.Nr + 1}, displacement {i + 1} for node {displ.NodeNr + 1} too small.\n"
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

            // Process Load cse input data:
            Lg($"{LoadCases.Count} load cases.");

            LoadCases.ForEach(ld => ld.ProcessData(Members, DoF));

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

            if (Param.ModalMethod != None)
            {
                ProcessCondensationData();
            }
#if DEBUG
            s.Stop();
            Debug.WriteLine($"Elapsed computing time for processing input: {s.ElapsedMilliseconds} msec.");
#endif
        }

        public void ArrangeNodesAndNumbers()
        {
            // adjust node, restraint and member numbers to base 0:
            Nodes.Sort((i, j) => i.Nr.CompareTo(j.Nr));
            int seqCheck = 0;
            foreach (var nd in Nodes)
            {
                nd.Nr--;
                SeqCheck(nd.Nr, "Node");
                nd.Restraints = new[] { 0, 0, 0, 0, 0, 0 }; //ALL free
            }
            Members.Sort((i, j) => i.Nr.CompareTo(j.Nr));
            seqCheck = 0;
            foreach (var mbr in Members)
            {
                mbr.Nr--;
                //nA and nB were decremented in the constructor
                mbr.NodeA = Nodes[mbr.nA];
                mbr.NodeB = Nodes[mbr.nB];

                SeqCheck(mbr.Nr, "Member");
                mbr.Process(Nodes);
            }
            NodesRestraints.Sort((i, j) => i.NodeNr.CompareTo(j.NodeNr));
            foreach (var nr in NodesRestraints)
            {
                nr.NodeNr--;
            }

            foreach (var nr in NodesRestraints)
            {
                Nodes[nr.NodeNr].Restraints = nr.Restraints;
            }

            // local method:
            void SeqCheck(int nr, string name)
            {
                if (seqCheck != nr)
                {
                    throw new ArgumentException($"{name} number {nr + 1} out of sequence");
                }
                seqCheck++;
            }
        }

        public int[] PartitionSystemMatrices()
        {
            // now set up the DoF vector:
            FreeCount = DoF;
            GlobalRestraints = new int[DoF];
            // NodeRestraints is a list of only restrained nodes
            foreach (var nr in NodesRestraints)
            {
                var nodeNr = nr.NodeNr;
                var restr = nr.Restraints; // int[6]
                FreeCount -= restr.Sum(); // Count the zeros
                for (var j = 0; j < 6; j++)
                {
                    // read restraints: 0 = not restrained (free), other value = fixed = false.
                    var n = restr[j];
                    // Restraints is  int[DoF]
                    GlobalRestraints[(nodeNr * 6) + j] = n;
                    // one of the 6 degrees of freedon of the construction may not be
                    // unconstrained:
                    //FreeCount -= n;
                }
            }

            // partition stiffness matrix / force vector:
            // index[i] says where a vector element or matrix row/column should goes when permuting.
            var index = new int[DoF]; // use for Permutation
            var j1 = 0;
            var j2 = FreeCount;
            for (var i = 0; i < DoF; i++)
            {
                index[GlobalRestraints[i] == 0 ? j1++ : j2++] = i;
                //if (GlobalRestraints[i] == 0)
                //{
                //    index[j1] = i;
                //    j1++;
                //}
                //else
                //{
                //    index[j2] = i;
                //    j2++;
                //}
            }
            return index;
            //TODO: check this!
        }
    }
    #endregion Methods
}

