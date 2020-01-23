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
        //wczytywanie rekordów z tabeli
       private void LoadRecordButton_Click(object sender, RoutedEventArgs e)
        {
            LoadRecords();
        }
        //dodawanie rekordu do tabeli
        private void AddRecordButton_Click(object sender, RoutedEventArgs e)
        {
            //walidacja brakujących pól
            if (ValidateInputFields())
            {
                //walidacja formatu daty
                if (ValidateInputFormat())
                {
                        //dodanie rekordu
                        AddRecord addRecord = new AddRecord(con.Connect());
                        addRecord.AddRec(fieldsData);
                        LoadRecords();
                        //czyszczenie tekstu z pól po wykorzystaniu
                        ClearingBoxes();
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
        //odczyt danych wejściowych
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
                ClearingBoxes();
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

            //sortowanie po dacie malejąco
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(RecordsListView.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Descending));

            //wystwietlanie istniejacych osób
            NameBox.ItemsSource = recordsListing.namesList;
            //wyswietlanie istniejących ćwiczeń
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
                        ClearingBoxes();
                    }
                }
            }
            else
            {
                MessageBox.Show("Wybierz z listy rekord do poprawy!");
            }
        }

        //wstawienie aktualnych danych rekordu który chcemy zmodyfikować
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

        private void ClearingBoxes()
        {
            NameBox.Text = "";
            ExerciseBox.Text = "";
            Date.SelectedDate = DateTime.Today;
            SeriesBox.Text = "";
            RepetitionsBox.Text = "";
            WeightBox.Text = "";
        }
    }

}
