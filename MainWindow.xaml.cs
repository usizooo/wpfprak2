using Newtonsoft.Json;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Notes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string path = "notes.json";
        private Dictionary<DateTime, List<Note>> dataBase;
        public delegate void MyEvent();
        private MyEvent? OnDataBaseChanged;

        public MainWindow()
        {
            InitializeComponent();

            OnDataBaseChanged += UpdateListBoxAndClearTextBox;
            OnDataBaseChanged += Serialization;

            DatePicker.SelectedDate = DateTime.Now;
            dataBase = Deserialization();
            OnDataBaseChanged?.Invoke();
        }
        
        private void Serialization()
        {
            using (StreamWriter streamWriter = new StreamWriter(path))
            {
                streamWriter.Write(JsonConvert.SerializeObject(dataBase));
            }
        }

        private Dictionary<DateTime, List<Note>> Deserialization()
        {
            using (StreamReader streamReader = new StreamReader(path))
            {
                string jsonData = streamReader.ReadToEnd();
                if (jsonData == string.Empty)
                {
                    throw new NullReferenceException("Файл пуст.");
                }
                return JsonConvert.DeserializeObject<Dictionary<DateTime, List<Note>>>(jsonData)
                    ?? throw new NullReferenceException("Данные файла повреждены повреждены.");
            }
        }

        private void UpdateListBoxAndClearTextBox()
        {
            Notes.Items.Clear();
            if (DatePicker.SelectedDate != null && dataBase != null && dataBase.ContainsKey((DateTime)DatePicker.SelectedDate))
            {                
                foreach (var note in dataBase[(DateTime)DatePicker.SelectedDate])
                {
                    Notes.Items.Add(note.Title);
                }
            }
            Title.Text = string.Empty; 
            Description.Text = string.Empty;
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DatePicker.SelectedDate != null && Notes.SelectedItem != null)
            {
                for (int i = 0; i < dataBase[(DateTime)DatePicker.SelectedDate].Count; i++)
                {
                    if (dataBase[(DateTime)DatePicker.SelectedDate][i].Title == Notes.SelectedItem.ToString())
                    {
                        dataBase[(DateTime)DatePicker.SelectedDate].RemoveAt(i);
                        break;
                    }
                }
                if (dataBase[(DateTime)DatePicker.SelectedDate].Count == 0)
                {
                    dataBase.Remove((DateTime)DatePicker.SelectedDate);
                }

                OnDataBaseChanged?.Invoke();
            }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (isEmptyTitle() || isDuplicateTitle())
            {
                return;
            }

            var note = new Note(Title.Text, Description.Text);
            if(DatePicker.SelectedDate != null)
            {
                if (dataBase.ContainsKey((DateTime)DatePicker.SelectedDate))
                    dataBase[(DateTime)DatePicker.SelectedDate].Add(note);
                else
                    dataBase.Add((DateTime)DatePicker.SelectedDate, new List<Note>() { note });
                OnDataBaseChanged?.Invoke();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (isEmptyTitle() || isNotSelectedNote())
            {
                return;
            }

            if (DatePicker.SelectedDate != null)
            {
                var note = dataBase[(DateTime)DatePicker.SelectedDate]
                    .Where(x => x.Title == Notes.SelectedItem.ToString())
                    .First();

                if (note.Title != Title.Text && isDuplicateTitle())
                {
                    return;
                }

                note.Title = Title.Text;
                note.Description = Description.Text;
                OnDataBaseChanged?.Invoke();
            }
        }

        private void Notes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePicker.SelectedDate != null && Notes.SelectedItem != null)
            {
                Title.Text = Notes.SelectedItem.ToString();
                foreach (var note in dataBase[(DateTime)DatePicker.SelectedDate])
                {
                    if (note.Title == Title.Text)
                    {
                        Description.Text = note.Description;
                    }
                }
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateListBoxAndClearTextBox();
        }

        private bool isEmptyTitle()
        {
            if (Title.Text == string.Empty)
            {
                MessageBox.Show(
                    "Нельзя создать заметку с пустым заголовком!",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return true;
            }
            return false;
        }

        private bool isDuplicateTitle()
        {
            if (DatePicker.SelectedDate != null && dataBase.ContainsKey((DateTime)DatePicker.SelectedDate)
                && dataBase[(DateTime)DatePicker.SelectedDate].Any(x => x.Title == Title.Text))
            {
                MessageBox.Show(
                    "По данной дате заметка с таким именем уже существует!",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return true;
            }
            return false;
        }

        private bool isNotSelectedNote()
        {
            if (Notes.SelectedItem == null)
            {
                MessageBox.Show(
                    "Не выбрана заметку, которые нужно сохранить",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return true;
            }
            return false;
        }
    }
}