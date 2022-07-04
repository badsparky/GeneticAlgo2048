using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;


public static class Program
{
    static System.Timers.Timer timer=CreateTimer();
    public static readonly int thread_max = 10;
     
    public static int times =20;//10以上は、諸事情により10の倍数のみ
        
    static Dictionary<string, Weight> weights;
    public static int counts;
    static bool ReadFromCsv=false;
    static string path_result = "WeightsValues.csv";
    static string path_Log = "WeightsValues_log.csv";
    public static int degit = 1;//小数点以下桁数

    static List<double> list_smoothness = new List<double>() { };
    static List<double> list_monotonicity = new List<double>() { };
    static List<double> list_emptyCells = new List<double>() {0  };
    static List<double> list_maxValue = new List<double>() { 0 };
    static List<double> Score=new List<double>();
    static List<List<double>> list_weights = new List<List<double>>
    {
        list_smoothness,
        list_monotonicity,
        list_emptyCells,
        list_maxValue,
        Score
     };

    static(double min, double max) limit_smoothness = (1, 6.9);
    static (double min, double max) limit_monotonicity = (0, 3.0);
    static (double min, double max) limit_emptyCells = (0, 0);
    static (double min, double max) limit_maxValue = (0, 0);

    public static readonly double[] min = 
    { 
        limit_smoothness.min,
        limit_monotonicity.min,
        limit_emptyCells.min,
        limit_maxValue.min
    };
    public static readonly double[] max =
{
        limit_smoothness.max,
        limit_monotonicity.max,
        limit_emptyCells.max,
        limit_maxValue.max
    };

    static void Initialize()
    {
        counts = 15;//初期生成個数
        if (ReadFromCsv && File.Exists("./" + path_result))
        {
            return;
        }
        else
        {
            GenerateWeights();
            CalValues();
            ReadFromCsv = true;
        }
        
    }
    public static void Main()
    {
        Initialize();
        while (Score.Count > 4) Run();
        openFile(path_result);
        openFile(path_Log);
    }
    static void Run()
    {
        ReadData();
        Genetic genetic=new Genetic(list_weights);
        genetic.RunGenetic();
        foreach (var list in list_weights) list.Clear();
        for (int i = 0; i < genetic.list_gene.Count; i++)
        {
            for (int j = 0; j < list_weights.Count-1; j++)
            {
                list_weights[j].Add(genetic.list_gene[i].weights[j]);
            }
        }
        counts = list_weights[0].Count;
        GenerateWeights();
        CalValues();
    }


    static void ReadData()
    {
        foreach(var list in list_weights) if(list!=null)list.Clear();
        string path = "./" + path_result;
        var lines = File.ReadAllLines(path, Encoding.GetEncoding("shift-jis")).Where((x,i)=>i!=0).ToArray();
        counts = lines.Length;
        foreach(var line in lines)
        {
            var values = line.Split(',').Select(x=>double.Parse(x)).ToArray();
            for (int i = 0; i < list_weights.Count; i++) list_weights[i].Add(values[i]);
        }
    }

    static void GenerateWeights()
    {

        bool IsRandomFill = true;

        weights = new Dictionary<string, Weight>() {
            { "smoothness",new Weight( limit_smoothness, list_smoothness, IsRandomFill ,0)},
            {"monotonicity", new Weight( limit_monotonicity, list_monotonicity, IsRandomFill ,1)},
            {"emptyCells", new Weight( limit_emptyCells, list_emptyCells, IsRandomFill ,2)},
            {"maxValue", new Weight( limit_maxValue, list_maxValue, IsRandomFill ,3)}
        };
    }



    public static void CalValues()
    {
        

        Progress.Initialize(thread_max, counts);
        timer.Start();

        for (int id = 0; id < counts; id++)
        {
            Progress.Refulesh_P();
            Main_Last(id);
            Progress.Increace_TCount();
        }

        Progress.Show();
        timer.Stop();
        using (StreamWriter w = new StreamWriter("./" + path_result, false, Encoding.GetEncoding("Shift_JIS")))
        {
            w.WriteLine ("smoothness,monotonicity,emptyCells,maxValue,Score");
            for (int i = 0; i < counts; i++)
            {
                foreach (var key in weights.Keys) w.Write(weights[key][i] + ",");
                w.Write(Score[i]+"\n");
            }
        }
        using (StreamWriter w = new StreamWriter("./" + path_Log, true, Encoding.GetEncoding("Shift_JIS")))
        {
            for (int i = 0; i < counts; i++)
            {
                foreach (var key in weights.Keys) w.Write(weights[key][i] + ",");
                w.Write(Score[i] + "\n");
            }
        }
        
    }

    public static void Main_Last( int id)
    {
        Evaluation2.smoothness = weights["smoothness"][id];
        Evaluation2.monotonicity = weights["monotonicity"][id];
        Evaluation2.emptyCells = weights["emptyCells"][id];
        Evaluation2.maxValue = weights["maxValue"][id];
        //*********************
        Type type_solver = typeof(B22Solver);//()内変更
        //************************
        string path_log = "";//マルチタスクだとバグる
        string path_result = $"ResultFile{id}.txt";
        DateTime start =DateTime.Now;
        DateTime end;
        Random rand;
        double sum_count_total = 0;
        int[] log_score = new int[times];
        int[] log_count = new int[times];
        rand = new Random();
        int limit_a = thread_max;
        int a = times<limit_a?times:limit_a;
        times -= times%a;
        for (int i = 0; i <times/a ; i++)
        {
            Progress.Refulesh_P();
            List<Task<int[]>> tasks = new List<Task<int[]>>();
            for (int j = 0; j < a; j++)
            {
                tasks.Add(Task.Run(() =>
                {
                    int tmp_id = Progress.GetId();
                    string[] str = new string[2];
                    str[0] = rand.Next().ToString();
                    str[1] = path_log;
                    return Main_Middle(str,type_solver,tmp_id);
                })) ;
                Thread.Sleep(10);
            }
            Task.WaitAll(tasks.ToArray());
            for (int j = 0; j < tasks.Count(); j++)
            {
                log_score[i*limit_a+j] = tasks[j].Result[0];
                log_count[i*limit_a+j] = tasks[j].Result[1];
                sum_count_total+=tasks[j].Result[1];
            }
        }
        



        end =DateTime.Now;
        int count_success = log_score.Count(x => x == 2048);
        int sum_count_success=0;
        double accuracy = (double)count_success/times;
        Score.Add(log_score.Average());
        using (FileStream fs = File.Create("./"+path_result));
        using (StreamWriter w=new StreamWriter("./"+path_result,true, Encoding.GetEncoding("Shift_JIS")))
        {
            w.WriteLine("**************************\n");
            for (int i = 0; i < log_score.Length; i++)
            {
                w.WriteLine($"  score : {log_score[i]} count : {log_count[i]} ");
                if (log_score[i]==2048)sum_count_success += log_count[i];
            }

            string average_count= ""+(double)sum_count_success / count_success;
            w.WriteLine("\n**************************\n");
            foreach (var key in weights.Keys)
            {
                w.WriteLine($"{key} : {weights[key][id]}");
            }
            w.WriteLine("平均スコアLog2 : "+log_score.Average());
            w.WriteLine("平均スコア : "+log_score.Select(x=>Math.Log(x,2)).Average());
            w.WriteLine("Solver : "+type_solver.Name);
            w.WriteLine("試行回数 : " + times);
            w.WriteLine("精度 : " + accuracy);
            w.WriteLine("平均手数 （成功）: " + (double.TryParse( average_count,out double result)?average_count:"no data"));
            w.WriteLine("平均手数 （全部）: " + sum_count_total/times);
            w.WriteLine("平均所要時間（全部） : " +(end-start).TotalMinutes/times+"分");
            w.Close();

            //openFile(path_result);
        }

    }
    static void  openFile(string path)
    {
        Process ps = new Process();
        ps.StartInfo.FileName = path;
        ps.Start();
    }
    public static int[] Main_Middle(string[] args,Type type_solver,int id)
    {
        int rand_seed = args.Length > 0 ? int.Parse(args[0]) : 0;
        string log_path = args.Length > 1 ? args[1] : "";

        Tuple<int, int> fieldsize = new Tuple<int, int>(4, 4);

        GameController cont = new GameController(fieldsize, rand_seed);

        cont.Run(typeof(B22Placer),type_solver,id, log_path : log_path,consoling:false );

        LinkedList<GameField> history=cont.history;

        int [] result=new int[2];
        result[0] = history.Last.Value.maxpiece;
        result[1] = cont.count;
        return result;
    }

    static System.Timers.Timer CreateTimer()
    {
        var tmp_timer = new System.Timers.Timer(1000);
        tmp_timer.Elapsed += (s, e) => {
            Progress.Show();
        };
        return tmp_timer;
    }
}