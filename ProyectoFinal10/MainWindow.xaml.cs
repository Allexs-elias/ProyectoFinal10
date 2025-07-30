using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace Proyecto_Final
{
    public partial class MainWindow : Window
    {
        private MongoClient _client;
        private IMongoDatabase _database;

        public MainWindow()
        {
            InitializeComponent();
            MainTabControl.SelectedIndex = 3; // Por ejemplo inicia en Buscar
            ActualizarColorBotones(3);

            ConectarMongoDB();
        }

        private async void ConectarMongoDB()
        {
            try
            {
                string connectionString = "mongodb+srv://<christianalbrand>:A6WfLHkUBnoizMDo@christian.go2j6da.mongodb.net/?retryWrites=true&w=majority&appName=Christian";

                _client = new MongoClient(connectionString);
                _database = _client.GetDatabase("noticiero_db");

                var colecciones = await _database.ListCollectionNamesAsync();
                var listaColecciones = await colecciones.ToListAsync();

                MessageBox.Show($"Conectado a MongoDB Atlas. Colecciones: {string.Join(", ", listaColecciones)}");
            }
            catch (MongoAuthenticationException)
            {
                MessageBox.Show("Error: Usuario o contraseña incorrectos.");
            }
            catch (MongoConfigurationException ex)
            {
                MessageBox.Show($"Error de configuración: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al conectar con MongoDB: {ex.Message}");
            }
        }

        private void ActualizarColorBotones(int index)
        {
            Brush normalColor = Brushes.White;
            Brush seleccionadoColor = new SolidColorBrush(Color.FromRgb(234, 154, 37)); // #FFEA9A25

            BtnDescubrir.Foreground = normalColor;
            BtnRecientes.Foreground = normalColor;
            BtnDestacado.Foreground = normalColor;
            BtnBuscar.Foreground = normalColor;

            switch (index)
            {
                case 0:
                    BtnDescubrir.Foreground = seleccionadoColor;
                    break;
                case 1:
                    BtnRecientes.Foreground = seleccionadoColor;
                    break;
                case 2:
                    BtnDestacado.Foreground = seleccionadoColor;
                    break;
                case 3:
                    BtnBuscar.Foreground = seleccionadoColor;
                    break;
            }
        }

        private void BtnDescubrir_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 0;
            ActualizarColorBotones(0);
        }

        private void BtnRecientes_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
            ActualizarColorBotones(1);
        }

        private void BtnDestacado_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 2;
            ActualizarColorBotones(2);
        }

        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 3;
            ActualizarColorBotones(3);
        }

        private void BtnBuscarEjecutar_Click(object sender, RoutedEventArgs e)
        {
            string textoBusqueda = TxtBuscar.Text.Trim();
            MessageBox.Show($"Buscando: {textoBusqueda}", "Buscar", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
