using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
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
using MongoDB.Bson;
using MongoDB.Driver;
using static WpfApp1.MainWindow;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int studentCounter = 1;
        private Student selectedStudent;
        public ObservableCollection<Student> Students { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            IMongoDatabase database = client.GetDatabase("SinhVien");
            IMongoCollection<Student> collection = database.GetCollection<Student>("students");

            // Load data into ObservableCollection
            Students = new ObservableCollection<Student>(collection.Find(new BsonDocument()).ToList());

            // Set DataGrid's ItemsSource to the ObservableCollection
            dataGrid.ItemsSource = Students;
        }

        public class Student
        {
            private static int idCounter = 1;
            public string id { get; set; }
            public string fullName { get; set; }
            public string age { get; set; }
            public string point { get; set; }
            public Student()
            {
                id = idCounter.ToString("D3");
                idCounter++;
            }
        }


        private void Button_Add(object sender, RoutedEventArgs e)
        {
            string fullName = fullNameTextBox.Text;
            string age = ageTextBox.Text;
            string point = pointTextBox.Text;

            Student newStudent = new Student
            {
                fullName = fullName,
                age = age,
                point = point
            };

            Students.Add(newStudent);

            MongoClient client = new MongoClient("mongodb://localhost:27017");
            IMongoDatabase database = client.GetDatabase("SinhVien");
            IMongoCollection<Student> collection = database.GetCollection<Student>("students");
            collection.InsertOne(newStudent);

            dataGrid.Items.Refresh();
        }

        private void Button_Edit(object sender, RoutedEventArgs e)
        {
            if (selectedStudent != null)
            {
                // Update the selected student's information
                selectedStudent.fullName = fullNameTextBox.Text;
                selectedStudent.age = ageTextBox.Text;
                selectedStudent.point = pointTextBox.Text;

                // Update the corresponding MongoDB document
                MongoClient client = new MongoClient("mongodb://localhost:27017");
                IMongoDatabase database = client.GetDatabase("SinhVien");
                IMongoCollection<Student> collection = database.GetCollection<Student>("students");

                var filter = Builders<Student>.Filter.Eq("_id", selectedStudent.id);
                var update = Builders<Student>.Update
                    .Set("fullName", selectedStudent.fullName)
                    .Set("age", selectedStudent.age)
                    .Set("point", selectedStudent.point);

                collection.UpdateOne(filter, update);

                // Refresh the DataGrid
                dataGrid.Items.Refresh();
            }
        }

        private void DeleteStudent(Student student)
        {
            Students.Remove(student);

            MongoClient client = new MongoClient("mongodb://localhost:27017");
            IMongoDatabase database = client.GetDatabase("SinhVien");
            IMongoCollection<Student> collection = database.GetCollection<Student>("students");

            var filter = Builders<Student>.Filter.Eq("_id", student.id);
            collection.DeleteOne(filter);

            dataGrid.Items.Refresh();
        }
        private void Button_Delete(object sender, RoutedEventArgs e)
        {
            if (selectedStudent != null)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this student?", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    DeleteStudent(selectedStudent);

                    // Clear TextBoxes after deletion
                    fullNameTextBox.Text = "";
                    ageTextBox.Text = "";
                    pointTextBox.Text = "";
                }
            }
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedStudent = (Student)dataGrid.SelectedItem;

            if (selectedStudent != null)
            {
                // Display selected student's information in TextBoxes
                fullNameTextBox.Text = selectedStudent.fullName;
                ageTextBox.Text = selectedStudent.age;
                pointTextBox.Text = selectedStudent.point;
            }
        }

        private void Button_Reset(object sender, RoutedEventArgs e)
        {
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            IMongoDatabase database = client.GetDatabase("SinhVien");
            IMongoCollection<Student> collection = database.GetCollection<Student>("students");
            dataGrid.Items.Refresh();
            fullNameTextBox.Text = "";
            ageTextBox.Text = "";
            pointTextBox.Text = "";
        }
    }
}
