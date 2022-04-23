using System;

namespace Left4Dead
{
    public class AK47Model : Model
    {
        public AK47Model(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }

        public AK47Model(Model copy) : base(copy) { }

        public override char DisplayModel(int rotation) { return ModelData[rotation]; }

        public override string ToString()
        {
            return "AK47 weapon model";
        }
    }
}
