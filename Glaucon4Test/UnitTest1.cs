

using System.Globalization;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics;
using gl = Terwiel.Glaucon;
using System.Text.RegularExpressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestGlaucon
{
    [TestClass]
    public partial class UnitTestGlaucon2
    {        
        const string pathName = @"E:\Users\erik\Documents\Visual Studio 2022\Projects\Glaucon4\Glaucon4Test\Resources\";
        static BinaryWriter bw;   
        static DenseMatrix deflection, Reactions;
        static DenseMatrix  EndForces;   
       
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Debug.WriteLine("Enter " + MethodBase.GetCurrentMethod().Name);
            var resultsDir = pathName +  @"Results\";
            if (!Directory.Exists(resultsDir))
            {
                Directory.CreateDirectory(resultsDir);
            }
            if (Directory.EnumerateFileSystemEntries(resultsDir).Any())
            {
                string[] files = Directory.GetFiles(resultsDir);
                foreach (string s in files)
                    File.Delete(s);
            }
            var culture =
               Thread.CurrentThread.CurrentCulture =
               Thread.CurrentThread.CurrentUICulture =
               CultureInfo.CreateSpecificCulture("en-US");
            var nf = culture.NumberFormat.NumberDecimalSeparator;
            Assert.IsTrue(nf.Equals("."), "Error: Culture's decimal separator must be '.'");           
            //bw = new BinaryWriter(ms = new MemoryStream());
            Debug.WriteLine("Exit " + MethodBase.GetCurrentMethod().Name);
        }
        // Use TestInitialize to run code before running each test 

        
        string nativeProviderPath = 
            @"E:\Users\erik\Documents\Visual Studio 2023\Projects\Glaucon4\Glaucon4\bin\Debug\net6.0-windows10.0.22621.0\MKL"
            ;

        [TestInitialize]
        public void MyTestInitialize()
        {

            Control.NativeProviderPath = nativeProviderPath;
            Control.UseNativeMKL();

        }      
      

        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()],
        public static void MyClassCleanup()
        {
            bw.Close();
        }       

        [TestCleanup]
        public void CleanupTest()
        {            
            gl.Glaucon.K = null;
            gl.Glaucon.M = null;
        }
    
        void CheckMatrix(Matrix<double> is_, Matrix<double> soll, int digits,  string name)
        {
            Debug.WriteLine($"Frobenius norm {name} Is={is_.FrobeniusNorm()}, Soll={soll.FrobeniusNorm()}");
            Debug.WriteLine($"Infinity norm {name} Is={is_.InfinityNorm()}, Soll={soll.InfinityNorm()}");
            Debug.WriteLine($"Euclidian norm {name} Is={Math.Sqrt(is_.PointwiseMultiply(is_).Column(0).Sum())}, " + 
            $"Soll={Math.Sqrt(soll.PointwiseMultiply(soll).Column(0).Sum())}\n");

            Debug.Assert(is_.AlmostEqualRelative(soll,Math.Pow(10,-digits)),$"{name} not good.");          
        }

        //void CheckMatrix(Matrix<double> is_, Matrix<double> soll, int digits,  string name)
        //{
        //    double s = 1;
        //    for (int i = 0; i < soll.RowCount; i++)
        //        for (int j = 0; j < soll.ColumnCount; j++)
        //        {
        //            s = Math.Log10(Math.Abs((soll[i, j] - is_[i, j]) / soll[i, j]));
        //            if (!double.IsNaN(s) && !double.IsPositiveInfinity(s))
        //                Assert.IsTrue(s < -digits, $"{name} not good at [{i},{j}]: {is_[i, j]} must be {soll[i, j]}.");
        //        }
        //    Debug.WriteLine($"{name}: precision: {s}");
        //}

        void CheckVector(Vector<double> is_, Vector<double> soll, int digits,  string name )
        {
            // Debug.WriteLine($"Frobenius norm {name} Is={is_.FrobeniusNorm()}, Soll={soll.FrobeniusNorm()}");
            Debug.WriteLine($"Infinity norm {name} Is={is_.InfinityNorm()}, Soll={soll.InfinityNorm()}");
            Debug.WriteLine($"Euclidian norm {name} Is={Math.Sqrt(is_.PointwiseMultiply(is_).Sum())}, " + 
            $"Soll={Math.Sqrt(soll.PointwiseMultiply(soll).Sum())}\n");

            Debug.Assert(is_.AlmostEqualRelative(soll,Math.Pow(10,-digits)),$"{name} not good.");    
        }

        void CheckVector2(Vector<double> is_, Vector<double> soll, int digits, string name)
        {
            double s = 1;
            for (int i = 0; i < soll.Count; i++)
            {
                s = Math.Log10(Math.Abs((soll[i] - is_[i]) / soll[i]));
                if (!double.IsNaN(s) && !double.IsPositiveInfinity(s))
                    Assert.IsTrue(s < -digits, $"{name} not good at [{i}]: {is_[i]} must be {soll[i]}.");
            }
            Debug.WriteLine($"{name}: precision: {s}");
        }

    }
}
