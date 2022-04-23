using System;

namespace Left4Dead
{
    public class SniperRifleModel : Model
    {
        public SniperRifleModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }

        public SniperRifleModel(Model copy) : base(copy) { }

        public override char DisplayModel(int rotation) { return ModelData[rotation]; }

        public override string ToString()
        {
            return "Sniper rifle weapon model";
        }
    }
}
