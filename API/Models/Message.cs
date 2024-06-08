public class Message
{
    public int startCityId { get; set; }
    public int endCityId { get; set; }

    public Message(int startCityId, int endCityId)
    {
        this.startCityId = startCityId;
        this.endCityId = endCityId;
    }

    public Message()
    {
    }
}
