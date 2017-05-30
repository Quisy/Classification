using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Classification.App.Utils;

namespace Classification.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var dbTrain = new Database();
            var dbTest = new Database();

            dbTrain.Load(@"C:\Users\Mariusz\Desktop\Maple_Oak.txt");

            for (int i = 0; i < dbTrain.Objects.Count; i = i+2)
            {
                dbTest.AddObject(dbTrain.Objects[i]);
                dbTrain.RemoveObject(dbTrain.Objects[i]);
            }

            var classifier = new Classifier(dbTrain, dbTest);
            classifier.ClassifyNearestNeighbour();
            classifier.ClassifyKNearestNeighbour(10);

        }
    }
}
