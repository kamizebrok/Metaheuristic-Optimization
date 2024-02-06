using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
namespace AI_Optymalization_Program
{
    public class ReferenceMenager
    {
        public ReferenceMenager(MainWindow mainWindow)
        {
            Project_Path = Get_Project_Path();
            OptimizationAlgorithms_Folder_Path = Directory.GetDirectories(Project_Path, "OptimizationAlgorithms", SearchOption.AllDirectories).FirstOrDefault();
            TestFunctions_Folder_Path = Directory.GetDirectories(Project_Path, "TestFunctions", SearchOption.AllDirectories).FirstOrDefault();
            Interfaces_Folder_Path = Directory.GetDirectories(Project_Path, "Interfaces", SearchOption.AllDirectories).FirstOrDefault();
            csproj_Path = Directory.GetFiles(Project_Path, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();
            StateWriter_Folder_Path = Directory.GetDirectories(Project_Path, "Date", SearchOption.AllDirectories).FirstOrDefault();
            this.mainWindow = mainWindow;
        }
        Stopwatch stopwatch;
        MainWindow mainWindow;
        FileMenager fileMenager = new FileMenager();
        TextReportGenerator textReportGenerator = new TextReportGenerator();
        PdfReportGenerator pdfReportGenerator = new PdfReportGenerator();
        public enum E_ReferenceType { OptimizationAlgorithm, TestFunction, Interface }
        public List<Result> results = new List<Result>();
        string Project_Path = string.Empty;
        string OptimizationAlgorithms_Folder_Path = string.Empty;
        string TestFunctions_Folder_Path = string.Empty;
        string Interfaces_Folder_Path = string.Empty;
        string StateWriter_Folder_Path = string.Empty;
        string csproj_Path;
        public List<string> testFunctions = new List<string>();
        public List<string> optimizationAlgorithms = new List<string>();
        List<int> D = new List<int>();
        List<int> DSaver = new List<int>();
        List<int> M = new List<int>();
        List<int> I = new List<int>();
        string lastFunction;
        int lastI;
        int lastD;
        int lastM;
        BigInteger numerOfActions = 0;
        BigInteger actionsDone = 0;
        int procentDone = 0;
        List<string> TestFunctions = new List<string>();
        List<string> OptymalizationAlgorithms = new List<string>();
        public long progressProcentage = 0;
        int timeleft = 0;
        public void AddReference(E_ReferenceType referenceType, string pathToReferenceFile)
        {
            string chosenReferenceType = string.Empty;
            if (referenceType == E_ReferenceType.OptimizationAlgorithm)
            {
                chosenReferenceType = OptimizationAlgorithms_Folder_Path;
            }
            else if (referenceType == E_ReferenceType.TestFunction)
            {
                chosenReferenceType = TestFunctions_Folder_Path;
            }
            else if (referenceType == E_ReferenceType.Interface)
            {
                chosenReferenceType = Interfaces_Folder_Path;
            }
            string fileName = pathToReferenceFile.Split('\\').Last();
            string newReferenceFile = chosenReferenceType + "\\" + fileName;
            File.Copy(pathToReferenceFile, newReferenceFile, true);
            AddReferenceToProject(csproj_Path, newReferenceFile);
            GetReferenceNames();
            if (referenceType == E_ReferenceType.TestFunction)
            {
                AddFunctionToDimentionFile(pathToReferenceFile.Split('\\').Last().Split('.').First());
            }
        }
        public void RunProject(bool continuation)
        {
            bool isItMonster = false;
            stopwatch = Stopwatch.StartNew();
            CountNumberOfActions();
            bool temptContinuation = continuation;
            fileMenager.DelatePDF(StateWriter_Folder_Path + "\\BestResults.pdf");
            fileMenager.DelateResultsFile(StateWriter_Folder_Path + "\\Results.txt", continuation);
            fileMenager.WriteValuesToRun(StateWriter_Folder_Path + "\\ValuesToRun.txt", OptymalizationAlgorithms, TestFunctions, I, D, M);
            foreach (string optymalizationAlgorithm in OptymalizationAlgorithms)
            {
                foreach (string testFunction in TestFunctions)
                {
                    if (TwoDimention(testFunction) && (D.Count != 1 || D.First() != 2))
                    {
                        DSaver = D;
                        D = new List<int>() { 2 };
                    }
                    else if ((!TwoDimention(testFunction) && DSaver.Count != 0))
                    {
                        D = DSaver;
                    }
                    foreach (int i in I)
                    {
                        foreach (int m in M)
                        {
                            foreach (int d in D)
                            {
                                if (!temptContinuation)
                                {
                                    if (testFunction != "DrBrociek")
                                    {
                                        isItMonster = false;
                                    }
                                    else 
                                    {
                                        isItMonster = true;
                                    }
                                    object[] functionValues = { d, m, i, continuation,isItMonster };
                                    if (!continuation)
                                    {
                                        results.Add(new Result(optymalizationAlgorithm, testFunction, d, m, i));
                                        SaveProcessingData();
                                    }
                                    RunFunction(optymalizationAlgorithm, testFunction, functionValues);
                                    SaveResult();
                                }
                                else
                                {
                                    if (testFunction == lastFunction && i == lastI && m == lastM && d == lastD)
                                    {
                                        temptContinuation = false;
                                        if (testFunction != "DrBrociek")
                                        {
                                            isItMonster = false;
                                        }
                                        else
                                        {
                                            isItMonster = true;
                                        }
                                        object[] functionValues = { d, m, i, continuation,isItMonster };
                                        if (!continuation)
                                        {
                                            results.Add(new Result(optymalizationAlgorithm, testFunction, d, m, i));
                                            SaveProcessingData();
                                        }
                                        RunFunction(optymalizationAlgorithm, testFunction, functionValues);
                                        continuation = false;
                                        SaveResult();
                                    }
                                }
                                UpdateProcentDone(testFunction, i, m, d, DSaver);
                            }
                        }
                    }
                    FindBestResult(optymalizationAlgorithm, testFunction);
                }
            }
            ClearData();
            stopwatch.Stop();
            MessageBox.Show("Optimization performed succesfully! Results saved in file 'Date'");
        }
        public void SetValues(List<string> optymalizationAlgorithms, List<string> chosenFunctions, int startI, int jumpI, int AmountI, int startD, int jumpD, int AmountD, int startM, int jumpM, int AmountM)
        {
            for (int i = 0; i < AmountD; i++)
            {
                D.Add(startD + jumpD * i);
            }
            for (int i = 0; i < AmountM; i++)
            {
                M.Add(startM + jumpM * i);
            }
            for (int i = 0; i < AmountI; i++)
            {
                I.Add(startI + jumpI * i);
            }
            TestFunctions = chosenFunctions;
            OptymalizationAlgorithms = optymalizationAlgorithms;

        }
        public void ContinueProject()
        {
            List<string> lastOperation = fileMenager.GetLastOperation(StateWriter_Folder_Path + "\\Results.txt");
            lastFunction = lastOperation[1];
            lastI = int.Parse(lastOperation[2]);
            lastM = int.Parse(lastOperation[3]);
            lastD = int.Parse(lastOperation[4]);
            ReadValuesToRun();
            GetResults();
            RunProject(true);
        }
        public void GetReferenceNames()
        {
            testFunctions.Clear();
            optimizationAlgorithms.Clear();
            List<string> temptTestFunctions = Directory.GetFiles(TestFunctions_Folder_Path, "*.dll").ToList();
            List<string> temptOptimizationAlgorithms = Directory.GetFiles(OptimizationAlgorithms_Folder_Path, "*.dll").ToList();
            foreach (string testFunction in temptTestFunctions)
            {
                testFunctions.Add(testFunction.Split('\\').Last().Split('.').First());
            }
            foreach (string optimizationAlgorithm in temptOptimizationAlgorithms)
            {
                optimizationAlgorithms.Add(optimizationAlgorithm.Split('\\').Last().Split('.').First());
            }
        }
        void GetResults()
        {
            string path = StateWriter_Folder_Path + "\\Results.txt";
            if (File.Exists(path))
            {
                if (File.ReadAllLines(path) != null)
                {
                    List<string> allLines = File.ReadAllLines(path).ToList();
                    for (int k = 0; k < allLines.Count - 1; k++)
                    {
                        List<string> temptValues = allLines[k].Split(';').ToList();
                        List<string> values = new List<string>();
                        List<double> XBest = new List<double>();
                        for (int i = 0; i < temptValues.Count; i++)
                        {
                            values.Add(temptValues[i].Split(':').Last().Replace(" ", ""));
                        }
                        List<string> temptXBest = new List<string>();
                        temptXBest.AddRange(temptValues[temptValues.Count - 2].Split(':').Last().Split(' ').ToList());
                        for (int i = 0; i < temptXBest.Count; i++)
                        {
                            if (temptXBest[i] == string.Empty)
                            {
                                temptXBest.RemoveAt(i);
                                i--;
                            }
                            else
                            {
                                XBest.Add(double.Parse(temptXBest[i].Replace(" ", "")));
                            }
                        }
                        results.Add(new Result(values[0], values[1], int.Parse(values[4]), int.Parse(values[3]), int.Parse(values[2])));
                        results.Last().AddResults(XBest, double.Parse(values[5]), int.Parse(values[7]));
                    }
                    List<string> temptValues1 = allLines.Last().Split(';').ToList();
                    List<string> values1 = new List<string>();
                    for (int i = 0; i < temptValues1.Count - 1; i++)
                    {
                        values1.Add(temptValues1[i].Split(':').Last().Replace(" ", ""));
                    }
                    results.Add(new Result(values1[0], values1[1], int.Parse(values1[4]), int.Parse(values1[3]), int.Parse(values1[2])));
                }
            }
        }
        void ReadValuesToRun()
        {
            string path = StateWriter_Folder_Path + "\\ValuesToRun.txt";
            if (File.Exists(path))
            {
                if (File.ReadAllLines(path) != null)
                {
                    string[] allLines = File.ReadAllLines(path);
                    string algorithmName = allLines[0];
                    TestFunctions = allLines[1].Split(':').Last().Split(' ').ToList();
                    for (int i = 0; i < TestFunctions.Count; i++)
                    {
                        if (TestFunctions[i] == string.Empty)
                        {
                            TestFunctions.RemoveAt(i);
                            i--;
                        }
                    }
                    List<string> temptI = allLines[2].Split(':').Last().Split(' ').ToList();
                    foreach (string value in temptI)
                    {
                        if (value != string.Empty)
                        {
                            I.Add(int.Parse(value));
                        }
                    }
                    List<string> temptD = allLines[3].Split(':').Last().Split(' ').ToList();
                    foreach (string value in temptD)
                    {
                        if (value != string.Empty)
                        {
                            D.Add(int.Parse(value));
                        }
                    }
                    List<string> temptM = allLines[4].Split(':').Last().Split(' ').ToList();
                    foreach (string value in temptM)
                    {
                        if (value != string.Empty)
                        {
                            M.Add(int.Parse(value));
                        }
                    }
                }
            }
        }
        void RunFunction(string chosenAlgorithm, string chosenTestFunction, object[] functionValues)
        {
            object instanceOfAlgorithm = 0;
            object instanceOfFunction = 0;
            Type typeOfAlgorithm = typeof(int);
            Type typeOfFunction = typeof(int);
            string namespaceandNameOfAlgorithm = string.Empty;
            if (CheckIfFileExists(OptimizationAlgorithms_Folder_Path + "\\" + chosenAlgorithm + ".dll"))
            {
                Assembly externalAssembly = Assembly.LoadFrom(OptimizationAlgorithms_Folder_Path + "\\" + chosenAlgorithm + ".dll");
                chosenAlgorithm = ReplaceSpace(chosenAlgorithm);
                string chosenNamespaceAndClass;
                chosenNamespaceAndClass = FindClass(externalAssembly, chosenAlgorithm, "Solve");
                if (chosenNamespaceAndClass != null)
                {
                    typeOfAlgorithm = externalAssembly.GetType(chosenNamespaceAndClass);
                    instanceOfAlgorithm = Activator.CreateInstance(typeOfAlgorithm);
                }
            }
            if (CheckIfFileExists(TestFunctions_Folder_Path + "\\" + chosenTestFunction + ".dll"))
            {
                Assembly externalAssembly = Assembly.LoadFrom(TestFunctions_Folder_Path + "\\" + chosenTestFunction + ".dll");
                chosenTestFunction = ReplaceSpace(chosenTestFunction);
                string chosenNamespaceAndClass;
                chosenNamespaceAndClass = FindClass(externalAssembly, chosenTestFunction, "Count_E");
                if (chosenNamespaceAndClass != null)
                {
                    typeOfFunction = externalAssembly.GetType(chosenNamespaceAndClass);
                    instanceOfFunction = Activator.CreateInstance(typeOfFunction);
                }
            }
            typeOfAlgorithm.GetProperty("path").SetValue(instanceOfAlgorithm, (StateWriter_Folder_Path));
            typeOfAlgorithm.GetProperty("minXi").SetValue(instanceOfAlgorithm, typeOfFunction.GetProperty("Xmin").GetValue(instanceOfFunction));
            typeOfAlgorithm.GetProperty("maxXi").SetValue(instanceOfAlgorithm, typeOfFunction.GetProperty("Xmax").GetValue(instanceOfFunction));
            typeOfAlgorithm.GetProperty("fitnessFunction").SetValue(instanceOfAlgorithm, Delegate.CreateDelegate(typeOfAlgorithm.GetProperty("fitnessFunction").PropertyType, null, typeOfFunction.GetMethod("Count_E")));
            typeOfAlgorithm.GetMethod("Solve").Invoke(instanceOfAlgorithm, functionValues);
            object _XBest = typeOfAlgorithm.GetProperty("XBest").GetValue(instanceOfAlgorithm);
            object _FBest = typeOfAlgorithm.GetProperty("FBest").GetValue(instanceOfAlgorithm);
            object _NumberOfEvaluationFitnessFunction = typeOfAlgorithm.GetProperty("NumberOfEvaluationFitnessFunction").GetValue(instanceOfAlgorithm);
            if (_XBest is List<double> XBest && _FBest is double FBest && _NumberOfEvaluationFitnessFunction is int NumberOfEvaluationFitnessFunction)
            {
                results.Last().AddResults(XBest, FBest, NumberOfEvaluationFitnessFunction);
            }
        }
        string Get_Project_Path()
        {
            string ProjectName = Assembly.GetExecutingAssembly().GetName().Name;
            string incorrectProject_Path = Directory.GetCurrentDirectory();
            string[] parts = incorrectProject_Path.Split('\\');
            int index = parts.Length;
            string project_path = string.Empty;
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == ProjectName)
                {
                    index = i;
                }
            }
            for (int i = 0; i < index; i++)
            {
                if (i != index - 1)
                {
                    project_path += parts[i] + "\\";
                }
                else
                {
                    project_path += parts[i];
                }
            }
            return project_path;

        }
        bool CheckIfFileExists(string directory)
        {
            if (File.Exists(directory))
            {
                return true;
            }
            else
            {
                Console.WriteLine($"File {directory} does not exist.");
                return false;
            }
        }
        void AddReferenceToProject(string projectFilePath, string dllPath)
        {
            XDocument projectFile = XDocument.Load(projectFilePath);
            XElement itemGroup = projectFile.Root.Elements("ItemGroup").FirstOrDefault(ig => ig.Elements("Reference").Any());
            if (itemGroup == null)
            {
                itemGroup = new XElement("ItemGroup");
                projectFile.Root.Add(itemGroup);
            }
            itemGroup.Add(new XElement("Reference", new XAttribute("Include", Path.GetFileNameWithoutExtension(dllPath)), new XElement("HintPath", dllPath)));
            projectFile.Save(projectFilePath);
        }
        string FindClass(Assembly assembly, string chosenNamespace, string chosenMethod)
        {
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.IsClass)
                {
                    if (type.FullName.Contains(chosenNamespace))
                    {
                        if (MethodExists(type, chosenMethod))
                        {
                            return type.FullName;
                        }
                    }
                }
            }
            Console.WriteLine($"There is no class in {chosenNamespace} namespace");
            return null;
        }
        bool MethodExists(Type type, string chosenMethod)
        {
            if (chosenMethod == null)
            {
                return false;
            }
            foreach (MethodInfo method in type.GetMethods())
            {
                if (chosenMethod == method.Name) { return true; }
            }
            return false;
        }
        string ReplaceSpace(string algorithm_Name)
        {
            for (int i = 0; i < algorithm_Name.Length; i++)
            {
                if (algorithm_Name[i] == ' ')
                {
                    char[] charArray = algorithm_Name.ToCharArray();
                    charArray[i] = '_';
                    return new string(charArray);
                }
            }
            return algorithm_Name;
        }
        void SaveProcessingData()
        {
            string path = StateWriter_Folder_Path + "\\Results.txt";
            string newContext = $"Algorithm: {results.Last().OptymalizationAlgorithm} ; " +
                  $"TestFunction: {results.Last().TestFunction} ; " +
                  $"I: {results.Last().I} ; " +
                  $"M: {results.Last().M} ; " +
                  $"D: {results.Last().D} ; ";
            textReportGenerator.GenerateReport(path, newContext);
        }
        void SaveResult()
        {
            string path = StateWriter_Folder_Path + "\\Results.txt";
            string newContext = $"FBest: {results.Last().FBest} ; " +
                                $"XBest: {string.Join("   ", results.Last().XBest)} ; " +
                                $"NumberOfEvaluationFitnessFunction: {results.Last().NumberOfEvaluationFitnessFunction} \n";
            textReportGenerator.GenerateReport(path, newContext);
        }
        void FindBestResult(string processingOptymalizationAlgorithm, string finishedTestFunction)
        {
            double bestResult = double.MaxValue;
            int index = 0;
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].TestFunction == finishedTestFunction && results[i].OptymalizationAlgorithm==processingOptymalizationAlgorithm)
                {
                    if (bestResult > results[i].FBest)
                    {
                        index = i;
                        bestResult = results[i].FBest;
                    }
                }
            }
            string newContext = $"Optymalization algorithm: {results[index].OptymalizationAlgorithm} \n" +
                               $"Fitness function: {results[index].TestFunction} \n" +
                               $"Number of dimention: {results[index].D} \n" +
                               $"Number of population: {results[index].M} \n" +
                               $"Number of iteration: {results[index].I} \n" +
                               $"FBest: {results[index].FBest} \n" +
                               $"XBest: {string.Join("  ", results[index].XBest)} \n" +
                               $"Number of evaluation fitness function: {results[index].NumberOfEvaluationFitnessFunction} \n" +
                               "------------------------------------------------------------------------------------------------";
            pdfReportGenerator.GenerateReport(StateWriter_Folder_Path + "\\BestResults.pdf", newContext);
        }
        bool TwoDimention(string chosenFunction)
        {
            if (File.ReadAllText(StateWriter_Folder_Path + "\\TwoDimentionFunctions.txt").Contains(chosenFunction))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        void AddFunctionToDimentionFile(string chosenTestFunction)
        {
            if (CheckIfFileExists(TestFunctions_Folder_Path + "\\" + chosenTestFunction + ".dll"))
            {
                Assembly externalAssembly = Assembly.LoadFrom(TestFunctions_Folder_Path + "\\" + chosenTestFunction + ".dll");
                chosenTestFunction = ReplaceSpace(chosenTestFunction);
                string chosenNamespaceAndClass;
                chosenNamespaceAndClass = FindClass(externalAssembly, chosenTestFunction, "Count_E");
                if (chosenNamespaceAndClass != null)
                {
                    Type typeOfFunction = externalAssembly.GetType(chosenNamespaceAndClass);
                    object instanceOfFunction = Activator.CreateInstance(typeOfFunction);
                    object _twoDimensions = typeOfFunction.GetProperty("twoDimensions").GetValue(instanceOfFunction);
                    if (_twoDimensions is bool twoDimensions)
                    {
                        if (twoDimensions == true)
                        {
                            string path = StateWriter_Folder_Path + "\\TwoDimentionFunctions.txt";
                            string existingContent = File.ReadAllText(path);
                            string updatedContent;
                            if (existingContent != string.Empty && !existingContent.Contains(chosenTestFunction))
                            {
                                updatedContent = existingContent + "\n" + chosenTestFunction;
                            }
                            else
                            {
                                updatedContent = chosenTestFunction;
                            }
                            File.WriteAllText(path, updatedContent);
                        }
                    }
                }
            }
        }
        void ClearData()
        {
            results.Clear();
            D.Clear();
            DSaver.Clear();
            M.Clear();
            I.Clear();
            TestFunctions.Clear();
        }
        void CountNumberOfActions()
        {
            numerOfActions = OptymalizationAlgorithms.Count * TestFunctions.Count * I.Sum() * M.Sum() * D.Sum();
        }
        void UpdateProcentDone(string testFunction, int I, int M, int D, List<int> DSaver)
        {
            if (TwoDimention(testFunction) && DSaver.Count != 0)
            {
                actionsDone += I * M * DSaver.Sum();
            }
            else
            {
                actionsDone += I * M * D;
            }
            procentDone = (int)(actionsDone * 100 / numerOfActions);
            if (procentDone != 0)
            {
                timeleft = (100 * stopwatch.Elapsed.Seconds / procentDone - stopwatch.Elapsed.Seconds);
            }
            else
            {
                timeleft = (100 * stopwatch.Elapsed.Seconds - stopwatch.Elapsed.Seconds);
            }
            mainWindow.UpdateProgressBar(procentDone, timeleft);
        }
    }
}



