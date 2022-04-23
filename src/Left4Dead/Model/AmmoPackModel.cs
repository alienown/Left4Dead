using System;

namespace Left4Dead
{
    public class AmmoPackModel : Model
    {
        public AmmoPackModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }

        public AmmoPackModel(Model copy) : base(copy) { }

        public override char DisplayModel(int rotation) { return ModelData[rotation]; }

        public override string ToString()
        {
            return "Ammo pack model";
        }
    }
}
