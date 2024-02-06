using System.Collections.Generic;
public class Result
{
    public Result(string OptymalizationAlgorithm, string TestFunction, int D, int M, int I)
    {
        this.OptymalizationAlgorithm = OptymalizationAlgorithm;
        this.TestFunction = TestFunction;
        this.D = D;
        this.M = M;
        this.I = I;
    }
    public void AddResults(List<double> XBest, double FBest,int NumberOfEvaluationFitnessFunction)
    {
        this.XBest = XBest;
        this.FBest = FBest;
        this.NumberOfEvaluationFitnessFunction = NumberOfEvaluationFitnessFunction;
    }
    public string OptymalizationAlgorithm { get; set; }
    public string TestFunction { get; set; }
    public int D { get; set; }
    public int M { get; set; }
    public int I { get; set; }
    public List<double> XBest { get; set; }
    public double FBest { get; set; }
    public int NumberOfEvaluationFitnessFunction { get; set; }
}