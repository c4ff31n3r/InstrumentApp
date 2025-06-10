using Npgsql;
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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для AddInstrumentWindow.xaml
    /// </summary>
    public partial class AddInstrumentWindow : Window
    {
        private readonly string connectionString;
        public AddInstrumentWindow(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;

            cbType.SelectionChanged += (s, e) =>
            {
                var selected = (cbType.SelectedItem as ComboBoxItem)?.Content.ToString();
                guitarOptions.Visibility = selected == "guitar" ? Visibility.Visible : Visibility.Collapsed;
                pianoOptions.Visibility = selected == "piano" ? Visibility.Visible : Visibility.Collapsed;
            };
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var name = txtName.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите имя инструмента.");
                return;
            }

            if (!int.TryParse(txtStrings.Text, out var strings))
            {
                MessageBox.Show("Введите корректное количество струн/клавиш.");
                return;
            }

            var type = (cbType.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (type == "other")
                type = "base";

            if (string.IsNullOrWhiteSpace(type))
            {
                MessageBox.Show("Выберите тип инструмента.");
                return;
            }

            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand("INSERT INTO musical_instruments (name, strings_count, type) VALUES (@n, @s, @t) RETURNING instrument_id", conn);
            cmd.Parameters.AddWithValue("n", name);
            cmd.Parameters.AddWithValue("s", strings);
            cmd.Parameters.AddWithValue("t", type);
            var id = (int)cmd.ExecuteScalar();

            if (type == "guitar")
            {
                var gtype = (cbGuitarType.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (string.IsNullOrWhiteSpace(gtype))
                {
                    MessageBox.Show("Выберите тип гитары.");
                    return;
                }

                var pickup = chkPickup.IsChecked == true;

                var cmd2 = new NpgsqlCommand("INSERT INTO guitars (instrument_id, guitar_type, has_pickup) VALUES (@i, @t, @p)", conn);
                cmd2.Parameters.AddWithValue("i", id);
                cmd2.Parameters.AddWithValue("t", gtype);
                cmd2.Parameters.AddWithValue("p", pickup);
                cmd2.ExecuteNonQuery();

                NewInstrument = new Guitar(name, strings, gtype, pickup);
            }
            else if (type == "piano")
            {
                if (!int.TryParse(txtPedals.Text, out var pedals))
                {
                    MessageBox.Show("Введите количество педалей.");
                    return;
                }

                var grand = chkGrand.IsChecked == true;

                var cmd2 = new NpgsqlCommand("INSERT INTO pianos (instrument_id, pedal_count, is_grand) VALUES (@i, @p, @g)", conn);
                cmd2.Parameters.AddWithValue("i", id);
                cmd2.Parameters.AddWithValue("p", pedals);
                cmd2.Parameters.AddWithValue("g", grand);
                cmd2.ExecuteNonQuery();

                NewInstrument = new Piano(name, strings, pedals, grand);
            }
            else
            {
                // Просто базовый инструмент
                NewInstrument = new MusicalInstrument(name, strings);
            }

            DialogResult = true;
            Close();
        }

        public MusicalInstrument NewInstrument { get; private set; }
    }
}
