#region FileHeader
// Project: Glaucon4
// Filename:   WriteXML.cs
// Last write: 4/30/2023 2:29:51 PM
// Creation:   4/24/2023 12:01:10 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;


namespace Terwiel.Glaucon
{

    /*
     * The following classes provide a mechanism to annotated certain vectors and matirces
     * in such a way, that they are recognized to be output to the XML file in a specific way
     * not provided by the XmlDocument possibilities.
     * the Eigenvector and Eigenvalues vector/matrix were annotated in this way.
     * XmlVector- and XmlMatrix annotated variables can be defined as Packed. This means they
     * are not fully output, but only the vector/matrix items that are not zero.
     * To be used only for:
     * - Properties 
     * - DenseVector and DenseMatrix types.
     */
    public enum Packing
    {
        Sparse = 0,
        Dense = 1
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class XmlMatrix : Attribute
    {
        private string name;

        private Packing Packing;
        //double version;

        public XmlMatrix(string name, Packing packing = Packing.Dense)
        {
            this.name = name;
            Packing = packing;
            // version = 1.0;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class XmlVector : Attribute
    {
        private string name;

        private Packing Packing;
        //double version;

        public XmlVector(string name, Packing packing = Packing.Dense)
        {
            this.name = name;
            Packing = packing;
            //version = 1.0;
        }
    }

    public partial class Glaucon
    {
        /// <summary>
        /// Recursively scan the internal property data structure, as defined
        /// by the XMl annotations of the properties.
        /// Dense Matrices and DenseVectors were given a custom annotation:
        /// XmlMatrix and XmlVector, respectively.
        /// </summary>
        /// <param name="outputFileName">The full file name to write the XML to</param>
        public void WriteXML(string outputFileName)
        {
            Debug.WriteLine("Enter " + MethodBase.GetCurrentMethod().Name);

            try
            {
                using (var writer = new XmlTextWriter(outputFileName, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;
                    var xmlDoc = new XmlDocument();
                    var rootElement = xmlDoc.CreateElement("Glaucon");
                    recurse(xmlDoc, rootElement, this);
                    xmlDoc.AppendChild(rootElement);
                    xmlDoc.WriteTo(writer);
                }
            }
            catch (Exception e)
            {
                Lg($"Error writing XML output file\n{e.Message}" );
            }

            Debug.WriteLine("Exit " + MethodBase.GetCurrentMethod().Name);
        }

        private void recurse(XmlDocument xmlDoc, XmlElement node, object obj)
        {
            Debug.WriteLine($"Enter {MethodBase.GetCurrentMethod().Name} {node.Name}");
            // First process all the attributes of the object:

            var propList = GetPropertyList(obj, typeof(XmlAttributeAttribute));

            var sba = new StringBuilder();
            sba.AppendLine();
            foreach (var attr in propList)
            {
                var name = GetName(attr, typeof(XmlAttributeAttribute));
                var descr = GetDescription(attr, typeof(DescriptionAttribute));
                sba.AppendLine($"{name} = {descr}");

                var attribute = xmlDoc.CreateAttribute(name);
                attribute.Value = attr.GetValue(obj).ToString();
                node.Attributes.Append(attribute);
            }

            var ArgCommentElement = xmlDoc.CreateComment(sba.ToString());
            node.PrependChild(ArgCommentElement);

            // now look for all DenseVectors and DenseVectors:

            propList = GetPropertyList(obj, typeof(XmlVector));

            foreach (var vectorProp in propList)
            {
                if (vectorProp.GetValue(obj) == null)
                {
                    continue;
                }

                //Debug.Assert(obj is DenseMatrix, $"{obj} is not a DenseVector");
                DenseVector v;
                var IsSparse = Sparse(vectorProp) == Packing.Sparse;
                // Vectors can be double or single (double):
                if (vectorProp.GetValue(obj) is DenseVector)
                {
                    v = (DenseVector)vectorProp.GetValue(obj);
                }
                else
                {
                    v = new DenseVector((vectorProp.GetValue(obj) as DenseVector).Count);
                    for (var i = 0; i < (vectorProp.GetValue(obj) as DenseVector).Count; i++)
                    {
                        v[i] = (vectorProp.GetValue(obj) as DenseVector)[i];
                    }
                }

                if (v == null)
                {
                    continue;
                }

                var descr = GetDescription(vectorProp, typeof(DescriptionAttribute));

                var CommentElement = xmlDoc.CreateComment(descr);
                node.AppendChild(CommentElement);

                var name = GetName(vectorProp, typeof(XmlVector));
                var newElement = xmlDoc.CreateElement(name);

                MakeAttribute(xmlDoc, newElement, "Count", v.Count());

                var sb = new StringBuilder();
                sb.AppendLine();
                for (var i = 0; i < v.Count; i++)
                {
                    if (IsSparse && v[i].AlmostEqual(0.0))
                    {
                        continue;
                    }

                    sb.Append($"{i} {v[i]:E15}{Environment.NewLine}");
                }

                newElement.InnerText = sb.ToString();
                node.AppendChild(newElement);
            }

            // Process the DenseMatrices. No provision was made for double DenseMatrices.
            propList = GetPropertyList(obj, typeof(XmlMatrix));

            foreach (var matrixProp in propList)
            {
                //Debug.Assert(obj is DenseMatrix, $"{obj} is not a DenseMatrix");
                var IsSparse = Sparse(matrixProp) == Packing.Sparse;

                var m = (DenseMatrix)matrixProp.GetValue(obj);
                if (m == null)
                {
                    continue;
                }

                var descr = GetDescription(matrixProp, typeof(DescriptionAttribute));
                var CommentElement = xmlDoc.CreateComment(descr);
                node.AppendChild(CommentElement);

                var name = GetName(matrixProp, typeof(XmlMatrix));
                var newElement = xmlDoc.CreateElement(name);

                MakeAttribute(xmlDoc, newElement, "RowCount", m.RowCount);
                MakeAttribute(xmlDoc, newElement, "ColumnCount", m.ColumnCount);

                var sb = new StringBuilder();
                sb.AppendLine();
                for (var i = 0; i < m.RowCount; i++)
                {
                    for (var j = 0; j < m.ColumnCount; j++)
                    {
                        if (IsSparse && m[i, j].AlmostEqual(0.0))
                        {
                            continue;
                        }

                        sb.Append($"{i} {j} {m[i, j]:E15}" + Environment.NewLine);
                    }
                }

                newElement.InnerText = sb.ToString();
                node.AppendChild(newElement);
            }

            // process the XmlElements:
            propList = GetPropertyList(obj, typeof(XmlElementAttribute));

            foreach (var elmProp in propList)
            {
                var o = elmProp.GetValue(obj);

                var descr = GetDescription(elmProp, typeof(DescriptionAttribute));
                var CommentElement = xmlDoc.CreateComment(descr);
                node.AppendChild(CommentElement);

                var name = GetName(elmProp, typeof(XmlElementAttribute));
                var newElement = xmlDoc.CreateElement(name);

                var s = Atomic(o);
                if (s != null)
                {
                    newElement.InnerText = s;
                }
                else
                {
                    recurse(xmlDoc, newElement, o);
                }

                node.AppendChild(newElement);
            }

            // process Arrays:

            propList = GetPropertyList(obj, typeof(XmlArrayAttribute));

            foreach (var arrayProp in propList) // PropertyInfos
            {
                // the List (or Array) does not have Attributes itself
                // Debug.Assert(obj is Array, $"{obj} is not an Array");

                var a = (Array)arrayProp.GetValue(obj);
                if (a?.Length > 0)
                {
                    // arrays have an annotation for the array itself and
                    // one for its items:
                    var listName = GetName(arrayProp, typeof(XmlArrayAttribute));
                    var itemName = GetName(arrayProp, typeof(XmlArrayItemAttribute));

                    var descr = GetDescription(arrayProp, typeof(DescriptionAttribute));
                    var CommentElement = xmlDoc.CreateComment(descr);
                    node.AppendChild(CommentElement);

                    var listElement = xmlDoc.CreateElement(listName);

                    for (var i = 0; i < a.Length; i++)
                    {
                        var o = a.GetValue(i);
                        var s = Atomic(o);
                        if (s != null)
                        {
                            listElement.InnerText += s + " ";
                        }
                        else
                        {
                            var itemElement = xmlDoc.CreateElement(itemName);

                            recurse(xmlDoc, itemElement, o);
                            listElement.AppendChild(itemElement);
                        }
                    }

                    node.AppendChild(listElement);
                }
            }

            Debug.WriteLine($"Exit {MethodBase.GetCurrentMethod().Name} {node.Name}");
        } // end recurse   

        private Packing Sparse(PropertyInfo pi)
        {
            foreach (var p in pi.CustomAttributes)
                foreach (var w in p.ConstructorArguments)
                {
                    if (w.ArgumentType == typeof(Packing))
                    {
                        return (Packing)w.Value;
                    }
                }

            return Packing.Dense;
        }

        private void MakeAttribute(XmlDocument doc, XmlElement node, string name, int a)
        {
            var attribute = doc.CreateAttribute(name);
            attribute.Value = a.ToString();
            node.Attributes.Append(attribute);
        }

        /// <summary>
        /// Is the property an "atomic" one (indivisible like int, bool or double)
        /// or not (LoadCase, UniformLoad etc.)
        /// </summary>
        /// <param name="o">The object</param>
        /// <returns>the objects value or null</returns>
        private string Atomic(object o)
        {
            if (o is double)
            {
                return $"{(double)o:E15}";
            }

            if (o is int)
            {
                return $"{(int)o}";
            }

            if (o is double)
            {
                return $"{(double)o:E10}";
            }

            if (o is string)
            {
                return (string)o;
            }

            if (o is bool)
            {
                return (bool)o ? "true" : "false";
            }

            return null;
        }

        /// <summary>
        /// get a list of the properties of an object of type T
        /// </summary>
        /// <param name="obj">The object of which we want the relevant properties</param>
        /// <param name="T">The type of the desired objects</param>
        /// <returns>list of properties of type T</returns>
        private IEnumerable<PropertyInfo> GetPropertyList(object obj, Type T)
        {
            return obj.GetType().GetProperties().Where(
                p => p.CustomAttributes.Any(s1 => s1.AttributeType == T));
        }

        private string GetDescription(PropertyInfo pi, Type D)
        {
            string name = null;
            foreach (var piAttr in pi.CustomAttributes)
            {
                if (piAttr.AttributeType == D)
                {
                    name = piAttr.ConstructorArguments[0].ToString().Replace("\"", " ");
                    break;
                }
            }

            return name;
        }

        // Get the node name from the CustmAttributes or whatever:
        private string GetName(PropertyInfo pi, Type T)
        {
            var name = "Missing";
            foreach (var piAttr in pi.CustomAttributes)
            {
                if (piAttr.AttributeType == T)
                {
                    name = piAttr.ConstructorArguments.Count > 0
                        ? piAttr.ConstructorArguments[0].ToString()
                        : piAttr.NamedArguments[0].ToString();
                    break;
                }
            }

            return name.Split('=').Last().Replace("\"", "").Replace("[]", "").Trim();
        }
    }
}
