using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace FlappyBird_AI
{
	public partial class Form1 : Form
	{

		public Form1()
		{
			InitializeComponent();

		}
		FlappyBird Fb;
		List<Tuple<PictureBox, PictureBox>> PipesPics;
		List<PictureBox> BirdsPics;
		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
			{
				Fb.Birds[0].Jump();
			}
			else if (e.KeyCode == Keys.P)
			{
				Game_Timer.Stop();
			}
		}
		private void Form1_Load(object sender, EventArgs e)
		{

		}

		private void Game_Timer_Tick(object sender, EventArgs e)
		{
			for (int i = 0; i < Fb.Pipes.Count; i++)
			{
				//Lower
				PipesPics[i].Item1.Location = new Point(Fb.Pipes[i].LowerPipeBoundes.X, Fb.Pipes[i].LowerPipeBoundes.Y);
				PipesPics[i].Item1.Size = new System.Drawing.Size(Fb.Pipes[i].LowerPipeBoundes.Weight, Fb.Pipes[i].LowerPipeBoundes.Height);
				//Upper
				PipesPics[i].Item2.Location = new Point(Fb.Pipes[i].UpperPipeBoundes.X, Fb.Pipes[i].UpperPipeBoundes.Y);
				PipesPics[i].Item2.Size = new System.Drawing.Size(Fb.Pipes[i].UpperPipeBoundes.Weight, Fb.Pipes[i].UpperPipeBoundes.Height);
			}
			for (int i = Fb.Pipes.Count; i < 5; i++)
			{
				//Lower
				PipesPics[i].Item1.Location = new Point(0, 0);
				PipesPics[i].Item1.Size = new System.Drawing.Size(0, 0);
				//Upper
				PipesPics[i].Item2.Location = new Point(0, 0);
				PipesPics[i].Item2.Size = new System.Drawing.Size(0, 0);
			}

			//Bird Jump------------------------------------------
			for (int j = 0; j < Fb.PopulationNumber; j++)
			{
				if (Fb.Birds[j].died)
				{
					BirdsPics[j].Location = new Point(0, 0);
					BirdsPics[j].Size = new System.Drawing.Size(0, 0);
				}
				else
				{
					BirdsPics[j].Size = new System.Drawing.Size(25, 25);
					BirdsPics[j].Location = new Point(34, Fb.Birds[j].BirdBounds.Y);
				}
			}

			int Bircount = 0;
			for (int i = 0; i < Fb.Birds.Count; i++)
			{
				if (Fb.Birds[i].died == false)
				{
					Bircount++;

				}
			}
			label1.Text = "Generation : " + Fb.Generation;
			label2.Text = "Children Alive : " + Bircount;
			label3.Text = "Score : " + Fb.Score;

		}

		private void button1_Click(object sender, EventArgs e)
		{
			Game_Timer.Start();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			Game_Timer.Stop();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < Fb.Birds.Count; i++)
			{
				if (Fb.Birds[i].died == false)
				{
					SaveFileDialog s = new SaveFileDialog();
					if (s.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					{
						Fb.NN[i].SaveNetwork(s.FileName);
					}
					break;
				}
			}
		}

		private void button4_Click(object sender, EventArgs e)
		{
			OpenFileDialog s = new OpenFileDialog();
			if (s.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				Fb = new FlappyBird(s.FileName);
				Fb.PopulationNumber = 1;
				StartTraining();
			}
		}
		private void button5_Click(object sender, EventArgs e)
		{
			Fb = new FlappyBird();
			StartTraining();
		}

		void StartTraining()
		{
			PipesPics = new List<Tuple<PictureBox, PictureBox>>();
			BirdsPics = new List<PictureBox>();
			var bmp = new Bitmap(FlappyBird_AI.Properties.Resources.bird);
			Game_Timer.Interval = 1;
			Game_Timer.Start();
			for (int i = 0; i < 5; i++)
			{
				var bmp1 = new Bitmap(FlappyBird_AI.Properties.Resources.pipe);
				PictureBox pipe1 = new PictureBox();
				pipe1.Image = bmp1;
				pipe1.BackColor = Color.Transparent;
				pipe1.SizeMode = PictureBoxSizeMode.StretchImage;
				pipe1.Location = new Point(0, 0);
				pipe1.Size = new System.Drawing.Size(0, 0);
				pictureBox2.Controls.Add(pipe1);
				//Upper
				var bmp2 = new Bitmap(FlappyBird_AI.Properties.Resources.pipedown);
				PictureBox pipe2 = new PictureBox();
				pipe2.Image = bmp2;
				pipe2.BackColor = Color.Transparent;
				pipe2.SizeMode = PictureBoxSizeMode.StretchImage;
				pipe2.Location = new Point(0, 0);
				pipe2.Size = new System.Drawing.Size(0, 0);
				pictureBox2.Controls.Add(pipe2);
				PipesPics.Add(new Tuple<PictureBox, PictureBox>(pipe1, pipe2));
			}
			for (int i = 0; i < Fb.PopulationNumber; i++)
			{
				PictureBox p = new PictureBox();
				p.Image = bmp;
				p.BackColor = Color.Transparent;
				p.Location = new Point(34, 144);
				p.Size = new System.Drawing.Size(25, 25);
				p.SizeMode = PictureBoxSizeMode.StretchImage;
				pictureBox2.Controls.Add(p);
				BirdsPics.Add(p);
			}
		}

	}
	struct Boundes
	{
		public int X;
		public int Y;
		public int Height;
		public int Weight;
	}

	class FlappyBird
	{
		public List<Pipe> Pipes;
		public List<Bird> Birds;
		public Vector<double> NextPipe;
		public List<NeuralNetwork> NN;
		public int PopulationNumber = 10;
		public int Generation = 1;
		public int Score = 0;
		public string TrainingPath = "";
		double MutationRate = 0.01;
		Vector<double> Fitness;
		Timer GameTimer;
		Random rnd;
		int Counter = 0;
		public FlappyBird(string Training)
		{
			TrainingPath = Training;
			NewFlappyBird();
		}
		public FlappyBird()
		{
			NewFlappyBird();
		}

		void NewFlappyBird()
		{

			Birds = new List<Bird>();
			Pipes = new List<Pipe>();
			NN = new List<NeuralNetwork>();
			rnd = new Random();
			NextPipe = Vector<double>.Build.Dense(PopulationNumber, 9999);
			Fitness = Vector<double>.Build.Dense(PopulationNumber, 0);
			for (int i = 0; i < PopulationNumber; i++)
			{
				NN.Add(new NeuralNetwork(new int[] { 5, 1 }));
				Birds.Add(new Bird());
				if (TrainingPath != "")
				{
					NN[i].LoadNetwork(TrainingPath);
				}
			}

			GameTimer = new System.Windows.Forms.Timer();
			GameTimer.Interval = 10; // specify interval time as you want
			GameTimer.Tick += new EventHandler(BirdGameTimerEvent);
			GameTimer.Start();
		}
		void BirdGameTimerEvent(object source, EventArgs e)
		{
			Vector<double> NextPipeIndex = Vector<double>.Build.Dense(PopulationNumber, -1);
			NextPipe = Vector<double>.Build.Dense(PopulationNumber, 9999);
			if (Counter % 120 == 0)
			{
				Pipe pip = new Pipe(rnd);
				Pipes.Add(pip);
			}
			for (int j = 0; j < PopulationNumber; j++)
			{
				for (int i = 0; i < Pipes.Count; i++)
				{
					if (NextPipe[j] > (Pipes[i].UpperPipeBoundes.X + Pipes[i].UpperPipeBoundes.Weight - Birds[j].BirdBounds.X))
					{
						NextPipe[j] = (uint)(Pipes[i].UpperPipeBoundes.X + Pipes[i].UpperPipeBoundes.Weight - Birds[j].BirdBounds.X);
						NextPipeIndex[j] = i;
					}
				}
			}



			/////////////////////Neural Network////////////////////////////

			for (int i = 0; i < PopulationNumber; i++)
			{
				if (NextPipeIndex[i] != -1)
				{
					Vector<double> Inputs = Vector<double>.Build.Dense(5);
					Inputs[0] = (double)NextPipe[i];
					Inputs[1] = (double)(Birds[i].BirdBounds.Y + Birds[i].BirdBounds.Height - Pipes[(int)(NextPipeIndex[i])].LowerPipeBoundes.Y);
					Inputs[2] = (double)(Pipes[(int)(NextPipeIndex[i])].UpperPipeBoundes.Y + Pipes[(int)(NextPipeIndex[i])].UpperPipeBoundes.Height - Birds[i].BirdBounds.Y);
					Inputs[3] = (double)Birds[i].BirdBounds.Y;
					if (Birds[i].Jumping)
					{
						Inputs[4] = 1;
					}
					Inputs = NN[i].Normalize(Inputs);


					double ddd = NN[i].FeedForward(Inputs)[0];
					if (NN[i].FeedForward(Inputs)[0] >= 0.5)
					{
						Birds[i].Jump();
					}
				}

			}

			//Bird Jump------------------------------------------
			for (int j = 0; j < PopulationNumber; j++)
			{
				if (Birds[j].BirdBounds.Y >= 280 || Birds[j].BirdBounds.Y < 0)
				{
					Birds[j].die();
				}
				for (int i = 0; i < Pipes.Count; i++)
				{
					if (CheckIntersection(Birds[j].BirdBounds, Pipes[i].LowerPipeBoundes) || CheckIntersection(Birds[j].BirdBounds, Pipes[i].UpperPipeBoundes))
					{
						Birds[j].die();
					}
				}
				if (Birds[j].died == false)
				{
					Fitness[j]++;
				}

			}

			for (int i = 0; i < Pipes.Count; i++)
			{
				if (Pipes[i].LowerPipeBoundes.X + Pipes[i].LowerPipeBoundes.Weight < 0)
				{
					Pipes.Remove(Pipes[i]);
				}

			}
			if ((Pipes[(int)NextPipeIndex[Fitness.MaximumIndex()]].LowerPipeBoundes.X - Birds[Fitness.MaximumIndex()].BirdBounds.X) == 0)
			{
				Score++;
			}

			Counter++;
			if (GenerationDied() == true)
			{
				GameTimer.Stop();
				NewGeneration();

			}


		}
		double Normalize(double x, double max)
		{
			return Math.Round(100 * x / max);
		}
		void NewGeneration()
		{
			//Increase Mutation Rate When Fitness is too low
			if (Fitness.Maximum() < 120)
			{
				MutationRate = 0.4;
			}
			else
			{
				MutationRate = 0.01;
			}
			Generation++;
			double total2 = 0;
			for (int i = 0; i < Birds.Count; i++)
			{
				total2 += Fitness[i];
			}
			double total = 0;
			for (int i = 0; i < Birds.Count; i++)
			{
				Fitness[i] = Normalize(Fitness[i], total2);
				total += Fitness[i];
			}
			// New Generation
			int[] Proba = new int[(int)total];
			int index = 0;

			Array.Sort(Fitness.AsArray());
			Array.Reverse(Fitness.AsArray());
			if (total > 0)
				for (int child = 0; child < PopulationNumber; child++)
				{
					for (int i = 0; i < Fitness[child]; i++)
					{
						Proba[index] = child;
						index++;
					}
				}
			total = index;

			for (int child = 0; child < PopulationNumber; child++)
			{
				//Biases
				Vector<double>[] NewBiases = new Vector<double>[NN[child].Biases.Length];
				for (int layers = 0; layers < NN[child].Biases.Length; layers++)
				{
					NewBiases[layers] = Vector<double>.Build.Dense(NN[child].Biases[layers].Count);
					for (int j = 0; j < NN[child].Biases[layers].Count; j++)
					{
						//Mutant Genes
						if ((rnd.Next(0, 100) < (MutationRate * 100)) || (total == 0))
							NewBiases[layers][j] = rnd.Next(-2, 2);
						else
						{
							//Parent Genes
							NewBiases[layers][j] = NN[Proba[rnd.Next(0, (int)total)]].Biases[layers][j];
						}
					}
				}
				NN[child].Biases = NewBiases;

				//Matrices
				Matrix<double>[] NewWeights = new Matrix<double>[NN[child].Weights.Length];
				for (int layers = 0; layers < NN[child].Weights.Length; layers++)
				{
					NewWeights[layers] = Matrix<double>.Build.Dense(NN[child].Weights[layers].RowCount, NN[child].Weights[layers].ColumnCount);
					for (int i = 0; i < NN[child].Weights[layers].RowCount; i++)
					{
						for (int j = 0; j < NN[child].Weights[layers].ColumnCount; j++)
						{
							//Mutant Genes
							if ((rnd.Next(0, 100) < (MutationRate * 100)) || (total == 0))
								NewWeights[layers][i, j] = rnd.Next(-2, 2);
							else
							{
								//Parent Genes
								NewWeights[layers][i, j] = NN[Proba[rnd.Next(0, (int)total)]].Weights[layers][i, j];
							}
						}
					}
				}
				NN[child].Weights = NewWeights;

			}

			Reset();
			for (int i = 0; i < PopulationNumber; i++)
			{
				Birds.Add(new Bird());
			}
			Counter = 0;
			GameTimer.Start();

		}
		bool GenerationDied()
		{
			for (int i = 0; i < Birds.Count; i++)
				if (Birds[i].died == false)
					return false;
			return true;
		}
		void Reset()
		{
			Score = 0;
			Pipes.Clear();
			Birds.Clear();
		}
		bool CheckIntersection(Boundes b1, Boundes b2)
		{
			if (((b1.X + b1.Weight >= b2.X) && (b1.X + b1.Weight <= b2.X + b2.Weight) ||
				(b1.X >= b2.X) && (b1.X <= b2.X + b2.Weight)) &&
				((b1.Y + b1.Height >= b2.Y) && (b1.Y + b1.Height <= b2.Y + b2.Height) ||
				(b1.Y >= b2.Y) && (b1.Y <= b2.Y + b2.Height))
				)
				return true;
			return false;
		}
	}
	class Pipe
	{
		const int HoleSize = 90;
		public Boundes UpperPipeBoundes;
		public Boundes LowerPipeBoundes;
		Timer PipeGameTimer;
		int Speed = 2;


		public Pipe(Random rnd)
		{
			int HolePos = rnd.Next(100, 250);
			UpperPipeBoundes.Y = 0;
			UpperPipeBoundes.Height = HolePos - HoleSize;
			UpperPipeBoundes.Weight = 40;
			UpperPipeBoundes.X = 450;

			LowerPipeBoundes.Y = HolePos;
			LowerPipeBoundes.Height = 304 - HolePos;
			LowerPipeBoundes.Weight = 40;
			LowerPipeBoundes.X = 450;

			PipeGameTimer = new System.Windows.Forms.Timer();
			PipeGameTimer.Interval = 10;
			PipeGameTimer.Tick += new EventHandler(PipeGameTimerEvent);
			PipeGameTimer.Start();
		}

		void PipeGameTimerEvent(object source, EventArgs e)
		{
			UpperPipeBoundes.X -= Speed;
			LowerPipeBoundes.X -= Speed;
		}
	}
	class Bird
	{
		public Boundes BirdBounds;
		public bool died = false, Jumping = false;
		Timer BirdGameTimer;
		int Counter = 0;
		int Gravity = 2;
		int JumpSpeed = 3;
		int JumpAmplitude = 15;
		public Bird(int Gravity_, int JumpSpeed_, int JumpAmplitude_)
		{
			Gravity = Gravity_;
			JumpSpeed = JumpSpeed_;
			JumpAmplitude = JumpAmplitude_;
			Start();
		}
		void Start()
		{
			BirdGameTimer = new System.Windows.Forms.Timer();
			BirdGameTimer.Interval = 10; // specify interval time as you want
			BirdGameTimer.Tick += new EventHandler(BirdGameTimerEvent);
			BirdGameTimer.Start();
		}
		public Bird()
		{

			BirdBounds.X = 34;
			BirdBounds.Y = 140;
			BirdBounds.Height = 25;
			BirdBounds.Weight = 25;
			BirdGameTimer = new System.Windows.Forms.Timer();
			BirdGameTimer.Interval = 1; // specify interval time as you want
			BirdGameTimer.Tick += new EventHandler(BirdGameTimerEvent);
			BirdGameTimer.Start();
		}
		public void Jump()
		{
			Counter = 0;
			Jumping = true;
		}
		public void reset()
		{
			died = false;
			BirdGameTimer.Stop();
			BirdBounds.Y = 140;
			BirdGameTimer.Start();
		}
		public void die()
		{
			died = true;
			BirdGameTimer.Stop();
		}
		void BirdGameTimerEvent(object source, EventArgs e)
		{
			if ((Jumping == true) && (Counter < JumpAmplitude))
			{
				BirdBounds.Y -= JumpSpeed;
				Counter++;
			}
			else
			{
				BirdBounds.Y += Gravity;
			}

		}
	}
}
