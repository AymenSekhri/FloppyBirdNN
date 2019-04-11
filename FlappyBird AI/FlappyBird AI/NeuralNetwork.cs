using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace FlappyBird_AI
{
	public class NeuralNetwork
	{
		int[] Layers;
		public Vector<double>[] Biases;
		public Matrix<double>[] Weights;
		double LearningRate = 0.01;
		public void LoadNetwork(string path)
		{
			byte[] saved = System.IO.File.ReadAllBytes(path);
			int arrindex = 0;
			for (int i = 0; i < Biases.Length; i++)
			{
				double[] block = Biases[i].ToArray();
				Buffer.BlockCopy(saved, arrindex, block, 0, 8 * Biases[i].Count);
				Biases[i] = DenseVector.Build.DenseOfArray(block);
				arrindex += 8 * Biases[i].Count;
			}
			for (int i = 0; i < Weights.Length; i++)
			{
				double[,] block = Weights[i].ToArray();
				Buffer.BlockCopy(saved, arrindex, block, 0, 8 * (Weights[i].ColumnCount * Weights[i].RowCount));
				Weights[i] = DenseMatrix.Build.DenseOfArray(block);
				arrindex += 8 * (Weights[i].ColumnCount * Weights[i].RowCount);
			}
		}

		public void SaveNetwork(string path)
		{
			int size = 0;
			for (int i = 0; i < Biases.Length; i++)
			{
				size += Biases[i].Count;
			}
			for (int i = 0; i < Weights.Length; i++)
			{
				size += Weights[i].ColumnCount * Weights[i].RowCount;
			}
			byte[] saved = new byte[size * 8];
			int arrindex = 0;
			for (int i = 0; i < Biases.Length; i++)
			{
				Buffer.BlockCopy(Biases[i].ToArray(), 0, saved, arrindex, 8 * Biases[i].Count);
				arrindex += 8 * Biases[i].Count;
			}
			for (int i = 0; i < Weights.Length; i++)
			{
				Buffer.BlockCopy(Weights[i].ToArray(), 0, saved, arrindex, 8 * (Weights[i].ColumnCount * Weights[i].RowCount));
				arrindex += 8 * (Weights[i].ColumnCount * Weights[i].RowCount);
			}
			System.IO.File.WriteAllBytes(path, saved);
		}
		public NeuralNetwork(int[] LayerSizes)
		{
			Layers = LayerSizes;
			Biases = new Vector<double>[LayerSizes.Length - 1];
			Weights = new Matrix<double>[LayerSizes.Length - 1];
			for (int i = 1; i < LayerSizes.Length; i++)
			{
				Biases[i - 1] = Vector<double>.Build.Random(LayerSizes[i]);
			}
			for (int i = 1; i < LayerSizes.Length; i++)
			{
				Weights[i - 1] = Matrix<double>.Build.Random(LayerSizes[i], LayerSizes[i - 1]);
			}
		}
		public Vector<double> FeedForward(Vector<double> inputs)
		{
			Vector<double> NextLayer = inputs;
			for (int i = 0; i < Weights.Length; i++)
			{
				NextLayer = VectorSegmoid(Weights[i] * NextLayer + Biases[i]);

			}
			return NextLayer;
		}
		public void Train(Vector<double>[] Inputs, Vector<double>[] DesiredOutputs, double Eta)
		{
			LearningRate = Eta;
			for (int i = 0; i < Inputs.Length; i++)
			{
				Partial_Train(Inputs[i], DesiredOutputs[i]);
			}
		}
		public void Partial_Train(Vector<double> Inputs, Vector<double> DesiredOutputs)
		{
			Vector<double>[] Delta_b = new Vector<double>[Biases.Length];
			Matrix<double>[] Delta_w = new Matrix<double>[Weights.Length];
			Vector<double>[] A = new Vector<double>[Layers.Length];
			Vector<double>[] Z = new Vector<double>[Layers.Length - 1];
			Vector<double> Delta_Z;
			Vector<double> NextLayer = Inputs;
			A[0] = Inputs;
			for (int i = 0; i < Weights.Length; i++)
			{
				Z[i] = Weights[i] * NextLayer + Biases[i];
				A[i + 1] = VectorSegmoid(Z[i]);
				NextLayer = A[i + 1];
			}
			//Get dC/dZ of last layer
			//Delta_Z = VectorSegmoid(Z[Weights.Length - 1]).Subtract(DesiredOutputs);
			Delta_Z = Cost_Prime(A[Weights.Length], DesiredOutputs).PointwiseMultiply(Segmoid_Prime(Z[Weights.Length - 1]));

			//Get dC/dw & dC/db of last layer
			Delta_b[Weights.Length - 1] = Delta_Z;
			Delta_w[Weights.Length - 1] = new DenseMatrix(Weights[Weights.Length - 1].RowCount, Weights[Weights.Length - 1].ColumnCount);
			Delta_w[Weights.Length - 1] = Delta_Z.ToColumnMatrix() * A[Weights.Length - 1].ToRowMatrix(); //Result is Matrix Not a Vector

			//for (int i2 = 0; i2 < Layers[Weights.Length]; i2++)
			//{
			//	Delta_w[Weights.Length - 1].SetRow(i2, Delta_Z[i2] * A[Weights.Length - 1]);
			//}

			for (int i = Weights.Length - 2; i >= 0; i--)
			{
				//Get dC/dZ of other layers
				Delta_Z = (Weights[i + 1].Transpose() * Delta_Z).PointwiseMultiply(Segmoid_Prime(Z[i]));
				//Get dC/dw & dC/db of other layers
				Delta_b[i] = Delta_Z;
				Delta_w[i] = new DenseMatrix(Weights[i].RowCount, Weights[i].ColumnCount);
				Delta_w[i] = Delta_Z.ToColumnMatrix() * A[i].ToRowMatrix(); //Result is Matrix Not a Vector
			}
			//string d = Weights[1].ToString();

			for (int i = 0; i < Weights.Length; i++)
			{
				Biases[i] = Biases[i].Subtract(Delta_b[i].Multiply(LearningRate));
				Weights[i] = Weights[i].Subtract(Delta_w[i].Multiply(LearningRate));
			}
			//MessageBox.Show(d + "\n" + Weights[1].ToString());
		}

		Vector<double> Segmoid_Prime(Vector<double> x)
		{
			Vector<double> tmp = Vector<double>.Build.Dense(x.Count);
			for (int i = 0; i < x.Count; i++)
			{
				tmp[i] = Math.Exp(-x[i]) / Math.Pow((1 + Math.Exp(-x[i])), 2);
			}
			return tmp;
		}
		Vector<double> Cost_Prime(Vector<double> x, Vector<double> y)
		{
			return (x - y);
		}
		Vector<double> VectorSegmoid(Vector<double> x)
		{
			Vector<double> tmp = Vector<double>.Build.Dense(x.Count);
			for (int i = 0; i < x.Count; i++)
			{
				tmp[i] = 1 / (1 + Math.Exp(-x[i]));
			}
			return tmp;
		}
		public Vector<double> Normalize(Vector<double> x)
		{
			Vector<double> tmp = Vector<double>.Build.Dense(x.Count);
			double sum = 0;
			for (int i = 0; i < x.Count; i++)
			{
				sum += x[i];
			}
			sum = sum / x.Count();
			double range = x.Maximum() - x.Minimum();
			for (int i = 0; i < x.Count; i++)
			{
				tmp[i] = (x[i] - sum) / range;
			}
			return tmp;
		}
	}
}
