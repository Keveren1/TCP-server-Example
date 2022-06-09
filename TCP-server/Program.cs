using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using MovieLIB;
using System.Text.Json;
using System.Net.Http;

namespace TCP_server
{
    class Program
    {
        //Linket vi bruger når vi skal hente data fra vores REST service (Skal være kørende)
        public static readonly string url = "http://localhost:49203/api/Movie";
        //Afsnittet med nextId og listen er taget direkte fra REST servicen
        private static int nextId = 1;

        private static List<Movie> data = new List<Movie>()
        {
            new Movie() {Id = nextId++, MovieName = "Shrek 2", LengthInMinutes = 90, CountryOfOrigin = "Japan"},
            new Movie() {Id = nextId++, MovieName = "Shrek 1", LengthInMinutes = 70, CountryOfOrigin = "Japan"},
            new Movie() {Id = nextId++, MovieName = "Shrek 3", LengthInMinutes = 120, CountryOfOrigin = "Turkmenistan"},
        };

        static void Main(string[] args)
        {
            Console.WriteLine("TCP Server");

            //Vi opretter vores listener, så den kan lytte på Wi-Fi og Ethernet forbindelser, fra alle de Ip'er der er knyttet til samme netværk.
            TcpListener listener = new TcpListener(IPAddress.Any, 43214);

            listener.Start();

            //Vi laver while(true) loop, da vi vil gøre vores server concurrent. Dette gør vi ved at kunne acceptere flere clienter og køre vores Task.Run og vores Handle Client metode.
            while (true)
            {
                TcpClient socket = listener.AcceptTcpClient();
                Task.Run(() => HandleClient(socket));
            }



        }

        //Her opretter vi vores HandleClient Metode
        static void HandleClient(TcpClient socket)
        {
            //socket.GetStream() sørger for at vi kan sende og modtage data .GetStream metoden returnere en NetworkStream
            NetworkStream ns = socket.GetStream();
           
            //StreamReaderen er designet til at læse data fra en NetworkStream, den bruger også NetworkStream
            StreamReader reader = new StreamReader(ns);
            
            //Vi bruger StreamWriter er en nem måde at skrive data til en NetworkStream, den bruger også NeworkStream
            StreamWriter writer = new StreamWriter(ns);

            //Her læser vi alt data der bliver sendt
            string message = reader.ReadLine();
            
            // her opretter vi et array for at kunne splitte vores message op
            string [] array = message.Split(' ');

            //her sætter vi en variabel (helst af listenavn) som skal være om. I eksemplet under er der brugt "null", men "" kan også bruges
            string movieList = "null";

            //Hvis vores første plads i arrayet er == "GetAll" kalder vi GetAll() metoden. Ellers hvis første plad i arrayet == GetByCountry
            //OBS: dette er det vi gør når vi er forbundet til vores REST service.
            if (array[0] == "GetAll")
            {
                movieList = GetAll().Result;
            }
            else if (array[0] == "GetByCountry")
            {
                movieList = GetByCountry(array[1]).Result;
            }
            //Vi gør det samme i den udkommenterede metode nedenfor, bare hvor vi gør det hele lokalt med listen der er sat ind ovenfor

            //List<Movie> movieList = new List<Movie>();
            //if (array[0] == "GetAll")
            //{
            //    movieList = GetAll();
            //} else if(array[0] == "GetByCountry")
            //{
            //    movieList = GetByCountry(array[1]);
            //}

            //her konvertere vi et JSON objekt til en string. Deserialize er at gøre det omvendt
            string Json = JsonSerializer.Serialize(movieList);

            //her udskriver vi vores objekt
            writer.WriteLine(Json);

            //her flusher vi, det er vigtigt at skylle ud efter sig
            writer.Flush();
            
            //her lukkre vi vores socket.
            socket.Close();
        }

        //Her gør vi brug af async Tasks, vha. multithreading. en async metode kører synkront indtil den når en await expression
        static async Task<string> GetAll()
        {
            //vi benytter os af HttpClient til at kunne sende HTTP requests og modtage svar fra en resource.
            HttpClient client = new HttpClient();
            try
            {

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string body = await response.Content.ReadAsStringAsync();
                return body;
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        static async Task<string> GetByCountry(string country)
        {
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync(url + "?country=" + country);
                response.EnsureSuccessStatusCode();
                string body = await response.Content.ReadAsStringAsync();
                return body;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        //static List<Movie> GetAll()
        //{
        //    return data;
        //}
        //static List<Movie> GetByCountry(string country)
        //{

        //    return data.FindAll(c => c.CountryOfOrigin == country);
        //}


    }
}
