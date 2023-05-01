﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Terwiel.Glaucon4
{
    public partial class Glaucon
    {
        Dictionary<int, string> Messages = new Dictionary<int, string>()
        {

{ 0 ,"error-free completion"},
{ 1 ,"unknown error"},
{ 2 ,"error with the command line options (see Section 11, above)"},
{ 3 ,"error with the command line option for shear deformation -s"},
{ 4 ,"error with the command line option for geometric stiffness -g"},
{ 5 ,"error with the command line option for lumped mass -l"},
{ 6 ,"error with the command line option for modal analysis method -m"},
{ 7 ,"error with the command line option for modal analysis tolerance -t"},
{ 8 ,"error with the command line option for modal analysis shift -f"},
{ 9 ,"error with the command line option for pan rate -p"},
{ 10 ,"error with the command line option for matrix condensation -r"},
{ 11 ,"error in opening the Input Data file"},
{ 12 ,"error in opening the temporary cleaned input data file for writing"},
{ 13 ,"error in opening the temporary cleaned input data file for reading"},
{ 14 ,"error in opening the Output Data file"},
{ 15 ,"error in creating the path for temporary output data files"},
{ 16 ,"error in creating the temporary output data file path name"},
{ 17 ,"error in opening the .CSV (spread-sheet) output data file"},
{ 18 ,"error in opening the .M (matlab) output data file"},
{ 19 ,"error in opening the interior force output data file for writing"},
{ 20 ,"error in opening the interior force output data file for reading"},
{ 21 ,"error in opening the undeformed mesh ouput data file"},
{ 22 ,"error in opening the deformed mesh ouput data file"},
{ 23 ,"error in opening the plotting script file for writing first static load case plots"},
{ 24 ,"error in opening the plotting script file for appending second and higher static load case results"},
{ 25 ,"error in opening the plotting script file for appending modal plots"},
{ 26 ,"error in opening the plotting script file for appending modal animations"},
{ 27 ,"error in opening the modal mesh data file"},
{ 28 ,"error in opening the modal mesh animation data file"},
{ 29 ,"error in opening the mass data debugging file , MassData.txt"},
{ 30 ,"cubic curvefit system matrix for element deformation is not positive definite"},
{ 31 ,"non-positive definite structural static stiffness matrix"},
{ 32 ,"error in eigen-problem analysis"},
{ 40 ,"error in input data file"},
{ 41 ,"input data formatting error in the node data, node number out of range"},
{ 42 ,"input data formatting error in node or element data, unconnected node"},
{ 51 ,"input data formatting error in the frame element data, frame element number out of range"},
{ 52 ,"input data formatting error in the frame element data, node number out of range"},
{ 53 ,"input data formatting error in the frame element data, negative section value"},
{ 54 ,"input data formatting error in the frame element data, cross section area is 0 (zero)"},
{ 55 ,"input data formatting error in the frame element data, shear area and shear modulus are 0 (zero)"},
{ 56 ,"input data formatting error in the frame element data, torsional moment of inertia is 0 (zero)"},
{ 57 ,"input data formatting error in the frame element data, bending moment of inertia is 0 (zero)"},
{ 58 ,"input data formatting error in the frame element data, modulus value is non-positive"},
{ 59 ,"input data formatting error in the frame element data, mass density value is non-positive"},
{ 60 ,"input data formatting error in the frame element data, frame element starts and stops at the same node"},
{ 61 ,"input data formatting error in the frame element data, frame element has length of zero"},
{ 71 ,"input data formatting error with the \"shear\" variable specifying shear deformation"},
{ 72 ,"input data formatting error with the \"geom\" variable specifying geometric stiffness"},
{ 73 ,"input data formatting error with the \"exagg_static\" variable specifying static mesh exageration"},
{ 74 ,"input data formatting error with the \"dx\" variable specifying the length of the internal force x-axis increment"},
{ 80 ,"input data formatting error in reaction data, number of nodes with reactions out of range"},
{ 81 ,"input data formatting error in reaction data, node number out of range"},
{ 82 ,"input data formatting error in reaction data, reaction data is not 1 (one) or 0 (zero)"},
{ 83 ,"input data formatting error in reaction data, specified node has no reactions"},
{ 84 ,"input data formatting error in reaction data, under-restrained structure"},
{ 85 ,"input data formatting error in reaction data, fully restrained structure"},
{ 86 ,"input data formatting error in extra node inertia data, node number out of range"},
{ 87 ,"input data formatting error in extra beam mass data, frame element number out of range"},
{ 88 ,"input data formatting error in mass data, frame element with non-positive mass"},
{ 90 ,"input data formatting error in matrix condensation data, number of nodes with condensed degrees of freedom are less than the total number of nodes"},
{ 91 ,"input data formatting error in matrix condensation data, node number out of range"},
{ 92 ,"input data formatting error in matrix condensation data, mode number out of range"},
{ 94 ,"input data formatting error in matrix condensation data, number of condensed degrees of freedom greater than number of modes"},
{ 100 ,"input data formatting error in load data"},
{ 101 ,"number of static load cases must be greater than zero"},
{ 102 ,"number of static load cases must be less than 30"},
{ 121 ,"input data formatting error in nodal load data, node number out of range"},
{ 131 ,"input data formatting error in uniformly-distributed load data, number of uniform loads is greater than the number of frame elements"},
{ 132 ,"input data formatting error in uniformly-distributed load data, frame element number out of range"},
{ 140 ,"input data formatting error in trapezoidally-distributed load data, too many trapezoidally distributed loads"},
{ 141 ,"input data formatting error in trapezoidally-distributed load data, frame element number out of range"},
{ 142 ,"input data formatting error in trapezoidally-distributed load data, x1 < 0"},
{ 143 ,"input data formatting error in trapezoidally-distributed load data, x1 > x2"},
{ 144 ,"input data formatting error in trapezoidally-distributed load data, x2 > L"},
{ 150 ,"input data formatting error in concentrated internal load data, number concentrated loads greater than number of frame elements"},
{ 151 ,"input data formatting error in internal concentrated load data, frame element number out of range"},
{ 152 ,"input data formatting error in internal concentrated load data, x-location less than 0 or grater than L"},
{ 160 ,"input data formatting error in thermal load data, number thermal loads greater than number of frame elements"},
{ 161 ,"input data formatting error in thermal load data, frame element number out of range"},
{ 162 ,"input data formatting error in thermal load data, frame element number out of range"},
{ 171 ,"input data formatting error in prescribed displacement data, prescribed displacements may be applied only at coordinates with reactions"},
{ 181 ,"elastic instability (elastic + geometric stiffness matrix not positive definite)"},
{ 182 ,"large strain (the average axial strain in one or more elements is greater than 0.001)"},
{ 183 ,"large strain and elastic instability"},
{ 200 ,"memory allocation error"},
{ 201 ,"error in opening an output data file saving a vector of \"floats\""},
{ 202 ,"error in opening an output data file saving a vector of \"ints\""},
{ 203 ,"error in opening an output data file saving a matrix of \"floats\""},
{ 204 ,"error in opening an output data file saving a matrix of \"doubles\""},
{ 205 ,"error in opening an output data file saving a symmetric matrix of \"floats\""},
{ 206 ,"error in opening an output data file saving a symmetric matrix of \"doubles\"" }
        };
    }
}