using System;
using Raylib_cs;

namespace Server
{
    class NN
    {
        public int[] NetworkShape;
        public Layer[] Layers;

        public NN(int[] networkShape)
        {
            NetworkShape = networkShape;

            Layers = new Layer[NetworkShape.Length - 1];

            for (int i = 0; i < Layers.Length; i++)
            {
                Layers[i] = new Layer(NetworkShape[i], NetworkShape[i + 1]);
            }
        }

        public float[] Brain(float[] inputs)
        {
            for (int i = 0; i < Layers.Length; i++)
            {
                if (i == 0)
                {
                    Layers[i].Forward(inputs);
                    Layers[i].Activation();
                }
                else if (i == Layers.Length - 1)
                {
                    Layers[i].Forward(Layers[i - 1].Nodes);
                }
                else
                {
                    Layers[i].Forward(Layers[i - 1].Nodes);
                    Layers[i].Activation();
                }
            }

            return Layers[Layers.Length - 1].Nodes;
        }

        public Layer[] CopyLayers()
        {
            Layer[] tmpLayers = new Layer[NetworkShape.Length - 1];

            for (int i = 0; i < Layers.Length; i++)
            {
                tmpLayers[i] = new Layer(NetworkShape[i], NetworkShape[i + 1]);

                Array.Copy(Layers[i].Weights, tmpLayers[i].Weights, Layers[i].Weights.GetLength(0) * Layers[i].Weights.GetLength(1));
                Array.Copy(Layers[i].Biases, tmpLayers[i].Biases, Layers[i].Biases.GetLength(0));
            }

            return tmpLayers;
        }

        public void MutateNetwork(float mutationChance, float mutationAmount)
        {
            for (int i = 0; i < Layers.Length; i++)
            {
                Layers[i].MutateLayer(mutationChance, mutationAmount);
            }
        }

        public class Layer
        {
            public float[,] Weights;
            public float[] Biases;
            public float[] Nodes;

            private int _numInputs;
            private int _numNodes;

            public Layer(int numInputs, int numNodes)
            {
                _numInputs = numInputs;
                _numNodes = numNodes;

                Weights = new float[numNodes, numInputs];
                Biases = new float[numNodes];
            }

            public void Forward(float[] inputs)
            {
                Nodes = new float[_numNodes];

                for (int i = 0; i < _numNodes; i++)
                {
                    for (int j = 0; j < _numInputs; j++)
                    {
                        Nodes[i] += inputs[j] * Weights[i, j];
                    }

                    Nodes[i] += Biases[i];
                }
            }

            public void Activation()
            {
                for (int i = 0; i < Nodes.Length; i++)
                {
                    Nodes[i] = (float)Math.Tanh(Nodes[i]);
                }
            }

            public void MutateLayer(float mutationChance, float mutationAmount)
            {
                for(int i = 0; i < _numNodes; i++)
                {
                    for(int j = 0; j < _numInputs; j++)
                    {
                        if(Raylib.GetRandomValue(0, 100) / 100.0f < mutationChance)
                        {
                            Weights[i,j] += Raylib.GetRandomValue(-100, 100) / 100.0f * mutationAmount;
                        }

                        if(Raylib.GetRandomValue(0, 100) / 100.0f < mutationChance)
                        {
                            Biases[i] += Raylib.GetRandomValue(-100, 100) / 100.0f * mutationAmount;
                        }
                    }
                }
            }
        }
    }
}
