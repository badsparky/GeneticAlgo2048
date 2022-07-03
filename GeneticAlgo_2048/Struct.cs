using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

struct Weight
{
    static int count;
    static double[] default_Weight = {0.1, 1.0, 2.7, 1.0 };
    public (double min, double max) Weight_limit { get; private set; }
    public List<double> Weights { get; private set; }
    public bool IsAutoFill;
    public Weight((double min, double max) limit, List<double> doubles_w, bool isAutoFill,int id)
    {
        count = Program.counts;
        Weight_limit = (limit.min, limit.max);
        int effective = Program.degit;
        IsAutoFill = (isAutoFill);
        Weights = new List<double>();
        Random random = new Random();
        List<double> doubles_weight = new List<double>(doubles_w);
        doubles_w.Clear();
        for (int i = 0; i < count; i++)
        {
            double weight;

            if (isAutoFill && doubles_weight.Count == 0)  weight = random.NextDouble() * (Weight_limit.max - Weight_limit.min) + Weight_limit.min; 
            else if (doubles_weight.Count() == 1)  weight = doubles_weight[0]; 
            else if(doubles_weight.Count()>1) weight = doubles_weight[i];
            else  weight = default_Weight[id];

            weight = Math.Floor(weight * Math.Pow(10,effective)) / Math.Pow(10, effective);
            Weights.Add(weight);
            doubles_w.Add(weight);
        }
    }
    public double this[int index] {
        get { return  Weights[index]; }
    }
}



static class  Progress
{
    static int times = Program.thread_max;
    static bool[] list_id = new bool[times];
    static int max_Pcounts;
    static int[] _Progress;
    static int Tcounts;
    public static void Initialize(int thread_max,int counts)
    {
        max_Pcounts = 1500;
        _Progress = new int[thread_max+1];
        Tcounts = counts;
        Console.WriteLine("Process Is OnGoing.....\n");
    }
    public static int GetId() {
        for (int i = 0; i < times; i++)
        {
            if (!list_id[i])
            {
                list_id[i] = true;
                return i;
            }
        }
        return 0;
    }
    public static void Increace_PCount(int id) { _Progress[id]++; }
    public static void Increace_TCount() { _Progress[_Progress.Length - 1]++; }
    public static void Show()
    {
        double tmp_p;
        for (int i = 0; i < _Progress.Length - 1; i++)
        {
             tmp_p = _Progress[i] < max_Pcounts+1 ? (double)_Progress[i] / max_Pcounts  : 0.99;
            Console.WriteLine($"Process{i} [{ new String('=', (int)(tmp_p*13))+ (tmp_p != 1 ? ">":""),-13}] {(Math.Floor(tmp_p * 100 * 10) / 10).ToString("0.0") + "%",6}");
        }
        tmp_p = (double)_Progress[_Progress.Length - 1] / Tcounts;
        Console.WriteLine($"\nTotalProcess [{new String('=', (int)(tmp_p * 13)) + (tmp_p != 1 ? ">" : ""),-13}] {(Math.Floor(tmp_p * 100 * 10) / 10).ToString("0.0") + "%",6}");
        Console.SetCursorPosition(0,2);
        
    }
    public static void Done_P(int id) { _Progress[id] = max_Pcounts; }
    public static void Refulesh_P() { for (int i = 0; i < _Progress.Length - 1; i++) _Progress[i] = 0; list_id = new bool[times]; }
}