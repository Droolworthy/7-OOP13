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
        private DetailWarehouse _detailWarehouse = new DetailWarehouse();
        private Dictionary<string, int> _worksPrice = new Dictionary<string, int>();
        private int _moneyStation = 100000;

        public CarService()
        {
            CreateQueueClients();

            CreateWorksPrice();
        }

        public void Work()
        {
            bool isWork = true;

            string commandAcceptClient = "1";
            string commandExit = "2";

            while (isWork)
            {
                if (_clients.Count > 0)
                {
                    Console.WriteLine($"Принять клиента - {commandAcceptClient} \nВыход - {commandExit}");

                    Console.Write("\nВвод: ");
                    string userInput = Console.ReadLine();

                    if (userInput == commandAcceptClient)
                    {
                        Client client = _clients.Dequeue();

                        int priceRepair = CalculateTotalPrice(client);

                        OutputInfo(client, priceRepair);

                        if (client.IsEnoughMoney(priceRepair))
                        {
                            int index = UserUtils.GenerateRandomNumber(0, _detailWarehouse.GetDetailsCount());

                            Detail detail = _detailWarehouse.GetDetailByIndex(index);

                            if (TryFindPart(client))
                            {
                                if (CanRepairCar(client, detail))
                                {
                                    client.PaysAmountRepairs(priceRepair);

                                    _moneyStation += priceRepair;

                                    DescribeResult($"\nВам нужно заплатить - {priceRepair} рублей", $"\nКлиент - {client.Name} отдаёт вам {priceRepair} рублей.",
                                        $"\nУ клиента {client.Name} осталось в кошельке - {client.Money}. Он сел в свою {client.CarName} и уехал.");
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

        private int GeneratePriceForWork()
        {
            int minimumMoneyToPay = 2000;
            int maximumMoneyToPay = 5000;
            int moneyToPay = UserUtils.GenerateRandomNumber(minimumMoneyToPay, maximumMoneyToPay);

            return moneyToPay;
        }

        private int CalculateTotalPrice(Client client)
        {
            int priceDetail = 0;
            int detailsCount = _worksPrice.Count;
            int priceWork = GeneratePriceForWork();

            for (int i = 0; i < detailsCount; i++)
            {
                int detailPrice = _worksPrice.ElementAt(i).Value;

                string detailName = _worksPrice.ElementAt(i).Key;

                if (detailName == client.Breakage)
                {
                    priceDetail = detailPrice;
                }
            }

            int priceRepair = priceDetail + priceWork;

            return priceRepair;
        }

        private void OutputInfo(Client client, int cost)
        {
            ShowQueueClients();

            DescribeResult($"\nБаланс денег автосервиса - {_moneyStation} рублей.", $"\nВ автосерис приехала машина - {client.CarName}",
                $"Из машины вышел клиент - {client.Name}.");

            DescribeResult($"У него в кармане - {client.Money} рублей.", $"\nЗдравствуйте. У машины проблемы с деталью - {client.Breakage}.",
                $"\nЗдравствуйте. Вам придётся заплатить - {cost} рублей за деталь и работу. У вас хватит денег? \n\nДля продолжения нажмите любую клавишу...");

            Console.ReadKey();
        }

        private bool TryFindPart(Client client)
        {
            DescribeResult($"\nСейчас посмотрю. Да хватит. У вас есть - {client.Breakage}?", "Хм...Сейчас посмотрим есть ли " +
                    "она у нас на складе. Минутку...", "\nДля продолжения нажмите любую клавишу...");

            Console.ReadKey();

            if (_detailWarehouse.ChecksAvailabilityPart(client))
            {
                DescribeResult($"\nДа, действительно у нас есть - {client.Breakage}", $"Сейчас наш механик занимается вашей {client.CarName}. " +
                    $"Вам придётся подождать...", "Клиент отдыхает в комнате досуга. Пора браться за работу. \n\nДля продолжения нажмите любую клавишу...");

                Console.ReadKey();

                return true;
            }
            else
            {
                int moneyToPayFine = GeneratePriceForWork();

                client.AcceptMoneyFine(moneyToPayFine);

                _moneyStation -= moneyToPayFine;

                DescribeResult($"\nК сожелению у нас нету - {client.Breakage}", $"Мы вам выплатим штраф в размере - {moneyToPayFine} рублей",
                        $"\nУ клиента {client.Name} осталось в кошельке - {client.Money}. Он сел в свою {client.CarName} и уехал. " +
                        $"\n\nДля того чтобы запустить следующую машину нажмите любую клавишу...");

                return false;
            }
        }

        private bool CanRepairCar(Client client, Detail detail)
        {
            int detailsCount = _detailWarehouse.GetDetailsCount();

            for (int i = 0; i < detailsCount; i++)
            {
                Detail element = _detailWarehouse.GetDetailByIndex(i);

                if (detail == element)
                {
                    if (client.Breakage == detail.Name)
                    {
                        _detailWarehouse.RemoveDetail(detail);

                        DescribeResult($"\nНаш механик успешно заменили деталь - {detail.Name}", $"Всё готово {client.Name}. " +
                            $"Давайте расчитаемся с вами.", "Для продолжения нажмите любую клавишу...");

                        Console.ReadKey();

                        return true;
                    }
                }
            }

            int moneyToPayFine = GeneratePriceForWork();

            client.AcceptMoneyFine(moneyToPayFine);

            _moneyStation -= moneyToPayFine;

            _detailWarehouse.RemoveDetail(detail);

            DescribeResult($"\nПриносим свои извинения, у нас новый механик. Недавно устроился на работу...", $"Он ошибся с деталью " +
                $"и поменял - {detail.Name}, а не {client.Breakage}.", $"Мы вам выплатим штраф в размере - {moneyToPayFine} рублей.");

            Console.WriteLine($"\nУ клиента {client.Name} осталось в кошельке - {client.Money}. Он сел в свою {client.CarName} и уехал.");

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

        private void CreateWorksPrice()
        {
            _worksPrice.Add("Ремень ГРМ", 5000);
            _worksPrice.Add("Аккумулятор", 500);
            _worksPrice.Add("Рулевая рейка", 1500);
            _worksPrice.Add("Тормозная колодка", 600);
            _worksPrice.Add("Фара", 7000);
            _worksPrice.Add("Бампер", 3000);
            _worksPrice.Add("Маховик", 4000);
            _worksPrice.Add("Турбокомпрессор", 2000);
            _worksPrice.Add("Бензобак", 5100);
            _worksPrice.Add("Фильтр", 2000);
            _worksPrice.Add("Поршень", 300);
            _worksPrice.Add("Цилиндр", 400);
        }

        private void CreateQueueClients()
        {
            _clients.Enqueue(new Client("Василий", 6000, "LADA Granta", "Аккумулятор"));
            _clients.Enqueue(new Client("Михаил", 1235, "Škoda Octavia", "Маховик"));
            _clients.Enqueue(new Client("Елена", 10000, "Hyundai Solaris", "Аккумулятор"));
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

        public int Money { get; private set; }

        public void AcceptMoneyFine(int moneyFine)
        {
            Money += moneyFine;
        }

        public void PaysAmountRepairs(int priceRepair)
        {
            Money -= priceRepair;
        }

        public bool IsEnoughMoney(int money) => Money > money;
    }

    class DetailWarehouse
    {
        private List<Detail> _details = new List<Detail>();

        public DetailWarehouse()
        {
            AddDetails();
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

        public bool ChecksAvailabilityPart(Client client)
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

        private void AddDetails()
        {
            _details.Add(new Detail("Ремень ГРМ"));
            _details.Add(new Detail("Аккумулятор"));
            _details.Add(new Detail("Рулевая рейка"));
            _details.Add(new Detail("Тормозная колодка"));
            _details.Add(new Detail("Фара"));
            _details.Add(new Detail("Бампер"));
            _details.Add(new Detail("Маховик"));
            _details.Add(new Detail("Турбокомпрессор"));
            _details.Add(new Detail("Бензобак"));
            _details.Add(new Detail("Фильтр"));
            _details.Add(new Detail("Поршень"));
            _details.Add(new Detail("Цилиндр"));
        }
    }

    class Detail
    {
        public Detail(string appellation)
        {
            Name = appellation;
        }

        public string Name { get; private set; }
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
