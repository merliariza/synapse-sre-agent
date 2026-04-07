namespace SynapseSRE.Domain.Entities;

public class ActivityLog
{
public Guid Id { get; set; }
public Guid? UserId { get; set; }
public string Action { get; set; } = string.Empty; 


public string Details { get; set; } = "{}"; 

public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

public User? User { get; set; }
}