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
        private List<Detail> _pricesWork = new List<Detail>();
        private PartWarehouse _partWarehouse = new PartWarehouse();
        private int _moneyServiceStation = 100000;

        public CarService()
        {
            AddPricesWork();

            CreateQueueClients();
        }

        public void Work()
        {
            while (_clients.Count > 0)
            {
                Client client = _clients.Dequeue();                
                CarService carService = new CarService();

                string commandAcceptClient = "1";
                string commandExit = "2";

                int priceRepair = CalculateTotalPrice(client, carService);

                Console.WriteLine($"Принять клиента - {commandAcceptClient} \nВыход - {commandExit}");

                Console.Write("\nВвод: ");
                string userInput = Console.ReadLine();

                if (userInput == commandAcceptClient)
                {
                    Serve(client, priceRepair);

                    if (client.IsEnoughMoney(priceRepair))
                    {
                        int element = UserUtils.GenerateRandomNumber(0, _partWarehouse.GetDetailsCount());

                        Detail detail = _partWarehouse.GetDetailByIndex(element);

                        if (FindPart(client, carService))
                        {
                            if (RepairCar(detail, client, carService))
                            {
                                PayRepair(client, carService, priceRepair);
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

        public int PayFine()
        {
            int minimumMoneyToPayFine = 2000;
            int maximumMoneyToPayFine = 5000;
            int moneyToPayFine = UserUtils.GenerateRandomNumber(minimumMoneyToPayFine, maximumMoneyToPayFine); 

            return moneyToPayFine;
        }

        public int CalculateTotalPrice(Client client, CarService carService)
        {
            int priceDetail = 0;
            int priceWork = 0;
            int detailsCount = _partWarehouse.GetDetailsCount();
            int pricesWorkCount = carService.GetPricesWorkCount();

            for (int i = 0; i < detailsCount; i++)
            {
                Detail detail = _partWarehouse.GetDetailByIndex(i);

                if (detail.Name == client.Breakage)
                {
                    priceDetail = detail.Price;
                }
            }

            for (int i = 0; i < pricesWorkCount; i++)
            {
                Detail element = carService.GetElementByIndex(i);

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
            return _pricesWork.ElementAt(index);
        }

        public int GetPricesWorkCount()
        {
            return _pricesWork.Count;
        }

        private void Serve(Client client, int cost)
        {
            ShowQueueClients();

            DescribeResult($"\nБаланс денег автосервиса - {_moneyServiceStation} рублей.", $"\nВ автосерис приехала машина - {client.CarName}",
                $"Из машины вышел клиент - {client.Name}.");

            DescribeResult($"У него в кармане - {client.Money} рублей.", $"\nЗдравствуйте. У машины проблемы с деталью - {client.Breakage}.",
                $"\nЗдравствуйте. Вам придётся заплатить - {cost} рублей за деталь и работу. У вас хватит денег? \n\nДля продолжения нажмите любую клавишу...");

            Console.ReadKey();
        }

        private void PayRepair(Client client, CarService carService, int cost)
        {
            client.PaysAmountRepairs(client, carService);

            _moneyServiceStation += cost;

            DescribeResult($"\nВам нужно заплатить - {cost} рублей", $"\nКлиент - {client.Name} отдаёт вам {cost} рублей.",
                $"\nУ клиента {client.Name} осталось в кошельке - {client.Money}. Он сел в свою {client.CarName} и уехал.");
        }

        private bool RepairCar(Detail detail, Client client, CarService carService)
        {
            if (TryRepairCar(client, detail))
            {
                _partWarehouse.RemoveDetail(detail);

                DescribeResult($"\nНаш механик успешно заменили деталь - {detail.Name}", $"Всё готово {client.Name}. " +
                    $"Давайте расчитаемся с вами.", "Для продолжения нажмите любую клавишу...");

                Console.ReadKey();

                return true;
            }
            else
            {
                int moneyToPayFine = client.AcceptMoneyFine(carService);

                _moneyServiceStation -= moneyToPayFine;

                _partWarehouse.RemoveDetail(detail);

                DescribeResult($"\nПриносим свои извинения, у нас новый механик. Недавно устроился на работу...", $"Он ошибся с деталью " +
                    $"и поменял - {detail.Name}, а не {client.Breakage}.", $"Мы вам выплатим штраф в размере - {moneyToPayFine} рублей.");

                Console.WriteLine($"\nУ клиента {client.Name} осталось в кошельке - {client.Money}. Он сел в свою {client.CarName} и уехал.");

                return false;
            }
        }

        private bool FindPart(Client client, CarService carService)
        {
            DescribeResult($"\nСейчас посмотрю. Да хватит. У вас есть - {client.Breakage}?", "Хм...Сейчас посмотрим есть ли " +
                    "она у нас на складе. Минутку...", "\nДля продолжения нажмите любую клавишу...");

            Console.ReadKey();

            if (_partWarehouse.TryGetDetail(client))
            {
                DescribeResult($"\nДа, действительно у нас есть - {client.Breakage}", $"Сейчас наш механик занимается вашей {client.CarName}. " +
                    $"Вам придётся подождать...", "Клиент отдыхает в комнате досуга. Пора браться за работу. \n\nДля продолжения нажмите любую клавишу...");

                Console.ReadKey();

                return true;
            }
            else
            {
                int moneyToPayFine = client.AcceptMoneyFine(carService);

                _moneyServiceStation -= moneyToPayFine;

                DescribeResult($"\nК сожелению у нас нету - {client.Breakage}", $"Мы вам выплатим штраф в размере - {moneyToPayFine} рублей",
                        $"\nУ клиента {client.Name} осталось в кошельке - {client.Money}. Он сел в свою {client.CarName} и уехал. " +
                        $"\n\nДля того чтобы запустить следующую машину нажмите любую клавишу...");

                return false;
            }
        }

        private bool TryRepairCar(Client client, Detail detail)
        {
            int detailsCount = _partWarehouse.GetDetailsCount();

            for (int i = 0; i < detailsCount; i++)
            {
                Detail element = _partWarehouse.GetDetailByIndex(i);

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

        public int AcceptMoneyFine(CarService carService)
        {
            int moneyFine = carService.PayFine();

            Money += moneyFine;

            return moneyFine;
        }

        public void PaysAmountRepairs(Client client, CarService carService)
        {
            int priceRepair = carService.CalculateTotalPrice(client, carService);

            Money -= priceRepair;
        }

        public bool IsEnoughMoney(int money) => Money > money;
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
