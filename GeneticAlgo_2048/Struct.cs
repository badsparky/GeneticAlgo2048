using System;
using System.Collections.Generic;
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
    static int line;
    static int thread_max = Program.thread_max;
    static int count_times = 0;
    static bool[] list_id = new bool[thread_max];
    static int max_Pcounts;
    static int[] _Progress;
    static int Tcounts;
    static int generation;
    static Action stop;
    static Action start;
    static DateTime startTime;
    static DateTime endTime;
    static DateTime tmp_time;
    static TimeSpan TimerPerThreadMAx=new TimeSpan(0,2,0);

    public static void Initialize(int thread_max,int counts, Action Stop,Action Start)
    {
        max_Pcounts = 1500;
        _Progress = new int[thread_max+1];
        Tcounts = counts;
        line = 0;
        stop = Stop;
        start = Start;
        startTime= DateTime.Now;
        tmp_time = startTime;
        generation = Program.Generation;
        endTime = startTime + new TimeSpan(TimerPerThreadMAx.Ticks * (generation * Program.counts * Program.times / thread_max));
    }
    public static int GetId() {
        for (int i = 0; i < thread_max; i++)
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
    public static void Increace_TCount() { _Progress[_Progress.Length - 1]++; count_times = 0;  }
    public static void Show()
    {
        Console.SetCursorPosition(0,line);
        double tmp_p;
        for (int i = 0; i < _Progress.Length - 1; i++)
        {
             tmp_p = _Progress[i] < max_Pcounts+1 ? (double)_Progress[i] / max_Pcounts  : 0.99;
            Console.WriteLine($"Process{count_times+i} [{ new String('=', (int)(tmp_p*13))+ (tmp_p != 1 ? ">":""),-13}] {(Math.Floor(tmp_p * 100 * 10) / 10).ToString("0.0") + "%",6}");
        }
        tmp_p = (double)_Progress[_Progress.Length - 1] / Tcounts;
        Console.WriteLine($"\nTotalProcess [{new String('=', (int)(tmp_p * 13)) + (tmp_p != 1 ? ">" : ""),-13}] {(Math.Floor(tmp_p * 100 * 10) / 10).ToString("0.0") + "%",6}");
        Console.Write($"\nGeneration : {Program.Generation,-3} " );  
        Console.WriteLine($"\nThis will end at : {endTime}");
    }
    public static void Done_P(int id) { _Progress[id] = max_Pcounts; }
    public static void Refulesh_P() 
    { 
        for (int i = 0; i < _Progress.Length - 1; i++) _Progress[i] = 0; 
        list_id = new bool[thread_max]; 
        count_times += thread_max; 
        TimerPerThreadMAx=new TimeSpan((TimerPerThreadMAx+ (DateTime.Now-tmp_time)).Ticks/2);
        tmp_time=DateTime.Now;
        endTime =startTime+ new TimeSpan(TimerPerThreadMAx.Ticks * (generation * Program.counts * Program.times / thread_max));
    }
    public static void Refulesh_T() { _Progress[_Progress.Length-1]=0; }
    public static void Console_Write(string content="",int i=1) 
    { 
        stop(); 
        Console.SetCursorPosition(0, line);
        Console.WriteLine(content); 
        line+=i; 
        start();
    }
    public static void ClearConsole()
    {
        Console.Clear();
        line = 0; 
    }
    
}