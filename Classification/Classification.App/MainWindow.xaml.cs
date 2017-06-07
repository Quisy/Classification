using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
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

        private Classifier ClassifyGiven()
        {
            List<ObjectModel> objectsToRemove = new List<ObjectModel>();

            var dbTrain = new Database(); ;
            var dbTest = new Database();

            foreach (var obj in _database.Objects)
            {
                dbTrain.AddObject(obj);
            }

            int objectsQuant = dbTrain.Objects.Count * int.Parse(TextBoxTestObjects.Text) / 100;
            dbTrain.FeaturesIDs = _database.FeaturesIDs;

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

            return _classifier;
        }


        private Classifier ClassifyCustom()
        {
            List<ObjectModel> objectsToRemove = new List<ObjectModel>();

            var dbTrain = new Database();
            var dbTest = new Database();

            foreach (var obj in _database.Objects)
            {
                dbTrain.AddObject(obj);
            }

            dbTrain.FeaturesIDs = _database.FeaturesIDs;

            dbTest.AddObject(new ObjectModel("A", new List<float> { 1, 1 }));

            _classifier = new Classifier(dbTrain, dbTest);

            _classifier.ClassifyNearestNeighbour();
            _classifier.ClassifyKNearestNeighbour(uint.Parse(TextBoxNeighbours.Text));
            _classifier.ClassifyNearesMean();
            _classifier.ClassifyKNearestMean(int.Parse(TextBoxMeans.Text));

            return _classifier;
        }

        private void ButtonClassify_Click(object sender, RoutedEventArgs e)
        {

            var classifier = ClassifyGiven();
            //var classifier = ClassifyCustom();
            DataGridClassification.ItemsSource = classifier.Results;
        }

        private void ButtonClassify2_Click(object sender, RoutedEventArgs e)
        {
            int k = int.Parse(TextBoxParts.Text);

            List<ObjectModel> objectsToRemove = new List<ObjectModel>();

            var dbTrain = new Database();
            var dbTest = new Database();

            foreach (var obj in _database.Objects)
            {
                dbTrain.AddObject(obj);
            }

            int objectsQuant = dbTrain.Objects.Count;

            int partQuantity = objectsQuant / k;

            IList<List<ObjectModel>> parts = new List<List<ObjectModel>>();

            var classAobjects = dbTrain.Objects.Where(o => o.ClassName.Equals(dbTrain.ClassNames[0])).ToList();
            var classBobjects = dbTrain.Objects.Where(o => o.ClassName.Equals(dbTrain.ClassNames[1])).ToList();

            for (int i = 0; i < k; i++)
            {
                var part = new List<ObjectModel>();
                var objAforPart = classAobjects.Skip(classAobjects.Count / k * i).Take(classAobjects.Count / k).ToList();
                var objBforPart = classBobjects.Skip(classBobjects.Count / k * i).Take(classBobjects.Count / k).ToList();

                foreach (var obj in objAforPart)
                {
                    part.Add(obj);
                }

                foreach (var obj in objBforPart)
                {
                    part.Add(obj);
                }

                parts.Add(part);
            }

            float tempResultNN = 0f, tempResultKNN = 0f, tempResultNM = 0f, tempResultKNM = 0f;

            foreach (var part in parts)
            {
                dbTest = new Database();
                dbTrain = new Database();
                dbTrain.FeaturesIDs = _database.FeaturesIDs;
                dbTrain.ClassNames = _database.ClassNames;

                foreach (var obj in part)
                {
                    dbTest.Objects.Add(obj);
                }

                var trainingParts = parts.Where(p => p != part).ToList();

                foreach (var trainingPart in trainingParts)
                {
                    foreach (var obj in trainingPart)
                    {
                        dbTrain.Objects.Add(obj);
                    }
                }

                _classifier = new Classifier(dbTrain, dbTest);

                _classifier.ClassifyNearestNeighbour();
                _classifier.ClassifyKNearestNeighbour(uint.Parse(TextBoxNeighbours.Text));
                _classifier.ClassifyNearesMean();
                _classifier.ClassifyKNearestMean(int.Parse(TextBoxMeans.Text));

                tempResultNN += _classifier.NearestNeighbourResult.Effectiveness;
                tempResultKNN += _classifier.KNearestNeighbourResult.Effectiveness;
                tempResultNM += _classifier.NearestMeanResult.Effectiveness;
                tempResultKNM += _classifier.KNearestMeanResult.Effectiveness;
            }

            float finalResultNN = tempResultNN / k;
            float finalResultKNN = tempResultKNN / k;
            float finalResultNM = tempResultNM / k;
            float finalResultKNM = tempResultKNM / k;

            List<MethodResult2> results = new List<MethodResult2>();

            results.Add(new MethodResult2 { MethodName = "NN", Effectiveness = finalResultNN });
            results.Add(new MethodResult2 { MethodName = "KNN", Effectiveness = finalResultKNN });
            results.Add(new MethodResult2 { MethodName = "NM", Effectiveness = finalResultNM });
            results.Add(new MethodResult2 { MethodName = "KNM", Effectiveness = finalResultKNM });

            DataGridClassification2.ItemsSource = results;
        }
    }
}
