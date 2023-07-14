namespace OOP13
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CarService carService = new CarService();

            carService.Work(carService);
        }
    }

    class CarService
    {
        private Queue<Car> _cars = new Queue<Car>();
        private Queue<Client> _clients = new Queue<Client>();
        private List<Detail> _pricesWork = new List<Detail>();
        private PartWarehouse _partWarehouse = new PartWarehouse();
        private int _moneyServiceStation = 100000;

        public CarService()
        {
            AddPricesWork();

            CreateQueueCars();

            CreateQueueClients();
        }

        public void Work(CarService carService)
        {
            while (_cars.Count > 0)
            {
                Car car = _cars.Dequeue();
                Client client = _clients.Dequeue();
                Detail detail = _partWarehouse.GetDetailByIndex(UserUtils.GenerateRandomNumber(0, _partWarehouse.GetDetailsCount()));

                string commandAcceptClient = "1";
                string commandExit = "2";

                int priceRepair = client.CalculateTotalPrice(car, _partWarehouse, carService);

                Console.WriteLine($"Принять клиента - {commandAcceptClient} \nВыход - {commandExit}");

                Console.Write("\nВвод: ");
                string userInput = Console.ReadLine();

                if (userInput == commandAcceptClient)
                {
                    GetStartedClients(client, car, priceRepair);

                    if (client.CanPay(_partWarehouse, carService, car))
                    {
                        FindPart(car, client);

                        if (RepairCar(car, detail, client))
                        {
                            PayRepair(client, carService, car, priceRepair);
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nУ клиента не хватает денег для оплаты. Он уехал.");
                    }
                }
                else if (userInput == commandExit)
                {
                    return;
                }
                else
                {
                    Console.Clear();
                }

                Console.ReadKey();
                Console.Clear();
            }
        }

        public Detail GetPriceWorkByIndex(int index)
        {
            return _pricesWork.ElementAt(index);
        }

        public int GetPricesWorkCount()
        {
            return _pricesWork.Count;
        }

        private void GetStartedClients(Client client, Car car, int cost)
        {
            ShowQueueCars();

            ShowQueueClients();

            DescribeResult($"\nБаланс денег автосервиса - {_moneyServiceStation} рублей.", $"\nВ автосерис приехала машина - {car.Name}",
                $"Из машины вышел клиент - {client.Name}.");

            DescribeResult($"У него в кармане - {client.Money} рублей.", $"\nЗдравствуйте. У машины проблемы с деталью - {car.Breakage}.",
                $"\nЗдравствуйте. Вам придётся заплатить - {cost} рублей за деталь и работу. У вас хватит денег? \n\nДля продолжения нажмите любую клавишу...");

            Console.ReadKey();
        }

        private void PayRepair(Client client, CarService carService, Car car, int cost)
        {
            client.Buy(_partWarehouse, carService, car);

            _moneyServiceStation += cost;

            DescribeResult($"\nВам нужно заплатить - {cost} рублей", $"\nКлиент - {client.Name} отдаёт вам {cost} рублей.",
                $"\nУ клиента {client.Name} осталось в кошельке - {client.Money}. Он сел в свою {car.Name} и уехал.");
        }

        private bool RepairCar(Car car, Detail detail, Client client)
        {
            if (TryRepairCar(car, detail))
            {
                _partWarehouse.RemoveDetail(detail);

                DescribeResult($"\nНаш механик успешно заменили деталь - {detail.Name}", $"Всё готово {client.Name}. " +
                    $"Давайте расчитаемся с вами.", "Для продолжения нажмите любую клавишу...");

                Console.ReadKey();

                return true;
            }
            else
            {
                int moneyToPayFine = client.PayFineCustomer();

                _partWarehouse.RemoveDetail(detail);

                _moneyServiceStation -= moneyToPayFine;

                DescribeResult($"\nПриносим свои извинения, у нас новый механик. Недавно устроился на работу...", $"Он ошибся с деталью " +
                    $"и поменял - {detail.Name}, а не {car.Breakage}.", $"Мы вам выплатим штраф в размере - {moneyToPayFine} рублей.");

                Console.WriteLine($"\nУ клиента {client.Name} осталось в кошельке - {client.Money}. Он сел в свою {car.Name} и уехал.");

                return false;
            }
        }

        private void FindPart(Car car, Client client)
        {
            if (_partWarehouse.TryGetDetail(car))
            {
                DescribeResult($"\nСейчас посмотрю. Да хватит. Дак у вас есть - {car.Breakage}?", "Хм...Сейчас посмотрим есть ли " +
                    "она у нас на складе. Минутку...", "\nДля продолжения нажмите любую клавишу...");

                Console.ReadKey();

                DescribeResult($"\nДа, действительно у нас есть - {car.Breakage}", $"Сейчас наш механик занимается вашей {car.Name}. " +
                    $"Вам придётся подождать...", "Клиент отдыхает в комнате досуга. Пора браться за работу. \n\nДля продолжения нажмите любую клавишу...");

                Console.ReadKey();
            }
            else
            {
                int moneyToPayFine = client.PayFineCustomer();

                _moneyServiceStation -= moneyToPayFine;

                DescribeResult($"\nК сожелению у нас нету - {car.Breakage}", $"Мы вам выплатим штраф в размере - {moneyToPayFine} рублей",
                        $"\nУ клиента {client.Name} осталось в кошельке - {client.Money}. Он сел в свою {car.Name} и уехал. " +
                        $"Для того чтобы запустить следующую машину нажмите любую клавишу...");
            }
        }

        private bool TryRepairCar(Car car, Detail detail)
        {
            for (int i = 0; i < _partWarehouse.GetDetailsCount(); i++)
            {
                if (detail == _partWarehouse.GetDetailByIndex(i))
                {
                    if (car.Breakage == detail.Name)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void DescribeResult(string initialDescription, string nextDescription, string finalDescription)
        {
            Console.WriteLine(initialDescription);
            Console.WriteLine(nextDescription);
            Console.WriteLine(finalDescription);
        }

        private void ShowQueueCars()
        {
            Console.WriteLine("\nСписок машин: ");

            foreach (Car car in _cars)
            {
                Console.WriteLine(car.Name);
            }
        }

        private void ShowQueueClients()
        {
            Console.WriteLine("\nСписок клиентов: ");

            foreach (Client client in _clients)
            {
                Console.WriteLine(client.Name);
            }
        }

        private void AddPricesWork()
        {
            _pricesWork.Add(new Detail("Ремень ГРМ", 2500));
            _pricesWork.Add(new Detail("Аккумулятор", 1000));
            _pricesWork.Add(new Detail("Рулевая рейка", 2000));
            _pricesWork.Add(new Detail("Тормозная колодка", 1000));
            _pricesWork.Add(new Detail("Фара", 900));
            _pricesWork.Add(new Detail("Бампер", 2500));
            _pricesWork.Add(new Detail("Маховик", 3500));
            _pricesWork.Add(new Detail("Турбокомпрессор", 3550));
            _pricesWork.Add(new Detail("Бензобак", 3900));
            _pricesWork.Add(new Detail("Фильтр", 350));
            _pricesWork.Add(new Detail("Поршень", 450));
            _pricesWork.Add(new Detail("Цилиндр", 875));
        }

        private void CreateQueueClients()
        {
            _clients.Enqueue(new Client("Василий", 6500));
            _clients.Enqueue(new Client("Михаил", 1235));
            _clients.Enqueue(new Client("Елена", 10000));
            _clients.Enqueue(new Client("Николай", 11234));
            _clients.Enqueue(new Client("Алексей", 9999));
            _clients.Enqueue(new Client("Владимир", 12085));
            _clients.Enqueue(new Client("Артемий", 3900));
        }

        private void CreateQueueCars()
        {
            _cars.Enqueue(new Car("LADA Granta", "Аккумулятор"));
            _cars.Enqueue(new Car("Škoda Octavia", "Маховик"));
            _cars.Enqueue(new Car("Hyundai Solaris", "Ремень ГРМ"));
            _cars.Enqueue(new Car("Lada Vesta", "Рулевая рейка"));
            _cars.Enqueue(new Car("ВАЗ-2121", "Тормозная колодка"));
            _cars.Enqueue(new Car("ВАЗ-2114 ", "Турбокомпрессор"));
            _cars.Enqueue(new Car("Москвич - 412 ", "Фара"));
        }
    }

    class Client
    {
        public Client(string name, int money)
        {
            Name = name;
            Money = money;
        }

        public string Name { get; private set; }

        public int Money { get; private set; }

        public void Buy(PartWarehouse partWarehouse, CarService carService, Car car)
        {
            Money -= CalculateTotalPrice(car, partWarehouse, carService);
        }

        public int CalculateTotalPrice(Car car, PartWarehouse partWarehouse, CarService carService)
        {
            int priceDetail = 0;
            int priceWork = 0;

            for (int i = 0; i < partWarehouse.GetDetailsCount(); i++)
            {
                if (partWarehouse.GetDetailByIndex(i).Name == car.Breakage)
                {
                    priceDetail = partWarehouse.GetDetailByIndex(i).Price;
                }
            }

            for (int i = 0; i < carService.GetPricesWorkCount(); i++)
            {
                if (car.Breakage == carService.GetPriceWorkByIndex(i).Name)
                {
                    priceWork = carService.GetPriceWorkByIndex(i).Price;
                }
            }

            int priceRepair = priceDetail + priceWork;

            return priceRepair;
        }

        public int PayFineCustomer()
        {
            int minimumMoneyToPayFine = 2000;
            int maximumMoneyToPayFine = 5000;

            int moneyToPayFine = UserUtils.GenerateRandomNumber(minimumMoneyToPayFine, maximumMoneyToPayFine);

            Money += moneyToPayFine;

            return moneyToPayFine;
        }

        public bool CanPay(PartWarehouse partWarehouse, CarService carService, Car car)
        {
            return Money >= CalculateTotalPrice(car, partWarehouse, carService);
        }
    }

    class PartWarehouse
    {
        private List<Detail> _details = new List<Detail>();
        private string[] _items = { "Ремень ГРМ", "Аккумулятор", "Рулевая рейка", "Тормозная колодка", "Фара", "Бампер",
        "Маховик", "Турбокомпрессор", "Бензобак", "Фильтр", "Поршень", "Цилиндр" };

        public PartWarehouse()
        {
            CreateDetails();
        }

        public Detail GetDetailByIndex(int index)
        {
            return _details.ElementAt(index);
        }

        public int GetDetailsCount()
        {
            return _details.Count;
        }

        public bool RemoveDetail(Detail index)
        {
            return _details.Remove(index);
        }

        public bool TryGetDetail(Car car)
        {
            for (int j = 0; j < _details.Count; j++)
            {
                if (_details[j].Name == car.Breakage)
                {
                    return true;
                }
            }

            return false;
        }

        private void CreateDetails()
        {
            for (int i = 0; i < _items.Length; i++)
            {
                AddDetails(i);
            }
        }

        private void AddDetails(int index)
        {
            int minimumNumberDetails = 1;
            int maximumNumberDetails = 2;

            for (int i = 0; i < UserUtils.GenerateRandomNumber(minimumNumberDetails, maximumNumberDetails); i++)
            {
                _details.Add(GetItemByIndex(index));
            }
        }

        private Detail GetItemByIndex(int index)
        {
            switch (_items[index])
            {
                case "Ремень ГРМ":
                    return new Detail("Ремень ГРМ", 5000);
                case "Аккумулятор":
                    return new Detail("Аккумулятор", 3000);
                case "Рулевая рейка":
                    return new Detail("Рулевая рейка", 2000);
                case "Тормозная колодка":
                    return new Detail("Тормозная колодка", 500);
                case "Фара":
                    return new Detail("Фара", 800);
                case "Бампер":
                    return new Detail("Бампер", 3000);
                case "Маховик":
                    return new Detail("Маховик", 3000);
                case "Турбокомпрессор":
                    return new Detail("Турбокомпрессор", 4000);
                case "Бензобак":
                    return new Detail("Бензобак", 4000);
                case "Фильтр":
                    return new Detail("Фильтр", 300);
                case "Поршень":
                    return new Detail("Поршень", 450);
                case "Цилиндр":
                    return new Detail("Цилиндр", 875);
            }

            return _details[UserUtils.GenerateRandomNumber(0, _details.Count)];
        }
    }

    class Detail
    {
        public Detail(string appellation, int cost)
        {
            Name = appellation;
            Price = cost;
        }

        public string Name { get; private set; }

        public int Price { get; private set; }
    }

    class Car
    {
        public Car(string appellation, string malfunction)
        {
            Name = appellation;
            Breakage = malfunction;
        }

        public string Name { get; private set; }

        public string Breakage { get; private set; }
    }

    class UserUtils
    {
        private static Random _random = new Random();

        internal static int GenerateRandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }
    }
}
