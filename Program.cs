using System;
using System.Collections.Generic;
using System.Linq;

// Паттерн: Singleton для глобального логгера
public sealed class Logger
{
    private static readonly Logger instance = new Logger();
    private readonly List<string> logs = new List<string>();

    static Logger() { }
    private Logger() { }

    public static Logger Instance => instance;

    public void Log(string message)
    {
        logs.Add($"{DateTime.Now}: {message}");
        Console.WriteLine($"ЛОГ: {message}");
    }

    public void ShowLogs()
    {
        Console.WriteLine("\n=== Журнал событий ===");
        foreach (var log in logs)
        {
            Console.WriteLine(log);
        }
    }
}

// Интерфейс для транзакций
public interface ITransaction
{
    decimal Amount { get; }
    string Description { get; }
    void Execute();
    void Undo();
}

// Базовый класс транзакции
public abstract class Transaction : ITransaction
{
    public decimal Amount { get; protected set; }
    public string Description { get; protected set; }
    public string Status { get; protected set; } = "Создана";

    public abstract void Execute();
    public abstract void Undo();

    // Паттерн: State для управления статусом транзакции
    public void ChangeStatus(string newStatus)
    {
        Status = newStatus;
        Logger.Instance.Log($"Транзакция '{Description}': статус изменен на '{newStatus}'");
    }
}

// Конкретные типы транзакций
public class PaymentTransaction : Transaction
{
    public string Recipient { get; }

    public PaymentTransaction(decimal amount, string recipient, string description)
    {
        Amount = amount;
        Recipient = recipient;
        Description = description;
    }

    public override void Execute()
    {
        ChangeStatus("В процессе");
        Logger.Instance.Log($"Платеж на сумму {Amount} для {Recipient}: {Description}");
        // Логика выполнения платежа...
        ChangeStatus("Завершена");
    }

    public override void Undo()
    {
        ChangeStatus("Отмена");
        Logger.Instance.Log($"Отмена платежа на сумму {Amount} для {Recipient}");
        // Логика отмены платежа...
        ChangeStatus("Отменена");
    }
}

public class TransferTransaction : Transaction
{
    public string FromAccount { get; }
    public string ToAccount { get; }

    public TransferTransaction(decimal amount, string fromAccount, string toAccount, string description)
    {
        Amount = amount;
        FromAccount = fromAccount;
        ToAccount = toAccount;
        Description = description;
    }

    public override void Execute()
    {
        ChangeStatus("В процессе");
        Logger.Instance.Log($"Перевод {Amount} с {FromAccount} на {ToAccount}: {Description}");
        // Логика выполнения перевода...
        ChangeStatus("Завершена");
    }

    public override void Undo()
    {
        ChangeStatus("Отмена");
        Logger.Instance.Log($"Отмена перевода {Amount} с {FromAccount} на {ToAccount}");
        // Логика отмены перевода...
        ChangeStatus("Отменена");
    }
}

public class DepositTransaction : Transaction
{
    public string Account { get; }

    public DepositTransaction(decimal amount, string account, string description)
    {
        Amount = amount;
        Account = account;
        Description = description;
    }

    public override void Execute()
    {
        ChangeStatus("В процессе");
        Logger.Instance.Log($"Пополнение {Account} на {Amount}: {Description}");
        // Логика выполнения пополнения...
        ChangeStatus("Завершена");
    }

    public override void Undo()
    {
        ChangeStatus("Отмена");
        Logger.Instance.Log($"Отмена пополнения {Account} на {Amount}");
        // Логика отмены пополнения...
        ChangeStatus("Отменена");
    }
}

// Паттерн: Strategy для расчета комиссий
public interface IFeeStrategy
{
    decimal CalculateFee(decimal amount);
}

public class PercentageFeeStrategy : IFeeStrategy
{
    private readonly decimal percentage;

    public PercentageFeeStrategy(decimal percentage)
    {
        this.percentage = percentage;
    }

    public decimal CalculateFee(decimal amount)
    {
        return amount * percentage / 100;
    }
}

public class FixedFeeStrategy : IFeeStrategy
{
    private readonly decimal fixedAmount;

    public FixedFeeStrategy(decimal fixedAmount)
    {
        this.fixedAmount = fixedAmount;
    }

    public decimal CalculateFee(decimal amount)
    {
        return fixedAmount;
    }
}

// Паттерн: Decorator для добавления логирования и кэширования
public class LoggingTransactionDecorator : ITransaction
{
    private readonly ITransaction transaction;

    public decimal Amount => transaction.Amount;
    public string Description => transaction.Description;

    public LoggingTransactionDecorator(ITransaction transaction)
    {
        this.transaction = transaction;
    }

    public void Execute()
    {
        Logger.Instance.Log($"Начало выполнения: {Description}");
        transaction.Execute();
        Logger.Instance.Log($"Завершение выполнения: {Description}");
    }

    public void Undo()
    {
        Logger.Instance.Log($"Начало отмены: {Description}");
        transaction.Undo();
        Logger.Instance.Log($"Завершение отмены: {Description}");
    }
}

// Паттерн: Observer для уведомлений
public interface ITransactionNotifier
{
    void Attach(ITransactionObserver observer);
    void Detach(ITransactionObserver observer);
    void Notify(ITransaction transaction, string message);
}

public interface ITransactionObserver
{
    void Update(ITransaction transaction, string message);
}

public class TransactionNotifier : ITransactionNotifier
{
    private readonly List<ITransactionObserver> observers = new List<ITransactionObserver>();

    public void Attach(ITransactionObserver observer)
    {
        observers.Add(observer);
    }

    public void Detach(ITransactionObserver observer)
    {
        observers.Remove(observer);
    }

    public void Notify(ITransaction transaction, string message)
    {
        foreach (var observer in observers)
        {
            observer.Update(transaction, message);
        }
    }
}

public class EmailNotification : ITransactionObserver
{
    public void Update(ITransaction transaction, string message)
    {
        Console.WriteLine($"Email: {message} по транзакции '{transaction.Description}'");
    }
}

public class SmsNotification : ITransactionObserver
{
    public void Update(ITransaction transaction, string message)
    {
        Console.WriteLine($"SMS: {message} по транзакции '{transaction.Description}'");
    }
}

// Паттерн: Command для отмены/повтора операций
public interface ITransactionCommand
{
    void Execute();
    void Undo();
}

public class TransactionCommand : ITransactionCommand
{
    private readonly ITransaction transaction;

    public TransactionCommand(ITransaction transaction)
    {
        this.transaction = transaction;
    }

    public void Execute()
    {
        transaction.Execute();
    }

    public void Undo()
    {
        transaction.Undo();
    }
}

// Паттерн: Facade для упрощения клиентского кода
public class BankingSystemFacade
{
    private readonly TransactionNotifier notifier = new TransactionNotifier();
    private readonly Stack<ITransactionCommand> executedCommands = new Stack<ITransactionCommand>();
    private readonly Stack<ITransactionCommand> undoneCommands = new Stack<ITransactionCommand>();
    private IFeeStrategy feeStrategy = new PercentageFeeStrategy(1.0m);

    public BankingSystemFacade()
    {
        // Добавляем наблюдателей по умолчанию
        notifier.Attach(new EmailNotification());
        notifier.Attach(new SmsNotification());
    }

    public void SetFeeStrategy(IFeeStrategy strategy)
    {
        feeStrategy = strategy;
    }

    public void ProcessTransaction(ITransaction transaction)
    {
        var decoratedTransaction = new LoggingTransactionDecorator(transaction);
        var command = new TransactionCommand(decoratedTransaction);

        // Рассчитываем комиссию
        decimal fee = feeStrategy.CalculateFee(transaction.Amount);
        Logger.Instance.Log($"Комиссия за транзакцию: {fee}");

        // Выполняем команду
        command.Execute();
        executedCommands.Push(command);

        // Уведомляем наблюдателей
        notifier.Notify(transaction, $"Транзакция выполнена. Сумма: {transaction.Amount}, комиссия: {fee}");
    }

    public void UndoLastTransaction()
    {
        if (executedCommands.Count == 0)
        {
            Console.WriteLine("Нет выполненных транзакций для отмены.");
            return;
        }

        var command = executedCommands.Pop();
        command.Undo();
        undoneCommands.Push(command);
    }

    public void RedoLastUndoneTransaction()
    {
        if (undoneCommands.Count == 0)
        {
            Console.WriteLine("Нет отмененных транзакций для повтора.");
            return;
        }

        var command = undoneCommands.Pop();
        command.Execute();
        executedCommands.Push(command);
    }
}

// Главный класс программы с меню
class Program
{
    static void Main(string[] args)
    {
        var bankingSystem = new BankingSystemFacade();

        while (true)
        {
            Console.WriteLine("\n=== Умная система обработки транзакций ===");
            Console.WriteLine("1. Создать платеж");
            Console.WriteLine("2. Создать перевод");
            Console.WriteLine("3. Создать пополнение");
            Console.WriteLine("4. Отменить последнюю транзакцию");
            Console.WriteLine("5. Повторить отмененную транзакцию");
            Console.WriteLine("6. Изменить стратегию комиссий");
            Console.WriteLine("7. Показать журнал событий");
            Console.WriteLine("8. Выход");
            Console.Write("Выберите действие: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    CreatePayment(bankingSystem);
                    break;
                case "2":
                    CreateTransfer(bankingSystem);
                    break;
                case "3":
                    CreateDeposit(bankingSystem);
                    break;
                case "4":
                    bankingSystem.UndoLastTransaction();
                    break;
                case "5":
                    bankingSystem.RedoLastUndoneTransaction();
                    break;
                case "6":
                    ChangeFeeStrategy(bankingSystem);
                    break;
                case "7":
                    Logger.Instance.ShowLogs();
                    break;
                case "8":
                    return;
                default:
                    Console.WriteLine("Неверный выбор. Попробуйте снова.");
                    break;
            }
        }
    }

    static void CreatePayment(BankingSystemFacade bankingSystem)
    {
        Console.Write("Введите сумму платежа: ");
        decimal amount = decimal.Parse(Console.ReadLine());
        Console.Write("Введите получателя: ");
        string recipient = Console.ReadLine();
        Console.Write("Введите описание: ");
        string description = Console.ReadLine();

        var transaction = new PaymentTransaction(amount, recipient, description);
        bankingSystem.ProcessTransaction(transaction);
    }

    static void CreateTransfer(BankingSystemFacade bankingSystem)
    {
        Console.Write("Введите сумму перевода: ");
        decimal amount = decimal.Parse(Console.ReadLine());
        Console.Write("Введите счет отправителя: ");
        string from = Console.ReadLine();
        Console.Write("Введите счет получателя: ");
        string to = Console.ReadLine();
        Console.Write("Введите описание: ");
        string description = Console.ReadLine();

        var transaction = new TransferTransaction(amount, from, to, description);
        bankingSystem.ProcessTransaction(transaction);
    }

    static void CreateDeposit(BankingSystemFacade bankingSystem)
    {
        Console.Write("Введите сумму пополнения: ");
        decimal amount = decimal.Parse(Console.ReadLine());
        Console.Write("Введите счет для пополнения: ");
        string account = Console.ReadLine();
        Console.Write("Введите описание: ");
        string description = Console.ReadLine();

        var transaction = new DepositTransaction(amount, account, description);
        bankingSystem.ProcessTransaction(transaction);
    }

    static void ChangeFeeStrategy(BankingSystemFacade bankingSystem)
    {
        Console.WriteLine("Выберите стратегию комиссий:");
        Console.WriteLine("1. Процентная комиссия (1.5%)");
        Console.WriteLine("2. Фиксированная комиссия (10)");
        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                bankingSystem.SetFeeStrategy(new PercentageFeeStrategy(1.5m));
                Console.WriteLine("Установлена процентная комиссия 1.5%");
                break;
            case "2":
                bankingSystem.SetFeeStrategy(new FixedFeeStrategy(10m));
                Console.WriteLine("Установлена фиксированная комиссия 10");
                break;
            default:
                Console.WriteLine("Неверный выбор. Стратегия не изменена.");
                break;
        }
    }
}