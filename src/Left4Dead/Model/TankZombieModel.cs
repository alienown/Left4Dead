using System;

namespace Left4Dead
{
    public class TankZombieModel : Model
    {
        public TankZombieModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }

        public TankZombieModel(Model copy) : base(copy) { }

        public override char DisplayModel(int rotation) { return ModelData[rotation]; }

        public override string ToString()
        {
            return "Tank zombie model";
        }
    }
}
