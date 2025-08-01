using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Proyecto_Final
{
    public partial class MainWindow : Window
    {
        private string connectionString = "mongodb+srv://omareliasbanderas:A6WfLHkUBnoizMDo@christian.go2j6da.mongodb.net/?retryWrites=true&w=majority&appName=Christian";
        private DataTable dataTable;

        public MainWindow()
        {
            InitializeComponent();
            MainTabControl.SelectedIndex = 3;
            ActualizarColorBotones(3);
        }

        private void ActualizarColorBotones(int index)
        {
            var normalColor = Brushes.White;
            var seleccionadoColor = new SolidColorBrush(Color.FromRgb(234, 154, 37));

            BtnDescubrir.Foreground = normalColor;
            BtnRecientes.Foreground = normalColor;
            BtnDestacado.Foreground = normalColor;
            BtnBuscar.Foreground = normalColor;

            switch (index)
            {
                case 0: BtnDescubrir.Foreground = seleccionadoColor; break;
                case 1: BtnRecientes.Foreground = seleccionadoColor; break;
                case 2: BtnDestacado.Foreground = seleccionadoColor; break;
                case 3: BtnBuscar.Foreground = seleccionadoColor; break;
            }
        }

        private void BtnDescubrir_Click(object sender, RoutedEventArgs e) => CambiarTab(0);
        private void BtnRecientes_Click(object sender, RoutedEventArgs e) => CambiarTab(1);
        private void BtnDestacado_Click(object sender, RoutedEventArgs e) => CambiarTab(2);
        private void BtnBuscar_Click(object sender, RoutedEventArgs e) { CambiarTab(3); CargarNoticias(); }

        private void CambiarTab(int index)
        {
            MainTabControl.SelectedIndex = index;
            ActualizarColorBotones(index);
        }

        private void BtnBuscarEjecutar_Click(object sender, RoutedEventArgs e)
        {
            string textoBusqueda = TxtBuscar.Text.Trim();
            if (dataTable != null)
                dataTable.DefaultView.RowFilter = $"titulo LIKE '%{textoBusqueda.Replace("'", "''")}%'";
        }

        private void CargarNoticias()
        {
            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("noticiero_db");
                var collection = database.GetCollection<BsonDocument>("noticias");
                var noticias = collection.Find(new BsonDocument()).ToList();

                dataTable = new DataTable();
                dataTable.Columns.Add("titulo");
                dataTable.Columns.Add("descripcion");
                dataTable.Columns.Add("fecha_publicacion");
                dataTable.Columns.Add("categoria");
                dataTable.Columns.Add("autor");
                dataTable.Columns.Add("es_destacado", typeof(bool));

                foreach (var noticia in noticias)
                {
                    string titulo = noticia.GetValue("titulo", "").AsString;
                    string descripcion = noticia.GetValue("descripcion", noticia.GetValue("contenido", "")).AsString;
                    string categoria = noticia.GetValue("categoria", "").AsString;
                    string autor = noticia.GetValue("autor", "").AsString;
                    string fecha_publicacion = noticia.Contains("fecha_publicacion") && noticia["fecha_publicacion"].IsValidDateTime
                        ? noticia["fecha_publicacion"].ToLocalTime().ToShortDateString()
                        : "No registrada";
                    bool destacado = noticia.Contains("es_destacado") && noticia["es_destacado"].AsBoolean;
                    dataTable.Rows.Add(titulo, descripcion, fecha_publicacion, categoria, autor, destacado);


                }

                DgNoticias.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar noticias: " + ex.Message); }
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("noticiero_db");
                var collection = database.GetCollection<BsonDocument>("noticias");

                var nuevaNoticia = new BsonDocument
                {
                    { "titulo", TxtTitulo.Text },
                    { "descripcion", TxtDescripcion.Text },
                    { "categoria", TxtCategoria.Text },
                    { "autor", TxtAutor.Text },
                    { "fecha_publicacion", DpFecha.SelectedDate ?? DateTime.Now }
                };

                collection.InsertOne(nuevaNoticia);
                MessageBox.Show("Noticia agregada.");
                CargarNoticias();
            }
            catch (Exception ex) { MessageBox.Show("Error al agregar: " + ex.Message); }
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (DgNoticias.SelectedItem == null) { MessageBox.Show("Selecciona una noticia."); return; }

            try
            {
                var fila = (DataRowView)DgNoticias.SelectedItem;
                string tituloOriginal = fila["titulo"].ToString();

                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("noticiero_db");
                var collection = database.GetCollection<BsonDocument>("noticias");

                var filtro = Builders<BsonDocument>.Filter.Eq("titulo", tituloOriginal);
                var actualizacion = Builders<BsonDocument>.Update
                    .Set("titulo", TxtTitulo.Text)
                    .Set("descripcion", TxtDescripcion.Text)
                    .Set("categoria", TxtCategoria.Text)
                    .Set("autor", TxtAutor.Text)
                    .Set("fecha_publicacion", DpFecha.SelectedDate ?? DateTime.Now);

                collection.UpdateOne(filtro, actualizacion);
                MessageBox.Show("Noticia actualizada.");
                CargarNoticias();
            }
            catch (Exception ex) { MessageBox.Show("Error al actualizar: " + ex.Message); }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (DgNoticias.SelectedItem == null) { MessageBox.Show("Selecciona una noticia."); return; }

            try
            {
                var fila = (DataRowView)DgNoticias.SelectedItem;
                string titulo = fila["titulo"].ToString();

                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("noticiero_db");
                var collection = database.GetCollection<BsonDocument>("noticias");

                var filtro = Builders<BsonDocument>.Filter.Eq("titulo", titulo);
                collection.DeleteOne(filtro);

                MessageBox.Show("Noticia eliminada.");
                CargarNoticias();
            }
            catch (Exception ex) { MessageBox.Show("Error al eliminar: " + ex.Message); }
        }

        private void DgNoticias_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DgNoticias.SelectedItem != null)
            {
                var fila = (DataRowView)DgNoticias.SelectedItem;
                TxtTitulo.Text = fila["titulo"].ToString();
                TxtDescripcion.Text = fila["descripcion"].ToString();
                TxtCategoria.Text = fila["categoria"].ToString();
                TxtAutor.Text = fila["autor"].ToString();

                if (DateTime.TryParse(fila["fecha_publicacion"].ToString(), out DateTime fecha))
                    DpFecha.SelectedDate = fecha;
                else
                    DpFecha.SelectedDate = null;
            }
        }

        private void BtnFavorito_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            string titulo = (string)boton.Tag;

            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("noticiero_db");
                var collection = database.GetCollection<BsonDocument>("noticias");

                var filtro = Builders<BsonDocument>.Filter.Eq("titulo", titulo);
                var update = Builders<BsonDocument>.Update.Set("es_destacado", true);
                collection.UpdateOne(filtro, update);

                MessageBox.Show("Agregado a Destacados.");
                CambiarTab(2);
                CargarNoticiasDestacadas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar a destacados: " + ex.Message);
            }
        }

        private void BtnEliminarFavorito_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            string titulo = (string)boton.Tag;

            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("noticiero_db");
                var collection = database.GetCollection<BsonDocument>("noticias");

                var filtro = Builders<BsonDocument>.Filter.Eq("titulo", titulo);
                var update = Builders<BsonDocument>.Update.Set("es_destacado", false);
                collection.UpdateOne(filtro, update);

                MessageBox.Show("Eliminado de destacados.");
                CargarNoticiasDestacadas(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar de destacados: " + ex.Message);
            }
        }

        private void CargarNoticiasDestacadas()
        {
            try
            {
                PanelDestacados.Children.Clear(); 

                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("noticiero_db");
                var collection = database.GetCollection<BsonDocument>("noticias");

                var filtro = Builders<BsonDocument>.Filter.Eq("es_destacado", true);
                var noticias = collection.Find(filtro).ToList();

                foreach (var noticia in noticias)
                {
                    string titulo = noticia.GetValue("titulo", "").AsString;
                    string imagen = "https://via.placeholder.com/400x140"; 
                    Border border = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(250, 212, 91)),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(10),
                        Margin = new Thickness(10),
                        Width = 300
                    };

                    Grid grid = new Grid();

                   
                    Image img = new Image
                    {
                        Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imagen)),
                        Height = 140,
                        Stretch = Stretch.Fill
                    };
                    grid.Children.Add(img);

                    Button btnEliminar = new Button
                    {
                        Content = "★",
                        Width = 30,
                        Height = 30,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        Background = new SolidColorBrush(Color.FromRgb(234, 154, 37)),
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(5),
                        Tag = titulo
                    };
                    btnEliminar.Click += BtnEliminarFavorito_Click;
                    grid.Children.Add(btnEliminar);

                    // Texto
                    StackPanel textoPanel = new StackPanel
                    {
                        Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0)),
                        VerticalAlignment = VerticalAlignment.Bottom
                    };

                    TextBlock text = new TextBlock
                    {
                        Text = titulo,
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        Padding = new Thickness(5),
                        TextWrapping = TextWrapping.Wrap
                    };
                    textoPanel.Children.Add(text);
                    grid.Children.Add(textoPanel);

                    border.Child = grid;
                    PanelDestacados.Children.Add(border);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar destacados: " + ex.Message);
            }
        }




    }
}
