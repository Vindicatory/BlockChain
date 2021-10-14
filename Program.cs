using System;
using System.Collections.Generic;
using System.Timers;

namespace BlockChain
{
    class Operations
    {
        public List<User> users = new List<User>() { };
        public List<Miner> miners = new List<Miner>() { };


        public struct Transaction
        {
            private int initiator;
            private int target;
            private int open_key;
            private int coin;

            // Добавить время !!!

            public int Initiator
            {
                get { return initiator; }
            }
            public int Target
            {
                get { return target; }
            }
            public int Open_Key
            {
                get { return open_key; }
            }
            public int Coin
            {
                get { return coin; }
            }
            public void Init(int Initiator, int Target, int Open_key, int Coin)
            {
                this.initiator = Initiator;
                this.target = Target;
                this.open_key = Open_key;
                this.coin = Coin;
            }

            public void TransactionInfo()
            {
                Console.WriteLine("NEW TRANSACTION: " + "initiator = " + initiator.ToString() + " target = " + target.ToString() + " open_key = " + open_key + " coin = " + coin.ToString());
            }
        }
        private int reward = 1 ;

        public int Reward
        {
            get { return reward; }
        }

        private List<Transaction> transactionsPool = new List<Transaction> { };

        public List<Transaction> TransactionsPool
        {
            get { return transactionsPool; }
        }

        public void DeleteTransactions()
        {
            transactionsPool.Clear();
        }

        public void AddToPool(Transaction trans)
        {
            transactionsPool.Add(trans);
        }

        

        public List<Block> BlockChain = new List<Block> { };

        public int TallyWalletByUser(User user) // Перегрузка
        {
            return TallyWallet(user.Initiator);
        }
        public int TallyWallet(int target) // Считаем сколько денег у таргета
        {
            int sum = 0;
            foreach (Block block in BlockChain)
            {
                if (block.transaction.Target == target)
                {
                    sum += block.transaction.Coin; // Если пользователю послали денег, то добавим
                }
                if (block.transaction.Initiator == target)
                {
                    sum -= block.transaction.Coin; // Если пользователь послал, то отнимем
                }
            }
           return sum;
        }

        public struct Block
        {
            public Transaction transaction;
            public float subscribe;
            public DateTime time;
        }

        public void AddBlock(Block block)
        {
            BlockChain.Add(block);
        }
    }

    // Базовый класс для пользователя
    class User
    {
        static Random random = new Random();
        protected static int user_id = 0;
        protected int initiator;
        protected int closed_key;
        protected int open_key;
        protected float chance_to_generate_transaction = 0.15f;

        public User() // Конструктор типа 
        {
            initiator = user_id;
            closed_key = user_id.GetHashCode();
            open_key = user_id.GetHashCode();
            user_id++;
        }

        public void SetChance_to_generate_transaction(Operations net)
        {
            if (net.TallyWalletByUser(this) > 10 )
            {
                chance_to_generate_transaction = 0.4f;
            }
        }

        public void UserInfo()
        {
            Console.WriteLine("initiator = " + initiator.ToString() + " closed_key = " + closed_key + " open_key = " + open_key.ToString() + " user_id = " + user_id.ToString());
        }

        public void UserStep()
        {
            if ((float)random.NextDouble() < chance_to_generate_transaction)
            {
                InitializeRandomTransaction();
            }
        }
        public Operations.Transaction InitializeRandomTransaction() // Метод формирующий заявку на транзакцию
        {
            Operations.Transaction trans = new Operations.Transaction();
            int target = random.Next(0, user_id);
            trans.Init(initiator, target, open_key, (int) random.Next(1, 100));
            trans.TransactionInfo();
            return trans;
        }

        // GETTERS
        public int Initiator
        {
            get { return initiator; }
        }
        public int Closed_Key
        {
            get { return closed_key; }
        }
        public int Open_Key
        {
            get { return open_key; }
        }
        //


    }

    // Класс Miner, наследник User
    class Miner:User
    {
        // Пока что пул транзакций общий для всех, потом сделать поиск только по соседним нодам

        private float performance; // 0f до 1f

        // Неплохо бы добавить proof-of-work
        public void CatchTransaction(Operations net, Operations.Transaction trans)
        {
            int tmp = net.TallyWallet(trans.Initiator);

            if (tmp < trans.Coin || tmp <= net.Reward)
            {
                Console.WriteLine("Недостаточно средств, User " + trans.Initiator + " имеет: " + tmp + " хочет перевести: " + trans.Coin);
                return;
            }
            else
            {
                Console.WriteLine("Достаточно средств, User " + trans.Initiator + " имеет: " + tmp + " хочет перевести: " + trans.Coin);

                // Время на исполнение

                // Добавляем блок по запросу
                net.AddBlock(new Operations.Block
                {
                    transaction = trans,
                    subscribe = this.Open_Key,
                    time = DateTime.Now
                });
                // Добавляем блок с переводом комиссии себе
                Operations.Transaction comission = new Operations.Transaction();
                comission.Init(trans.Initiator, this.Initiator, this.Open_Key, net.Reward);

                net.AddBlock(new Operations.Block
                {
                    transaction = comission,
                    subscribe = comission.Open_Key,
                    time = DateTime.Now
                });

                // Добавляем блок с остатком инициатору
                Operations.Transaction returntransaction = new Operations.Transaction();
                returntransaction.Init(trans.Initiator, trans.Initiator, this.Open_Key, tmp - trans.Coin - comission.Coin);

                net.AddBlock(new Operations.Block
                {
                    transaction = returntransaction,
                    subscribe = returntransaction.Open_Key,
                    time = DateTime.Now
                });

                Console.WriteLine("User " + trans.Initiator + " Перевел to User " + trans.Target + " " + trans.Coin + " Монет ");
                Console.WriteLine("User " + comission.Initiator + " Комиссия майнеру " + comission.Target + " " + comission.Coin + " Монет ");
                Console.WriteLine("User " + returntransaction.Initiator + " Вернул себе " + " " + returntransaction.Coin + " Монет ");

            }

        }
    }



    class Program
    {
        static void Main()
        {
            int number_of_users = 10;
            int number_of_miners = 3;


            Operations net = new Operations(); // Инициализируем сеть

            for (int i = 0; i < number_of_users; i++) // Создаем пользователей
            {
                User tmp = new User();
                tmp.SetChance_to_generate_transaction(net);
                net.users.Add(tmp);
            }
            for (int i = 0; i < number_of_miners; i++) // Создаем майнеров
            {
                Miner tmp = new Miner();
                tmp.SetChance_to_generate_transaction(net);
                net.miners.Add(tmp);
            }

            Console.WriteLine("################################### INIT COMPLETED ###################################");

            // Первая транзакция
            Operations.Transaction initialTransaction = new Operations.Transaction();
            initialTransaction.Init(-1, 0, 0, 50); // Транзакция пришла от виртуального -1-го пользователя

            // Первый блок
            net.AddBlock(new Operations.Block() { transaction = initialTransaction,
                subscribe = 0, time = DateTime.Now });

            Console.WriteLine("USERS DATA: ");

            foreach (User user in net.users)
            {
                user.UserInfo();
            }

            Console.WriteLine(" ");
            Console.WriteLine("MINERS DATA: ");

            foreach (Miner miner in net.miners)
            {
                miner.UserInfo();
            }


            // Примитивный цикл для имитации процесса добавления транзакций и их исполнения


            for (int i =0; i<1; i++)
            {
                //System.Threading.Thread.Sleep(0);
                Console.WriteLine();
                Console.WriteLine("Cycle " + i.ToString());


                net.AddToPool(net.users[0].InitializeRandomTransaction()); // Добавляем в пул транзакцию


                if (net.TransactionsPool.Count > 0)
                {
                    foreach (Operations.Transaction trans in net.TransactionsPool)
                    {
                        net.miners[0].CatchTransaction(net, trans);
                    }
                }

                Console.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$");


                Console.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$");

                net.DeleteTransactions();
            }


            Console.ReadKey();
        }
    }
}
