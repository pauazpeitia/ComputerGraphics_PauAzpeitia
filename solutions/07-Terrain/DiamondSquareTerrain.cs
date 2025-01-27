using System;

public class DiamondSquareTerrain
{
    private int size;
    private float[,] heightMap;
    private Random random;
    private float roughness;

    public DiamondSquareTerrain(int size, float roughness)
    {
        this.size = size;
        this.roughness = roughness;
        this.random = new Random();
        heightMap = new float[size, size];

        InitializeCorners();
        GenerateTerrain();
    }

    private void InitializeCorners()
    {
        // Inicializar las esquinas con valores aleatorios
        heightMap[0, 0] = RandomValue();
        heightMap[0, size - 1] = RandomValue();
        heightMap[size - 1, 0] = RandomValue();
        heightMap[size - 1, size - 1] = RandomValue();
    }

    private void GenerateTerrain()
    {
        int stepSize = size - 1;
        float scale = roughness;

        while (stepSize > 1)
        {
            DiamondStep(stepSize, scale);
            SquareStep(stepSize, scale);

            stepSize /= 2;   // Reducimos el tamaño del paso
            scale /= 2;      // Reducimos la aleatoriedad
        }
    }

    private void DiamondStep(int stepSize, float scale)
    {
        int halfStep = stepSize / 2;

        for (int x = halfStep; x < size - 1; x += stepSize)
        {
            for (int y = halfStep; y < size - 1; y += stepSize)
            {
                float average = (heightMap[x - halfStep, y - halfStep] +
                                 heightMap[x - halfStep, y + halfStep] +
                                 heightMap[x + halfStep, y - halfStep] +
                                 heightMap[x + halfStep, y + halfStep]) / 4.0f;

                heightMap[x, y] = average + RandomValue() * scale;
            }
        }
    }

    private void SquareStep(int stepSize, float scale)
    {
        int halfStep = stepSize / 2;

        for (int x = 0; x < size; x += halfStep)
        {
            for (int y = (x + halfStep) % stepSize; y < size; y += stepSize)
            {
                float sum = 0;
                int count = 0;

                if (x - halfStep >= 0)
                {
                    sum += heightMap[x - halfStep, y];
                    count++;
                }
                if (x + halfStep < size)
                {
                    sum += heightMap[x + halfStep, y];
                    count++;
                }
                if (y - halfStep >= 0)
                {
                    sum += heightMap[x, y - halfStep];
                    count++;
                }
                if (y + halfStep < size)
                {
                    sum += heightMap[x, y + halfStep];
                    count++;
                }

                heightMap[x, y] = sum / count + RandomValue() * scale;
            }
        }
    }

    private float RandomValue()
    {
        return (float)(random.NextDouble() * 2 - 1); // Valor aleatorio entre -1 y 1
    }

    public float[,] GetHeightMap()
    {
        return heightMap;
    }
}
