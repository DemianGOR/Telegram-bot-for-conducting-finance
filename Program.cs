using SerGOFinance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.Collections;
using Telegram.Bot.Types.ReplyMarkups;

namespace SerGOFinance
{
   
        class Program
        {
         
            public static bool ZpBlyat;
            public static bool Withdrow;
            public static bool AddCategory;
            public static int buffer;
            
        public static bool Category1;


        public static Category CB;
            public static async  Task Main(string[] args)
            {
                int ClientMoney;


                TelegramBotClient client = new TelegramBotClient("1510877399:AAH3ESE7PkXdjkFjCzUkOcQ1_KNQZtT1rfk");

                client.OnMessage += async (object sender, MessageEventArgs message) =>
                {


                    // СПИСКИ ПО БАЗЕ ДАННЫХ______________________________________________________________________________________________________________________

                    ApplicationContext db = new ApplicationContext();
                    List<Outcomes> outcomess = new List<Outcomes>();
                    List<Incomes> incomess = new List<Incomes>();
                    var users = db.Users.ToList();

                    long chatId = message.Message.Chat.Id;

                    if (message.Message.Text == "/start")
                    {
                        await client.SendTextMessageAsync(chatId, "Введите команду /rbuttons для начала работы с Ботом,а после нажмите мой ID");
                      
                    }
                    //СОЗДАНИЕ ЮЗЕРА______________________________________________________________________________________________________________________

                    if (message.Message.Text == "Мой ID")
                    {
                        await client.SendTextMessageAsync(chatId, $"Ваш чат ID = {chatId}");


                        if (!users.Exists(x => x.Id == chatId))
                        {
                            User newUser = new User { Id = Convert.ToInt32(chatId), Balance = 0 };
                            db.Users.Add(newUser);
                            users.Add(newUser);

                            db.SaveChanges();
                            foreach (User u in users)
                            {
                                Console.WriteLine($"{u.Id}. {u.Balance}");
                            }
                        }
                        else
                        {
                            await client.SendTextMessageAsync(chatId, "Вы уже внесены в базу данных нашего бота");

                        }



                    }
                    //ПРОВЕРКА БАЛАНСА______________________________________________________________________________________________________________________

                    if (message.Message.Text == "Баланс")
                    {
                        User curUser = users.Where(x => x.Id == chatId).FirstOrDefault();

                        await client.SendTextMessageAsync(chatId, $"на счету {curUser.Balance} грн.");
                        return;
                    }
                 /*   if (message.Message.Text == "Создать категорию")
                    {
                       
                        await client.SendTextMessageAsync(chatId, "Введите имя категории");
                      
                        
                        Category1 = true; 
                       
                    }
                    if (Category1 && message.Message.Text != "Создать категорию")
                    {
                        var categories = new List<Category>();


                        
                         categories = db.Category.ToList();

                        if (categories.Exists(x => x.Name == message.Message.Text.ToString().ToLower()))
                        {

                            CB = categories.Where(x => x.Name == message.Message.Text.ToString().ToLower()).FirstOrDefault();
                        }
                        else
                        {
                            CB = new Category { Name = message.Message.Text.ToString().ToLower(), UserId= Convert.ToInt32(chatId) };

                            db.Category.Add(CB);

                            categories.Add(CB);

                            db.SaveChanges();
                        }


                        Category1 = false;
             
                        
                    }*/
                        // ДЕПОЗИТ ДЕНЕГ НА СЧЕТ______________________________________________________________________________________________________________________

                        if (message.Message.Text == "Депозит средств на счет")
                        {

                        ZpBlyat = true;
                        Withdrow = false;
                        await client.SendTextMessageAsync(chatId, "Введите сумму депозита");
                        return;
                    }
                    if (ZpBlyat && Int32.TryParse(message.Message.Text, out ClientMoney))
                    {

                        


                       
                        buffer = ClientMoney;
                        AddCategory = true;

                        var categories = new List<Category>();


                        categories = db.Category.ToList();
                        incomess = db.Incomes.ToList();

                        var result = from c in categories
                                     
                                     join u in users on c.UserId equals chatId

                                     select new { Name = c.Name ,UserId =chatId};

                        result = result.ToList().Distinct();
                        List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>(); // Создаём массив колонок
                         
                        foreach (var item in result)
                        { // Можно использовать и foreach
                            InlineKeyboardButton button1 = new InlineKeyboardButton() { CallbackData = $"{item.Name}", Text = item.Name.ToString() };//Создаём кнопку
                            InlineKeyboardButton[] row1 = new InlineKeyboardButton[1] { button1 }; // Создаём массив кнопок,в нашем случае он будет из одного элемента
                            list.Add(row1);//И добавляем его
                        }
                        InlineKeyboardButton button = new InlineKeyboardButton() { CallbackData = "Создать категорию", Text = "Создать категорию" };
                        InlineKeyboardButton[] row = new InlineKeyboardButton[1] { button };
                        list.Add(row);



                        var inline = new InlineKeyboardMarkup(list);//создаём клавиатуру
                        await client.SendTextMessageAsync(message.Message.Chat.Id, "Выбирете категорию доходов", replyMarkup: inline);//отправка
                        async void CallBackHandler(object sc, Telegram.Bot.Args.CallbackQueryEventArgs ev)
                        {
                            if (ev.CallbackQuery.Data == "Создать категорию")
                            {
                                await client.SendTextMessageAsync(chatId, "Введите имя категории");


                                Category1 = true;

                            }
                            


                                
                            
                                foreach (var item in result)
                            {
                                var message = ev.CallbackQuery.Message;
                                if (ev.CallbackQuery.Data == $"{item.Name}")
                                {
                                    var curUser = users.Where(x => x.Id == chatId).FirstOrDefault();
                                    curUser.Balance += buffer;

                                    var categories = db.Category.ToList();

                                    if (categories.Exists(x => x.Name == ev.CallbackQuery.Data.ToString().ToLower()))
                                    {

                                        CB = categories.Where(x => x.Name == ev.CallbackQuery.Data.ToString().ToLower()).FirstOrDefault();
                                    }
                                    else
                                    {
                                        CB = new Category { Name = ev.CallbackQuery.Data.ToString().ToLower() };

                                        db.Category.Add(CB);
                                    }

                                    Incomes income = new Incomes { Amount = buffer, Date = DateTime.Now, User = curUser, Category = CB };
                                    await client.SendTextMessageAsync(chatId, $"теперь на счету {curUser.Balance} грн.");
                                    db.Incomes.Add(income);
                                    incomess.Add(income);
                                    db.Update(curUser);
                                    db.SaveChanges();
                                    AddCategory = false;
                                    client.OnCallbackQuery -= CallBackHandler;
                                    return;
                                }


                            }
                            client.OnCallbackQuery -= CallBackHandler;
                        };
                        
                            client.OnCallbackQuery += CallBackHandler;
                       
                    }
                    if (Category1 && message.Message.Text != buffer.ToString())
                    {
                        var categories = new List<Category>();
                        var curUser = users.Where(x => x.Id == chatId).FirstOrDefault();
                        curUser.Balance += buffer;

                        categories = db.Category.ToList();

                        if (categories.Exists(x => x.Name == message.Message.Text.ToString().ToLower()))
                        {

                            CB = categories.Where(x => x.Name == message.Message.Text.ToString().ToLower()).FirstOrDefault();
                            Category1 = false;
                        }
                        else
                        {
                            CB = new Category { Name = message.Message.Text.ToString().ToLower(), UserId = Convert.ToInt32(chatId) };

                            db.Category.Add(CB);

                            categories.Add(CB);

                            Incomes income = new Incomes { Amount = buffer, Date = DateTime.Now, User = curUser, Category = CB };
                            await client.SendTextMessageAsync(chatId, $"теперь на счету {curUser.Balance} грн.");
                            db.Incomes.Add(income);
                            incomess.Add(income);
                            db.Update(curUser);
                            db.SaveChanges();
                            AddCategory = false;
                            Category1 = false;
                           
                            return;
                        }
                    }
                    //СНЯТИЕ ДЕНЕГ СО СЧЕТА______________________________________________________________________________________________________________________

                    if (message.Message.Text == "Снятие средств")
                    {
                        Withdrow = true;
                        ZpBlyat = false;
                        await client.SendTextMessageAsync(chatId, "Введите сумму для снятия");
                        return;
                    }
                    
                    if (Withdrow && Int32.TryParse(message.Message.Text, out ClientMoney))
                    {
                        buffer = ClientMoney;
                        AddCategory = true;

                        var categories = new List<Category>();


                        categories = db.Category.ToList();
                        outcomess = db.Outcomes.ToList();

                        var result = from c in categories

                                     join u in users on c.UserId equals chatId
                                     join o in outcomess on c.Id equals o.Category.Id
                                     select new { Name = c.Name, UserId = chatId };

                        result = result.ToList().Distinct();
                        List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>(); // Создаём массив колонок

                        foreach (var item in result)
                        { // Можно использовать и foreach
                            InlineKeyboardButton button1 = new InlineKeyboardButton() { CallbackData = $"{item.Name}", Text = item.Name.ToString() };//Создаём кнопку
                            InlineKeyboardButton[] row1 = new InlineKeyboardButton[1] { button1 }; // Создаём массив кнопок,в нашем случае он будет из одного элемента
                            list.Add(row1);//И добавляем его
                        }
                        InlineKeyboardButton button = new InlineKeyboardButton() { CallbackData = "Создать категорию", Text = "Создать категорию" };
                        InlineKeyboardButton[] row = new InlineKeyboardButton[1] { button };
                        list.Add(row);



                        var inline = new InlineKeyboardMarkup(list);//создаём клавиатуру
                        await client.SendTextMessageAsync(message.Message.Chat.Id, "Выбирете категорию расходов", replyMarkup: inline);//отправка
                        async void CallBackHandler(object sc, Telegram.Bot.Args.CallbackQueryEventArgs ev)
                        {
                            if (ev.CallbackQuery.Data == "Создать категорию")
                            {
                                await client.SendTextMessageAsync(chatId, "Введите имя категории");


                                Category1 = true;

                            }





                            foreach (var item in result)
                            {
                                var message = ev.CallbackQuery.Message;
                                if (ev.CallbackQuery.Data == $"{item.Name}")
                                {
                                    var curUser = users.Where(x => x.Id == chatId).FirstOrDefault();
                                    curUser.Balance -= buffer;

                                    var categories = db.Category.ToList();

                                    if (categories.Exists(x => x.Name == ev.CallbackQuery.Data.ToString().ToLower()))
                                    {

                                        CB = categories.Where(x => x.Name == ev.CallbackQuery.Data.ToString().ToLower()).FirstOrDefault();
                                    }
                                    else
                                    {
                                        CB = new Category { Name = ev.CallbackQuery.Data.ToString().ToLower() };

                                        db.Category.Add(CB);
                                    }

                                    Outcomes outcome = new Outcomes { Amount = buffer, Date = DateTime.Now, User = curUser, Category = CB };
                                    await client.SendTextMessageAsync(chatId, $"теперь на счету {curUser.Balance} грн.");
                                    db.Outcomes.Add(outcome);
                                    outcomess.Add(outcome);
                                    db.Update(curUser);
                                    db.SaveChanges();
                                    AddCategory = false;
                                    client.OnCallbackQuery -= CallBackHandler;
                                    return;
                                }


                            }
                            client.OnCallbackQuery -= CallBackHandler;
                        };

                        client.OnCallbackQuery += CallBackHandler;

                    }
                    if (Category1 && message.Message.Text != buffer.ToString())
                    {
                        var categories = new List<Category>();
                        var curUser = users.Where(x => x.Id == chatId).FirstOrDefault();
                        curUser.Balance -= buffer;

                        categories = db.Category.ToList();

                        if (categories.Exists(x => x.Name == message.Message.Text.ToString().ToLower()))
                        {

                            CB = categories.Where(x => x.Name == message.Message.Text.ToString().ToLower()).FirstOrDefault();
                            Category1 = false;
                        }
                        else
                        {
                            CB = new Category { Name = message.Message.Text.ToString().ToLower(), UserId = Convert.ToInt32(chatId) };

                            db.Category.Add(CB);

                            categories.Add(CB);

                            Outcomes outcame = new Outcomes { Amount = buffer, Date = DateTime.Now, User = curUser, Category = CB };
                            await client.SendTextMessageAsync(chatId, $"теперь на счету {curUser.Balance} грн.");
                            db.Outcomes.Add(outcame);
                            outcomess.Add(outcame);
                            db.Update(curUser);
                            db.SaveChanges();
                            AddCategory = false;
                            Category1 = false;

                            return;
                        }
                    }

                    // МЕСЯЧНЫЙ ОТЧЕТ ПО ДОХОДАМ______________________________________________________________________________________________________________________

                    if (message.Message.Text == "Отчет за месяц по Доходам")
                    {
                        DateTime now = DateTime.Now;
                        DateTime first = new DateTime(now.Year, now.Month, 1);
                        DateTime last = new DateTime(now.Year, now.Month + 1, 1).AddDays(-1);

                        var categories = new List<Category>();


                        categories = db.Category.ToList();
                        incomess = db.Incomes.ToList();

                        var result = from c in categories
                                     join o in incomess on c.Id equals o.Category.Id
                                     join u in users on o.User.Id equals chatId

                                     select new { Name = c.Name, Amount = o.Amount, Date = o.Date };



                        var curElem = result.Where(x => x.Date.Month == last.Month).FirstOrDefault();
                        (string Name, int Amount, DateTime Date) FB = (curElem.Name, curElem.Amount, curElem.Date);
                        result.ToList().Remove(curElem);

                        var listToSend = new List<(string Name, int Amount, DateTime Date)>();
                        var curName = curElem.Name;
                        var names = new List<string>();
                        var noduplicates1 = result.ToList().Distinct();
                        foreach (var item in noduplicates1)
                        {
                            if (first.Month > item.Date.Month && last.Month > item.Date.Month)
                            {

                                noduplicates1.ToList().Remove(item);

                            }
                        }

                        names.Add(curName);
                        listToSend.Add((curElem.Name, curElem.Amount, curElem.Date));

                        foreach (var item in noduplicates1)
                        {

                            if (first.Month == item.Date.Month && curName != item.Name && !names.Exists(x => x == item.Name))
                            {

                                var toAd = (item.Name, item.Amount, item.Date);
                                listToSend.Add(toAd);

                                curName = item.Name;
                                names.Add(item.Name);
                            }
                            else if (first.Month == item.Date.Month)
                            {

                                var curItem = listToSend.Where(x => x.Name == item.Name).LastOrDefault();

                                listToSend.Remove(curItem);


                                curItem.Amount += item.Amount;

                                listToSend.Add(curItem);


                            }


                        }
                        FB.Amount = listToSend[0].Amount - FB.Amount;
                        listToSend[0] = FB;

                        foreach (var item in listToSend)
                        {


                            await client.SendTextMessageAsync(chatId, $"{item.Amount  } - {item.Name}");

                        }


                    }

                    // ОТЧЕТ ЗА ВСЕ ВРЕМЯ ПО ДОХОДАМ______________________________________________________________________________________________________________________

                    if (message.Message.Text == "Отчет за все время по Доходам")
                    {
                        var categories = new List<Category>();

                        categories = db.Category.ToList();
                        incomess = db.Incomes.ToList();

                        var result = from c in categories
                                     join o in incomess on c.Id equals o.Category.Id
                                     join u in users on o.User.Id equals chatId
                                     select new { Name = c.Name, Amount = o.Amount };


                        var curElem = result.FirstOrDefault();
                        (string Name, int Amount) FB = (curElem.Name, curElem.Amount);
                        result.ToList().Remove(curElem);

                        var listToSend = new List<(string Name, int Amount)>();
                        var curName = curElem.Name;
                        var names = new List<string>();

                        names.Add(curName);
                        listToSend.Add((curElem.Name, curElem.Amount));

                        foreach (var item in result)
                        {
                            if (curName != item.Name && !names.Exists(x => x == item.Name))
                            {

                                var toAd = (item.Name, item.Amount);
                                listToSend.Add(toAd);

                                curName = item.Name;
                                names.Add(item.Name);
                            }
                            else
                            {

                                var curItem = listToSend.Where(x => x.Name == item.Name).LastOrDefault();
                                listToSend.Remove(curItem);

                                curItem.Amount += item.Amount;
                                listToSend.Add(curItem);



                            }


                        }
                        FB.Amount = listToSend[0].Amount - FB.Amount;
                        listToSend[0] = FB;

                        foreach (var item in listToSend)
                        {
                            await client.SendTextMessageAsync(chatId, $"{item.Amount } - {item.Name}");
                        }


                    }
                    // ОТЧЕТ ЗА ВСЕ ВРЕМЯ ПО РАСХОДАМ______________________________________________________________________________________________________________________

                    if (message.Message.Text == "Отчет за все время по Расходам")
                    {
                        var categories = new List<Category>();

                        categories = db.Category.ToList();
                        outcomess = db.Outcomes.ToList();

                        var result = from c in categories
                                     join o in outcomess on c.Id equals o.Category.Id 
                                     join u in users on o.User.Id equals chatId
                                     select new { Name=c.Name, Amount = o.Amount };


                        var curElem = result.FirstOrDefault();
                        (string Name, int Amount) FB = (curElem.Name, curElem.Amount);
                        result.ToList().Remove(curElem);

                        var listToSend = new List<(string Name, int Amount)>();
                        var curName = curElem.Name;
                        var names = new List<string>();

                        names.Add(curName);
                        listToSend.Add((curElem.Name, curElem.Amount));
                        
                        foreach ( var item in result)
                        {
                            if(curName != item.Name && !names.Exists(x=>x==item.Name))
                            {

                                var toAd = (item.Name, item.Amount);
                                listToSend.Add(toAd);

                                curName=item.Name;
                                names.Add(item.Name);
                            }
                            else 
                            {

                                    var curItem = listToSend.Where(x => x.Name == item.Name).LastOrDefault();
                                    listToSend.Remove(curItem);

                                    curItem.Amount += item.Amount;
                                    listToSend.Add(curItem);
                                

                                
                            }
                               
                            
                        }
                        FB.Amount = listToSend[0].Amount - FB.Amount;
                        listToSend[0] = FB;

                        foreach(var item in listToSend)
                        {
                            await client.SendTextMessageAsync(chatId, $"{item.Amount} - {item.Name}");
                        }


                    }
                    // МЕСЯЧНЫЙ ОТЧЕТ ПО РАСХОДАМ______________________________________________________________________________________________________________________

                    if (message.Message.Text == "Отчет за месяц по Расходам")
                    {
                        DateTime now = DateTime.Now;
                        DateTime first = new DateTime(now.Year, now.Month, 1);
                        DateTime last = new DateTime(now.Year, now.Month + 1, 1).AddDays(-1);

                        var categories = new List<Category>();
                      

                        categories = db.Category.ToList();
                        outcomess = db.Outcomes.ToList();

                       var  result = from c in categories
                                     join o in outcomess on c.Id equals o.Category.Id
                                     join u in users on o.User.Id equals chatId
                                    
                                     select new { Name = c.Name, Amount = o.Amount , Date = o.Date};

                      

                        var curElem = result.Where(x=>x.Date.Month == last.Month).FirstOrDefault();
                        (string Name, int Amount, DateTime Date) FB = (curElem.Name, curElem.Amount ,curElem.Date);
                        result.ToList().Remove(curElem);

                        var listToSend = new List<(string Name, int Amount, DateTime Date)>();
                        var curName = curElem.Name;
                        var names = new List<string>();
                        var noduplicates1 = result.ToList().Distinct();
                        foreach (var item in noduplicates1)
                        {
                            if(first.Month > item.Date.Month && last.Month > item.Date.Month)
                            {

                                noduplicates1.ToList().Remove(item);

                            }
                        }
                       
                        names.Add(curName);
                        listToSend.Add((curElem.Name, curElem.Amount, curElem.Date));

                        foreach (var item in noduplicates1)
                        {

                            if (first.Month==item.Date.Month && curName != item.Name && !names.Exists(x => x == item.Name))
                            {

                                var toAd = (item.Name, item.Amount, item.Date);
                                listToSend.Add(toAd);

                                curName = item.Name;
                                names.Add(item.Name);
                            }
                           else if(first.Month==item.Date.Month)
                           {
                               
                                    var curItem = listToSend.Where(x => x.Name == item.Name).LastOrDefault();
                                    
                                    listToSend.Remove(curItem);
                               

                                    curItem.Amount += item.Amount;
                                
                                    listToSend.Add(curItem);


                           }


                        }
                        FB.Amount = listToSend[0].Amount - FB.Amount;
                        listToSend[0] = FB;
                        
                        foreach (var item in listToSend)
                        {

                            
                            await client.SendTextMessageAsync(chatId, $"{item.Amount  } - {item.Name}");
                            
                        }


                    }

                    //КНОПОЧНОЕ МЕНЮ______________________________________________________________________________________________________________________

                    if (message.Message.Text == "/rbuttons" || message.Message.Text == "Перейти в Главное меню")
                    {
                      
                        var keyboard = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                        {
                            Keyboard = new[] {
                                                new[] // row 1
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("Расходы"),
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("Доходы"),

                                                },
                                                new[] // row 2
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("Баланс"),
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("Мой ID"),
                                                  

                                                },
                                                
                                            },
                            ResizeKeyboard = true

                        };


                        await client.SendTextMessageAsync(message.Message.Chat.Id, "Вы в главном меню!", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboard);
                    }
                    // обработка reply кнопок
                    if (message.Message.Text == "Расходы")
                    {
                        var keyboard1 = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                        {
                            Keyboard = new[] {
                                                new[] // row 1
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("Снятие средств "),
                                                   

                                                },
                                                new[] // row 2
                                                {
                                                     new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("Отчет за месяц по Расходам"),
                                                     new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("Отчет за все время по Расходам"),

                                                },
                                                new[] // row 2
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("Перейти в Главное меню"),


                                                },
                                            },
                            ResizeKeyboard = true

                        };


                        await client.SendTextMessageAsync(message.Message.Chat.Id, "Вы вошли в категорию Расходы", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboard1);
                    }
                    if (message.Message.Text == "Доходы")
                    {
                        var keyboard1 = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                        {
                            Keyboard = new[] {
                                                new[] // row 1
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("Депозит средств на счет"),


                                                },
                                                new[] // row 2
                                                {
                                                     new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("Отчет за месяц по Доходам"),
                                                     new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("Отчет за все время по Доходам"),

                                                },
                                                new[] // row 2
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("Перейти в Главное меню"),


                                                },
                                            },
                            ResizeKeyboard = true

                        };


                        await client.SendTextMessageAsync(message.Message.Chat.Id, "Вы вошли в категорию Доходы", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboard1);
                    }



                };

                client.StartReceiving();
                Console.ReadLine();


            }
        }
    }
