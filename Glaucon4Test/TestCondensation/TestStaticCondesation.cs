using MathNet.Numerics;
using System.Reflection;
using Terwiel.Glaucon;
using System.Diagnostics;

namespace UnitTestGlaucon
{
    [TestFixture]
    public partial class TestStaticCond : UnitTestBase
    {

        [Test]
        public void TestStatic()
        {
            #if false
            int ok = 0;
            int[]c = new int[]{1,2,5,0,  0,0 };
            int [] r =new int[6];
            int N=6,n=3;
            int k=0;
            for (int i = 0; i < N; i++)
            {
                ok = 1;
                for (int j = 0; j < n; j++)
                {
                    if (c[j] == i)
                    {
                        ok = 0;
                        break;
                    }
                }
                if (ok == 1)
                    r[k++] = i;
            }
            #endif
            var K = (DenseMatrix)Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {23,-20,3,0,0 },
                {-20,48,-28,0,0 },
                {3,-28,59,-40,6},
                {0,0,-40,96,-56 },
                {0,0,6,-56,50 }
            });
            WriteMatrix(K);
            //Glaucon.K = (DenseMatrix)Glaucon.K.Divide(6d);
            int DoF = 5;
            int Cdof = 2;
            Glaucon.DoFsToCondense = new[] { 1,3 };
            //Glaucon.Cdof = Glaucon.DoFsToCondense.Length;
            Permutation p = new Permutation(new int[]{1,3,0,2,4 });
            K.PermuteColumns(p);
            WriteMatrix(K);
            K.PermuteRows(p);
            WriteMatrix(K);
            var R = (DenseMatrix) Matrix<double>.Build.DenseOfArray(new double[,] {{0,0,0,0,2d } }).Transpose();
            R.PermuteRows(p);
            var Kaa = K.SubMatrix(0,Cdof,0,Cdof);
            WriteMatrix(Kaa);
            var Kac = K.SubMatrix(0,Cdof,Cdof,DoF- Cdof);
            WriteMatrix(Kac);
            var Kca = K.SubMatrix(Cdof,DoF-Cdof,0,Cdof);
            WriteMatrix(Kca);
            var Kcc = K.SubMatrix(Cdof,DoF-Cdof, Cdof,DoF - Cdof);
            WriteMatrix(Kcc);
            var Ra = (DenseVector) R.SubMatrix(0,Cdof,0,1).Column(0);
            var Rc = (DenseVector) R.SubMatrix(Cdof,DoF-Cdof,0,1).Column(0);
            var s = Kaa - Kac.Multiply(Kcc.Inverse()).Multiply(Kca); 
            var t = Kac.Multiply(Kcc.Inverse());
             

            Glaucon.StaticCondensation();

            
            var A = Glaucon.Kc * 13d / 9d;
        }

        private void WriteMatrix(Matrix<double> matrix)
        {
            for(int i = 0; i < matrix.RowCount; i++)
            {
                for (int j=0; j < matrix.ColumnCount; j++)
                {
                    Debug.Write(String.Format("{0,5} ",matrix[i,j]));
                }
                Debug.Write("\n");
            }
            Debug.Write("\n");
        }
    }
}
