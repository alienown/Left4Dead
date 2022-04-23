namespace Left4Dead
{
    public class InterfaceDirector
    {
        private IInterfaceBuilder builder;

        public InterfaceDirector(IInterfaceBuilder builder)
        {
            this.builder = builder;
        }

        public void Construct()
        {
            builder.SetBackgroundColor();
            builder.SetUpperSide();
            builder.SetUpperRightCorner();
            builder.SetRightSide();
            builder.SetLowerRightCorner();
            builder.SetLowerSide();
            builder.SetLowerLeftCorner();
            builder.SetLeftSide();
            builder.SetUpperLeftCorner();
        }
    }
}
