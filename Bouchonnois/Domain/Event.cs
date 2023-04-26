namespace Bouchonnois.Domain;

public record Event(DateTime Date, string Message)
{
    public override string ToString()
    {
        return string.Format("{0:HH:mm} - {1}", Date, Message);
    }
}