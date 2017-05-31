using System;
using System.Collections.Generic;
using System.Windows;
using Classification.App.Helpers;
using Classification.App.Models;
using Classification.App.Utils;

namespace Classification.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Database _database;
        private Classifier _classifier;
        private Selector _selector;
        private Random _random;
        private string _filename;

        public MainWindow()
        {
            InitializeComponent();

            _database = new Database();
            _random = new Random();
        }

        private void ButtonLoadFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                _filename = dialog.FileName;
                _database.Load(_filename);
                _selector = new Selector(_database);
            }
        }

        private void ButtonSelectFisher_Click(object sender, RoutedEventArgs e)
        {
            var result = _selector.Fisher(int.Parse(TextBoxFeaturesToSelect.Text));

            ListBoxResultFisher.ItemsSource = result;
        }

        private void ButtonSelectSFS_Click(object sender, RoutedEventArgs e)
        {
            var result = _selector.FisherSFS(int.Parse(TextBoxFeaturesToSelect.Text));

            ListBoxResultSFS.ItemsSource = result;
        }

        private void ButtonClassify_Click(object sender, RoutedEventArgs e)
        {
            List<ObjectModel> objectsToRemove = new List<ObjectModel>();

            var dbTrain = _database;
            var dbTest = new Database();

            //foreach (var obj in _database.Objects)
            //{
            //    dbTrain.AddObject(obj);
            //}

            int objectsQuant = dbTrain.Objects.Count * int.Parse(TextBoxTestObjects.Text) / 100;

            for (int i = 0; i < objectsQuant; i++)
            {
                int r = _random.Next(objectsQuant);
                dbTest.AddObject(_database.Objects[r]);
                objectsToRemove.Add(_database.Objects[r]);
            }

            foreach (var obj in objectsToRemove)
            {
                dbTrain.RemoveObject(obj);
            }

            _classifier = new Classifier(dbTrain, dbTest);

            _classifier.ClassifyNearestNeighbour();
            _classifier.ClassifyKNearestNeighbour(uint.Parse(TextBoxNeighbours.Text));
            _classifier.ClassifyNearesMean();
            _classifier.ClassifyKNearestMean(int.Parse(TextBoxMeans.Text));

            DataGridClassification.ItemsSource = _classifier.Results;
        }
    }
}
