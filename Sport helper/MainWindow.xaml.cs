using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AirtableApiClient;
using AppLogic;
using Sport_logic;

namespace Sport_helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static RecordsListing recordsListing;
        static List<object> fieldsData = new List<object>();
        static Connection con = new Connection();
        string[] inputFields = { "Dane osobowe", "Ćwiczenie", "Data", "Serie", "Powtórzenia", "Ciężar" };
        public List<string> Ids;
        public MainWindow()
        {
            InitializeComponent();
            //pobranie aktualnych rekodów z tabeli
            recordsListing = new RecordsListing(con.Connect());
           //MainAsync().Wait();
            LoadRecords();
        }

       private void LoadRecordButton_Click(object sender, RoutedEventArgs e)
        {
            LoadRecords();
        }
        //add record to table
        private void AddRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInputFields())
            {
                if (ValidateInputFormat())
                {
                        AddRecord addRecord = new AddRecord(con.Connect());
                        addRecord.AddRec(fieldsData);
                        LoadRecords();
                }
            }

        }

        private bool ValidateInputFields()
        {
            fieldsData = GetherInputData();
            int count = 0;
            foreach (string data in fieldsData)
            {
                if (data == "")
                {
                    MessageBox.Show("Uzupełnij " + inputFields[count] + "!!");
                    return false;
                }
                count++;
            }

            return true;
        }
        //read input from fields
        private List<object> GetherInputData()
        {
            fieldsData = new List<object>();

            fieldsData.Add(NameBox.Text);
            fieldsData.Add(ExerciseBox.Text);
            fieldsData.Add(Date.SelectedDate.ToString());
            fieldsData.Add(SeriesBox.Text);
            fieldsData.Add(RepetitionsBox.Text);
            fieldsData.Add(WeightBox.Text);

            return fieldsData;
        }
        //method for checking necessary formats
        private bool ValidateInputFormat()
        {
            //prepare Data as an object
            var dataFormat = fieldsData[2].ToString().Substring(0, 10);
            fieldsData[2] = DateTime.Parse(dataFormat);
            //check if numbers fields are really numbers
            for (int i = 3; i < 6; i++)
            {
                bool check = int.TryParse(fieldsData[i].ToString(), out int receivedNumber);

                if (!check)
                {
                    MessageBox.Show("Popraw " + inputFields[i] + "!!");
                    return false;
                }
                else
                {
                    fieldsData[i] = receivedNumber;
                }

            }
            return true;
        }

        
        private void DeleteRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (RecordsListView.SelectedIndex != -1)
            {
                //DataForListView record = RecordsListView.SelectedItem;
                DeleteRecord deleteRecord = new DeleteRecord(con.Connect());
                deleteRecord.Delete(FindId());
                LoadRecords();
            }
            else
            {
                MessageBox.Show("Wybierz z listy rekord do usunięcia!");
            }
        }

        private void LoadRecords()
        {
            //przygotowanie rekordów do ListView
            recordsListing.Listing();
            recordsListing.GetDataForListView();
            //czyszczenie aktualnie pokazywanych rekordów
            RecordsListView.ClearValue(ItemsControl.ItemsSourceProperty);
            RecordsListView.Items.Clear();
            //dodanie nowych rekordów do widoku
            RecordsListView.ItemsSource = recordsListing.items;

            //sortowanie po dacie
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(RecordsListView.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Ascending));

            NameBox.ItemsSource = recordsListing.namesList;
            ExerciseBox.ItemsSource = recordsListing.exerciseList;
        }

        private string FindId()
        {
            string id = "";
            DataForListView record = RecordsListView.SelectedValue as DataForListView;

            var selectedId = record.Id;
            object searchedId;
            //string airtableRecordId;
            foreach (AirtableRecord checkedRecord in recordsListing.records)
            {
                
                checkedRecord.Fields.TryGetValue("Id", out searchedId);
                if(selectedId == searchedId.ToString())
                {
                    id = checkedRecord.Id;
                }
            }

            return id;
        }

        private void EditRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (RecordsListView.SelectedIndex != -1)
            {
                if (ValidateInputFields())
                {
                    if (ValidateInputFormat())
                    {
                        EditRecord editRecord = new EditRecord(con.Connect());
                        editRecord.EditRec(fieldsData, FindId());
                        RecordsListView.SelectedIndex = -1;
                        LoadRecords();
                    }
                }
            }
            else
            {
                MessageBox.Show("Wybierz z listy rekord do poprawy!");
            }
        }

        private void RecordsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataForListView record = RecordsListView.SelectedValue as DataForListView;
            if (RecordsListView.SelectedIndex != -1)
            {
                NameBox.Text = record.Name;
                ExerciseBox.Text = record.Exercise;
                Date.SelectedDate = DateTime.Parse(record.Date);
                SeriesBox.Text = record.Series;
                RepetitionsBox.Text = record.Repetitions;
                WeightBox.Text = record.Weight;
            }
        }
    }

}
