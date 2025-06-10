using System.Collections.Generic;
using Npgsql;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<MusicalInstrument> instruments = new();
        private string connString = "Host=localhost;Username=postgres;Password=257p~pGD;Database=musicdb";

        public MainWindow()
        {
            InitializeComponent();
            LoadInstrumentsFromDb();
        }

        private void LoadInstrumentsFromDb()
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT * FROM musical_instruments", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(1);
                var strings = reader.GetInt32(2);
                var type = reader.GetString(3);
                if (type == "guitar")
                    instruments.Add(new Guitar(name, strings, "акустическая", true));
                else if (type == "piano")
                    instruments.Add(new Piano(name, strings, 3, true));
                else
                    instruments.Add(new MusicalInstrument(name, strings));

                InstrumentList.Items.Add(name);
            }
        }

        private void AddInstrument_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddInstrumentWindow(connString);
            var result = dialog.ShowDialog();

            if (result == true && dialog.NewInstrument != null)
            {
                instruments.Add(dialog.NewInstrument);
                InstrumentList.Items.Add(dialog.NewInstrument.Name);
                OutputText.Text = "Инструмент добавлен.";
            }
        }

        private void DeleteInstrument_Click(object sender, RoutedEventArgs e)
        {
            var index = InstrumentList.SelectedIndex;
            if (index == -1) return;

            var instrument = instruments[index];
            instruments.RemoveAt(index);
            InstrumentList.Items.RemoveAt(index);

            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            using var cmd = new NpgsqlCommand("DELETE FROM musical_instruments WHERE name = @n", conn);
            cmd.Parameters.AddWithValue("n", instrument.Name);
            cmd.ExecuteNonQuery();

            OutputText.Text = "Инструмент удалён.";
        }

        private void ShowActions_Click(object sender, RoutedEventArgs e)
        {
            var index = InstrumentList.SelectedIndex;
            if (index == -1) return;

            var instrument = instruments[index];
            var viewWindow = new ViewInstrumentWindow(instrument, connString);
            var result = viewWindow.ShowDialog();

            if (result == true)
            {
                // Обновить отображение в списке
                InstrumentList.Items[index] = instrument.Name;
                OutputText.Text = "Информация об инструменте обновлена.";
            }
        }
    }

    public class MusicalInstrument
    {
        public string Name { get; set; }
        public int StringsCount { get; set; }

        public MusicalInstrument(string name, int stringsCount)
        {
            Name = name;
            StringsCount = stringsCount;
        }

        public virtual string Play() => $"{Name} играет!";
        public virtual string Tune() => $"{Name} настраивается.";
        public virtual string Describe() => $"Это {Name} с {StringsCount} струнами/клавишами.";
        public virtual string SpecialFeature() => $"У этого инструмента уникальное звучание.";
    }

    public class Guitar : MusicalInstrument
    {
        public string GuitarType { get; set; }
        public bool HasPickup { get; set; }

        public Guitar(string name, int stringsCount, string guitarType, bool hasPickup)
            : base(name, stringsCount)
        {
            GuitarType = guitarType;
            HasPickup = hasPickup;
        }

        public string Strum() => $"{Name} перебирает струны.";
        public string ChangeStrings() => $"У {Name} меняются струны.";
        public override string Describe()
        {
            return $"Это {GuitarType} гитара {Name} с {StringsCount} струнами. " +
                   (HasPickup ? "У неё есть звукосниматель." : "Без звукоснимателя.");
        }
        public override string SpecialFeature() => "Гитара может играть как аккорды, так и мелодии.";
    }

    public class Piano : MusicalInstrument
    {
        public int PedalCount { get; set; }
        public bool IsGrand { get; set; }

        public Piano(string name, int stringsCount, int pedalCount, bool isGrand)
            : base(name, stringsCount)
        {
            PedalCount = pedalCount;
            IsGrand = isGrand;
        }

        public string PressPedal() => $"Нажата педаль у {Name}.";
        public string CleanKeys() => $"Клавиши {Name} очищаются.";
        public override string Describe()
        {
            return $"Это {(IsGrand ? "рояль" : "пианино")} {Name} с {StringsCount} клавишами и {PedalCount} педалями.";
        }
        public override string SpecialFeature() => "Пианино может играть и мелодию, и аккомпанемент одновременно.";
    }
}
