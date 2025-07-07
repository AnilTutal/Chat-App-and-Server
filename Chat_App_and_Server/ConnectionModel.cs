namespace Chat_App_and_Server;

public class ConnectionModel
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string RemoteIpPoint { get; set; }

    public DateTime Timestampt { get; set; }

    public bool Status { get; set; }
}
