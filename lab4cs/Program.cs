using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace lab4
{
    
    public class Station
    {
        public string Name { get; set; }
        public DateTime ArrivalTime { get; set; }
        public DateTime DepartureTime { get; set; }
        public int AvailableSeats { get; set; }
    }

    
    public class Route
    {
        public List<Station> Stations { get; set; }
        public int TotalSeats { get; set; }
        public List<string> DaysOfWeek { get; set; }
        public int FlightNumber { get; set; }

        public Route()
        {
            Stations = new List<Station>();
        }
    }

    public class TicketCounter : IEnumerable<Route>
    {
        private List<Route> routes = new List<Route>();

        public void AddRoute(Route route)
        {
            routes.Add(route);
        }

        public void RemoveRoute(Route route)
        {
            routes.Remove(route);
        }

        public IEnumerable<Route> GetRoutes()
        {
            return routes;
        }

        public IEnumerator<Route> GetEnumerator()
        {
            return routes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Serializer
    {
        public static void Serialize<T>(T obj, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                serializer.Serialize(fs, obj);
            }
        }

        public static T Deserialize<T>(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                return (T)serializer.Deserialize(fs);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            TicketCounter ticketCounter = new TicketCounter();

            if (args.Length > 0 && args[0] == "-auto")
            {
                // Автоматичний режим, де дані зчитуються з файлу або генеруються.
                Route route1 = new Route
                {
                    TotalSeats = 100,
                    DaysOfWeek = new List<string> { "Monday", "Wednesday", "Friday" },
                    FlightNumber = 101
                };
                route1.Stations.Add(new Station
                {
                    Name = "Station A",
                    DepartureTime = DateTime.Parse("08:00"),
                    ArrivalTime = DateTime.Parse("08:30"),
                    AvailableSeats = 100
                });
                route1.Stations.Add(new Station
                {
                    Name = "Station B",
                    DepartureTime = DateTime.Parse("09:00"),
                    ArrivalTime = DateTime.Parse("09:30"),
                    AvailableSeats = 90
                });

                Route route2 = new Route
                {
                    TotalSeats = 120,
                    DaysOfWeek = new List<string> { "Tuesday", "Thursday" },
                    FlightNumber = 102
                };
                route2.Stations.Add(new Station
                {
                    Name = "Station A",
                    DepartureTime = DateTime.Parse("10:00"),
                    ArrivalTime = DateTime.Parse("10:30"),
                    AvailableSeats = 120
                });

                ticketCounter.AddRoute(route1);
                ticketCounter.AddRoute(route2);

                // Серіалізуємо об'єкти в файл для наступного запуску.
                Serializer.Serialize(ticketCounter, "ticketcounter.xml");

                Console.WriteLine("Data has been generated and saved to ticketcounter.xml.");
            }
            else
            {
                // Інтерактивний режим, де користувач може додавати/видаляти маршрути та взаємодіяти з квитковою касою.
                if (File.Exists("ticketcounter.xml"))
                {
                    ticketCounter = Serializer.Deserialize<TicketCounter>("ticketcounter.xml");
                    Console.WriteLine("Data has been loaded from ticketcounter.xml.");
                }

                while (true)
                {
                    Console.WriteLine("Ticket Counter Menu:");
                    Console.WriteLine("1. Add Route");
                    Console.WriteLine("2. Remove Route");
                    Console.WriteLine("3. List Routes");
                    Console.WriteLine("4. Exit");
                    Console.Write("Enter your choice: ");
                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            // Додати новий маршрут.
                            Route newRoute = new Route();
                            Console.Write("Total Seats: ");
                            newRoute.TotalSeats = int.Parse(Console.ReadLine());
                            Console.Write("Days of Week (comma-separated): ");
                            newRoute.DaysOfWeek = Console.ReadLine().Split(',').Select(s => s.Trim()).ToList();
                            Console.Write("Flight Number: ");
                            newRoute.FlightNumber = int.Parse(Console.ReadLine());

                            while (true)
                            {
                                Station station = new Station();
                                Console.Write("Station Name (or 'done' to finish): ");
                                string stationName = Console.ReadLine();
                                if (stationName.ToLower() == "done")
                                    break;

                                station.Name = stationName;
                                Console.Write("Departure Time (HH:mm): ");
                                station.DepartureTime = DateTime.Parse(Console.ReadLine());
                                Console.Write("Arrival Time (HH:mm): ");
                                station.ArrivalTime = DateTime.Parse(Console.ReadLine());
                                Console.Write("Available Seats: ");
                                station.AvailableSeats = int.Parse(Console.ReadLine());

                                newRoute.Stations.Add(station);
                            }

                            ticketCounter.AddRoute(newRoute);
                            break;

                        case "2":
                            // Видалити маршрут.
                            Console.Write("Enter Flight Number to remove: ");
                            int flightNumberToRemove = int.Parse(Console.ReadLine());
                            Route routeToRemove = ticketCounter.GetRoutes()
                                .FirstOrDefault(r => r.FlightNumber == flightNumberToRemove);
                            if (routeToRemove != null)
                            {
                                ticketCounter.RemoveRoute(routeToRemove);
                                Console.WriteLine($"Route with Flight Number {flightNumberToRemove} removed.");
                            }
                            else
                            {
                                Console.WriteLine($"Route with Flight Number {flightNumberToRemove} not found.");
                            }

                            break;

                        case "3":
                            // Вивести список маршрутів.
                            Console.WriteLine("List of Routes:");
                                
                            foreach (var route in ticketCounter)
                            {
                                Console.WriteLine(
                                    $"Flight Number: {route.FlightNumber}, Total Seats: {route.TotalSeats}");
                                Console.WriteLine("Stations:");
                                foreach (var station in route.Stations)
                                {
                                    Console.WriteLine(
                                        $"- {station.Name}, Departure: {station.DepartureTime}, Arrival: {station.ArrivalTime}, Seats: {station.AvailableSeats}");
                                }

                                Console.WriteLine($"Days of Week: {string.Join(", ", route.DaysOfWeek)}");
                                Console.WriteLine();
                            }

                            break;

                        case "4":
                            // Завершити програму і зберегти дані.
                            Serializer.Serialize(ticketCounter, "ticketcounter.xml");
                            Console.WriteLine("Data has been saved. Exiting...");
                            return;

                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
            }
        }
    }
}