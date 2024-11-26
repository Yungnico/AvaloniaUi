using Avalonia.Controls;
using System.Net.Http;
using System.IO;
using Avalonia.Interactivity;
using NAudio.Wave;
using System;

// SE TARDA ALREDEDOR DE 3 SEGUNDOS EN REPRODUCIR LA CANCION, SI LE DAN A PLAY Y NO REPRODUCE ALTIRO, ESPEREN UN SEGUNDO, ES PQ SE DEMORA EN DESCARGAR
// SE PODRIA DESCARGAR DE ANTEMANO HACIENDO EL GET ANTES, DE ELEGIR LA CANCION, tener el get listo? no se si funcionaria
// se podria agregar un json que tenga el url de las canciones
// y hacer una lista de canciones, o que la persona elija la cancion, por el nombre, buscarla en un json con estos datos
// y mostrarla y reproducirla
namespace musica
{
      
        public partial class MainWindow : Window
    {
        private WaveOutEvent? _waveOut;            // Controlador de audio 
        private Stream? _audioStream;             // Stream para manejar el audio descargado
        private StreamMediaFoundationReader? _audioReader; // Lector de stream para reproducción
        private bool _isPaused = false;           // Estado de pausa

        public MainWindow()
        {
            InitializeComponent();
            
            PlayButton.Click += Empezar;// Empieza la cancion
            PauseButton.Click += Pause;// Pausa la cancion
            StopButton.Click += Stop;// Detiene la cancion
        }

        // Maneja la reproducción
        private async void Empezar(object? sender, RoutedEventArgs e)
        {   //aqui el url de la cancion en mp3 creo que sirve en .wav tambien
            string urlCancion = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3";

            if (_waveOut == null || _audioReader == null)
            {        
                //se hace un get desde el url de la pagina, si no hay problemas en la descarga del archivo se procede para reproducirla
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(urlCancion);
                if (!response.IsSuccessStatusCode)
                {
               
                    Console.WriteLine("Error al descargar el archivo de audio.");
                    return;
                }

                // Crea un stream desde la respuesta
                _audioStream = await response.Content.ReadAsStreamAsync();
                _audioReader = new StreamMediaFoundationReader(_audioStream);
                // Se crea un StreamMediaFoundationReader para leer el archivo MP3.

                _waveOut = new WaveOutEvent();
                _waveOut.PlaybackStopped += CancionTermino;
                _waveOut.Init(_audioReader);
                // Se inicializa _waveOut con los datos del audio para comenzar la reproducción
                // el _waveOut es el que tiene todos los metodos para reproducir, detener, inciar, y pausar la cacion

                _waveOut.Play();
                _isPaused = false;
                //se pone play a la cancion y se le entrega el booleano de que no esta pausado
            }
            // si esta pausado solo se reanuda la cancion
            else if (_isPaused)
            {
                // Reanuda la reproducción
                _waveOut.Play();
                _isPaused = false;
            }
        }

        // Maneja la pausa
        private void Pause(object? sender, RoutedEventArgs e)
        {
            // si el waveOut tiene alguna cancion y no esta pausado entonces lo pausa, en caso contrario no hace nada
            if (_waveOut != null && !_isPaused)
            {
                _waveOut.Pause();
                _isPaused = true;
            }
        }

        // Si dan al boton stop, Llama a la funcion parar cancion
        private void Stop(object? sender, RoutedEventArgs e)
        {
            PararCancion();
        }

        private void PararCancion()
        {
            //si el waveout no esta vacio, osea que la cancion se detuvo en mitad y se dio al boton de detener se borra la cancion
            if (_waveOut != null)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
                _waveOut = null;
            }

            if (_audioReader != null)
            {
                _audioReader.Dispose();
                _audioReader = null;
            }

            if (_audioStream != null)
            {
                _audioStream.Dispose();
                _audioStream = null;
            }

            _isPaused = false;
        }

        // Maneja el final de la canción,  cuando la cancion termine llama a la funcion PararCancion para que se libere el waveout
        private void CancionTermino(object? sender, StoppedEventArgs e)
        {
            PararCancion();
        }
    }
}
