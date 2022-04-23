namespace Left4Dead
{
    public interface IAggregate
    {
        IPageIterator CreateIterator();
    }
}
