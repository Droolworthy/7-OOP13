namespace OOP13
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CarService carService = new CarService();

            carService.Work();
        }
    }

    class CarService
    {
        private Queue<Client> _clients = new Queue<Client>();
        private List<Detail> _details = new List<Detail>();
        private DetailWarehouse _detailWarehouse = new DetailWarehouse();
        private int _moneyStation = 100000;

        public CarService()
        {
            AddDetails();

            CreateQueueClients();
        }

        public void Work()
        {
            bool isWork = true;

            string commandAcceptClient = "1";
            string commandExit = "2";

            while (isWork)
            {
                if(_clients.Count > 0)
                {
                    Console.WriteLine($"Принять клиента - {commandAcceptClient} \nВыход - {commandExit}");

                    Console.Write("\nВвод: ");
                    string userInput = Console.ReadLine();

                    if (userInput == commandAcceptClient)
                    {
                        Client client = _clients.Dequeue();

                        int priceRepair = CalculateTotalPrice(client);

                        Serve(client, priceRepair);

                        if (client.IsEnoughMoney(priceRepair))
                        {
                            int index = UserUtils.GenerateRandomNumber(0, _detailWarehouse.GetDetailsCount());

                            Detail detail = _detailWarehouse.GetDetailByIndex(index);

                            if (TryFindPart(client))
                            {
                                if (CanRepairCar(detail, client))
                                {
                                    PayForRepairs(client, priceRepair);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nУ клиента не хватает денег для оплаты. Он уехал.");
                        }
                    }
                    else if (userInput == commandExit)
                    {
                        isWork = false;
                    }
                    else
                    {
                        Console.Clear();
                    }

                    Console.ReadKey();
                    Console.Clear();
                }
                else
                {
                    isWork = false;
                }
            }                             
        }

        public int PayFine()
        {
            int minimumMoneyToPayFine = 2000;
            int maximumMoneyToPayFine = 5000;
            int moneyToPayFine = UserUtils.GenerateRandomNumber(minimumMoneyToPayFine, maximumMoneyToPayFine);

            return moneyToPayFine;
        }

        public int CalculateTotalPrice(Client client)
        {
            int priceDetail = 0;
            int priceWork = 0;
            int detailsCount = _detailWarehouse.GetDetailsCount();
            int pricesWorkCount = GetPricesWorkCount();

            for (int i = 0; i < detailsCount; i++)
            {
                Detail detail = _detailWarehouse.GetDetailByIndex(i);

                if (detail.Name == client.Breakage)
                {
                    priceDetail = detail.Price;
                }
            }

            for (int i = 0; i < pricesWorkCount; i++)
            {
                Detail element = GetElementByIndex(i);

                if (client.Breakage == element.Name)
                {
                    priceWork = element.Price;
                }
            }

            int priceRepair = priceDetail + priceWork;

            return priceRepair;
        }

        public Detail GetElementByIndex(int index)
        {
            return _details.ElementAt(index);
        }

        public int GetPricesWorkCount()
        {
            return _details.Count;
        }

        private void Serve(Client client, int cost)
        {
            ShowQueueClients();

            DescribeResult($"\nБаланс денег автосервиса - {_moneyStation} рублей.", $"\nВ автосерис приехала машина - {client.CarName}",
                $"Из машины вышел клиент - {client.Name}.");

            DescribeResult($"У него в кармане - {client.Money} рублей.", $"\nЗдравствуйте. У машины проблемы с деталью - {client.Breakage}.",
                $"\nЗдравствуйте. Вам придётся заплатить - {cost} рублей за деталь и работу. У вас хватит денег? \n\nДля продолжения нажмите любую клавишу...");

            Console.ReadKey();
        }

        private void PayForRepairs(Client client, int cost) 
        {
            client.PaysAmountRepairs(client);

            _moneyStation += cost;

            DescribeResult($"\nВам нужно заплатить - {cost} рублей", $"\nКлиент - {client.Name} отдаёт вам {cost} рублей.",
                $"\nУ клиента {client.Name} осталось в кошельке - {client.Money}. Он сел в свою {client.CarName} и уехал.");
        }

        private bool CanRepairCar(Detail detail, Client client)
        {
            if (СouldRepairCar(client, detail))
            {
                _detailWarehouse.RemoveDetail(detail);

                DescribeResult($"\nНаш механик успешно заменили деталь - {detail.Name}", $"Всё готово {client.Name}. " +
                    $"Давайте расчитаемся с вами.", "Для продолжения нажмите любую клавишу...");

                Console.ReadKey();

                return true;
            }
            else
            {
                int moneyToPayFine = client.AcceptMoneyFine();

                _moneyStation -= moneyToPayFine;

                _detailWarehouse.RemoveDetail(detail);

                DescribeResult($"\nПриносим свои извинения, у нас новый механик. Недавно устроился на работу...", $"Он ошибся с деталью " +
                    $"и поменял - {detail.Name}, а не {client.Breakage}.", $"Мы вам выплатим штраф в размере - {moneyToPayFine} рублей.");

                Console.WriteLine($"\nУ клиента {client.Name} осталось в кошельке - {client.Money}. Он сел в свою {client.CarName} и уехал.");

                return false;
            }
        }

        private bool TryFindPart(Client client)
        {
            DescribeResult($"\nСейчас посмотрю. Да хватит. У вас есть - {client.Breakage}?", "Хм...Сейчас посмотрим есть ли " +
                    "она у нас на складе. Минутку...", "\nДля продолжения нажмите любую клавишу...");

            Console.ReadKey();

            if (_detailWarehouse.TryGetDetail(client))
            {
                DescribeResult($"\nДа, действительно у нас есть - {client.Breakage}", $"Сейчас наш механик занимается вашей {client.CarName}. " +
                    $"Вам придётся подождать...", "Клиент отдыхает в комнате досуга. Пора браться за работу. \n\nДля продолжения нажмите любую клавишу...");

                Console.ReadKey();

                return true;
            }
            else
            {
                int moneyToPayFine = client.AcceptMoneyFine();

                _moneyStation -= moneyToPayFine;

                DescribeResult($"\nК сожелению у нас нету - {client.Breakage}", $"Мы вам выплатим штраф в размере - {moneyToPayFine} рублей",
                        $"\nУ клиента {client.Name} осталось в кошельке - {client.Money}. Он сел в свою {client.CarName} и уехал. " +
                        $"\n\nДля того чтобы запустить следующую машину нажмите любую клавишу...");

                return false;
            }
        }

        private bool СouldRepairCar(Client client, Detail detail)
        {
            int detailsCount = _detailWarehouse.GetDetailsCount();

            for (int i = 0; i < detailsCount; i++)
            {
                Detail element = _detailWarehouse.GetDetailByIndex(i);

                if (detail == element)
                {
                    if (client.Breakage == detail.Name)
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

        private void ShowQueueClients()
        {
            Console.WriteLine("\nСписок клиентов: ");

            foreach (Client client in _clients)
            {
                Console.WriteLine(client.Name);
            }
        }

        private void AddDetails()
        {
            _details.Add(new Detail("Ремень ГРМ", 2500));
            _details.Add(new Detail("Аккумулятор", 1000));
            _details.Add(new Detail("Рулевая рейка", 2000));
            _details.Add(new Detail("Тормозная колодка", 1000));
            _details.Add(new Detail("Фара", 900));
            _details.Add(new Detail("Бампер", 2500));
            _details.Add(new Detail("Маховик", 3500));
            _details.Add(new Detail("Турбокомпрессор", 3550));
            _details.Add(new Detail("Бензобак", 3900));
            _details.Add(new Detail("Фильтр", 350));
            _details.Add(new Detail("Поршень", 450));
            _details.Add(new Detail("Цилиндр", 875));
        }

        private void CreateQueueClients()
        {
            _clients.Enqueue(new Client("Василий", 6000, "LADA Granta", "Аккумулятор"));
            _clients.Enqueue(new Client("Михаил", 1235, "Škoda Octavia", "Маховик"));
            _clients.Enqueue(new Client("Елена", 10000, "Hyundai Solaris", "Ремень ГРМ"));
            _clients.Enqueue(new Client("Николай", 11234, "Lada Vesta", "Рулевая рейка"));
            _clients.Enqueue(new Client("Алексей", 9999, "ВАЗ-2121", "Тормозная колодка"));
            _clients.Enqueue(new Client("Владимир", 12085, "ВАЗ-2114 ", "Турбокомпрессор"));
            _clients.Enqueue(new Client("Артемий", 3900, "Москвич - 412 ", "Фара"));
        }
    }

    class Client
    {
        public Client(string appellation, int cash, string appellationAuto, string malfunction)
        {
            Name = appellation;
            CarName = appellationAuto;
            Breakage = malfunction;
            Money = cash;
        }

        public string Name { get; private set; }

        public string CarName { get; private set; }

        public string Breakage { get; private set; }

        public int Money { get; protected set; }

        public int AcceptMoneyFine()
        {
            CarService carService = new CarService();

            int moneyFine = carService.PayFine();

            Money += moneyFine;

            return moneyFine;
        }

        public void PaysAmountRepairs(Client client)
        {
            CarService carService = new CarService();

            int priceRepair = carService.CalculateTotalPrice(client);

            Money -= priceRepair;
        }

        public bool IsEnoughMoney(int money) => Money > money;
    }

    class DetailWarehouse
    {
        private List<Detail> _details = new List<Detail>();
        private string[] _items = { "Ремень ГРМ", "Аккумулятор", "Рулевая рейка", "Тормозная колодка", "Фара", "Бампер",
        "Маховик", "Турбокомпрессор", "Бензобак", "Фильтр", "Поршень", "Цилиндр" };

        public DetailWarehouse()
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

        public bool RemoveDetail(Detail detail)
        {
            return _details.Remove(detail);
        }

        public bool TryGetDetail(Client client)
        {
            for (int i = 0; i < _details.Count; i++)
            {
                if (_details[i].Name == client.Breakage)
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
            int numberDetails = UserUtils.GenerateRandomNumber(minimumNumberDetails, maximumNumberDetails);

            for (int i = 0; i < numberDetails; i++)
            {
                _details.Add(GetItemByIndex(index));
            }
        }

        private Detail GetItemByIndex(int index)
        {
            int detail = UserUtils.GenerateRandomNumber(0, _details.Count);

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

            return _details[detail];
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

    class UserUtils
    {
        private static Random _random = new Random();

        internal static int GenerateRandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }
    }
}
