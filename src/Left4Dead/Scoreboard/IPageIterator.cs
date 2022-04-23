namespace Left4Dead
{
    public interface IPageIterator
    {
        bool HasNextPage();
        bool HasPreviousPage();
        void NextPage();
        void PreviousPage();
        object GetPage();
    }
}
