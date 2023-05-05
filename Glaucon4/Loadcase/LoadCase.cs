#region FileHeader
// Project: Glaucon4
// Filename:   LoadCase.cs
// Last write: 4/30/2023 2:37:19 PM
// Creation:   4/24/2023 11:59:09 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System.Diagnostics;
using System.Diagnostics.Contracts;

using MathNet.Numerics.LinearAlgebra.Double;

namespace Terwiel.Glaucon
{
    public partial class Glaucon
    {
        /// <summary>
        /// Load case class.
        /// </summary>
        public partial class LoadCase
        {
             /// <summary>
            /// Gets the load nr.
            /// </summary>
            public int Nr { get; set; }

            public double RMS_Error;
            /// <summary>
            /// // equivalent end forces from mech loads between the nodes. global.
            /// </summary>
            private DenseVector[] eqFMech;

            /// <summary>
            /// // equivalent end forces from temp loads between the nodes. global.
            /// </summary>
            private DenseVector[] eqFTemp;

            /// <summary>
            /// thermal load vectors, all load cases.
            /// </summary>
            private DenseMatrix TempForces;

            /// <summary>
            /// gravity acceleration in 3 directions
            /// </summary>
            public DenseVector g;

            /// <summary>
            /// The construction has mechanical loads.
            /// </summary>
            private static bool hasMechLoads;

            /// <summary>
            /// System mechanical loads matrix.
            /// </summary>
            public DenseMatrix MechForces;

            public List<IntPointLoad> IntPointLoads;
            public List<NodalLoad> NodalLoads;
            public List<PrescrDisplacement> PrescrDisplacements;

            /// <summary>
            /// prescribed node displacements.
            /// </summary>
            public DenseMatrix PrescrDispl;

            /// <summary>
            /// Member end forces for each and every load case
            /// Each row represents the forces for this load case.
            /// Each column is a vector of 6 for Node A, and 6 for node B (12 total)
            /// </summary>
            public DenseMatrix Q;

            public List<TempLoad> TempLoads;

            public List<TrapLoad> TrapLoads;

            /// <summary>
            /// uniform distributed member loads
            /// </summary>
            public List<UniformLoad> UniformLoads;

            /// <summary>
            /// System displacement vector.
            /// </summary>
            public DenseMatrix Displacements;

            public DenseMatrix[] IntForces;
            public DenseMatrix[] TransvDispl;

            private double error;

            /// <summary>
            /// total load vector for a load case.
            /// </summary>
            private DenseMatrix F;
#if DEBUG

            /// <summary>
            /// System stiffness matrix
            /// public for testing
            /// </summary>
            public DenseMatrix Ku;
#endif
            public bool Active;
            /// <summary>
            /// total reaction force vector.
            /// </summary>
            public DenseMatrix Reactions;

            public LoadCase(){ }

            public LoadCase(
                int nr,
                double[] g, // must be double

                List<NodalLoad> nl,
                List<UniformLoad> ul,
                List<TrapLoad> tl,
                List<TempLoad> temp,
                List<IntPointLoad> ip,
                List<PrescrDisplacement> prs,
                bool active = true
                )
            {
                Nr = nr - 1;
                this.g = DenseVector.OfArray(g);
                NodalLoads = nl;

                UniformLoads = ul;
                TrapLoads = tl;
                TempLoads = temp;
                PrescrDisplacements = prs;
                IntPointLoads = ip;
                Active = active;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="LoadCase"/> class.
            /// </summary>
            /// <Param name="Nr">
            /// Load case number
            /// </Param>
            /// <Param name="mbrs">
            /// Construction members.
            /// </Param>
            /// <Param name="DoF">
            /// Degrees of freedom
            /// </Param>
            /// <exception cref="InvalidDataException">
            /// </exception>
            public void ProcessData(int Nr, List<Member> mbrs, int DoF)
            {
                Contract.Requires(mbrs != null);
                int mbrCount = mbrs.Count;
                Contract.Requires(mbrCount > 0);

                Q = new DenseMatrix(mbrCount, 12);

                eqFMech = new DenseVector[mbrCount];
                eqFTemp = new DenseVector[mbrCount];

                // Because each member will probably have gravity forces, we should 
                // initialize each array element properly:
                for (var i = 0; i < mbrCount; i++)
                {
                    eqFMech[i] = new DenseVector(12);
                    eqFTemp[i] = new DenseVector(12);
                }

                // prescribed node displacements:
                // Only at fixed (restrained) DoFs
                // prescrDispl = new DenseMatrix(DoF, 1);

                // mechanical component of general force vector
                // These are the NodeLoads:
                MechForces = new DenseMatrix(DoF, 1);
                TempForces = new DenseMatrix(DoF, 1); // thermal component of general temerature load vector

                F = new DenseMatrix(DoF, 1); // external load vector

                // no need to initialize load data vectors and matrices to zero 

                // begin load-case loop 
                // gravity loads applied uniformly to all frame elements ------- 
                for (var i = 0; i < 3; i++)
                {
                    //g[i] = buffer.ReadDouble();
                    if (!Globals.AlmostEqual(g[i], 0d))
                    {
                        hasMechLoads = true;
                        break;
                    }
                }

                for (var i = 0; i < mbrCount; i++)
                    mbrs[i].GetGravityLoadVector(g).CopyTo(eqFMech[i]);

                // node point loads in global structural coordinates

                foreach (var pl in NodalLoads)
                {
                    hasMechLoads = true;
                    // ! global structural coordinates ! 
                    var n = pl.NodeNr; // node Nr. base 0
                    for (var j = 0; j < 6; j++)
                        // NOT added to the equiv. force vector!!
                        MechForces[6 * n + j, 0] = pl.Load[j];
                }

                WriteMatrix(MEMBER, $"lc_{Nr}_", "F_mech_node.mat", "F_mech_node", MechForces);

                // uniformly distributed loads, (UnifLods)
                //var nU = buffer.ReadInt32();
                if (UniformLoads != null)
                {                    
                    foreach (var uLoad in UniformLoads)
                    {
                        // local member coordinates !
                        int n;
                        var fixedEndForces = new DenseVector(12); // temp vector

                        var mbr = mbrs[n = uLoad.MemberNr];
                        fixedEndForces = uLoad.GetLoadVector(mbr);
#if false
                        var effLength = mbr.Le;                       

                        // First in local member coordinates:
                        fixedEndForces[0] = fixedEndForces[6] = uLoad.Q[0] * effLength / 2.0;
                        fixedEndForces[1] = fixedEndForces[7] = uLoad.Q[1] * effLength / 2.0;
                        fixedEndForces[2] = fixedEndForces[8] = uLoad.Q[2] * effLength / 2.0;

                        // fixedEndForces[3] = fixedEndForces[9] = 0.0;
                        fixedEndForces[10] = -(fixedEndForces[4] = -uLoad.Q[2] * effLength * effLength / 12.0);
                        fixedEndForces[11] = -(fixedEndForces[5] = uLoad.Q[1] * effLength * effLength / 12.0);
#endif                        
                        // And then globalize, if necessary:
                        // [F] = [T]' * [FixedEndForces] 
                        // add to equiv. force vector
                        if (Param.QLoadsLocal)
                            eqFMech[n] += (DenseVector)mbr.Gamma.TransposeThisAndMultiply(fixedEndForces);
                        else
                            eqFMech[n] += fixedEndForces;                        
                    }
                     hasMechLoads |= UniformLoads.Count > 0;
                    // end uniformly distributed loads
                }

                // trapezoidal distributed loads (TrapLoads)
                // nW = buffer.ReadInt32();
                if (TrapLoads != null)
                {
                    Lg($"There are {TrapLoads.Count} trapezoidally-distributed loads");

                    foreach (var trap in TrapLoads)
                    {
                        // ! local member coordinates ! 
                        var fixedEndForces = new DenseVector(12);
                        var mbr = mbrs[trap.MemberNr];
                        trap.GetLoadVector(mbr);

#if false
                        double R1o = 0, R2o = 0, f01 = 0, f02 = 0;
                        // var trap = new TrapLoad();

                        // var mbr = mbrs[trap.MemberNr];
                        var length = mbr.Length;

                        // x-axis trapezoidal loads (along the frame member length) 
                        trap.Loads[0].Build1(length, ref f01, ref f02);

                        fixedEndForces[0] = f01;
                        fixedEndForces[6] = f02;

                        // y-axis trapezoidal loads (across the frame member length) 
                        trap.Loads[1].Build2(length, ref R1o, ref R2o, ref f01, ref f02);

                        fixedEndForces[5] = -(4.0 * f01 + 2.0 * f02 + mbr.Ksy * (f01 - f02))
                                            / (length * length * (1.0 + mbr.Ksy));
                        fixedEndForces[11] = -(2.0 * f01 + 4.0 * f02 - mbr.Ksy * (f01 - f02))
                                             / (length * length * (1.0 + mbr.Ksy));

                        fixedEndForces[1] = R1o + fixedEndForces[5] / length + fixedEndForces[11] / length;
                        fixedEndForces[7] = R2o - fixedEndForces[5] / length - fixedEndForces[11] / length;

                        // z-axis trapezoidal loads (across the frame member length) 
                        trap.Loads[2].Build2(length, ref R1o, ref R2o, ref f01, ref f02);

                        fixedEndForces[4] = (4.0 * f01 + 2.0 * f02 + mbr.Ksz * (f01 - f02))
                                            / (length * length * (1.0 + mbr.Ksz));
                        fixedEndForces[10] = (2.0 * f01 + 4.0 * f02 - mbr.Ksz * (f01 - f02))
                                             / (length * length * (1.0 + mbr.Ksz));

                        fixedEndForces[2] = R1o - fixedEndForces[4] / length - fixedEndForces[10] / length;
                        fixedEndForces[8] = R2o + fixedEndForces[4] / length + fixedEndForces[10] / length;

                        // fixedEndForces[3] = fixedEndForces[9] = 0.0;
                        //TrapLoads.Add(trap);
#endif
                        // {F} = [T]'{Q} 
                        // add to equiv. force vector
                        eqFMech[mbr.Nr] += (DenseVector)mbr.Gamma.TransposeThisAndMultiply(fixedEndForces);
                    }
                    hasMechLoads |= TrapLoads.Count > 0;
                    // end trapezoidal loads
                }

                // internal member point loads (IntPointLoads)
                //var nP = buffer.ReadInt32();
                if (IntPointLoads != null)
                {
                    foreach (var pl in IntPointLoads.Where(ld => ld.Active))
                    {
                        // ! local member coordinates ! 
                        //var pointLoad = new PointLoad();
                        int n;
                        var mbr = mbrs[n = pl.MemberNr];

                        var FixedEndForces = pl.GetLoadVector(mbr);

                        eqFMech[n] += (DenseVector)mbr.Gamma.TransposeThisAndMultiply(FixedEndForces);
                        
                    }
                    hasMechLoads |= IntPointLoads.Count > 0;
                }

                // thermal loads are treated differntly from the other load types
                
                if (TempLoads.Count > 0)
                {
                    // moved to the member's eqFTemp vector:
                    foreach (var tl in TempLoads)
                    {
                        // ! local member coordinates ! 
                        var n = tl.MemberNr; // Member Nr. base 0
                                           
                        var mbr = mbrs[n];
                        var FixedEndForces = tl.GetLoadVector(mbr);

                        // {F} = [T]'{Q} 
                        // add to equiv. temp load vector
                        eqFTemp[n] += (DenseVector)mbr.Gamma.TransposeThisAndMultiply(FixedEndForces);

                        TempLoads.Add(tl);// TODO: is this superfluous?
                    }
                }

                // assemble all member equivalent loads into 
                // separate load vectors for mechanical and thermal loading
                for (var n = 0; n < mbrCount; n++)
                {
                    var n1 = mbrs[n].NodeA.Nr * 6;
                    var n2 = mbrs[n].NodeB.Nr * 6;
                    for (var i = 0; i < 6; i++)
                    {
                        MechForces[n1 + i, 0] += eqFMech[n][i];
                    }
                    for (var i = 6; i < 12; i++)
                    {
                        MechForces[n2 + i - 6, 0] += eqFMech[n][i];
                    }
                    for (var i = 0; i < 6; i++)
                    {
                        TempForces[n1 + i, 0] += eqFTemp[n][i];
                    }
                    for (var i = 6; i < 12; i++)
                    {
                        TempForces[n2 + i - 6, 0] += eqFTemp[n][i];
                    }
                }

                // prescribed displacements go directly in the global displacement vector               
                if (PrescrDisplacements != null)
                {
                    PrescrDispl = new DenseMatrix(DoF, 1);
                    foreach (var pd in PrescrDisplacements.Where(_p => _p.Active))
                    {
                        var n = pd.NodeNr; // node # (base 0)
                        for (var j = 0; j < 6; j++)
                        {
                            var disp = pd.Displacements[j];
                            PrescrDispl[6 * n + j, 0] = disp;
                        }
                    }
                    //hasMechLoads |= PrescrDisplacements.Count > 0;
                }
            }           

            /* See Logan, pg. 37, formula 2.5.1 or
            See McGuire pg 41, formula 3.7
            Kff: upper left
            Kss lower right
            Ksf: lower left
            Kfs : upper right  
           
            But only after permutation!
            Readony props! */
            private static DenseMatrix Kff => (DenseMatrix)SSM.SubMatrix(0, matrixPart, 0, matrixPart);

            private static DenseMatrix Kfs => (DenseMatrix)SSM.SubMatrix(0, matrixPart, matrixPart, DoF - matrixPart);

            // DenseMatrix Kss => (DenseMatrix) SSM.SubMatrix(matrixPart, DoF - matrixPart, matrixPart, DoF - matrixPart);
            private static DenseMatrix Ksf => (DenseMatrix)SSM.SubMatrix(matrixPart, DoF - matrixPart, 0, matrixPart);

            private DenseMatrix Displacements1 => (DenseMatrix)Displacements.SubMatrix(0, matrixPart, 0, 1);

            private DenseMatrix Displacements2 =>
                (DenseMatrix)Displacements.SubMatrix(matrixPart, DoF - matrixPart, 0, 1);

            private DenseMatrix F_mech1 => (DenseMatrix)MechForces.SubMatrix(0, matrixPart, 0, 1);

            private DenseMatrix F_mech2 => (DenseMatrix)MechForces.SubMatrix(matrixPart, DoF - matrixPart, 0, 1);

            private DenseMatrix F_temp1 => (DenseMatrix)TempForces.SubMatrix(0, matrixPart, 0, 1);

            private DenseMatrix Ff => (DenseMatrix)F.SubMatrix(0, matrixPart, 0, 1);

            private DenseMatrix Fs => (DenseMatrix)F.SubMatrix(matrixPart, DoF - matrixPart, 0, 1);

            /// <summary>
            /// Gets the partial vector of the prescribed displacements.
            /// </summary>
            private DenseMatrix PDf => (DenseMatrix)PrescrDispl.SubMatrix(0, matrixPart, 0, 1);

            /// <summary>
            /// Gets the partial vector of the prescribed displacements.
            /// </summary>
            private DenseMatrix PDs => (DenseMatrix)PrescrDispl.SubMatrix(matrixPart, DoF - matrixPart, 0, 1);

            /// <summary>
            /// Solve the matrix equation with MatNet.
            /// </summary>
            /// <Param name="k">
            /// system stiffness matrix
            /// </Param>
            /// <Param name="F">
            /// system force vector.
            /// </Param>
            /// <returns>
            /// The <see cref="DenseMatrix"/> displacement vectors.
            /// </returns>
            /// <exception cref="ArithmeticException">error in the solver</exception>
            private static DenseMatrix SolveIt(DenseMatrix k, DenseMatrix F)
            {
                Contract.Requires(k != null);
                Contract.Requires(F != null);
                DenseMatrix d;
                try
                {
                    return (DenseMatrix)k.Solve(F);
                }
                catch (Exception ex)
                {
                    var message = $"Solver error: {ex.Message}";
                    Lg(message);
                    throw new Exception(message);
                }
            }

            public void Solve(List<Member> Members)
            {
                Contract.Requires(Members != null);
                Displacements = new DenseMatrix(DoF, 1); // displacments of all nodes
                Reactions = new DenseMatrix(DoF - matrixPart, 1); // reaction forces			

                var dReaction = new DenseMatrix(DoF - matrixPart, 1); // incremental reaction forces	
                var delta_f = new DenseMatrix(matrixPart, 1); // incremental displ. of all free DoFs		

                // global system stiffness matrix  [SSM({D}^(i))], {D}^(0)={0} (i=0) 
                AssembleSystemStiffnessMatrix(Members); // is permuted now. No geom yet
#if DEBUG

                // keep for testing
                Ku = new DenseMatrix(DoF);
                SSM.CopyTo(Ku);
#endif

                // Glaucon.WriteMatrix($"lc_{Nr + 1}_", "Ku.mat", "Ku",  SSM);
                MechForces.PermuteRows(perm);
                TempForces.PermuteRows(perm);

                // Displacements.PermuteRows(Glaucon.perm);
                // First apply temperature "forces".
                if (TempLoads?.Count > 0)
                {
                    // get the displacements due to temperature changes alone:
                    // solve {F_t} = [SSM({D=0})] *{D_t}
                    SolveIt(Kff, F_temp1).CopyTo(delta_f);

                    Displacements.SetSubMatrix(0, matrixPart, 0, 1, delta_f);

                    // See McGuire pg 41, form 3.8.b
                    dReaction = Ksf * delta_f;
                    Reactions += dReaction;

                    if (Param.AccountForGeomStability)
                    {
                        // Q is filled for each member because we need Q[0].
                        MembersEndForces(Members);

                        // Assemble SSM anew, observing the (compressive?) end force (= Q[0] = T) due to 
                        // temperature changes.
                        // Q now has the temperature-induced forces.
                        AssembleSystemStiffnessMatrix(Members, true, Q);
                    }
                }

                // .. then apply mechanical loads only..
                if (hasMechLoads)
                {
                    // incremental displ at reactions = prescribed displacements 
                    // Initial displacements can be prescribed only at restrained DoFs.
                    // For an explanation of the processing of initial displacements,
                    // See Logan page 38-9,
                    // and McGuire, pg 122, 5.3.1 
                    // and Rao, pg 212, formula 6.15
                    if (PrescrDispl?.RowCount > 0)
                    {
                        PrescrDispl.PermuteRows(perm);
                        var F_ = Kfs * PDs;
                        MechForces.SetSubMatrix(0, matrixPart, 0, 1, F_mech1 - F_);
                    }

                    WriteMatrix(MEMBER, $"LC_{Nr}_", "Kfs.mat", "Kfs", Kfs);
                    WriteMatrix(MEMBER, $"LC_{Nr}_", "F_mech1.mat", "Fmech1", F_mech1);

                    // solve {F_m} = [SSM({D_t})] * {D_m}
                    SolveIt(Kff, F_mech1).CopyTo(delta_f); // McGuire form. 3.9

                    // combine {D} = {D_t} + {D_m}	
                    Displacements.SetSubMatrix(
                        0,
                        matrixPart,
                        0,
                        1,
                        Displacements1 + delta_f); // permuted displacement vector 
                    if (PrescrDispl != null)
                        // PDs would throw a null reference exception if tere are no prescribed displacements:
                        Displacements.SetSubMatrix(matrixPart, DoF - matrixPart,
                            0, 1, PDs);

                    dReaction = Ksf * Displacements1 - F_mech2; // OK
                }

                // combine temperature loads and mechanical loads:
                // {F} = {TempForces} + {mechForces} 
                F = TempForces + MechForces; // permuted force vector

                // Find member end forces {Q} for combined displacements due to 
                // mechanical plus temperature loads displacements {D}	
                MembersEndForces(Members);

                var dF = new DenseMatrix(matrixPart, 1);

                // check the equilibrium error	
                error = EquilibriumError(ref dF);

                // quasi Newton-Raphson iteration for geometric non-linearity  
                if (Param.AccountForGeomStability)
                {
                    Debug.WriteLine("Non-Linear Elastic Analysis ..");
                    var prevError = 2.0;
                    error = 1.0;
                    Param.Iterations = 0;

                    while (error > Param.EquilibriumTolerance /*&& prevError > error*/
                           && Param.Iterations < Param.MaximumIterations)
                    {
                        Param.Iterations++;

                        // assemble stiffness matrix [SSM({D}^(i))]
                        AssembleSystemStiffnessMatrix(Members, true, Q);

                        // compute equilibrium error, {dF}, at iteration i   
                        // {dF}^(i) = {F} - [SSM({D}^(i))]*{D}^(i)	      
                        // convergence criteria = || {dF}^(i) ||  /  || F || 
                        prevError = error;
                        error = EquilibriumError(ref dF); // generates dF

                        // solve {dF}^(i) = [SSM({D}^(i))] * {dD}^(i)	
                        SolveIt(Kff, dF).CopyTo(delta_f);

                        // increment {D}^(i+1) = {D}^(i) + {dD}^(i)	
                        Displacements.SetSubMatrix(0, matrixPart, 0, 1, Displacements1 + delta_f);

                        // member forces {Q} for displacements after i-th iteration:      
                        MembersEndForces(Members);
                    }

                    Param.EquilibriumError = Math.Max(Param.EquilibriumError, error);
                }

                Reactions = -Fs + Ksf * Displacements1;
                Displacements.PermuteRows(perm.Inverse());

                // Displacements.PermuteRows(Glaucon.perm.Inverse());
                MechForces.PermuteRows(perm.Inverse());

                WriteMatrix(MEMBER, $"lc_{Nr + 1}_", "mechForces.mat", "F", MechForces);

                // Glaucon.WriteMatrix($"lc_{Nr + 1}_",  "SSM.mat",  "SSM",SSM);
                WriteMatrix(MEMBER, $"lc_{Nr + 1}_", "Displ.mat", "Displ", Displacements);
                WriteMatrix(MEMBER, $"lc_{Nr + 1}_", "Reactions.mat", "Reactions", Reactions);
                WriteMatrix(MEMBER, $"lc_{Nr + 1}_", "Q.mat", "Q", Q);

                if (!Globals.AlmostEqual(Param.XIncrement, -1) && Param.XIncrement > 0)
                {
                    // Members.ToList().ForEach(m => m.GetPeaks(this, (DenseVector)Displacements.Column(0)) );
                    foreach (var member in Members)
                    {
                        member.GetMemberPeaks(this, (DenseVector)Displacements.Column(Nr));
                    }
                }
                GetPeakForces(Members, (DenseVector)Displacements.Column(Nr));
            }

            /// <summary>
            /// EQUILIBRIUM_ERROR -  compute {dF_q} =   {F_q} - [K_qq]{D_q} - [K_qr]{D_r}
            /// use only the upper-triangle of[K_qq]
            /// </summary>
            /// <Param name="dF">differential  force</Param>
            /// <returns>||dF|| /||F||</returns>
            private double EquilibriumError(ref DenseMatrix dF)
            {
                // compute equilibrium error at free Coord's
                (Ff - Kff * Displacements1 - Kfs * Displacements2).CopyTo(dF);
                var v = dF.Column(0);
                return Math.Sqrt(v.PointwiseMultiply(v).Sum())
                       / Math.Sqrt(Ff.Column(0).PointwiseMultiply(Ff.Column(0)).Sum());
            }

            /// <summary>
            /// MembersEndForces  -  evaluate the end forces for all elements.
            /// On entry, displacements, forces and the system stiffness matrix
            /// are all permutated.
            /// </summary>
            /// <Param name="members">Collection of frame elements</Param>
            private void MembersEndForces(List<Member> members)
            {                
                var d = new DenseMatrix(DoF, 1);
                Displacements.CopyTo(d);
                d.PermuteRows(perm.Inverse());
                var dGlobal = new DenseVector(12);

                // Parallel.ForEach(mbrs, mbr =>
                int n = 0;
                foreach (var mbr in members)
                {
                    dGlobal.SetSubVector(0, 6, d.Column(0).SubVector(mbr.NodeA.Nr * 6, 6));
                    dGlobal.SetSubVector(6, 6, d.Column(0).SubVector(mbr.NodeB.Nr * 6, 6));
                    Q.SetRow(n, mbr.MemberEndForces(dGlobal, eqFMech[n] + eqFTemp[n]));
                    WriteMatrix(MEMBER, $"Lc_{Nr + 1}_mb_{n + 1}", "_Q.mat", "Q", Q);
                    n++;
                }
            }
        }
    }
}
