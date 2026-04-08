public interface INotificationService {
    void SendAlert(string message);
}

public class ConsoleNotificationService : INotificationService {
    public void SendAlert(string message) {
        Console.WriteLine($"\n[ALERTA SRE] 📢 {message}\n");
    }
}