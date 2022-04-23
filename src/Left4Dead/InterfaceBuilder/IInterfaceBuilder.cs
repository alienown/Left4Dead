namespace Left4Dead
{
    public interface IInterfaceBuilder
    {
        void SetBackgroundColor();
        void SetUpperLeftCorner();
        void SetUpperSide();
        void SetUpperRightCorner();
        void SetRightSide();
        void SetLowerRightCorner();
        void SetLowerSide();
        void SetLowerLeftCorner();
        void SetLeftSide();
        Product GetProduct();
    }
}
