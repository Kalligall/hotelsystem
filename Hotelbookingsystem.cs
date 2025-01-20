using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;

class HotelBookingSystem
{
    private readonly string connectionString;

    public HotelBookingSystem()
    {
        connectionString = File.ReadAllText("connectionstring.txt");
    }

    public IDbConnection Connect()
    {
        return new SqlConnection(connectionString);
    }

    private void DisplayMenuHeader(string menuTitle)
    {
        Console.Clear();
        Console.WriteLine($"\n*** {menuTitle} ***");
    }

    public void ShowMainMenu()
    {
        while (true)
        {
            DisplayMenuHeader("Hotel Booking System");
            Console.WriteLine("1. Customer Menu");
            Console.WriteLine("2. Staff Menu");
            Console.WriteLine("q. Exit");
            Console.Write("Choose an option: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowCustomerMenu();
                    break;
                case "2":
                    ShowStaffMenu();
                    break;
                case "q":
                    Console.WriteLine("Exiting...");
                    Thread.Sleep(1000);
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    Console.ReadKey();
                    break;
            }
        }
    }
    public void ShowCustomerMenu()
    {
        while (true)
        {
            DisplayMenuHeader("Customer Menu");
            Console.WriteLine("1. View Available Rooms");
            Console.WriteLine("2. View Average Room Price");
            Console.WriteLine("3. View Cheapest Room");
            Console.WriteLine("4. Make a Reservation");
            Console.WriteLine("5. View Your Reservations");
            Console.WriteLine("6. View Services");
            Console.WriteLine("7. Add Service to Your Reservation");
            Console.WriteLine("q. Back to Main Menu");
            Console.Write("Choose an option: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowAvailableRooms();
                    break;
                case "2":
                    ShowAverageRoomPrice();
                    break;
                case "3":
                    ShowCheapestRoom();
                    break;
                case "4":
                    AddBooking();
                    break;
                case "5":
                    ShowCustomerBookings();
                    break;
                case "6":
                    ShowServices();
                    break;
                case "7":
                    AddServiceToRoom();
                    break;
                case "q":
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    Console.ReadKey();
                    break;
            }
        }
    }

    public void ShowStaffMenu()
    {
        while (true)
        {
            DisplayMenuHeader("Staff Menu");
            Console.WriteLine("1. View All Rooms");
            Console.WriteLine("2. View All Reservations");
            Console.WriteLine("3. Assign a Service to a Room");
            Console.WriteLine("4. View Staff Members");
            Console.WriteLine("5. View Services");
            Console.WriteLine("6. View Cheapest Room");
            Console.WriteLine("7. View Total Revenue");
            Console.WriteLine("q. Back to Main Menu");
            Console.Write("Choose an option: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowAllRooms();
                    break;
                case "2":
                    ShowBookings();
                    break;
                case "3":
                    AddServiceToRoom();
                    break;
                case "4":
                    ShowStaff();
                    break;
                case "5":
                    ShowServices();
                    break;
                case "6":
                    ShowCheapestRoom();
                    break;
                case "7":
                    ShowTotalRevenue();
                    break;
                case "q":
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    Console.ReadKey();
                    break;
            }
        }
    }

    public void ShowBookings()
    {
        using (var connection = Connect())
        {
            var query = @"
                SELECT b.id AS BookingID, c.first_name AS CustomerFirstName, c.last_name AS CustomerLastName, 
                       b.check_in_date AS CheckInDate, b.check_out_date AS CheckOutDate, b.status AS Status,
                       STRING_AGG(r.room_number, ', ') AS RoomNumbers
                FROM booking b
                JOIN customer c ON b.customer_id = c.id
                JOIN roomtobooking rb ON b.id = rb.booking_id
                JOIN room r ON rb.room_id = r.id
                GROUP BY b.id, c.first_name, c.last_name, b.check_in_date, b.check_out_date, b.status;
                ";

            var bookings = connection.Query(query);
            Console.WriteLine("\nBookings:");
            foreach (var booking in bookings)
            {
                Console.WriteLine($"Booking ID: {booking.BookingID}, Customer: {booking.CustomerFirstName} {booking.CustomerLastName}, " +
                                  $"Rooms: {booking.RoomNumbers}, From: {booking.CheckInDate}, To: {booking.CheckOutDate}, Status: {booking.Status}");
            }
            Console.ReadKey();
        }
    }

    public void ShowAllRooms()
    {
        using (var connection = Connect())
        {
            var query = "SELECT id, room_number, type, capacity, price, status FROM room";
            var rooms = connection.Query(query);
            Console.WriteLine("\nAvailable rooms:");
            foreach (var room in rooms)
            {
                Console.WriteLine($"ID: {room.id}, Room: {room.room_number}, Type: {room.type}, Capacity: {room.capacity}, Price: {room.price} USD, Status: {room.status}");
            }
            Console.ReadKey();
        }
    }

    public void ShowAvailableRooms()
    {
        using (var connection = Connect())
        {
            var query = "SELECT room_number, type, price FROM room WHERE status = 'Available'";
            var rooms = connection.Query(query);
            Console.WriteLine("\nAvailable Rooms:");
            foreach (var room in rooms)
            {
                Console.WriteLine($"Room Number: {room.room_number}, Type: {room.type}, Price: {room.price} USD");
            }
            Console.ReadKey();
        }
    }

    public void ShowCheapestRoom()
    {
        using (var connection = Connect())
        {
            var query = "SELECT TOP 1 room_number, price, type FROM room ORDER BY price ASC";
            var room = connection.QuerySingle(query);
            Console.WriteLine("\nShowing cheapest room:");
            Console.WriteLine($"Room Number: {room.room_number}, Price: {room.price} USD, Type: {room.type}");
            Console.ReadKey();
        }
    }

    public void ShowAverageRoomPrice()
    {
        using (var connection = Connect())
        {
            var query = "SELECT AVG(price) AS AveragePrice FROM room";
            var result = connection.QuerySingle(query);
            Console.WriteLine("\nAverage Room Price:");
            Console.WriteLine($"Average Price: {result.AveragePrice} USD");
            Console.ReadKey();
        }
    }

    public void AddBooking()
    {
        Console.Write("Enter customer ID: ");
        int customerId = int.Parse(Console.ReadLine());
        Console.Write("Enter check-in date (YYYY-MM-DD): ");
        string checkInDate = Console.ReadLine();
        Console.Write("Enter check-out date (YYYY-MM-DD): ");
        string checkOutDate = Console.ReadLine();

        using (var connection = Connect())
        {
            var bookingQuery = @"
                INSERT INTO booking (customer_id, check_in_date, check_out_date)
                VALUES (@CustomerId, @CheckInDate, @CheckOutDate);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int bookingId = connection.QuerySingle<int>(bookingQuery, new { CustomerId = customerId, CheckInDate = checkInDate, CheckOutDate = checkOutDate });

            Console.WriteLine("Enter room numbers to book (comma-separated): ");
            var roomNumbers = Console.ReadLine().Split(',').Select(num => num.Trim());

            foreach (var roomNumber in roomNumbers)
            {
                var roomIdQuery = "SELECT id FROM room WHERE room_number = @RoomNumber";
                var roomId = connection.QuerySingleOrDefault<int>(roomIdQuery, new { RoomNumber = roomNumber });

                if (roomId == 0)
                {
                    Console.WriteLine($"Room number {roomNumber} does not exist. Skipping...");
                    continue;
                }

                var roomToBookingQuery = @"
                    INSERT INTO roomtobooking (booking_id, room_id)
                    VALUES (@BookingId, @RoomId)";
                connection.Execute(roomToBookingQuery, new { BookingId = bookingId, RoomId = roomId });
            }

            Console.WriteLine("Booking added successfully!");
        }
        Console.ReadKey();
    }

    public void ShowStaff()
    {
        using (var connection = Connect())
        {
            var query = "SELECT first_name, last_name, role, phone_number FROM employee";
            var staff = connection.Query(query);

            Console.WriteLine("\nStaff Members:");
            foreach (var member in staff)
            {
                Console.WriteLine($"Name: {member.first_name} {member.last_name}, Role: {member.role}");
            }
            Console.ReadKey();
        }
    }

    public void ShowServices()
    {
        using (var connection = Connect())
        {
            var query = "SELECT id, name, description, price FROM service";
            var services = connection.Query(query);

            Console.WriteLine("\nAvailable Services:");
            foreach (var service in services)
            {
                Console.WriteLine($"ID: {service.id}, Name: {service.name}, Price: {service.price} USD");
                Console.WriteLine($"    Description: {service.description}");
            }
            Console.ReadKey();
        }
    }

    public void ShowTotalRevenue()
    {
        using (var connection = Connect())
        {
            var query = @"
            SELECT SUM(price) AS TotalRevenue
            FROM booking b
            JOIN roomtobooking rb ON b.id = rb.booking_id
            JOIN room r ON rb.room_id = r.id;
            ";
            var revenue = connection.QuerySingle(query);

            Console.WriteLine("\nTotal Revenue from Bookings:");
            Console.WriteLine($"Revenue: {revenue.TotalRevenue} USD");
            Console.ReadKey();
        }
    }

    public void ShowRoomsByType()
    {
        using (var connection = Connect())
        {
            var query = @"
            SELECT type, COUNT(*) AS TotalRooms
            FROM room
            WHERE status = 'Available'
            GROUP BY type;
            ";
            var rooms = connection.Query(query);

            Console.WriteLine("\nRooms by Type (Available):");
            foreach (var room in rooms)
            {
                Console.WriteLine($"Type: {room.type}, Total Rooms: {room.TotalRooms}");
            }
            Console.ReadKey();
        }
    }

    public void ShowCustomersWithBookings()
    {
        using (var connection = Connect())
        {
            var query = @"
            SELECT c.first_name, c.last_name, STRING_AGG(r.room_number, ', ') AS RoomNumbers, 
                   b.check_in_date, b.check_out_date
            FROM customer c
            JOIN booking b ON c.id = b.customer_id
            JOIN roomtobooking rb ON b.id = rb.booking_id
            JOIN room r ON rb.room_id = r.id
            GROUP BY c.first_name, c.last_name, b.check_in_date, b.check_out_date;
            ";
            var customers = connection.Query(query);

            Console.WriteLine("\nCustomers with Bookings:");
            foreach (var customer in customers)
            {
                Console.WriteLine($"Name: {customer.first_name} {customer.last_name}, Rooms: {customer.RoomNumbers}, " +
                                  $"From: {customer.check_in_date}, To: {customer.check_out_date}");
            }
            Console.ReadKey();
        }
    }

    public void ShowCustomerBookings()
    {
        Console.Clear();
        Console.WriteLine("Enter your first name: ");
        string firstName = Console.ReadLine();
        Console.WriteLine("Enter your last name: ");
        string lastName = Console.ReadLine();

        using (var connection = Connect())
        {
            var query = @"
            SELECT b.id AS BookingID, STRING_AGG(r.room_number, ', ') AS RoomNumbers, 
                   b.check_in_date AS CheckInDate, b.check_out_date AS CheckOutDate, b.status AS Status
            FROM booking b
            JOIN customer c ON b.customer_id = c.id
            JOIN roomtobooking rb ON b.id = rb.booking_id
            JOIN room r ON rb.room_id = r.id
            WHERE c.first_name = @FirstName AND c.last_name = @LastName
            GROUP BY b.id, b.check_in_date, b.check_out_date, b.status;
            ";

            var bookings = connection.Query(query, new { FirstName = firstName, LastName = lastName });

            if (!bookings.Any())
            {
                Console.WriteLine("\nNo bookings found for the provided name.");
            }
            else
            {
                Console.WriteLine($"\nBookings for {firstName} {lastName}:");
                foreach (var booking in bookings)
                {
                    Console.WriteLine($"Booking ID: {booking.BookingID}, Rooms: {booking.RoomNumbers}, " +
                                      $"From: {booking.CheckInDate}, To: {booking.CheckOutDate}, Status: {booking.Status}");
                }
            }
        }

        Console.WriteLine("\nPress any key to return to the menu...");
        Console.ReadKey();
    }
    public void AddServiceToRoom()
    {
        Console.Write("Enter room number: ");
        var roomNumber = Console.ReadLine();
        Console.Write("Enter service ID: ");
        var serviceId = int.Parse(Console.ReadLine());
        Console.Write("Enter employee ID: ");
        var employeeId = int.Parse(Console.ReadLine());

        using (var connection = Connect())
        {
            // Validate room
            var roomIdQuery = "SELECT id FROM room WHERE room_number = @RoomNumber";
            var roomId = connection.QuerySingleOrDefault<int>(roomIdQuery, new { RoomNumber = roomNumber });
            if (roomId == 0)
            {
                Console.WriteLine("Invalid room number. Operation aborted.");
                Console.ReadKey();
                return;
            }

            // Insert service assignment
            var query = @"
            INSERT INTO servicetoroom (room_id, service_id, employee_id)
            VALUES (@RoomId, @ServiceId, @EmployeeId);
        ";
            connection.Execute(query, new { RoomId = roomId, ServiceId = serviceId, EmployeeId = employeeId });

            Console.WriteLine("Service successfully assigned to the room.");
            Console.ReadKey();
        }
    }
}