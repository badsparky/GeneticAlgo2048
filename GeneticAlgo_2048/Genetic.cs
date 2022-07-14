using System;
using System.Linq;
using System.Collections.Generic;

class Genetic
{
    Random rand ;
    double  DecreaseBy =1;
    List<List<double>> WAndS;
    public List<Gene> list_gene;
    double[] ratio;
    int counts;

    public Genetic(List<List<double>> WeightsAndScore)
    {
        rand = new Random();
        this.WAndS = WeightsAndScore;
        list_gene = new List<Gene>();
        counts = WAndS[0].Count;

        double sum=WAndS[WAndS.Count-1].Sum();
        double max= WAndS[WAndS.Count - 1].Max();
        ratio = WAndS[WAndS.Count - 1].Select(x => (max-x) / (counts*max-sum)).ToArray();
        var tmp=ratio.Sum();

        for (int i = 0; i < counts; i++)
        {
            double[] weights =new double[WAndS.Count-1];
            for (int j = 0; j < weights.Length; j++) weights[j] = WAndS[j][i];
            list_gene.Add(new Gene(weights, WAndS[WAndS.Count - 1][i]));
        }
    }
    int[] Select ()
    {
        int[] result=new int[2];
        for (int i = 0; i < 2; i++)
        {
            double randomV=rand.NextDouble();
            for ( int j = 0; j < counts&&randomV>0; j++)
            {
                randomV-=ratio[j];
                result[i] = j;
            }
        }
        return result;
    }
    public void RunGenetic()
    {
        List<Gene> tmp_geneList=new List<Gene>();
        for (int i = 0; i < Math.Floor( counts*DecreaseBy); i++)
        {
            int[] parents=Select();
            bool[] child = Birth(list_gene[parents[0]].gene, list_gene[parents[1]].gene);
            tmp_geneList.Add(new Gene(child));
        }
        list_gene= tmp_geneList;
    }
    bool[] Birth(bool[] parent1,bool[] paret2)
    {
        bool [] child = new bool[parent1.Length];
        for (int i = 0; i < child.Length; i++)
        {
            child[i] = rand.NextDouble() > 0.5 ? parent1[i] : paret2[i];
        }
        return child;
    }
    


}