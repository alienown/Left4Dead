using System;

namespace Left4Dead
{
    public class PistolBulletModel : Model
    {
        public PistolBulletModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }

        public PistolBulletModel(Model copy) : base(copy) { }

        public override char DisplayModel(int rotation) { return ModelData[rotation]; }

        public override string ToString()
        {
            return "Pistol bullet model";
        }
    }
}
