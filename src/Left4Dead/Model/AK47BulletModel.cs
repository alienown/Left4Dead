using System;

namespace Left4Dead
{
    public class AK47BulletModel : Model
    {
        public AK47BulletModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }

        public AK47BulletModel(Model copy) : base(copy) { }

        public override char DisplayModel(int rotation) { return ModelData[rotation]; }

        public override string ToString()
        {
            return "AK47 bullet model";
        }
    }
}
