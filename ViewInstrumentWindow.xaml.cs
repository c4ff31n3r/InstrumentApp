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
    /// Логика взаимодействия для ViewInstrumentWindow.xaml
    /// </summary>
    public partial class ViewInstrumentWindow : Window
    {
        private MusicalInstrument instrument;
        private readonly string connStr;

        public ViewInstrumentWindow(MusicalInstrument inst, string connStr)
        {
            InitializeComponent();
            instrument = inst;
            this.connStr = connStr;

            txtName.Text = instrument.Name;
            txtStrings.Text = instrument.StringsCount.ToString();

            if (instrument is Guitar g)
            {
                AddExtraButton("Перебрать струны", (s, e) =>
                    MessageBox.Show(g.Strum()));
                AddExtraButton("Сменить струны", (s, e) =>
                    MessageBox.Show(g.ChangeStrings()));
            }
            else if (instrument is Piano p)
            {
                AddExtraButton("Нажать педаль", (s, e) =>
                    MessageBox.Show(p.PressPedal()));
                AddExtraButton("Очистить клавиши", (s, e) =>
                    MessageBox.Show(p.CleanKeys()));
            }
        }

        private void AddExtraButton(string title, RoutedEventHandler handler)
        {
            var btn = new Button
            {
                Content = title,
                Margin = new Thickness(0, 5, 0, 0)
            };
            btn.Click += handler;
            extraButtons.Children.Add(btn);
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(instrument.Play());
        }

        private void Tune_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(instrument.Tune());
        }

        private void Describe_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(instrument.Describe());
        }

        private void Feature_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(instrument.SpecialFeature());
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var newName = txtName.Text;
            if (!int.TryParse(txtStrings.Text, out int newCount))
            {
                MessageBox.Show("Некорректное число струн/клавиш.");
                return;
            }

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();
            var cmd = new NpgsqlCommand("UPDATE musical_instruments SET name = @n, strings_count = @s WHERE name = @old", conn);
            cmd.Parameters.AddWithValue("n", newName);
            cmd.Parameters.AddWithValue("s", newCount);
            cmd.Parameters.AddWithValue("old", instrument.Name);
            cmd.ExecuteNonQuery();

            instrument.Name = newName;
            instrument.StringsCount = newCount;

            MessageBox.Show("Изменения сохранены.");
        }
    }
}
