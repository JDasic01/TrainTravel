public class Id
{
    private static int nextId;

    public int EntityId { get; private set; }

    static Id()
    {
        nextId = 1;
    }

    public Id()
    {
        EntityId = Interlocked.Increment(ref nextId);
    }
}
