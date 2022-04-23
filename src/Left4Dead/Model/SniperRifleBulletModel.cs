using System;

namespace Left4Dead
{
    public class SniperRifleBulletModel : Model
    {
        public SniperRifleBulletModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }

        public SniperRifleBulletModel(Model copy) : base(copy) { }

        public override char DisplayModel(int rotation) { return ModelData[rotation]; }

        public override string ToString()
        {
            return "Sniper rifle bullet model";
        }
    }
}
