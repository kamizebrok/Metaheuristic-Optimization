using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AI_Optymalization_Program
{
    public partial class MainWindow : Window
    {
        ReferenceMenager referenceMenager;
        Task backgroundTask = null;
        int lastprogresbarValue = 0;
        public MainWindow()
        {
            referenceMenager = new ReferenceMenager(this);
            InitializeComponent();
            FillListBoxes();
        }
        private void ButtonAddAlgorythm_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Wybierz plik algorytmu";
            openFileDialog.Filter = "Pliki DLL (*.dll)|*.dll";

            if (openFileDialog.ShowDialog().Value)
            {
                referenceMenager.AddReference(ReferenceMenager.E_ReferenceType.OptimizationAlgorithm, openFileDialog.FileName);
                FillListBoxes();
            }
        }
        private void ButtonAddFunction_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Wybierz plik funkcji testowej";
            openFileDialog.Filter = "Pliki DLL (*.dll)|*.dll";

            if (openFileDialog.ShowDialog().Value)
            {
                referenceMenager.AddReference(ReferenceMenager.E_ReferenceType.Interface, openFileDialog.FileName);
                FillListBoxes();
            }
        }
        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            //if (!backgroundThread.IsAlive)
            //{
            //    backgroundThread = new Thread(referenceMenager.ContinueProject);
            //    backgroundThread.Start();
            //}
            //else 
            //{
            //    MessageBox.Show("Invalid data!");
            //}
        }
        private void ButtonRunProject_Click(object sender, RoutedEventArgs e)
        {
            if (CanRun())
            {
                List<string> chosenAlgorithms = ListBoxAlgorithms.SelectedItems.Cast<string>().ToList();
                List<string> chosenTestFunctions = ListBoxTestFunctions.SelectedItems.Cast<string>().ToList();
                int startIteration = int.Parse(TextBoxStartIteration.Text);
                int jumpIteration = int.Parse(TextBoxJumpIteration.Text);
                int amountOfIteration = int.Parse(TextBoxAmountOfIteration.Text);
                int startDimention = int.Parse(TextBoxStartDimention.Text);
                int jumpDimention = int.Parse(TextBoxJumpDimention.Text);
                int amountOfDimention = int.Parse(TextBoxAmountOfDimention.Text);
                int startPopulation = int.Parse(TextBoxStartPopulation.Text);
                int jumpPopulation = int.Parse(TextBoxJumpPopulation.Text);
                int amountOfPopulation = int.Parse(TextBoxAmountOfPopulation.Text);
                ProgressBar.Visibility = Visibility.Visible;
                backgroundTask = Task.Run(() =>
                {
                    referenceMenager.SetValues(chosenAlgorithms, chosenTestFunctions, startIteration, jumpIteration, amountOfIteration, startDimention, jumpDimention, amountOfDimention, startPopulation, jumpPopulation, amountOfPopulation);
                    referenceMenager.RunProject(false);
                    backgroundTask = null;
                });
            }
            else
            {
                MessageBox.Show("Invalid data!");
            }
        }
        void FillListBoxes()
        {
            ListBoxAlgorithms.Items.Clear();
            ListBoxTestFunctions.Items.Clear();
            referenceMenager.GetReferenceNames();
            foreach (string optimizationAlgorithm in referenceMenager.optimizationAlgorithms)
            {
                ListBoxAlgorithms.Items.Add(optimizationAlgorithm);
            }
            foreach (string testFunction in referenceMenager.testFunctions)
            {
                ListBoxTestFunctions.Items.Add(testFunction);
            }
        }
        bool CanRun()
        {
            return int.Parse(TextBoxAmountOfDimention.Text) > 0 &&
                   int.Parse(TextBoxAmountOfIteration.Text) > 0 &&
                   int.Parse(TextBoxAmountOfPopulation.Text) > 0 &&
                   int.Parse(TextBoxJumpDimention.Text) > 0 &&
                   int.Parse(TextBoxJumpIteration.Text) > 0 &&
                   int.Parse(TextBoxJumpPopulation.Text) > 0 &&
                   int.Parse(TextBoxStartDimention.Text) > 1 &&
                   int.Parse(TextBoxStartIteration.Text) > 0 &&
                   int.Parse(TextBoxStartPopulation.Text) > 2 &&
                   ListBoxAlgorithms.SelectedItem != null &&
                   ListBoxTestFunctions.SelectedItem != null &&
                   backgroundTask == null;
        }
        public void UpdateProgressBar(int procentDone, int timeLeft)
        {
            
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = procentDone;
                if (lastprogresbarValue != procentDone||procentDone==0) 
                {
                    lastprogresbarValue = procentDone;
                    if (ProgressBar.Template.FindName("timeLeftText", ProgressBar) is TextBlock timeLeftText)
                    {
                        timeLeftText.Text = ConvertTime(timeLeft);
                    }
                }
                if (ProgressBar.Value == 100)
                {
                    ProgressBar.Value = 0;
                    ProgressBar.Visibility = Visibility.Hidden;
                }
            });
        }
        string ConvertTime(int timeLeft)
        {
            string finalTime = string.Empty;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            if (timeLeft >= 60)
            {
                if (timeLeft % 60 >= 60)
                {
                    hours = (int)MathF.Floor((timeLeft / 3600));
                    minutes = (int)MathF.Floor((timeLeft % 3600 / 60));
                    seconds = (int)MathF.Floor((timeLeft % 3600 % 60));
                    finalTime = $"{hours} hours {minutes} minutes {seconds} seconds";
                }
                else
                {
                    minutes = (int)MathF.Floor((timeLeft / 60));
                    seconds = timeLeft % 60;
                    finalTime = $"{minutes} minutes {seconds} seconds";
                }
            }
            else
            {
                seconds = timeLeft;
                finalTime = $"{seconds} seconds";
            }
            return finalTime;
        }
    }
}
