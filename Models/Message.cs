public class Message
{
    public int start_city_id { get; set; }
    public int end_city_id { get; set; }

    public Message(int start_city_id, int end_city_id)
    {
        this.start_city_id = start_city_id;
        this.end_city_id = end_city_id;
    }

    public Message()
    {
    }
}
