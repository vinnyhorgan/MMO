namespace Server
{
    class NN
    {
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
                Nodes = new float[numNodes];
            }

            public void Forward(float[] inputs)
            {
                for (int i = 0; i < _numNodes; i++)
                {
                    float sum = 0;

                    for (int j = 0; j < _numInputs; j++)
                    {
                        sum += inputs[j] * Weights[i, j];
                    }

                    Nodes[i] = sum + Biases[i];
                }
            }

            public void Activation()
            {
                for (int i = 0; i < _numNodes; i++)
                {
                    if (Nodes[i] < 0)
                    {
                        Nodes[i] = 0;
                    }
                }
            }
        }

        public int[] NetworkShape = { 2, 4, 4, 2 };
        public Layer[] Layers;

        public void Init()
        {
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
    }
}
