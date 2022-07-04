using System;
using System.Linq;
using System.Collections.Generic;

class Gene
{
    double a = Math.Pow(0.1,Program.degit);
    public bool[] gene;
    public double[] weights;
    public static int geneLength=0;
    bool initialized;
    public readonly double value;
    double[] min=Program.min;
    double[] max = Program.max; 
    int [] range=new int[Program.min.Length];
    void Initialize()
    {
        for (int i = 0; i < min.Length; i++)
        {
            range[i] = (int)((max[i]-min[i])/a);
        }
        geneLength = range.Sum();
        initialized = true;
    }

    public Gene(double []weights,double value)
    {
        if(!initialized) Initialize();
        this.value = value;
        this.weights = weights;
        gene = new bool[geneLength];
        EnCode();
    }
    public Gene(bool[] gene)
    {
        if (!initialized) Initialize();
        this.gene = gene;
        this.weights = new double[min.Length];
        DeCode();
    }

    public void EnCode()
    {
        int tmp_0 = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            var tmp_array = MakeRandomBooleanArray(range[i], (int)((weights[i] - min[i]) / a));
            for (int j = 0; j < range[i]; j++)
            {
                gene[j + tmp_0] = tmp_array[j];
            }
            tmp_0 += range[i];
        }
    }
    void DeCode()
    {
        int tmp_0 = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = min[i];
            int tmp_count = 0;
            for (int j = 0; j < range[i]; j++)
            {
                if(gene[j + tmp_0]) tmp_count++;
            }
            weights[i] = tmp_count*a;
            tmp_0 += range[i];
        }
    }

    bool[] MakeRandomBooleanArray(int len,int countOfTrue)
    {
        var tmp_result=new bool [len];
        while (countOfTrue > 0)
        {
            countOfTrue--;
            tmp_result[countOfTrue] = true;
        }
        return tmp_result.OrderBy(i=>Guid.NewGuid()).ToArray();
    }
}